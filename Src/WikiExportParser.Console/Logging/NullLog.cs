namespace WikiExportParser.Logging
{
    internal class NullLog : ILog
    {
        public static readonly ILog Instance = new NullLog();

        private NullLog() { }

        public void Warning(string message)
        {
        }

        public void Error(string message)
        {
        }

        public void Information(string message)
        {
        }
    }
}
