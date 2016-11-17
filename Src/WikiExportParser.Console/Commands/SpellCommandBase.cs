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
            var blackList = EmbeddedResources.LoadString("Resources.SpellIgnoredPages.txt").Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var spellPages = spellLists
                .SelectMany(p => p.OutLinks) // Liste des liens sortants
                .Distinct()
                .Where(p => !blackList.Any(blp => p.Name.Equals(blp, StringComparison.OrdinalIgnoreCase)))
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
            var pagesNames = EmbeddedResources.LoadString("Resources.SpellLists.txt")
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries))
                .ToDictionary(l => l[0], l => l[1]);

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