namespace WikiExportParser.Wiki.Parsing.Spells
{
    using System;
    using System.Text.RegularExpressions;
    using PathfinderDb.Schema;

    internal static class SchoolParser
    {
        private const string UniversalSchoolPattern = "'''École''' Universel";

        private const string SchoolPattern = @"'''[ÉE]cole''' \[\[(?<Title>[^\]]+)\]\]( \(\[\[branche [^\]]+\|(?<SubSchool>[^\]]+)\]\]\))?";

        public static void ParseSchool(string html, Spell spell, ILog log)
        {
            var schoolMatch = Regex.Match(html, SchoolPattern, RegexOptions.CultureInvariant);

            if (schoolMatch.Success)
            {
                spell.School = ParseSpellSchoolTitle(schoolMatch.Groups["Title"].Value);
            }
            else if (html.IndexOf(UniversalSchoolPattern, StringComparison.OrdinalIgnoreCase) != -1)
            {
                spell.School = SpellSchool.Universal;
            }
            else
            {
                throw new ParseException("Impossible de lire l'école de magie");
            }

            // TODO lire les branches
        }

        private static SpellSchool ParseSpellSchoolTitle(string title)
        {
            switch (title.ToLowerInvariant())
            {
                case "abjuration":
                    return SpellSchool.Abjuration;

                case "divination":
                case "école divination|divination":
                    return SpellSchool.Divination;

                case "enchantement":
                    return SpellSchool.Enchantment;

                case "évocation":
                    return SpellSchool.Evocation;

                case "illusion":
                    return SpellSchool.Illusion;

                case "invocation":
                    return SpellSchool.Conjuration;

                case "nécromancie":
                case "nécromancie (école)|nécromancie":
                    return SpellSchool.Necromancy;

                case "transmutation":
                    return SpellSchool.Transmutation;

                case "universelle":
                    return SpellSchool.Universal;

                default:
                    throw new ParseException(string.Format("École {0} non reconnue", title));
            }
        }
    }
}