namespace WikiExportParser.Wiki.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Pathfinder.DataSet;

    internal class SpellListParser
    {
        private const string LevelHeaderPattern = "^===(Sorts|Formules).*(niveau (?<Level>\\d+)|de (?<Level>\\d+)(er|e) niveau)( \\((tours de magie|oraisons)\\))?===$";

        private const string SpellPattern = @"^\* '''''\[\[?(?<Name>[^\]]+)\]\]?( \((?<Components>[MFX])(, (?<Components>[MFX]))*\))?( \((?<APG>APG)\))?[\.]?'''''\s*(\((?<Components>[MFX])(, (?<Components>[MFX]))*\))?(?<APG>'''\(APG\)''')?\.?";

        private readonly ILog log;

        private readonly WikiExport wiki;

        private WikiPage page;

        private string listName;

        private int currentLevel;

        public SpellListParser(WikiExport wiki, ILog log)
        {
            this.wiki = wiki;
            this.log = log ?? Logging.NullLog.Instance;
        }

        public void Parse(WikiPage page, string listName, List<Spell> spells)
        {
            this.page = page;
            this.listName = listName;

            log.Information("Début de l'analyse de la page \"{0}\"", this.page.Title);

            var wiki = this.page.Raw;

            var lines = wiki.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            this.currentLevel = -1;

            foreach (var line in lines)
            {
                ReadSpellLine(spells, line);
            }
        }

        private void ReadSpellLine(List<Spell> spells, string line)
        {
            Match match;
            if (line.StartsWith("===") && !line.StartsWith("===="))
            {
                match = Regex.Match(line, "(niveau (?<Level>\\d+)|(?<Level>\\d+)(er|e) niveau)", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

                if (!match.Success)
                {
                    log.Warning("Niveau indétectable dans l'entête \"{0}\"", line);
                }
                else
                {
                    this.currentLevel = int.Parse(match.Groups["Level"].Value);
                    //log.Information("Détection du début d'une liste de sort de niveau {0}", this.currentLevel);
                }
                return;
            }

            if (line.Length == 0 || line[0] != '*')
            {
                return;
            }

            match = Regex.Match(line, SpellPattern, RegexOptions.ExplicitCapture);

            if (!match.Success)
            {
                log.Warning("Sort introuvable sur la ligne \"{0}\"", line);
                return;
            }

            var isApg = match.Groups["APG"].Success;
            var name = match.Groups["Name"].Value;
            var components = match.Groups["Component"].Value;
            var description = line.Substring(match.Length).Trim();

            var wikiName = WikiName.FromLink(name);
            if (string.IsNullOrWhiteSpace(wikiName.Namespace))
            {
                wikiName.Namespace = "Pathfinder-RPG";
            }
            var spellPageId = wikiName.Id;

            var spell = spells.FirstOrDefault(s => s.Id == spellPageId);

            if (spell == null)
            {
                var page = this.wiki.FindPage(wikiName);

                if (page == null)
                {
                    log.Error("Le sort \"{0}\" pour l'id {1} ne désigne pas une page existante", name, spellPageId);
                }
                else
                {
                    log.Warning("La page \"{0}\" pour l'id {1} n'est pas répertoriée comme un sort", name, spellPageId);
                }
                return;
            }

            var list = spell.Levels.FirstOrDefault(l => l.List == this.listName);

            if (this.listName == SpellList.Ids.Cleric && !spell.Levels.Any(l => l.List == SpellList.Ids.Oracle))
            {
                spell.Levels = spell.Levels.Concat(new[] { new SpellListLevel { List = SpellList.Ids.Oracle, Level = this.currentLevel } }).ToArray();
            }

            if (list == null)
            {
                //log.Information("Ajout du sort {0} à la liste {1} {2}", spell.Name, this.listName, this.currentLevel);
                spell.Levels = spell.Levels.Concat(new[] { new SpellListLevel { List = this.listName, Level = this.currentLevel } }).ToArray();
            }
            else
            {
                if (list.Level != this.currentLevel)
                {
                    log.Error("Erreur de niveau pour le sort \"{0}\" (liste niv. {1} et description niv. {2})", spell.Name, this.currentLevel, list.Level);
                }
            }
        }
    }
}