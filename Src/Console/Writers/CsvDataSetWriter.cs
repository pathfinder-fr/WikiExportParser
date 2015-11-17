namespace WikiExportParser.Writers
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using PathfinderDb.Schema;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class CsvDataSetWriter : IDataSetWriter
    {
        private readonly CsvConfiguration config;

        public CsvDataSetWriter()
        {
            this.config = new CsvConfiguration
            {
                CultureInfo = CultureInfo.InvariantCulture,
            };
        }

        public bool Accept(string name, DataSet dataSet, Dictionary<string, string> options)
        {
            return options.ContainsKey("csv");

        }

        public void Write(string name, DataSet dataSet, string directory)
        {
            var folder = Path.Combine(directory, name);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (dataSet.Spells != null && dataSet.Spells.Count != 0)
            {
                this.WriteSpellDataSet(Path.Combine(folder, "spells.csv"), dataSet);
            }

            if (dataSet.Feats != null && dataSet.Feats.Count != 0)
            {
                this.WriteFeatDataSet(Path.Combine(folder, "feats.csv"), dataSet);
            }

            if (dataSet.Monsters != null && dataSet.Monsters.Count != 0)
            {
                this.WriteMonsterDataSet(Path.Combine(folder, "monsters.csv"), dataSet);
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
                get { return this.spell.Id; }
            }

            public string Name
            {
                get { return this.spell.Name; }
            }

            public string English
            {
                get
                {
                    if (this.spell.Localization == null)
                        return null;
                    return this.spell.Localization.GetLocalizedEntry("en-US", "name", null);
                }
            }

            public string Source
            {
                get { return this.spell.Source.Id; }
            }

            public string Descriptor
            {
                get { return this.spell.Descriptor.ToString(); }
            }

            public string School
            {
                get { return this.spell.School.ToString(); }
            }

            public string Alchemist { get { return this.FindLevel("alchemist"); } }

            public string AntiPaladin { get { return this.FindLevel("antipaladin"); } }

            public string Bard { get { return this.FindLevel("bard"); } }

            public string Cleric { get { return this.FindLevel("cleric"); } }

            public string Druid { get { return this.FindLevel("druid"); } }

            public string Inquisitor { get { return this.FindLevel("inquisitor"); } }

            public string Summoner { get { return this.FindLevel("summoner"); } }

            public string Magus { get { return this.FindLevel("magus"); } }

            public string Oracle { get { return this.FindLevel("orache"); } }

            public string Paladin { get { return this.FindLevel("paladin"); } }

            public string Ranger { get { return this.FindLevel("ranger"); } }

            public string Witch { get { return this.FindLevel("witch"); } }

            public string Wizard { get { return this.FindLevel("sorcerer-wizard"); } }

            public string Components { get { return this.spell.Components == null ? (string)null : this.spell.Components.Kinds.ToString(); } }

            public string Range { get { return this.spell.Range == null ? (string)null : this.spell.Range.ToString(); } }

            public string Target { get { return this.spell.Target == null ? (string)null : this.spell.Target.Value; } }

            public string CastingTime { get { return this.spell.CastingTime.ToString(); } }

            public string Summary { get { return this.spell.Summary; } }

            public string FrUrl
            {
                get { return this.spell.Source.References.FirstOrDefaultSelect(r => r.Name == "Wiki Pathfinder-fr.org", x => x.HrefString); }
            }

            public string EnUrl
            {
                get { return this.spell.Source.References.FirstOrDefaultSelect(r => r.Name == "Paizo PRD", x => x.HrefString); }
            }

            private string FindLevel(string list)
            {
                return this.spell.Levels.FirstOrDefaultSelect(l => l.List == list, x => x.Level.ToString());
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
                get { return this.feat.Id; }
            }

            public string Name
            {
                get { return this.feat.Name; }
            }

            public string Source
            {
                get { return this.feat.Source.Id; }
            }

            public string Types
            {
                get { return string.Join(", ", this.feat.Types); }
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
                get { return this.monster.Id; }
            }

            public string Name
            {
                get { return this.monster.Name; }
            }

            public string Source
            {
                get { return this.monster.Source.Id; }
            }

            public string Type
            {
                get { return this.monster.Type.ToString(); }
            }

            public string Climate
            {
                get { return this.monster.Climate.ToString(); }
            }

            public string Environment
            {
                get { return this.monster.Environment.ToString(); }
            }

            public decimal CR
            {
                get { return this.monster.CR; }
            }

            public string English
            {
                get { return this.monster.OpenLocalization().GetLocalizedEntry(DataSetLanguages.English, "name", string.Empty); }
            }
        }
    }
}