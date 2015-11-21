namespace WikiExportParser
{
    using PathfinderDb.Schema;
    using System;

    internal static class References
    {
        public const string PathfinderFrWiki = "Wiki Pathfinder-fr.org";

        public const string PaizoPrd = "Paizo PRD";

        public const string BbeDrp = "DRP Black-Book-Éditions";

        public static ElementReference FromFrWiki(string url)
        {
            return new ElementReference
            {
                Name = PathfinderFrWiki,
                Href = new Uri(url)
            };
        }

        public static ElementReference FromFrBookWithPage(string sourceId, int page)
        {
            return new ElementReference
            {
                Name = string.Format("{0} p.{1}", SourceIdToName(sourceId), page),
                HrefString = string.Format("book:/isbn:{0}/page/{1}", SourceIdToFrISBN(sourceId), page)
            };
        }

        public static string SourceIdToName(string sourceId)
        {
            switch (sourceId)
            {
                case Source.Ids.Bestiary: return "Bestiaire";
                case Source.Ids.Bestiary2: return "Bestiaire 2";
                case Source.Ids.Bestiary3: return "Bestiaire 3";
                default: return null;
            }
        }

        public static string SourceIdToFrISBN(string sourceId)
        {
            switch (sourceId)
            {
                case Source.Ids.Bestiary: return "978-2-915847-88-8";
                case Source.Ids.Bestiary2: return "978-2-36328-103-6";
                case Source.Ids.Bestiary3: return "978-2-36328-001-5";
                default: return null;
            }
        }

        public static ElementReference FromFrDrp(string pageId)
        {
            var drpPageName = new char[pageId.Length];
            var wordStart = true;
            for (int i = 0; i < pageId.Length; i++)
            {
                var c = pageId[i];
                if (char.IsLetterOrDigit(c))
                {
                    if (wordStart)
                    {
                        drpPageName[i] = char.ToUpperInvariant(c);
                        wordStart = false;
                    }
                    else
                    {
                        drpPageName[i] = c;
                    }
                }
                else
                {
                    drpPageName[i] = '-';
                    wordStart = true;
                }
            }

            var drpLink = string.Format("http://www.regles-pathfinder.fr/{0}.html", new string(drpPageName));
            return new ElementReference
            {
                Name = References.BbeDrp,
                Href = new Uri(drpLink)
            };
        }
    }
}
