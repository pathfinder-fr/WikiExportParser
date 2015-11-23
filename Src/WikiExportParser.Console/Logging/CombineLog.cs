// -----------------------------------------------------------------------
// <copyright file="CombineLog.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace WikiExportParser.Logging
{
    public class CombineLog : ILog
    {
        private readonly IEnumerable<ILog> logs;

        public CombineLog(IEnumerable<ILog> logs)
        {
            this.logs = logs;
        }

        public CombineLog(params ILog[] logs)
        {
            this.logs = logs;
        }

        public void Information(string message)
        {
            foreach (var log in logs)
            {
                log.Information(message);
            }
        }

        public void Warning(string message)
        {
            foreach (var log in logs)
            {
                log.Warning(message);
            }
        }

        public void Error(string message)
        {
            foreach (var log in logs)
            {
                log.Error(message);
            }
        }
    }
}