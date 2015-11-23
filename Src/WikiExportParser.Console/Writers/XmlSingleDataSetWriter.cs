// -----------------------------------------------------------------------
// <copyright file="XmlSingleDataSetWriter.cs" organization="Pathfinder-Fr">
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
    public class XmlSingleDataSetWriter : IDataSetWriter
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof (DataSet));


        public bool Accept(string name, DataSet dataSet, Dictionary<string, string> options)
        {
            return true;
        }

        public void Write(string name, DataSet dataSet, string directory)
        {
            var path = Path.Combine(directory, string.Format("{0}.xml", name));
            using (var writer = XmlWriter.Create(path, new XmlWriterSettings {Indent = true}))
            {
                serializer.Serialize(writer, dataSet);
            }
        }
    }
}