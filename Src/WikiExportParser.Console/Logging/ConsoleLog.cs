// -----------------------------------------------------------------------
// <copyright file="ConsoleLog.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace WikiExportParser.Logging
{
    public class ConsoleLog : ILog
    {
        public int ErrorCount { get; set; }

        public int WarningCount { get; set; }

        public void Information(string message)
        {
            Write(message, ConsoleColor.White);
        }

        public void Warning(string message)
        {
            Write(message, ConsoleColor.Yellow);
            WarningCount++;
        }

        public void Error(string message)
        {
            Write(message, ConsoleColor.Red);
            ErrorCount++;
        }

        private void Write(string message, ConsoleColor color = ConsoleColor.White)
        {
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = previous;
        }
    }
}