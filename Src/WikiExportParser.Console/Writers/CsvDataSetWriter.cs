// -----------------------------------------------------------------------
// <copyright file="CsvDataSetWriter.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using PathfinderDb.Schema;

namespace WikiExportParser.Writers
{
    public class CsvDataSetWriter : IDataSetWriter
    {
        private readonly Configuration config;

        public CsvDataSetWriter()
        {
            config = new Configuration
            {
                CultureInfo = CultureInfo.InvariantCulture
            };
        }

        public bool Accept(string name, DataSet dataSet, Dictionary<string, string> options)
        {
            return options.ContainsKey("csv");
        }

        public void Write(string name, DataSet dataSet, string directory)
        {
            var folder = directory;
            if (!string.IsNullOrEmpty(name))
            {
                folder = Path.Combine(directory, name);
            }

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (dataSet.Spells != null && dataSet.Spells.Count != 0)
            {
                WriteSpellDataSet(Path.Combine(folder, "spells.csv"), dataSet);
            }

            if (dataSet.Feats != null && dataSet.Feats.Count != 0)
            {
                WriteFeatDataSet(Path.Combine(folder, "feats.csv"), dataSet);
            }

            if (dataSet.Monsters != null && dataSet.Monsters.Count != 0)
            {
                WriteMonsterDataSet(Path.Combine(folder, "monsters.csv"), dataSet);
            }
        }

        private void WriteSpellDataSet(string path, DataSet dataSet)
        {
            using (var streamWriter = OpenFile(path))
            using (var writer = new CsvWriter(streamWriter, config))
            {
                writer.WriteHeader<SpellWrapper>();
                foreach (var spell in dataSet.Spells.Select(s => new SpellWrapper(s)))
                {
                    writer.WriteRecord(spell);
                }
            }
        }

        private void WriteFeatDataSet(string path, DataSet dataSet)
        {
            using (var streamWriter = OpenFile(path))
            using (var writer = new CsvWriter(streamWriter, config))
            {
                writer.WriteHeader<FeatWrapper>();
                foreach (var feat in dataSet.Feats.Select(f => new FeatWrapper(f)))
                {
                    writer.WriteRecord(feat);
                }
            }
        }

        private void WriteMonsterDataSet(string path, DataSet dataSet)
        {
            using (var streamWriter = OpenFile(path))
            using (var writer = new CsvWriter(streamWriter, config))
            {
                writer.WriteHeader<MonsterWrapper>();
                foreach (var feat in dataSet.Monsters.Select(m => new MonsterWrapper(m)))
                {
                    writer.WriteRecord(feat);
                }
            }
        }

        private static StreamWriter OpenFile(string path)
        {
            return new StreamWriter(path, false, Encoding.Default);
        }

        class SpellWrapper
        {
            private readonly Spell spell;

            public SpellWrapper(Spell spell)
            {
                this.spell = spell;
            }

            public string Id
            {
                get { return spell.Id; }
            }

            public string Name
            {
                get { return spell.Name; }
            }

            public string English
            {
                get
                {
                    if (spell.Localization == null)
                        return null;
                    return spell.Localization.GetLocalizedEntry("en-US", "name", null);
                }
            }

            public string Source
            {
                get { return spell.Source.Id; }
            }

            public string Descriptor
            {
                get { return spell.Descriptor.ToString(); }
            }

            public string School
            {
                get { return spell.School.ToString(); }
            }

            public string Alchemist
            {
                get { return FindLevel("alchemist"); }
            }

            public string AntiPaladin
            {
                get { return FindLevel("antipaladin"); }
            }

            public string Bard
            {
                get { return FindLevel("bard"); }
            }

            public string Cleric
            {
                get { return FindLevel("cleric"); }
            }

            public string Druid
            {
                get { return FindLevel("druid"); }
            }

            public string Inquisitor
            {
                get { return FindLevel("inquisitor"); }
            }

            public string Summoner
            {
                get { return FindLevel("summoner"); }
            }

            public string Magus
            {
                get { return FindLevel("magus"); }
            }

            public string Oracle
            {
                get { return FindLevel("orache"); }
            }

            public string Paladin
            {
                get { return FindLevel("paladin"); }
            }

            public string Ranger
            {
                get { return FindLevel("ranger"); }
            }

            public string Witch
            {
                get { return FindLevel("witch"); }
            }

            public string Wizard
            {
                get { return FindLevel("sorcerer-wizard"); }
            }

            public string Components
            {
                get { return spell.Components == null ? null : spell.Components.Kinds.ToString(); }
            }

            public string Range
            {
                get { return spell.Range == null ? null : spell.Range.ToString(); }
            }

            public string Target
            {
                get { return spell.Target == null ? null : spell.Target.Value; }
            }

            public string CastingTime
            {
                get { return spell.CastingTime.ToString(); }
            }

            public string Summary
            {
                get { return spell.Summary; }
            }

            public string FrUrl
            {
                get { return spell.Source.References.FirstOrDefaultSelect(r => r.Name == "Wiki Pathfinder-fr.org", x => x.HrefString); }
            }

            public string EnUrl
            {
                get { return spell.Source.References.FirstOrDefaultSelect(r => r.Name == "Paizo PRD", x => x.HrefString); }
            }

            private string FindLevel(string list)
            {
                return spell.Levels.FirstOrDefaultSelect(l => l.List == list, x => x.Level.ToString());
            }
        }

        class FeatWrapper
        {
            private readonly Feat feat;

            public FeatWrapper(Feat feat)
            {
                this.feat = feat;
            }

            public string Id
            {
                get { return feat.Id; }
            }

            public string Name
            {
                get { return feat.Name; }
            }

            public string Source
            {
                get { return feat.Source.Id; }
            }

            public string Types
            {
                get { return string.Join(", ", feat.Types); }
            }
        }

        class MonsterWrapper
        {
            private readonly Monster monster;

            public MonsterWrapper(Monster monster)
            {
                this.monster = monster;
            }

            public string Id
            {
                get { return monster.Id; }
            }

            public string Name
            {
                get { return monster.Name; }
            }

            public string Source
            {
                get { return monster.Source.Id; }
            }

            public string Type
            {
                get { return monster.Type.ToString(); }
            }

            public string Climate
            {
                get { return monster.Climate.ToString(); }
            }

            public string Environment
            {
                get { return monster.Environment.ToString(); }
            }

            public decimal CR
            {
                get { return monster.CR; }
            }

            public string English
            {
                get { return monster.OpenLocalization().GetLocalizedEntry(DataSetLanguages.English, "name", string.Empty); }
            }
        }
    }
}