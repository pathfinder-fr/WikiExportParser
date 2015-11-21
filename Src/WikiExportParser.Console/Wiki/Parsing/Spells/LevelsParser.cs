namespace WikiExportParser.Wiki.Parsing.Spells
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using PathfinderDb.Schema;

    internal static class LevelsParser
    {
        private const string ClassPattern = @"\[\[{0}(\|{1})?\]\] (?<Level>\d+)";

        private const string EnsWizPattern1 = @"\[\[ensorceleur\|Ens\]\]/\[\[magicien\|Mag\]\] (?<Level>\\d+)";

        private const string EnsWizPattern2 = @"\[\[ensorceleur\]\]/\[\[magicien\]\] (?<Level>\d+)";

        public static void ParseLevels(string html, Spell spell, ILog log)
        {
            List<SpellListLevel> levels = new List<SpellListLevel>();

            ParseClassLevel(html, levels, SpellList.Ids.Bard, string.Format(ClassPattern, "barde", "Bard"));
            if (!ParseClassLevel(html, levels, SpellList.Ids.SorcererWizard, EnsWizPattern1))
            {
                // Version alternative
                if (!ParseClassLevel(html, levels, SpellList.Ids.SorcererWizard, string.Format(ClassPattern, "magicien", "Mag")))
                {
                    ParseClassLevel(html, levels, SpellList.Ids.SorcererWizard, EnsWizPattern2);
                }
            }

            ParseClassLevel(html, levels, SpellList.Ids.Ranger, string.Format(ClassPattern, "rôdeur", "Rôd"));
            ParseClassLevel(html, levels, SpellList.Ids.Paladin, string.Format(ClassPattern, "paladin", "Pal"));
            ParseClassLevel(html, levels, SpellList.Ids.Druid, string.Format(ClassPattern, "druide", "Dru"));
            ParseClassLevel(html, levels, SpellList.Ids.Cleric, string.Format(ClassPattern, "prêtre", "Prê"));
            ParseClassLevel(html, levels, SpellList.Ids.Inquisitor, string.Format(ClassPattern, "inquisiteur", "Inq"));
            ParseClassLevel(html, levels, SpellList.Ids.Summoner, string.Format(ClassPattern, "invocateur|conjurateur", "Inv"));
            ParseClassLevel(html, levels, SpellList.Ids.Witch, string.Format(ClassPattern, "sorcière", "Sor"));
            ParseClassLevel(html, levels, SpellList.Ids.Alchemist, string.Format(ClassPattern, "alchimiste", "Alch"));
            ParseClassLevel(html, levels, SpellList.Ids.Oracle, string.Format(ClassPattern, "oracle", "Ora"));
            ParseClassLevel(html, levels, SpellList.Ids.Oracle, string.Format(ClassPattern, "prêtre", "Prê"));
            ParseClassLevel(html, levels, SpellList.Ids.Magus, string.Format(ClassPattern, "magus", "magus"));
            ParseClassLevel(html, levels, SpellList.Ids.AntiPaladin, string.Format(ClassPattern, "antipaladin", "antipaladin"));

            if (levels.Count == 0)
            {
                throw new ParseException("Impossible de détecter les niveaux de classe");
            }

            spell.Levels = levels.ToArray();
        }

        private static bool ParseClassLevel(string html, List<SpellListLevel> levels, string id, string pattern)
        {
            var match = Regex.Match(html, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            if (match.Success)
            {
                if (!levels.Any(l => l.List == id))
                {
                    levels.Add(new SpellListLevel { List = id, Level = int.Parse(match.Groups["Level"].Value) });
                }
                return true;
            }

            return false;
        }
    }
}