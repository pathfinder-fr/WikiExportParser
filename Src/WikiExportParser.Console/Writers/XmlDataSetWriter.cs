// -----------------------------------------------------------------------
// <copyright file="XmlDataSetWriter.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using PathfinderDb.Schema;

namespace WikiExportParser.Writers
{
    public class XmlDataSetWriter : IDataSetWriter
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(DataSet));

        public bool Accept(string name, DataSet dataSet, Dictionary<string, string> options)
        {
            return true;
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

            DataSet copy;

            // headers
            if (dataSet.Header != null)
            {
                copy = new DataSet { Sources = dataSet.Sources, Header = dataSet.Header };
                WriteXml(Path.Combine(folder, "header.xml"), copy);
            }

            // spells
            if ((dataSet.SpellLists != null && dataSet.SpellLists.Count != 0) || (dataSet.Spells != null && dataSet.Spells.Count != 0))
            {
                copy = new DataSet { Sources = dataSet.Sources, SpellLists = dataSet.SpellLists, Spells = dataSet.Spells };
                WriteXml(Path.Combine(folder, "spells.xml"), copy);
            }

            // feats
            if ((dataSet.Feats != null && dataSet.Feats.Count != 0))
            {
                copy = new DataSet { Sources = dataSet.Sources, Feats = dataSet.Feats };
                WriteXml(Path.Combine(folder, "feats.xml"), copy);
            }

            // monsters
            if ((dataSet.Monsters != null && dataSet.Monsters.Count != 0))
            {
                copy = new DataSet { Sources = dataSet.Sources, Monsters = dataSet.Monsters };
                WriteXml(Path.Combine(folder, "monsters.xml"), copy);
            }
        }

        private void WriteXml(string path, DataSet dataSet)
        {
            using (var writer = XmlWriter.Create(path, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, dataSet);
            }
        }
    }
}