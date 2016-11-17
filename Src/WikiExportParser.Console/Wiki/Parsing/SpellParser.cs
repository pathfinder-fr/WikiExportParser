// -----------------------------------------------------------------------
// <copyright file="SpellParser.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using PathfinderDb.Schema;
using WikiExportParser.Logging;
using WikiExportParser.Wiki.Parsing.Spells;

namespace WikiExportParser.Wiki.Parsing
{
    /// <summary>
    /// Décrypte et génère un sort <see cref="Spell" /> à partir d'une page extraite du wiki.
    /// </summary>
    internal class SpellParser
    {
        private readonly Spell spell;

        private readonly string html;

        private ILog log;

        public SpellParser(Spell spell, string html)
        {
            this.spell = spell;
            this.html = html;
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
            this.log = log ?? NullLog.Instance;

            SchoolParser.ParseSchool(html, spell, this.log);
            DescriptorParser.ParseDescriptor(html, spell, this.log);
            LevelsParser.ParseLevels(html, spell, this.log);
            RangeParser.ParseRange(html, spell, this.log);
            TargetParser.ParseTarget(html, spell, this.log);
            ComponentsParser.ParseComponents(html, spell, this.log);
            CastingTimeParser.ParseCastingTime(html, spell, this.log);
            SavingThrowParser.ParseSavingThrow(html, spell, this.log);
            MagicResistanceParser.ParseMagicResistance(html, spell, this.log);
            ParseSource();
        }

        public static void Flush(ILog log = null)
        {
            log = log ?? NullLog.Instance;

            TargetParser.Flush(log);
        }

        private static Spell Parse(WikiPage page, ILog log = null)
        {
            var spell = new Spell();
            spell.Name = page.Title;
            spell.Id = page.Id;

            // Ajout source wiki
            spell.Source.References.Add(References.FromFrWiki(page.Url));

            // Ajout source drp
            spell.Source.References.Add(References.FromFrDrp(page.Id));

            var wiki = page.Raw;

            if (wiki.IndexOf("'''École'''", StringComparison.Ordinal) == -1 && wiki.IndexOf("'''Ecole'''", StringComparison.Ordinal) == -1)
            {
                throw new ParseException("École de magie introuvable");
            }

            SpellParser parser = new SpellParser(spell, wiki);
            parser.Execute(log);

            return spell;
        }

        private void ParseSource()
        {
            var sourceId = MarkupUtil.DetectSourceSnippet(html);

            if (sourceId == null &&
                (spell.Name.Equals("Brise", StringComparison.OrdinalIgnoreCase) ||
                 spell.Name.Equals("Choc", StringComparison.OrdinalIgnoreCase) ||
                 spell.Name.Equals("Détremper", StringComparison.OrdinalIgnoreCase) ||
                 spell.Name.Equals("Pénombre", StringComparison.OrdinalIgnoreCase) ||
                 spell.Name.Equals("Racine", StringComparison.OrdinalIgnoreCase) ||
                 spell.Name.Equals("Scoop", StringComparison.OrdinalIgnoreCase)
                    ))
            {
                sourceId = Source.Ids.PaizoBlog;
                spell.Source.References.Add(new ElementReference {HrefString = "http://www.black-book-editions.fr/index.php?site_id=59&download_id=138", Name = "Page téléchargement BBE"});
                spell.Source.References.Add(new ElementReference {HrefString = "http://www.pathfinder-fr.org/Wiki/GetFile.aspx?File=%2fADJ%2fPathfinder-RPG%2fUMToursDeMagie.pdf", Name = "Téléchargement Pathfinder-fr.org"});
                spell.Source.References.Add(new ElementReference {HrefString = "http://www.black-book-editions.fr/index.php?site_id=59&actu_id=398", Name = "Annonce BBE"});
                spell.Source.References.Add(new ElementReference {HrefString = "http://www.pathfinder-fr.org/Blog/post/Un-coup-de-baguette-magique.aspx", Name = "Annonce Pathfinder-fr.org"});
            }

            if (sourceId != null)
            {
                spell.Source.Id = sourceId;
            }
            else
            {
                spell.Source.Id = Source.Ids.PathfinderRpg;
            }
        }
    }
}