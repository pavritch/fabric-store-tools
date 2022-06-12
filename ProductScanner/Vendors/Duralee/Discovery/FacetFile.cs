namespace Duralee.Discovery
{
    public class FacetFile
    {
        public string Content { get; set; }
        public string FacetName { get; set; }
        public string FacetValue { get; set; }

        public FacetFile(string content, string facetName, string facetValue)
        {
            Content = content;
            FacetName = facetName;
            FacetValue = facetValue;
        }
    }
}