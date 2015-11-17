namespace WikiExportParser.Wiki.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Pathfinder.DataSet;
    using Spells;

    /// <summary>
    /// Décrypte et génère un sort <see cref="Spell"/> à partir d'une page extraite du wiki.
    /// </summary>
    internal partial class SpellParser
    {
        private readonly Spell spell;

        private readonly string html;

        private ILog log;

        public SpellParser(Spell spell, string html)
        {
            this.spell = spell;
            this.html = html;
        }

        public static IDictionary<string, string> ParseDescriptions(IEnumerable<WikiPage> pages, IDictionary<string, string> descriptions)
        {
            var regex = new Regex(@"^\* '''''\[\[((?<Link>[^\|\]]+)\|)?(?<Title>[^\|\]]+)\]\]'{2,5}\s*(?<Foc>('')?\([MF]\)('')?\s*)?(''\((?:APG|AdM|Blog Paizo|UC)\)''\.?\s+)?(?<Foc>\(\s*[MF, ]+\)\s*)?\(\s*(\[\[[^\]]+\]\](,?\s*)?)*\s*\)\s*(?<Foc>\(\s*[MF, ]+\)\s*)?\. *(?<Desc>[^\r\n]*)", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.Multiline);

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

            return descriptions;
        }

        public static bool TryParse(WikiPage page, out Spell spell, ILog log = null)
        {
            spell = null;
            try
            {
                spell = Parse(page, log);
                return true;
            }
            catch (ParseException e)
            {
                log.Error("{0}: {1}", page.Title, e.Message);
                return false;
            }
        }

        public void Execute(ILog log = null)
        {
            this.log = log ?? Logging.NullLog.Instance;

            SchoolParser.ParseSchool(this.html, this.spell, this.log);
            DescriptorParser.ParseDescriptor(this.html, this.spell, this.log);
            LevelsParser.ParseLevels(this.html, this.spell, this.log);
            RangeParser.ParseRange(this.html, this.spell, this.log);
            TargetParser.ParseTarget(this.html, this.spell, this.log);
            ComponentsParser.ParseComponents(this.html, this.spell, this.log);
            CastingTimeParser.ParseCastingTime(this.html, this.spell, this.log);
            SavingThrowParser.ParseSavingThrow(this.html, this.spell, this.log);
            MagicResistanceParser.ParseMagicResistance(this.html, this.spell, this.log);
            this.ParseSource();
        }

        public static void Flush(ILog log = null)
        {
            log = log ?? Logging.NullLog.Instance;

            TargetParser.Flush(log);
        }

        private static Spell Parse(WikiPage page, ILog log = null)
        {
            var spell = new Spell();
            spell.Name = page.Title;
            spell.Id = Ids.Normalize(page.Id);

            // Ajout source wiki
            spell.Source.References.Add(References.FromFrWiki(page.Url));

            // Ajout source drp
            spell.Source.References.Add(References.FromFrDrp(page.Id));

            var html = page.Raw;

            if (html.IndexOf("'''École'''", StringComparison.Ordinal) == -1 && html.IndexOf("'''Ecole'''", StringComparison.Ordinal) == -1)
            {
                throw new ParseException("École de magie introuvable");
            }

            SpellParser parser = new SpellParser(spell, html);
            parser.Execute(log);

            return spell;
        }

        private void ParseSource()
        {
            var sourceId = MarkupUtil.DetectSourceSnippet(html);

            if (sourceId == null &&
                (this.spell.Name.Equals("Brise", StringComparison.OrdinalIgnoreCase) ||
                this.spell.Name.Equals("Choc", StringComparison.OrdinalIgnoreCase) ||
                this.spell.Name.Equals("Détremper", StringComparison.OrdinalIgnoreCase) ||
                this.spell.Name.Equals("Pénombre", StringComparison.OrdinalIgnoreCase) ||
                this.spell.Name.Equals("Racine", StringComparison.OrdinalIgnoreCase) ||
                this.spell.Name.Equals("Scoop", StringComparison.OrdinalIgnoreCase)
                ))
            {
                sourceId = Source.Ids.PaizoBlog;
                spell.Source.References.Add(new ElementReference { HrefString = "http://www.black-book-editions.fr/index.php?site_id=59&download_id=138", Name = "Page téléchargement BBE" });
                spell.Source.References.Add(new ElementReference { HrefString = "http://www.pathfinder-fr.org/Wiki/GetFile.aspx?File=%2fADJ%2fPathfinder-RPG%2fUMToursDeMagie.pdf", Name = "Téléchargement Pathfinder-fr.org" });
                spell.Source.References.Add(new ElementReference { HrefString = "http://www.black-book-editions.fr/index.php?site_id=59&actu_id=398", Name = "Annonce BBE" });
                spell.Source.References.Add(new ElementReference { HrefString = "http://www.pathfinder-fr.org/Blog/post/Un-coup-de-baguette-magique.aspx", Name = "Annonce Pathfinder-fr.org" });
            }

            if (sourceId != null)
            {
                spell.Source.Id = sourceId;
            }
        }
    }
}