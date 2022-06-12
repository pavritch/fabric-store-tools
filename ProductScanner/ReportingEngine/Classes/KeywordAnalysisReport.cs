using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities.Extensions;

namespace ReportingEngine.Classes
{
    public class KeywordAnalysisReport : ReportBase
    {
        #region Locals

        private readonly string rootFolder;
        private readonly string outputFilename;
        private List<string> searchExpressions;

        /// <summary>
        /// this is the collection which is built up during the scanning process.
        /// </summary>
        private ConcurrentDictionary<string, KeywordResult> keywordResults;

        #endregion

        #region Private Properties

        /// <summary>
        /// Name of vendor to show in report.
        /// </summary>
        private string ManufacturerName { get; set; }

        /// <summary>
        /// Collection to hold results from scanning. Later, can build report from this since it has everything needed.
        /// </summary>
        private ConcurrentDictionary<string, KeywordResult> KeywordResults
        {
            get
            {
                if (keywordResults == null)
                {
                    keywordResults = new ConcurrentDictionary<string, KeywordResult>();
                }

                return keywordResults;
            }
        }

        private List<string> SearchExpressions
        {
            get
            {
                return searchExpressions;
            }
        }

        #endregion

        public string ErrorMessage { get; private set; }

        public KeywordAnalysisReport(string manufacturerName, string rootFolder, string outputFilename, CancellationToken cancelToken = default(CancellationToken))
            : base(cancelToken)
        {
            this.ManufacturerName = manufacturerName;
            this.rootFolder = rootFolder;
            this.outputFilename = outputFilename;
        }

        /// <summary>
        /// Load up the list of keywords or regex to look for.
        /// </summary>
        /// <remarks>
        /// White space and lines that start with comments are ignored.
        /// </remarks>
        private void LoadSearchExpressions()
        {
            var text = "KeywordsForAnalysis.txt".GetEmbeddedTextFile();
            var list = new List<string>();

            using (StringReader reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("//"))
                        list.Add(line.Trim());
                }
            }

