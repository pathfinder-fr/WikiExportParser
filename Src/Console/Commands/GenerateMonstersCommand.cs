// -----------------------------------------------------------------------
// <copyright file="GenerateMonstersCommand.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

namespace WikiExportParser.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PathfinderDb.Schema;
    using Wiki;
    using Wiki.Parsing;

    public class GenerateMonstersCommand : ICommand
    {
        public ILog Log { get; set; }

        public Wiki.WikiExport Wiki { get; set; }

        public string Help
        {
            get { return "(expérimental) Exporte les monstres"; }
        }

        public string Alias
        {
            get { return "monsters"; }
        }

        public void Execute(DataSetCollection dataSets)
        {
            var ignoredPages = EmbeddedResources.LoadString("Resources.MonsterIgnoredPages.txt").Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Index d'après la page "Monstres"
            var indexPage = this.Wiki.Pages[WikiName.FromString("Pathfinder-RPG.Monstres")];
            var pages = indexPage
                .OutLinks
                .Where(p => !ignoredPages.Any(i => i.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                .Where(p => !p.Name.EndsWith("archétype", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(p => p.WikiName);
            this.Log.Information("{0} pages chargées depuis l'index", pages.Count);

            // Index d'après la page "Glossaire des monstres"
            indexPage = this.Wiki.Pages[WikiName.FromString("Pathfinder-RPG.Glossaire des monstres")];
            var glossaryOutPages = indexPage
                .OutLinks
                .Where(p => !ignoredPages.Any(i => i.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                .Where(p => !p.Name.EndsWith("archétype", StringComparison.OrdinalIgnoreCase))
                .ToList();

            this.Log.Information("{0} pages chargées depuis le glossaire", glossaryOutPages.Count);

            var addedCount = 0;
            foreach (var glossaryOutPage in glossaryOutPages.Where(glossaryOutPage => !pages.ContainsKey(glossaryOutPage.WikiName)))
            {
                pages.Add(glossaryOutPage.WikiName, glossaryOutPage);
                addedCount++;
            }
            this.Log.Information("{0} monstres ajoutés depuis le glossaire", addedCount);

            var monsters = new List<Monster>(pages.Count);

            var parser = new MonsterParser(this.Log);
            foreach (var monsterPage in pages)
            {
                List<Monster> pageMonsters;
                if (TryParse(parser, monsterPage.Value, this.Wiki, out pageMonsters, this.Log))
                {
                    monsters.AddRange(pageMonsters);
                }
            }

            this.Log.Information("Nombre total de monstres lus : {0}", monsters.Count);

            this.Log.Information("Chargement du glossaire des monstres...");
            var glossaryPage = this.Wiki.Pages[WikiName.FromString("Pathfinder-RPG.Glossaire des monstres")];
            var glossaryParser = new MonsterGlossaryParser(this.Log);
            glossaryParser.ParseAll(glossaryPage, monsters);


            // Adding to dataset
            foreach (var source in monsters.GroupBy(s => s.Source.Id).Where(s => !string.IsNullOrEmpty(s.Key)))
            {
                var localSource = source;
                var dataSet = dataSets.ResolveDataSet(source.Key);
                dataSet.Sources.GetOrAdd(s => s.Id.Equals(localSource.Key), () => new Source { Id = localSource.Key });
                dataSet.Monsters.AddRange(source.OrderBy(m => m.Id));
            }

            // Monsters dataset
            //var ds = dataSets.ResolveDataSet("monsters");
            //ds.Monsters.AddRange(monsters);
            //foreach (var source in monsters.Select(s => s.Source.Id).Distinct().Where(s => !string.IsNullOrEmpty(s)))
            //{
            //    ds.Sources.Add(new Source { Id = source });
            //}

            parser.Flush();
        }

        internal bool TryParse(MonsterParser parser, WikiPage monsterPage, WikiExport wikiExport, out List<Monster> monster, ILog log)
        {
            try
            {
                monster = Parse(parser, monsterPage, wikiExport);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Page \"{0}\" (id '{1}') : {2}",
                    monsterPage.Name,
                    monsterPage.Id,
                    ex.RecursiveMessage());
                monster = null;

                return false;
            }
        }

        internal List<Monster> Parse(MonsterParser parser, WikiPage page, WikiExport wikiExport)
        {
            WikiName redirection;
            var count = 0;
            while (page.IsRedirection(out redirection))
            {
                page = this.Wiki.Pages[redirection];
                count++;

                if (count > 10)
                {
                    throw new InvalidOperationException("Boucle de redirections détectée");
                }
            }

            var raw = page.Raw;

            var result = parser.ParseAll(page, raw);

            if (result.Count == 0)
            {
                throw new InvalidOperationException("Aucun bloc BD détecté");
            }

            return result;
        }
    }
}