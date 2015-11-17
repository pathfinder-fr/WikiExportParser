// -----------------------------------------------------------------------
// <copyright file="DescriptionParser.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

namespace WikiExportParser.Wiki.Parsing.Spells
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using PathfinderDb.Schema;

    internal static class DescriptionParser
    {
        private static readonly List<string> noListSpells;

        private static readonly Regex regex = new Regex(@"^\* '''''\[\[((?<Link>[^\|\]]+)\|)?(?<Title>[^\|\]]+)\]\]'{2,5}\s*(?<Foc>('')?\([MF]\)('')?\s*)?(''\((?:APG|AdM|Blog Paizo|UC)\)''\.?\s+)?(?<Foc>\(\s*[MF, ]+\)\s*)?\(\s*(\[\[[^\]]+\]\](,?\s*)?)*\s*\)\s*(?<Foc>\(\s*[MF, ]+\)\s*)?\. *(?<Desc>[^\r\n]*)", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

        static DescriptionParser()
        {
            noListSpells = EmbeddedResources.LoadString("Resources.SpellNoList.txt").Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static void ParseDescriptions(IEnumerable<Spell> spells, IEnumerable<WikiPage> pages, IDictionary<string, string> descriptions, ILog log)
        {
            foreach (var page in pages)
            {
                foreach (Match match in regex.Matches(page.Raw))
                {
                    var title = match.Groups["Title"].Value.Replace('’', '\'');
                    if (descriptions.ContainsKey(title))
                    {
                        // Remplacement ?
                    }
                    else
                    {
                        var description = match.Groups["Desc"].Value;
                        description = MarkupUtil.RemoveMarkup(description);
                        descriptions.Add(title, description);
                    }
                }
            }

            //return descriptions;
            //var descriptions = WikiExportParser.Wiki.Parsing.SpellParser.ParseDescriptions(spellLists, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
            var usedDescriptions = new List<string>();

            foreach (var spell in spells)
            {
                string desc;
                var title = spell.Name.Replace('’', '\'');
                if (descriptions.TryGetValue(title, out desc))
                {
                    spell.Summary = desc;
                    usedDescriptions.Add(title);
                }
                else if (noListSpells.All(s => s != spell.Id))
                {
                    log.Warning("Impossible de trouver la description du sort {0} (id {1}) dans les listes de sorts", spell.Name, spell.Id);
                }
            }
        }
    }
}