using System.Collections.Concurrent;

namespace ReportingEngine.Classes
{
    /// <summary>
    /// Holds the results for a single search keyword.
    /// </summary>
    /// <remarks>
    /// Objects only created for found phrases - to avoid clutter.
    /// </remarks>
    public class KeywordResult
    {
        /// <summary>
        /// The regex from the master list used to go searching.
        /// </summary>
        public string SearchPhrase { get; set; }

        // dic[matchedText, list[wherefound]]
        public ConcurrentDictionary<string, ConcurrentBag<SnippetLocation>> Snippets { get; set; }

        public KeywordResult(string phrase)
        {
            this.SearchPhrase = phrase;
            Snippets = new ConcurrentDictionary<string, ConcurrentBag<SnippetLocation>>();
        }
    }
}
