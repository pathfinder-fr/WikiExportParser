// -----------------------------------------------------------------------
// <copyright file="WikiPage.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace WikiExportParser.Wiki
{
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
            displayName = source.Title;
            wikiName = WikiName.FromString(source.FullName);

            Url = string.Format("http://www.pathfinder-fr.org/Wiki/Pathfinder-RPG.{0}.ashx", Name);
        }

        public string Title
        {
            get { return source.Title; }
        }

        public List<WikiName> Categories
        {
            get { return source.CategoriesNames.Select(c => wiki.CategoriesIndex[c]).ToList(); }
        }

        public DateTime LastModified
        {
            get { return source.LastModified; }
        }

        public int Version
        {
            get { return source.Version; }
        }

        public IEnumerable<WikiPage> InLinks
        {
            get { return source.InLinksNames.Select(c => wiki.Pages.GetOrEmpty(c)).Where(c => c != null); }
        }

        public IEnumerable<WikiPage> OutLinks
        {
            get { return source.OutLinksNames.Select(c => wiki.Pages.GetOrEmpty(c)).Where(c => c != null); }
        }

        public string FileName
        {
            get { return source.FileName; }
        }

        /// <summary>
        /// Obtient le contenu HTML de la page.
        /// </summary>
        public string Body
        {
            get { return source.Body; }
        }

        public string Raw
        {
            get { return source.Raw ?? string.Empty; }
        }

        public string FullName
        {
            get { return source.FullName; }
        }

        public WikiName WikiName
        {
            get { return wikiName; }
        }

        public string Name
        {
            get
            {
                if (name == null)
                {
                    var i = FullName.IndexOf('.');

                    if (i != -1)
                        name = FullName.Substring(i + 1);
                    else
                        name = FullName;
                }

                return name;
            }
        }

        public string DisplayName
        {
            get
            {
                if (displayName == null)
                    displayName = Path.GetFileNameWithoutExtension(FileName) ?? string.Empty;

                return displayName;
            }
        }

        public string Id
        {
            get { return WikiName.Id; }
        }

        public string HtmlBody
        {
            get { return WebUtility.HtmlDecode(Body).Replace("<br />", "<br />\r\n"); }
        }

        public string Url { get; private set; }

        public bool IsRedirection(out WikiName target)
        {
            var raw = Raw.Trim();
            if (raw.StartsWith(">>> "))
            {
                target = WikiName.FromString(raw.Substring(4));
                return true;
            }
            target = null;
            return false;
        }
    }
}