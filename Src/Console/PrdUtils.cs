namespace WikiExportParser
{
    using System.Text.RegularExpressions;
    using PathfinderDb.Schema;

    internal static class PrdUtils
    {
        private const string PrdUrlFormat = "http://paizo.com/pathfinderRPG/{0}/{1}.html#_{2}";

        public static string GenerateSpellPrdUrl(string englishSpellName, string sourceId)
        {
            var pageName = englishSpellName;

            var commaIndex = pageName.IndexOf(',');
            if (commaIndex != -1)
            {
                pageName = pageName.Substring(0, commaIndex).Trim();
            }

            pageName = Regex.Replace(pageName, " [IXV]+$", string.Empty, RegexOptions.CultureInvariant);
            pageName = pageName.ToLowerInvariant();
            pageName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pageName);
            pageName = pageName.Replace(" ", string.Empty);
            pageName = char.ToLowerInvariant(pageName[0]) + pageName.Substring(1);
            pageName = pageName.Replace("'", string.Empty);

            var anchorName = englishSpellName;

            anchorName = anchorName.ToLowerInvariant();
            anchorName = anchorName.Replace(' ', '-');
            anchorName = anchorName.Replace('\'', '-');

            string source;
            switch (sourceId)
            {
                case Source.Ids.AdvancedPlayerGuide:
                    source = "prd/advanced/spells";
                    break;

                case Source.Ids.UltimateMagic:
                    source = "prd/ultimateMagic/spells";
                    break;

                case Source.Ids.UltimateCombat:
                    source = "prd/ultimateCombat/spells";
                    break;

                default:
                    source = "prd/spells";
                    break;
            }

            return string.Format(PrdUrlFormat, source, pageName, anchorName);
        }
    }
}
