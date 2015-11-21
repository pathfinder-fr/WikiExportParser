namespace WikiExportParser.Commands
{
    using PathfinderDb.Schema;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WikiExportParser.Wiki;
    using WikiExportParser.Wiki.Parsing;

    public class GenerateFeatsCommand : ICommand
    {
        /// <summary>
        /// Liste des pages de la catégorie don à ignorer.
        /// </summary>
        private static readonly string[] IgnoredPages = new[] {
            "Les dons",
            "Tableau récapitulatif des dons",
            "Dons d'audace",
            "dons de capacités de classe",
            "dons de combat agile",
            "dons de combat futé",
            "dons de combat puissant",
            "Dons de combat à deux armes",
            "dons de combat à distance",
            "dons de combat à mains nues",
            "dons de critique",
            "dons de création d'objets",
            "dons de magie divers",
            "dons de magie renforcée",
            "dons de maîtrise des armes",
            "dons de maîtrise des armures",
            "dons de maîtrise des boucliers",
            "Dons de maîtrise des coups critiques",
            "dons de métamagie",
            "dons de spectacle",
            "dons d'équipe",
            "dons raciaux",
            "dons relatifs aux compétences",
            "tableau récapitulatif des dons (art de la guerre)",
        };

        public WikiExport Wiki { get; set; }

        public ILog Log { get; set; }

        public string Alias
        {
            get { return "feats"; }
        }

        public string Help
        {
            get { return "Exporte les données sur les dons contenus dans le wiki."; }
        }

        public void Execute(DataSetCollection dataSets)
        {
            var export = this.Wiki;

            var featPages = export.Pages
                // Toutes les pages ayant la catégorie "Don"
                .Where(p => p.Categories.Any(c => c.Name.Equals("Don", StringComparison.OrdinalIgnoreCase)))
                // Sauf celles explicitement exclues
                .Where(p => !IgnoredPages.Any(i => i.Equals(p.Title, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            this.Log.Information("{0} Dons chargés", featPages.Count);

            var feats = this.ReadFeats(featPages);

            // Feats by source
            foreach (var sourceFeats in feats.GroupBy(s => s.Source.Id).Where(s => !string.IsNullOrEmpty(s.Key)))
            {
                var dataSet = dataSets.ResolveDataSet(sourceFeats.Key);
                dataSet.Sources.GetOrAdd(s => s.Id.Equals(sourceFeats.Key), () => new Source { Id = sourceFeats.Key });
                dataSet.Feats.AddRange(sourceFeats);
            }

            // Feats dataset
            //var ds = dataSets.ResolveDataSet("feats");
            //ds.Feats.AddRange(feats);
            //foreach (var source in feats.Select(s => s.Source.Id).Distinct().Where(s => !string.IsNullOrEmpty(s)))
            //{
            //    ds.Sources.Add(new Source { Id = source });
            //}
        }

        private List<Feat> ReadFeats(IList<WikiPage> pages)
        {
            var feats = new List<Feat>(pages.Count);

            foreach (var featPage in pages)
            {
                Feat feat;
                if (FeatParser.TryParse(featPage, this.Wiki, out feat, this.Log))
                {
                    feats.Add(feat);
                }
            }

            return feats;
        }
    }
}
