namespace Maxwell.Metadata
{
    public class SearchCollection
    {
        public string Name { get; set; }
        public string QueryKey { get; set; }
        public string QueryValue { get; set; }

        public SearchCollection(string name, string queryKey, string queryValue)
        {
            Name = name;
            QueryKey = queryKey;
            QueryValue = queryValue;
        }
    }
}