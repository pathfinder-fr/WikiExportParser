namespace WikiExportParser.Logging
{
    using System;

    public class ConsoleLog : ILog
    {
        public int ErrorCount { get; set; }

        public int WarningCount { get; set; }

        public void Information(string message)
        {
            this.Write(message, ConsoleColor.White);
        }

        public void Warning(string message)
        {
            this.Write(message, ConsoleColor.Yellow);
            this.WarningCount++;
        }

        public void Error(string message)
        {
            this.Write(message, ConsoleColor.Red);
            this.ErrorCount++;
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
