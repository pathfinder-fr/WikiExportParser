//-----------------------------------------------------------------------
// <copyright file="WikiPage.cs" company="mB3M">
// Copyright (c) mB3M. Tous droits reserves.
// </copyright>
//-----------------------------------------------------------------------

namespace WikiExportParser.Wiki
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class WikiPage
    {
        private readonly XmlWikiPage source;

        private readonly WikiExport wiki;

        private readonly WikiName wikiName;

        private string name;

        private string displayName;

        public WikiPage(WikiExport wiki, XmlWikiPage source)
        {
            this.wiki = wiki;
            this.source = source;
            this.displayName = source.Title;
            this.wikiName = WikiName.FromString(source.FullName);

            this.Url = string.Format("http://www.pathfinder-fr.org/Wiki/Pathfinder-RPG.{0}.ashx", this.Name);
        }

        public string Title { get { return this.source.Title; } }

        public List<WikiName> Categories
        {
            get { return source.CategoriesNames.Select(c => wiki.CategoriesIndex[c]).ToList(); }
        }

        public DateTime LastModified
        {
            get { return this.source.LastModified; }
        }

        public int Version
        {
            get { return this.source.Version; }
        }

        public IEnumerable<WikiPage> InLinks
        {
            get
            {
                return this.source.InLinksNames.Select(c => this.wiki.Pages.GetOrEmpty(c)).Where(c => c != null);
            }
        }

        public IEnumerable<WikiPage> OutLinks
        {
            get
            {
                return this.source.OutLinksNames.Select(c => this.wiki.Pages.GetOrEmpty(c)).Where(c => c != null);
            }
        }

        public string FileName
        {
            get { return this.source.FileName; }
        }

        /// <summary>
        /// Obtient le contenu HTML de la page.
        /// </summary>
        public string Body
        {
            get { return this.source.Body; }
        }

        public string Raw
        {
            get { return this.source.Raw ?? string.Empty; }
        }

        public string FullName
        {
            get { return this.source.FullName; }
        }

        public WikiName WikiName
        {
            get { return this.wikiName; }
        }

        public string Name
        {
            get
            {
                if (name == null)
                {
                    var i = this.FullName.IndexOf('.');

                    if (i != -1)
                        name = this.FullName.Substring(i + 1);
                    else
                        name = this.FullName;
                }

                return this.name;
            }
        }

        public string DisplayName
        {
            get
            {
                if (this.displayName == null)
                    this.displayName = Path.GetFileNameWithoutExtension(this.FileName) ?? string.Empty;

                return this.displayName;
            }
        }

        public string Id
        {
            get
            {
                return this.WikiName.Id;
            }
        }

        public string HtmlBody
        {
            get { return System.Net.WebUtility.HtmlDecode(this.Body).Replace("<br />", "<br />\r\n"); }
        }

        public string Url { get; private set; }

        public bool IsRedirection(out WikiName target)
        {
            var raw = this.Raw.Trim();
            if (raw.StartsWith(">>> "))
            {
                target = WikiName.FromString(raw.Substring(4));
                return true;
            }
            else
            {
                target = null;
                return false;
            }
        }
    }
}