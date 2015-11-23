// -----------------------------------------------------------------------
// <copyright file="SpellGlossaryParser.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PathfinderDb.Schema;
using WikiExportParser.Logging;

namespace WikiExportParser.Wiki.Parsing
{
    internal class SpellGlossaryParser
    {
        private const string Pattern = @"\|\sclass=""gauche""\s\|\s(?<En>[^\[\r\n]+)(\s*\[\[(?<Prd>[^\r\n\]]+)\]\])?\r?\n\|\sclass=""gauche""\s\|\s(?<Fr>[^\[\r\n]+)";

        private readonly ILog log;

        private readonly WikiExport wiki;

        public SpellGlossaryParser(WikiExport wiki, ILog log)
        {
            this.wiki = wiki;
            this.log = log ?? NullLog.Instance;
        }

        public void Parse(WikiPage glossaryPage, List<Spell> spells)
        {
            var dict = new Dictionary<string, SpellGlossaryValue>(StringComparer.OrdinalIgnoreCase);
            var markup = glossaryPage.Raw;
            foreach (Match match in Regex.Matches(markup, Pattern, RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture))
            {
                var prdLink = match.Groups["Prd"].Value.Trim();
                var index = prdLink.LastIndexOf('|');
                if (index != -1)
                {
                    prdLink = prdLink.Substring(0, index);
                }

                var value = new SpellGlossaryValue
                {
                    En = match.Groups["En"].Value.Trim(),
                    Fr = match.Groups["Fr"].Value.Replace('’', '\'').Trim(),
                    PrdLink = prdLink
                };

                if (value.En != "?")
                {
                    SpellGlossaryValue existingEn;
                    if (dict.TryGetValue(value.Fr, out existingEn))
                    {
                        log.Warning("Le sort {0} est présent plusieurs fois dans le glossaire : {1} et {2}", value.Fr, existingEn.En, value.En);
                    }
                    else
                    {
                        dict.Add(value.Fr, value);
                    }
                }
            }

            foreach (var spell in spells)
            {
                SpellGlossaryValue value;
                if (dict.TryGetValue(spell.Name, out value))
                {
                    // Génération id basé sur le nom anglais
                    // Ignoré désormais, on préfère utiliser le nom en français
                    //spell.Id = Ids.Normalize(value.En);

                    // Ajout version anglaise du nom
                    spell.OpenLocalization().AddLocalizedEntry(DataSetLanguages.English, "name", value.En);

                    if (!string.IsNullOrEmpty(value.PrdLink))
                    {
                        // Ajout lien PRD
                        spell.Source.References.Add(new ElementReference {HrefString = value.PrdLink, Name = References.PaizoPrd, Lang = DataSetLanguages.English});
                    }

                    // Suppression du dictionnaire pour contrôle
                    dict.Remove(spell.Name);
                }
                else
                {
                    // Sort non présent dans le glossaire
                    log.Information("Le sort {0} ({1} {2}) n'est pas présent dans le glossaire anglais/français.", spell.Name, spell.Id, spell.Source.Id);
                }
            }

            foreach (var pair in dict)
            {
                log.Warning("Le sort {0} ({1}) du glossaire n'a pas été utilisé", pair.Value.Fr, pair.Key);
            }
        }

        class SpellGlossaryValue
        {
            public string En { get; set; }

            public string Fr { get; set; }

            public string PrdLink { get; set; }
        }
    }
}