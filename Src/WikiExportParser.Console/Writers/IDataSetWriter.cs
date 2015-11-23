// -----------------------------------------------------------------------
// <copyright file="IDataSetWriter.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using PathfinderDb.Schema;

namespace WikiExportParser.Writers
{
    public interface IDataSetWriter
    {
        bool Accept(string name, DataSet dataSet, Dictionary<string, string> options);

        void Write(string name, DataSet dataSet, string directory);
    }
}