            searchExpressions = list.Distinct().ToList();
        }

        public Task<bool> GenerateReportAsync(IProgress<int> progressIndicator = null)
        {
            base.ProgressIndicator = progressIndicator;
            return GenerateReport();
        }


        private string ReadTextFile(string filepath)
        {
            using (var stream = System.IO.File.OpenText(filepath))
            {
                return stream.ReadToEnd();
            }
        }


        /// <summary>
        /// Add a new result to the collection of results. Thread safe.
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="location"></param>
        /// <param name="?"></param>
        private void AddKeywordResult(string phrase, string location, string matchText, HtmlAgilityPack.HtmlNode node)
        {
            AddKeywordResult(phrase, location, matchText, node.OuterHtml);
        }

        private void AddKeywordResult(string phrase, string location, string matchText, string surroundingText)
        {
            //Debug.WriteLine(string.Format("Match on {0}: {1} in {2}", phrase, matchText, surroundingText));

            // get the object for the phrase (what we searched on)

            // concurrent collections are used for all of this so completely thread safe

            KeywordResult kw;
            if (!KeywordResults.TryGetValue(phrase, out kw))
            {
                // was not found, attempt to add, if the add fails, then was
                // just added, so get the object
                kw = new KeywordResult(phrase);
                if (!KeywordResults.TryAdd(phrase, kw))
                    kw = KeywordResults[phrase];
            }

            // kw now has the record we want to operate on irrespective of if needed to be added

            ConcurrentBag<SnippetLocation> locations;
            if (!kw.Snippets.TryGetValue(matchText, out locations))
            {
                // was not found, attempt to add, if the add fails, then was
                // just added, so get the object
                locations = new ConcurrentBag<SnippetLocation>();
                if (!kw.Snippets.TryAdd(matchText, locations))
                    locations = kw.Snippets[matchText];
            }

            // we now have the locations - add the new item

            locations.Add(new SnippetLocation(location, surroundingText));
        }


        /// <summary>
        /// Process a single html file and add results to global collection.
        /// </summary>
        /// <param name="filepath"></param>
        private void ProcessHtmlFile(string filepath)
        {
            var html = ReadTextFile(filepath);
            var doc = html.ParseHtmlPage();

            var options = RegexOptions.Compiled | RegexOptions.IgnoreCase;

            // this is a collection of nodes we wish to processes - they do not have any child nodes - innerHtml and innerText should be identical.

            // see what kinds of tags are coming back
            //var listTags = doc.DocumentNode.DescendantsAndSelf().Select(e => e.Name).Distinct().ToList();

            var excludedTagNames = new string[] {"#comment", "script", "#text", "style", "document", "head", "meta", "title", "link" };
            var nodes = doc.DocumentNode.DescendantsAndSelf().Where(e => !excludedTagNames.Contains(e.Name) && e.InnerHtml == e.InnerText && !string.IsNullOrWhiteSpace(e.InnerText)).ToList();

            foreach (var node in nodes)
            {
                if (string.IsNullOrWhiteSpace(node.InnerText))
                    continue;

                foreach (var expression in SearchExpressions)
                {
                    var matches = Regex.Matches(node.InnerText, expression, options);

                    for (int i = 0; i < matches.Count; i++)
                    {
                        var capturedText = matches[i].Groups[0].ToString();
                        AddKeywordResult(expression, filepath, capturedText, node);
                    }
                }
                ThrowOnCancel();
            }

        }

        /// <summary>
        /// Process a single json file and add results to global collection.
        /// </summary>
        /// <param name="filepath"></param>
        private void ProcessJsonFile(string filepath)
        {
            var json = ReadTextFile(filepath);

            var options = RegexOptions.Compiled | RegexOptions.IgnoreCase;

            // look for curly brace to see if might be complex json, as opposed to something really simple like plain text

            if (json.Contains("{") || json.Contains("["))
            {
                // looks to be a complex object, operate on the individual properties

                var o = JObject.Parse(json);

                foreach (JToken child in o.Children())
                {
                    foreach (JToken grandChild in child)
                    {
                        foreach (JToken grandGrandChild in grandChild)
                        {
                            var property = grandGrandChild as JProperty;

                            if (property != null)
                            {
                                //Debug.WriteLine(string.Format("{0}: {1}", property.Name, property.Value));

                                var value = property.Value.ToString();
                                foreach (var expression in SearchExpressions)
                                {
                                    var matches = Regex.Matches(value, expression, options);

                                    for (int i = 0; i < matches.Count; i++)
                                    {
                                        var capturedText = matches[i].Groups[0].ToString();
                                        AddKeywordResult(expression, filepath, capturedText, string.Format("{0}: {1}", property.Name, property.Value));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // looks to be simple object or text, operate on the entire json string rather than individual properties

                foreach (var expression in SearchExpressions)
                {
                    var matches = Regex.Matches(json, expression, options);

                    for (int i = 0; i < matches.Count; i++)
                    {
                        var capturedText = matches[i].Groups[0].ToString();
                        AddKeywordResult(expression, filepath, capturedText, json);
                    }
                }
            }
        }

        private async Task<bool> GenerateReport()
        {
            try
            {
                int countTotal;
                int countCompleted;

                LoadSearchExpressions();

                var fileList = RecursiveFolderScan(rootFolder).ToList();
                countTotal = fileList.Count();
                countCompleted = 0;

                // the action block takes a single input parameter which is 
                // the fully qualified filepth to a file to process.

                var actionBlock = new ActionBlock<string>((filepath) =>
                {
                    ThrowOnCancel();

                    try
                    {
                        Debug.WriteLine(string.Format("Processing: {0}", Path.GetFileName(filepath)));
                        // do work here for one file
                        if (string.Equals(Path.GetExtension(filepath), ".json", StringComparison.OrdinalIgnoreCase))
                            ProcessJsonFile(filepath);
                        else
                            ProcessHtmlFile(filepath);

                    }
                    catch (Exception Ex)
                    {
                        Debug.WriteLine("Exception: " + Ex.Message);
                    }

                    Interlocked.Increment(ref countCompleted);
                    ReportProgressPercent(countCompleted, countTotal);

                }, new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 12
                });

                // feed in the work items by scanning the target folder


                foreach (var filepath in fileList)
                    actionBlock.Post(filepath);

                // indicate all work items have been submitted
                actionBlock.Complete();

                // wait until all the files have been processed
                await actionBlock.Completion;

                ThrowOnCancel();

                ErrorMessage = string.Empty;
                var html = "KeywordAnalysisReport.html".GetEmbeddedTextFile();

                // create a type which will perfectly format to the json we want

                var aryDataMatched = (from kw in KeywordResults
                               orderby kw.Key
                               let snips = (from s in kw.Value.Snippets
                                            orderby s.Key
                                            let locs = (from loc in s.Value
                                                        select new
                                                        {
                                                            Path = loc.Location,
                                                            Fragment = loc.HtmlFragment
                                                        }).Take(100).ToArray()
                                            select new
                                            {
                                                MatchText = s.Key,
                                                Count = locs.Count(),
                                                Locations = locs

                                            }).Take(100).ToArray()
                               select new
                               {
                                   SearchPhrase = kw.Key,
                                   Count = snips.Count(),
                                   Snippets = snips
                               }).ToArray();

                var jsonDataMatched = JsonConvert.SerializeObject(aryDataMatched, SerializerSettings);

                // save a copy of the raw json too.
                jsonDataMatched.SaveTextAsFile(Path.ChangeExtension(outputFilename, ".json"));

                Func<ConcurrentDictionary<string, ConcurrentBag<SnippetLocation>>, Dictionary<string, int>> makePhrases = (sn) =>
                    {
                        // find all the distinct fragments 

                        var list = new List<SnippetLocation>();
                        foreach (var item in sn.Values)
                            list.AddRange(item);

                        var distinctList =  list.Select(loc => loc.HtmlFragment).Distinct().ToList();

                        var dic = new Dictionary<string, int>();

                        foreach (var item in distinctList)
                        {
                            int count = list.Where(e => e.HtmlFragment == item).Count();
                            dic.Add(item, count);
                        }

                        return dic;
                    };


                var aryDataPhrases = (from kw in KeywordResults
                                      orderby kw.Key
                                      let phrases = (from p in makePhrases(kw.Value.Snippets) 
                                                    orderby p.Key
                                                    select new
                                                    {
                                                        Fragment = p.Key.Left(200),
                                                        Count = p.Value
                                                    }).Take(1000)
                                      select new
                                      {
                                          SearchPhrase = kw.Key,
                                          Count = phrases.Count(),
                                          Phrases = phrases
                                      }).ToArray();

                var jsonDataPhrases = JsonConvert.SerializeObject(aryDataPhrases, SerializerSettings);

                // perform replacements

                html = html.Replace("{{manufacturer-name}}", ManufacturerName);
                html = html.Replace("{{report-date}}", DateTime.Now.ToString("f"));   // Monday, June 15, 2009 1:45 PM 
                html = html.Replace("{{csv-filename}}", rootFolder);
                html = html.Replace("//{{json-data-matched}}", string.Format("var jsonDataMatched = {0};", jsonDataMatched));
                html = html.Replace("//{{json-data-phrases}}", string.Format("var jsonDataPhrases = {0};", jsonDataPhrases));

                // save to disk
                html.SaveTextAsFile(outputFilename);

                return true;
            }
            catch (Exception Ex)
            {
                ErrorMessage = String.Format("Exception: {0}",  Ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Recursively scan the specified folder and return a list of html filepaths.
        /// </summary>
        /// <remarks>
        /// Note that json files also returned - because some vendors are using ajax for
        /// important information.
        /// </remarks>
        /// <param name="rootFolder"></param>
        /// <returns></returns>
        private List<string> RecursiveFolderScan(string rootFolder)
        {
            var list = new List<string>();

            if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
                throw new Exception("Missing root folder.");

            list.AddRange(Directory.EnumerateFiles(rootFolder, "*.html", SearchOption.AllDirectories));
            list.AddRange(Directory.EnumerateFiles(rootFolder, "*.htm", SearchOption.AllDirectories));
            list.AddRange(Directory.EnumerateFiles(rootFolder, "*.json", SearchOption.AllDirectories));

            return list;
        }
    }
}
