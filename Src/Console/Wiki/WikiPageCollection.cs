namespace WikiExportParser.Wiki
{
    using System.Collections.Generic;

    public class WikiPageCollection : IEnumerable<WikiPage>
    {
        private readonly Dictionary<WikiName, WikiPage> pages = new Dictionary<WikiName, WikiPage>();

        private readonly WikiExport wiki;

        internal WikiPageCollection(WikiExport wiki)
        {
            this.wiki = wiki;
        }

        internal void Add(WikiName name, WikiPage page)
        {
            this.pages.Add(name, page);
        }

        public WikiPage this[WikiName name]
        {
            get { return this.pages[name]; }
        }

        public int Count
        {
            get { return this.pages.Count; }
        }

        public bool TryGet(WikiName name, out WikiPage page)
        {
            return this.pages.TryGetValue(name, out page);
        }

        public WikiPage GetOrEmpty(WikiName name)
        {
            WikiPage page;
            if (!this.pages.TryGetValue(name, out page))
            {
                return new WikiPage(this.wiki, new XmlWikiPage { FullName = name.ToString(), Title = string.Format("Inconnue ({0})", name) });
            }

            return page;
        }

        public IEnumerator<WikiPage> GetEnumerator()
        {
            return this.pages.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
