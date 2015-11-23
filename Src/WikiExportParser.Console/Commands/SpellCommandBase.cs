// -----------------------------------------------------------------------
// <copyright file="SpellCommandBase.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using PathfinderDb.Schema;
using WikiExportParser.Logging;
using WikiExportParser.Wiki;
using WikiExportParser.Wiki.Parsing;
using WikiExportParser.Wiki.Parsing.Spells;

namespace WikiExportParser.Commands
{
    public abstract class SpellCommandBase
    {
        public WikiExport Wiki { get; set; }

        public ILog Log { get; set; }

        /// <summary>
        /// Lit et génère la liste des sorts.
        /// </summary>
        protected List<Spell> ReadSpells()
        {
            var wiki = Wiki;

            // Page contenant la liste des sorts
            var spellLists = new[]
            {
                WikiName.FromString("Pathfinder-RPG.liste des sorts"),
                WikiName.FromString("Pathfinder-RPG.liste des sorts (suite)"),
                WikiName.FromString("Pathfinder-RPG.liste des sorts (fin)")
            }.Select(pn => wiki.Pages[pn]);

            // Liste des pages à ignorer
            var blackList = new[]
            {
                "Pathfinder-RPG.liste des sorts",
                "Pathfinder-RPG.liste des sorts (fin)",
                "Pathfinder-RPG.liste des sorts (suite)",
                "Pathfinder-RPG.liste des sorts dantipaladin",
                "Pathfinder-RPG.liste des sorts dantipaladins",
                "Pathfinder-RPG.liste des sorts de bardes",
                "Pathfinder-RPG.liste des sorts de conjurateurs",
                "Pathfinder-RPG.liste des sorts de druides",
                "Pathfinder-RPG.liste des sorts Densorceleursmagiciens",
                "Pathfinder-RPG.liste des sorts de magus",
                "Pathfinder-RPG.liste des sorts de paladin",
                "Pathfinder-RPG.liste des sorts de paladins",
                "Pathfinder-RPG.liste des sorts de prêtres",
                "Pathfinder-RPG.liste des sorts de rôdeur",
                "Pathfinder-RPG.liste des sorts de rôdeurs",
                "Pathfinder-RPG.liste des sorts de sorcière",
                "Pathfinder-RPG.liste des sorts dinquisiteur",
                "Pathfinder-RPG.liste des formules dalchimiste",
                "Pathfinder-RPG.liste des sorts délémentaliste"
            };

            var spellPages = spellLists
                .SelectMany(p => p.OutLinks) // Liste des liens sortants
                .Distinct()
                .Where(p => !blackList.Any(blp => p.FullName.Equals(blp, StringComparison.OrdinalIgnoreCase)))
                .ToList()
                ;

            var nonSpell = new List<WikiPage>();
            var spells = new List<Spell>();

            foreach (var spellPage in spellPages)
            {
                Spell spell;

                if (!SpellParser.TryParse(spellPage, out spell, Log))
                {
                    nonSpell.Add(spellPage);
                }
                else
                {
                    spells.Add(spell);
                }
            }

            DescriptionParser.ParseDescriptions(spells, spellLists, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), Log);

            var consoleLog = Log as ConsoleLog;
            if (consoleLog != null)
            {
                Log.Information("Génération terminée. Nombre d'erreur : {0}. Nombre d'avertissements : {1}", consoleLog.ErrorCount, consoleLog.WarningCount);
            }
            Log.Information("Nombre de sorts lus : {0}", spells.Count);

            SpellParser.Flush(Log);

            return spells;
        }

        protected void AddSpellLists(List<Spell> spells)
        {
            // Liste des listes de sort
            var pagesNames = new Dictionary<string, string>
            {
                {"Pathfinder-RPG.liste des formules dalchimiste", SpellList.Ids.Alchemist},
                {"Pathfinder-RPG.liste des sorts dantipaladin", SpellList.Ids.AntiPaladin},
                {"Pathfinder-RPG.liste des sorts de bardes", SpellList.Ids.Bard},
                {"Pathfinder-RPG.liste des sorts de conjurateurs", SpellList.Ids.Summoner},
                {"Pathfinder-RPG.liste des sorts de druides", SpellList.Ids.Druid},
                {"Pathfinder-RPG.liste des sorts Densorceleursmagiciens", SpellList.Ids.SorcererWizard},
                {"Pathfinder-RPG.liste des sorts dinquisiteur", SpellList.Ids.Inquisitor},
                // liste ignorée car correspond à des sorts bonus, et pas des sorts normaux
                // { "Pathfinder-RPG.liste des sorts délémentaliste", SpellList.Ids.ElementalistWizard},
                {"Pathfinder-RPG.liste des sorts de magus", SpellList.Ids.Magus},
                {"Pathfinder-RPG.liste des sorts de paladins", SpellList.Ids.Paladin},
                {"Pathfinder-RPG.liste des sorts de prêtres", SpellList.Ids.Cleric},
                {"Pathfinder-RPG.liste des sorts de rôdeurs", SpellList.Ids.Ranger},
                {"Pathfinder-RPG.liste des sorts de sorcière", SpellList.Ids.Witch}
            };

            var spellListParser = new SpellListParser(Wiki, Log);

            foreach (var pageNamePair in pagesNames)
            {
                var pageName = pageNamePair.Key;
                var spellListName = pageNamePair.Value;

                var page = Wiki.Pages.GetOrEmpty(WikiName.FromString(pageName));

                if (page != null)
                {
                    spellListParser.Parse(page, spellListName, spells);
                }
            }
        }

        protected void GenerateIds(List<Spell> spells)
        {
            var glossaryPage = Wiki.Pages.FirstOrDefault(p => p.Name == "Glossaire des sorts");

            if (glossaryPage == null)
            {
                Log.Error("Page de glossaire des sorts introuvable");
                return;
            }

            var parser = new SpellGlossaryParser(Wiki, Log);
            parser.Parse(glossaryPage, spells);
        }
    }
}