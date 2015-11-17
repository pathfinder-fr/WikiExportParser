namespace WikiExportParser.Logging
{
    using System;
    using System.IO;

    public class FileLog : ILog, IDisposable
    {
        private readonly StreamWriter writer;

        public FileLog(string path)
        {
            this.writer = new StreamWriter(path);
        }

        ~FileLog()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Information(string message)
        {
            this.writer.WriteLine(string.Format("[{0:u}] INFO  {1}", DateTime.Now, message));
        }

        public void Warning(string message)
        {
            this.writer.WriteLine(string.Format("[{0:u}] WARN  {1}", DateTime.Now, message));
        }

        public void Error(string message)
        {
            this.writer.WriteLine(string.Format("[{0:u}] ERROR {1}", DateTime.Now, message));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.writer.Flush();
                this.writer.Dispose();
            }
        }
    }
}
