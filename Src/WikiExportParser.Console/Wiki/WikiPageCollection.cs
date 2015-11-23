// -----------------------------------------------------------------------
// <copyright file="WikiPageCollection.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

namespace WikiExportParser.Wiki
{
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
            pages.Add(name, page);
        }

        public WikiPage this[WikiName name]
        {
            get { return pages[name]; }
        }

        public int Count
        {
            get { return pages.Count; }
        }

        public bool TryGet(WikiName name, out WikiPage page)
        {
            return pages.TryGetValue(name, out page);
        }

        public WikiPage GetOrEmpty(WikiName name)
        {
            WikiPage page;
            if (!pages.TryGetValue(name, out page))
            {
                return new WikiPage(wiki, new XmlWikiPage {FullName = name.ToString(), Title = string.Format("Inconnue ({0})", name)});
            }

            return page;
        }

        public IEnumerator<WikiPage> GetEnumerator()
        {
            return pages.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}