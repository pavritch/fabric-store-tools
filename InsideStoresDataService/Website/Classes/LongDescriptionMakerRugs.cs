using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;

namespace Website
{
    public class LongDescriptionMakerRugs : RugsDescriptionMakerBase
    {
        private string vendorDescription; // could be null if not provided or too weak

        public LongDescriptionMakerRugs(InsideRugsProduct product)
            : base(product)
        {

            vendorDescription = GetVendorDescription();

            var list = new List<Action<PhraseManager>>();

            if (vendorDescription != null)
            {
                list.Add(ContributeVendorDescription);
                list.Add(ContributeBulletList);
            }
            else
            {
                // missing or weak vendor description, make our own

                list.Add(ContributeDefaultDescription);
                list.Add(ContributeBulletList);
            }
            
            mgr.AddContributions(list);
        }

        private string GetVendorDescription()
        {
            var paragraphs = product.Features.Description;

            if (paragraphs == null || paragraphs.Count() == 0)
                return null;

            var sb = new StringBuilder(512);

            int countSentences = 0;
            int countWords = 0; // used to identify weak descriptions to discard.

            foreach (var paragraph in paragraphs)
            {

                var sentences = new List<string>(Regex.Split(paragraph, @"(?<=[\.!\?])\s+"));

                var processedSentences = new List<string>();

                foreach (var sentence in sentences)
                {
                    var s1 = sentence.Trim();

                    // need to remove any sentences which do not end with period or ! (because seems to be some truncation.

                    if (s1.EndsWith(".") || s1.EndsWith("!"))
                    {
                        s1 = s1.Replace("dÃ©cor", "decor");

                        // make sure every sentence starts with capital letter
                        processedSentences.Add(HttpUtility.HtmlEncode(Char.ToUpper(s1[0]) + s1.Substring(1)));
                    }
                }

                if (processedSentences.Count() == 0)
                    continue;

                sb.Append("<p>");

                bool isFirst = true;
                foreach (var s in processedSentences)
                {
                    if (!isFirst)
                        sb.Append("  ");

                    sb.Append(s);
                    countSentences++;
                    isFirst = false;

                    // keep a running tally of word counts
                    var ary = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    countWords += ary.Count();
                }
                sb.Append("</p>");
            }

            // make sure we were able to keep something

            if (countSentences == 0 || countWords < 7)
                return null;

            return sb.ToString();
        }

        #region Description Contributors

        private void ContributeVendorDescription(PhraseManager mgr)
        {
            var list1 = new List<PhraseVariant>()
            {
                new PhraseVariant(vendorDescription, 100)
            };

            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);
            mgr.EndPhraseSet();
        }


        private void ContributeDefaultDescription(PhraseManager mgr)
        {
            var list1 = new List<PhraseVariant>();

            Action<string, int> add = (s, w) =>
            {
                // will have null input when a merge fails to resolve; so not used
                if (!string.IsNullOrWhiteSpace(s))
                    list1.Add(new PhraseVariant(s, w));
            };

            if (hasStyle && hasColor)
            {
                if (hasWeave && hasCountry)
                {
                    add(Merge("{Adj} {style} {color} area rug {weave} in {Country}.", encloseInHtml:true), 100);
                }
                else if (hasCountry)
                {
                    add(Merge("{Adj} {style} {color} area rug made in {Country}.", encloseInHtml: true), 100);
                }
                else
                {
                    add(Merge("{Adj} {style} {color} area rug. Highest quality.", encloseInHtml: true), 100);
                }
            }
            else if (hasStyle)
            {
                if (hasWeave && hasCountry)
                {
                    add(Merge("{Adj} {style} area rug {weave} in {Country}.", encloseInHtml: true), 100);
                }
                else if (hasCountry)
                {
                    add(Merge("{Adj} {style} area rug made in {Country}.", encloseInHtml: true), 100);
                }
                else
                {
                    add(Merge("{Adj} {style} area rug. Highest quality.", encloseInHtml: true), 100);
                }
            }
            else if (hasColor)
            {
                if (hasWeave && hasCountry)
                {
                    add(Merge("{Adj} {color} area rug {weave} in {Country}.", encloseInHtml: true), 100);
                }
                else if (hasCountry)
                {
                    add(Merge("{Adj} {color} area rug made in {Country}.", encloseInHtml: true), 100);
                }
                else
                {
                    add(Merge("{Adj} {color} area rug. Highest quality.", encloseInHtml: true), 100);
                }
            }
            else
            {
                add(Merge("{Name}.", encloseInHtml: true), 100);
            }
            
            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);

            mgr.EndPhraseSet();
        }


        private void ContributeBulletList(PhraseManager mgr)
        {

            int bulletCount = 0;

            var sb = new StringBuilder(512);

            Action<string> add = (s) =>
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return;

                    sb.AppendFormat("<li>{0}</li>", HttpUtility.HtmlEncode(Merge(s)));
                    bulletCount++;
                };

            sb.Append("<ul>");

            if (hasStyle)
                add("{Style}");

            if (hasWeave)
                add("{Weave}");

            if (product.Features.Material != null && product.Features.Material.Count() > 0)
            {
                add(product.Features.Material.ToMaterialString());
            }

            if (countSizes > 1)
                add("Available in {CountSizes}");

            if (hasOutdoor)
                add("Indoor/outdoor");

            if (hasRunners)
                add("Matching runners available");

            if (hasCountry)
                add("Made in {Country}");

            sb.Append("</ul>");

            if (bulletCount == 0)
                return;

            var list1 = new List<PhraseVariant>()
            {
                new PhraseVariant(sb.ToString(), 100)
            };

            mgr.BeginPhraseSet();
            mgr.PickAndAddPhraseVariant(list1);
            mgr.EndPhraseSet();
        }




        #endregion


    }
}
