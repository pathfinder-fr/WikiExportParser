// -----------------------------------------------------------------------
// <copyright file="MonsterGlossaryParser.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

namespace WikiExportParser.Wiki.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using PathfinderDb.Schema;

    public class MonsterGlossaryParser
    {
        private const string LeftAlignPrefix = "| CLASS=\"gauche\" |";
        private readonly ILog log;

        public MonsterGlossaryParser(ILog log)
        {
            this.log = log;
        }

        public void ParseAll(WikiPage glossaryPage, List<Monster> monsters)
        {
            using (var reader = new StringReader(glossaryPage.Raw))
            {
                Entry entry = null;
                var entryRow = 0;
                var separatorRead = false;
                string line;

                var rowIndex = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    rowIndex++;
                    line = line.Trim();

                    if (line.Equals("|}") || line.Equals("|-"))
                    {
                        // séparateur
                        if (entry != null)
                        {
                            this.TryAdd(entry, monsters);
                        }

                        entry = null;
                        separatorRead = true;
                    }
                    else if (separatorRead && line.Length != 0 && line[0] == '|')
                    {
                        if (line.StartsWith(LeftAlignPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            line = line.Substring(LeftAlignPrefix.Length).Trim();
                        }
                        else
                        {
                            line = line.Substring(1).Trim();
                        }

                        if (line.Equals("&nbsp;", StringComparison.OrdinalIgnoreCase))
                        {
                            line = string.Empty;
                        }

                        if (entry == null)
                        {
                            entry = new Entry();
                            entryRow = 0;
                        }
                        else
                        {
                            entryRow++;
                        }

                        switch (entryRow)
                        {
                            case 0:
                                // Anglais
                                ReadEnglishLine(line, entry);
                                break;

                            case 1:
                                // Français
                                ReadFrenchLine(line, entry);
                                break;

                            case 2:
                                // Règles
                                this.ReadRules(line, entry);
                                break;

                            case 3:
                                // Livre
                                this.ReadSourceBook(line, entry);
                                break;

                            case 4:
                                this.ReadFp(line, entry);
                                break;

                            default:
                                this.log.Warning("Plus de 4 colonnes de contenu sur la ligne {0}", rowIndex);
                                break;
                        }
                    }
                }
            }
        }

        private static void ReadEnglishLine(string line, Entry entry)
        {
            // On prend uniquement la seconde partie
            var words = line.Split(',');
            if (words.Length > 1)
            {
                line = words[1].Trim();
            }

            entry.EnglishName = line;
        }

        private static void ReadFrenchLine(string line, Entry entry)
        {
            var words = line.Split(',');
            if (words.Length > 1)
            {
                line = words[1].Trim();
            }

            if (line.StartsWith("[[") && line.EndsWith("]]"))
            {
                var i = line.IndexOf('|');
                if (i != -1)
                {
                    entry.FrenchLink = line.Substring(2, i - 2);
                    entry.FrenchName = line.Substring(i + 1, line.Length - i - 3);
                }
                else
                {
                    entry.FrenchName = line.Substring(2, line.Length - 4);
                    entry.FrenchLink = entry.FrenchName;
                }
            }
            else
            {
                entry.FrenchName = line;
                entry.FrenchLink = null;
            }
        }

        private void ReadRules(string line, Entry entry)
        {
            switch (line.ToLowerInvariant())
            {
                case "path":
                    entry.Ruleset = "Pathfinder";
                    break;

                case "3e":
                    entry.Ruleset = "DD3";
                    break;

                default:
                    this.log.Warning("Règles {0} non supportées", line);
                    break;
            }
        }

        private void ReadSourceBook(string line, Entry entry)
        {
            var sources = line.Split('/').Select(s => s.Trim()).ToArray();
            if (sources.Length != 0)
            {
                entry.Source = sources[0];
            }
        }

        private void ReadFp(string line, Entry entry)
        {
            switch (line)
            {
                case "0,5":
                    line = "0.5";
                    break;

                case "0,3":
                    line = "0.3";
                    break;
            }

            decimal fp;
            if (decimal.TryParse(line, NumberStyles.None | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out fp))
            {
                entry.FP = fp;
            }
        }

        private void TryAdd(Entry entry, IList<Monster> monsters)
        {
            Monster monster = null;
            if (!string.IsNullOrEmpty(entry.FrenchName))
            {
                monster = monsters.FirstOrDefault(m => m.Name.Equals(entry.FrenchName));
            }

            if (monster == null && !string.IsNullOrEmpty(entry.FrenchLink))
            {
                monster = monsters.FirstOrDefault(m => m.Id.Equals(Ids.Normalize(entry.FrenchLink)));
            }

            if (monster == null && !string.IsNullOrEmpty(entry.EnglishName))
            {
                // On n'ajoute plus les monstres uniquement à partir de leur nom anglais
                //monster = new Monster
                //{
                //    Id = string.Empty,
                //    Name = string.Empty,
                //    Source = entry.ElementSource,
                //};

                //monsters.Add(monster);
                //this.log.Warning("Ajout du monstre \"{0}\" à partir de son nom anglais uniquement (nom français: \"{1}\")", entry.EnglishName, entry.FrenchName);
            }

            if (monster != null)
            {
                monster.OpenLocalization().AddLocalizedEntry(DataSetLanguages.English, "id", Ids.Normalize(entry.EnglishName));
                monster.OpenLocalization().AddLocalizedEntry(DataSetLanguages.English, "name", entry.EnglishName);
            }
            else if (!string.IsNullOrEmpty(entry.FrenchLink))
            {
                this.log.Warning("Impossible de trouver le monstre nommé \"{0}\" alors qu'il existe un lien vers la page wiki [[{1}]]", entry.FrenchName, entry.FrenchLink);
            }
            else if(!string.IsNullOrEmpty(entry.FrenchName))
            {
                this.log.Information("Nouveau monstre à ajouter depuis le glossaire : \"{0}\"", entry.FrenchName);
            }
        }

        private class Entry
        {
            public string EnglishName { get; set; }

            public string FrenchName { get; set; }

            public string FrenchLink { get; set; }

            public string EnglishLink { get; set; }

            public decimal? FP { get; set; }

            public string Ruleset { get; set; }

            public string Source { get; set; }

            public ElementSource ElementSource
            {
                get
                {
                    var lower = (this.Source ?? string.Empty).ToLowerInvariant();

                    if (lower == "bestiary 4")
                    {
                        return new ElementSource { Id = PathfinderDb.Schema.Source.Ids.Bestiary4 };
                    }

                    if (lower.StartsWith("pf #"))
                    {
                        return new ElementSource { Id = PathfinderDb.Schema.Source.Ids.AdventurePath(int.Parse(lower.Substring(4))) };
                    }

                    return null;
                }
            }
        }
    }
}