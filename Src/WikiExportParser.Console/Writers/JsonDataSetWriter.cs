// -----------------------------------------------------------------------
// <copyright file="JsonDataSetWriter.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

namespace WikiExportParser.Writers
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using PathfinderDb.Schema;

    public class JsonDataSetWriter : IDataSetWriter
    {
        private static readonly JsonSerializer serializer = new JsonSerializer();

        static JsonDataSetWriter()
        {
            serializer.DefaultValueHandling = DefaultValueHandling.Ignore;
            serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        }

        public bool Accept(string name, DataSet dataSet, Dictionary<string, string> options)
        {
            return options.ContainsKey("json");
        }

        public void Write(string name, DataSet dataSet, string directory)
        {
            var folder = Path.Combine(directory, name);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            DataSet copy;

            // headers
            if (dataSet.Header != null)
            {
                copy = new DataSet { Sources = dataSet.Sources, Header = dataSet.Header };
                this.WriteJson(Path.Combine(folder, "header.json"), copy);
            }

            // spells
            if ((dataSet.SpellLists != null && dataSet.SpellLists.Count != 0) || (dataSet.Spells != null && dataSet.Spells.Count != 0))
            {
                copy = new DataSet { Sources = dataSet.Sources, SpellLists = dataSet.SpellLists, Spells = dataSet.Spells };
                this.WriteJson(Path.Combine(folder, "spells.json"), copy);
            }

            // feats
            if (dataSet.Feats != null && dataSet.Feats.Count != 0)
            {
                copy = new DataSet { Sources = dataSet.Sources, Feats = dataSet.Feats };
                this.WriteJson(Path.Combine(folder, "feats.json"), copy);
            }

            // monsters
            if (dataSet.Monsters != null && dataSet.Monsters.Count != 0)
            {
                copy = new DataSet { Sources = dataSet.Sources, Monsters = dataSet.Monsters };
                this.WriteJson(Path.Combine(folder, "monsters.json"), copy);
            }
        }

        private void WriteJson(string path, DataSet dataSet)
        {
            using (var writer = new StreamWriter(path))
            using(var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.Formatting = Formatting.Indented;

                serializer.Serialize(jsonWriter, dataSet);
            }
        }
    }
}