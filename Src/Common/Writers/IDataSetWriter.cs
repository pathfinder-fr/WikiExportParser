using Pathfinder.DataSet;
using System.Collections.Generic;

namespace WikiExportParser.Writers
{
    public interface IDataSetWriter
    {
        bool Accept(string name, DataSet dataSet, Dictionary<string, string> options);

        void Write(string name, DataSet dataSet, string directory);
    }
}
