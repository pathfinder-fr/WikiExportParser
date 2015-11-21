// -----------------------------------------------------------------------
// <copyright file="XmlDataSetWriter.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

namespace WikiExportParser.Writers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using PathfinderDb.Schema;

    public class XmlDataSetWriter : IDataSetWriter
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(DataSet));

        public bool Accept(string name, DataSet dataSet, Dictionary<string, string> options)
        {
            return true;
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
                this.WriteXml(Path.Combine(folder, "header.xml"), copy);
            }

            // spells
            if ((dataSet.SpellLists != null && dataSet.SpellLists.Count != 0) || (dataSet.Spells != null && dataSet.Spells.Count != 0))
            {
                copy = new DataSet { Sources = dataSet.Sources, SpellLists = dataSet.SpellLists, Spells = dataSet.Spells };
                this.WriteXml(Path.Combine(folder, "spells.xml"), copy);
            }

            // feats
            if ((dataSet.Feats != null && dataSet.Feats.Count != 0))
            {
                copy = new DataSet { Sources = dataSet.Sources, Feats = dataSet.Feats };
                this.WriteXml(Path.Combine(folder, "feats.xml"), copy);
            }

            // monsters
            if ((dataSet.Monsters != null && dataSet.Monsters.Count != 0))
            {
                copy = new DataSet { Sources = dataSet.Sources, Monsters = dataSet.Monsters };
                this.WriteXml(Path.Combine(folder, "monsters.xml"), copy);
            }
        }

        private void WriteXml(string path, DataSet dataSet)
        {
            using (var writer = System.Xml.XmlWriter.Create(path, new System.Xml.XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, dataSet);
            }
        }
    }
}