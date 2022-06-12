namespace ReportingEngine.Classes
{
    /// <summary>
    /// Holds location and surrounding fragment for a single found reference.
    /// </summary>
    public class SnippetLocation
    {
        /// <summary>
        /// The filepath or URL location where the html fragment was found.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// An html fragment containing the found phrase along with some nearby text.
        /// </summary>
        /// <remarks>
        /// Typically figuring that the fragment would be the containing html element, or possibly
        /// also it's parent -- anything that puts the phrase into context so we can heopfully
        /// see things like class identifiers that would help us zero in on a query.
        /// </remarks>
        public string HtmlFragment {get; set;}

        public SnippetLocation()
        {

        }

        public SnippetLocation(string location, string htmlFragment)
        {
            this.Location = location;
            this.HtmlFragment = htmlFragment;
        }
    }


}
