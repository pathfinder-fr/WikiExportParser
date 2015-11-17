namespace WikiExportParser.Wiki.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Pathfinder.DataSet;

    internal partial class FeatParser
    {
        private const string TypePattern = "'''Catégorie[\\.]?''' (: )?";

        private readonly Feat feat;

        private readonly WikiPage wikiPage;

        private readonly WikiExport export;

        private ILog log;

        private string[] lines;

        public FeatParser(Feat feat, WikiPage wikiPage, WikiExport export)
        {
            this.feat = feat;
            this.wikiPage = wikiPage;
            this.export = export;
        }

        public static bool TryParse(WikiPage page, WikiExport export, out Feat feat, ILog log = null)
        {
            feat = null;
            try
            {
                feat = Parse(page, export, log);
                return true;
            }
            catch (ParseException e)
            {
                log.Error("{0}: {1}", page.Title, e.Message);
                return false;
            }
        }

        public static Feat Parse(WikiPage page, WikiExport export, ILog log = null)
        {
            var feat = new Feat();
            var parser = new FeatParser(feat, page, export);
            parser.Execute(log);
            return feat;
        }

        public void Execute(ILog log = null)
        {
            this.log = log ?? Logging.NullLog.Instance;

            this.lines = this.wikiPage.Raw.Trim().Split('\n').Select(l => l.Trim()).ToArray();

            if (lines.Length == 0)
            {
                throw new ParseException("Aucune ligne de texte détectées");
            }

            this.feat.Name = this.wikiPage.Title;
            this.feat.Id = this.wikiPage.Id;
            this.feat.Source.References.Add(new ElementReference { Name = "Wiki Pathfinder-fr.org", Href = new Uri(this.wikiPage.Url) });

            this.ParseSource();
            this.ParseDescription();
            this.ParseType();
            this.ParsePrerequisites();
            this.ParseBenefit();
        }

        private void ParseSource()
        {
            var sourceId = MarkupUtil.DetectSourceSnippet(this.lines[0]);

            if (sourceId == null)
            {
                if (this.wikiPage.Categories.Any(c => c.Name == "Bestiaire"))
                {
                    sourceId = Source.Ids.Bestiary;
                }
                else if (this.wikiPage.Categories.Any(c => c.Name == "Bestiaire 2"))
                {
                    sourceId = Source.Ids.Bestiary2;
                }
            }

            if (sourceId == null)
            {
                sourceId = Source.Ids.PathfinderRpg;
            }

            this.feat.Source.Id = sourceId;
        }

        private void ParseDescription()
        {
            int i = 0;
            string line;
            do
            {
                line = this.lines[i];
                i++;
                if (line.Length != 0 && line[0] == '{')
                {
                    if (line.EndsWith("}"))
                    {
                        line = string.Empty;
                    }
                    else
                    {
                        line = line.Substring(line.IndexOf('}') + 1);
                    }
                }
            }
            while (line == string.Empty);

            if (!line.StartsWith("''") || !line.EndsWith("''"))
            {
                throw new ParseException("La première ligne n'est pas en italique");
            }

            this.feat.Description = line.Substring(2, line.Length - 4);
        }

        private void ParseType()
        {
            var categoryLine = this.lines.FirstOrDefault(l => Regex.Match(l, TypePattern, RegexOptions.CultureInvariant | RegexOptions.Multiline).Success);

            if (categoryLine == null)
            {
                this.feat.Types = new[] { FeatType.General };
                return;
            }

            // On démarre au second "'''"
            var startIndex = categoryLine.IndexOf("'''", categoryLine.IndexOf("'''") + 1) + 3;

            var categoriesMarkup = categoryLine.Substring(startIndex).Trim();

            if (categoriesMarkup[0] == ':')
                categoriesMarkup = categoriesMarkup.Substring(1).Trim();

            var categories = categoriesMarkup.Split(',').Select(c => c.Trim()).ToArray();

            var types = new List<FeatType>(categories.Length);

            foreach (var categoryMarkup in categories)
            {
                var lower = categoryMarkup.ToLowerInvariant();
                switch (lower)
                {
                    case "[[dons#audace|audace]]":
                    case "[[dons#donaudace|audace]]":
                        types.Add(FeatType.Grit);
                        break;

                    case "[[dons#doncombat|combat]]":
                    case "[[dons#combat|combat]]":
                        types.Add(FeatType.Combat);
                        break;

                    case "[[dons#ecole|école]]":
                    case "[[dons#donecole|école]]":
                        types.Add(FeatType.Style);
                        break;

                    case "[[dons#equipe|équipe]]":
                    case "[[dons#donequipe|équipe]]":
                        types.Add(FeatType.Teamwork);
                        break;

                    case "[[dons#metamagie|métamagie]]":
                    case "[[dons#donmetamagie|métamagie]]":
                        types.Add(FeatType.Metamagic);
                        break;

                    case "[[dons#doncreation|création d'objets]]":
                    case "[[dons#creation|création d'objets]]":
                    case "[[dons#creation|création]]":
                    case "[[dons#creation|création d’objets]]":
                        types.Add(FeatType.ItemCreation);
                        break;

                    case "[[dons#doncritique|critique]]":
                        types.Add(FeatType.Critical);
                        break;

                    case "monstre":
                        types.Add(FeatType.Monster);
                        break;

                    case "[[dons#donspectacle|spectacle]]":
                        types.Add(FeatType.Performance);
                        break;

                    default:
                        throw new ParseException(string.Format("Type de don \"{0}\" non reconnu", lower));
                }
            }

            this.feat.Types = types.ToArray();
        }

        private void ParsePrerequisites()
        {
            var conditionLine = lines.FirstOrDefault(l => l.StartsWith("'''Conditions.'''", StringComparison.OrdinalIgnoreCase) || l.StartsWith("'''Condition.'''", StringComparison.OrdinalIgnoreCase));

            if (conditionLine == null)
            {
                return;
            }

            var conditionMarkup = conditionLine.Substring(conditionLine.IndexOf("'''", conditionLine.IndexOf("'''") + 1) + 3).Trim();

            if (conditionMarkup.StartsWith("."))
            {
                conditionMarkup = conditionMarkup.Substring(1).Trim();
            }

            // On détermine si les conditions sont séparées par des ";" ou des ",".
            var hasSemiColons = conditionMarkup.IndexOf(';') != -1;

            IEnumerable<string> conditions;

            if (hasSemiColons)
            {
                conditions = conditionMarkup.Split(';');
            }
            else
            {
                conditions = conditionMarkup.Split(',');
            }

            conditions = conditions.Select(c => c.Trim());

            var preferequisiteParser = new PrerequisiteParser(this.wikiPage, this.log);

            this.feat.Prerequisites = conditions.Select(c => preferequisiteParser.Parse(c, this.export, hasSemiColons)).Where(p => p != null).ToArray();

        }

        private void ParseBenefit()
        {
            var benefit = this.ParseParagraph("'''Avantage.'''", "'''Avantages.'''");

            if (benefit == null)
            {
                this.log.Warning("{0}: Avantage introuvable", this.wikiPage.Title);
            }

            this.feat.Benefit = benefit;
        }

        private string ParseHtmlParagraph(params string[] prefix)
        {
            var html = System.Net.WebUtility.HtmlDecode(this.wikiPage.Body);

            var htmlLines = html.Split(new[] { "<br/>", "<br>", "<br />" }, StringSplitOptions.None);

            var lines = new List<string>();
            var inside = false;
            foreach (var htmlLine in htmlLines)
            {
                if (!inside && htmlLine.StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                {
                    string htmlPrefix = string.Empty;
                    if (prefix.Any(p => htmlLine.StartsWith((htmlPrefix = string.Format("<b>{0}.</b>", p)), StringComparison.OrdinalIgnoreCase)))
                    {
                        inside = true;
                        lines.Add(htmlLine.Substring(htmlPrefix.Length).Trim());
                    }
                }
                else if (inside)
                {
                    if (htmlLine.StartsWith("<b>", StringComparison.OrdinalIgnoreCase))
                    {
                        inside = false;
                    }
                    else
                    {
                        lines.Add(htmlLine);
                    }
                }
            }

            var htmlParagraph = string.Join("<br/>", lines);

            while (htmlParagraph.EndsWith("<br/>"))
            {
                htmlParagraph = htmlParagraph.Remove(htmlParagraph.Length - 5);
            }

            return MarkupUtil.RemoveHtmlMarkup(htmlParagraph);
        }

        private string ParseParagraph(params string[] prefix)
        {
            int firstLine = -1;
            int lastLine = -1;
            for (int i = 0; i < this.lines.Length; i++)
            {
                var line = this.lines[i];
                if (prefix.Any(p => line.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                {
                    firstLine = i;
                }
                else if (line.StartsWith("'''") && firstLine != -1)
                {
                    lastLine = i - 1;
                }
            }

            if (firstLine == -1)
            {
                return null;
            }

            var subLines = lines.Skip(firstLine);

            if (lastLine != -1)
            {
                subLines = subLines.Take(lastLine - firstLine + 1);
            }

            var markup = string.Join(Environment.NewLine, subLines.Where(l => !string.IsNullOrWhiteSpace(l.Trim())));

            // On supprime le "mot" du début
            markup = markup.Substring(markup.IndexOf("'''", markup.IndexOf("'''") + 1) + 3).Trim();

            return MarkupUtil.RemoveMarkup(markup);
        }
    }
}
