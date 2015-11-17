namespace WikiExportParser.Logging
{
    using System.Collections.Generic;

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
            foreach (var log in this.logs)
            {
                log.Information(message);
            }
        }

        public void Warning(string message)
        {
            foreach (var log in this.logs)
            {
                log.Warning(message);
            }
        }

        public void Error(string message)
        {
            foreach (var log in this.logs)
            {
                log.Error(message);
            }
        }
    }
}
