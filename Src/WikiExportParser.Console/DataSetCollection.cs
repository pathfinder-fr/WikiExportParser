// -----------------------------------------------------------------------
// <copyright file="DataSetCollection.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

namespace WikiExportParser
{
    using System;
    using System.Collections.Generic;
    using PathfinderDb.Schema;

    /// <summary>
    /// Collection de <see cref="DataSet" /> rangés selon leur source.
    /// </summary>
    public class DataSetCollection
    {
        private readonly IDictionary<string, DataSet> items = new Dictionary<string, DataSet>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Obtient la liste des <see cref="DataSet" />, rangés selon leur source.
        /// </summary>
        public IDictionary<string, DataSet> DataSets
        {
            get { return this.items; }
        }

        /// <summary>
        /// Obtient ou définit la langue à indiquer pour chaque dataset créé.
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// Renvoie en créeant si nécessaire le <see cref="DataSet" /> avecc le nom indiqué.
        /// </summary>
        public DataSet ResolveDataSet(string name)
        {
            DataSet dataSet;

            if (!this.items.TryGetValue(name, out dataSet))
            {
                dataSet = new DataSet();
                if (!string.IsNullOrEmpty(this.Lang))
                {
                    dataSet.Lang = this.Lang;
                }

                dataSet.Sources = new List<Source>();
                //dataSet.Sources.Add(new Source { Id = name });
                this.items.Add(name, dataSet);
            }

            return dataSet;
        }
    }
}