// -----------------------------------------------------------------------
// <copyright file="ILog.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

namespace WikiExportParser
{
    public interface ILog
    {
        void Information(string message);

        void Warning(string message);

        void Error(string message);
    }

    public static class LogExtensions
    {
        public static void Information(this ILog log, string format, params object[] args)
        {
            log.Information(string.Format(format, args));
        }

        public static void Warning(this ILog log, string format, params object[] args)
        {
            log.Warning(string.Format(format, args));
        }

        public static void Error(this ILog log, string format, params object[] args)
        {
            log.Error(string.Format(format, args));
        }
    }
}