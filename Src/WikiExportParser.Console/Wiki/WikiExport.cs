// -----------------------------------------------------------------------
// <copyright file="WikiExport.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace WikiExportParser.Wiki
{
    public class WikiExport
    {
        private readonly WikiPageCollection pages;

        private readonly SortedList<WikiName, WikiName> categories = new SortedList<WikiName, WikiName>();

        private readonly Dictionary<WikiName, ICollection<WikiName>> links = new Dictionary<WikiName, ICollection<WikiName>>();

        public WikiExport()
        {
            pages = new WikiPageCollection(this);
        }

        public WikiPageCollection Pages
        {
            get { return pages; }
        }

        public IEnumerable<WikiName> Categories
        {
            get { return categories.Values; }
        }

        public IDictionary<WikiName, WikiName> CategoriesIndex
        {
            get { return categories; }
        }

        public WikiPage FindPage(WikiName name)
        {
            WikiPage page;
            ICollection<WikiName> links;

            int i = 0;
            while (i < 10)
            {
                if (!pages.TryGet(name, out page))
                {
                    if (!this.links.TryGetValue(name, out links))
                    {
                        // Pas de lien
                        return null;
                    }

                    if (links.Count != 1)
                    {
                        // S'il s'agit d'une redirection, il n'y aura qu'un seul lien
                        // Sinon on ne fait rien
                        return null;
                    }

                    var linkName = links.ElementAt(0);

                    if (name == linkName)
                    {
                        // Boucle de redirection
                        return null;
                    }

                    name = linkName;
                }
                else
                {
                    return page;
                }

                i++;
            }

            return null;
        }

        public void Load(string path)
        {
            var serializer = new XmlSerializer(typeof (XmlWikiPage));

            foreach (var subDir in Directory.GetDirectories(path))
            {
                foreach (var fileName in Directory.GetFiles(subDir))
                {
                    if (Path.GetExtension(fileName).Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            LoadPage(serializer, subDir, fileName);
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException(string.Format("Erreur durant le chargement XML de la page {0}", fileName), ex);
                        }
                    }
                }
            }
        }

        private void LoadPage(XmlSerializer serializer, string subDir, string fileName)
        {
            // Désérialisation page
            XmlWikiPage xmlPage;
            using (var reader = new StreamReader(fileName))
            {
                xmlPage = (XmlWikiPage) serializer.Deserialize(reader);
                xmlPage.FileName = fileName;
            }

            if (string.IsNullOrWhiteSpace(xmlPage.FullName))
            {
                throw new ApplicationException(string.Format("La page du fichier {0} ne contient pas de nom complet", fileName));
            }

            var name = new WikiName(xmlPage.FullName);

            // Chargement catégories
            foreach (var categoryName in xmlPage.Categories)
            {
                var category = WikiName.FromString(categoryName);
                if (!categories.ContainsKey(category))
                {
                    categories.Add(category, category);
                }
            }

            // Chargement page
            var page = new WikiPage(this, xmlPage);
            pages.Add(name, page);

            // Chargement liens entrants
            ICollection<WikiName> targets;
            foreach (var linkName in xmlPage.InLinksNames)
            {
                if (!links.TryGetValue(linkName, out targets))
                {
                    targets = new HashSet<WikiName>();
                    links[linkName] = targets;
                }

                if (!targets.Contains(name))
                {
                    targets.Add(name);
                }
            }

            // Chargement liens sortants
            if (!links.TryGetValue(name, out targets))
            {
                targets = new HashSet<WikiName>();
                links[name] = targets;
            }

            foreach (var linkName in xmlPage.OutLinksNames)
            {
                if (!targets.Contains(name))
                {
                    targets.Add(name);
                }
            }
        }
    }
}