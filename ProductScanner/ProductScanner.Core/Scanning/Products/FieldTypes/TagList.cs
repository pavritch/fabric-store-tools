using System.Collections.Generic;
using System.Linq;
using Utilities.Extensions;

namespace ProductScanner.Core.Scanning.Products.FieldTypes
{
    public class TagList
    {
        private readonly List<string> _tags;

        public TagList(List<string> tags)
        {
            _tags = tags;
        }

        public List<string> GetFormattedTags()
        {
            var formattedTags = _tags.Select(FormatSingleTag).ToList();
            formattedTags = formattedTags.Select(x => x.TitleCase().Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            return formattedTags;
        }

        private string FormatSingleTag(string tag)
        {
            tag = tag.ReplaceWholeWord("Rugs", "");
            tag = tag.ReplaceWholeWord("Causal", "Casual");
            tag = tag.ReplaceWholeWord("Soutwestern", "Southwestern");
            tag = tag.ReplaceWholeWord("Traditonal", "Traditional");
            tag = tag.ReplaceWholeWord("Trasitional", "Transitional");
            tag = tag.ReplaceWholeWord("Dropstitch", "Drop Stitch");
            return tag;
        }
    }
}