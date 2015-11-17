using Pathfinder.DataSet;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

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
            var path = Path.Combine(directory, string.Format("{0}.xml", name));
            using (var writer = System.Xml.XmlWriter.Create(path, new System.Xml.XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, dataSet);
            }
        }
    }
}
