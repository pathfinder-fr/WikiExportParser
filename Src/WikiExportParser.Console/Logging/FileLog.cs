// -----------------------------------------------------------------------
// <copyright file="FileLog.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;

namespace WikiExportParser.Logging
{
    public class FileLog : ILog, IDisposable
    {
        private readonly StreamWriter writer;

        public FileLog(string path)
        {
            writer = new StreamWriter(path);
        }

        ~FileLog()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Information(string message)
        {
            writer.WriteLine("[{0:u}] INFO  {1}", DateTime.Now, message);
        }

        public void Warning(string message)
        {
            writer.WriteLine("[{0:u}] WARN  {1}", DateTime.Now, message);
        }

        public void Error(string message)
        {
            writer.WriteLine("[{0:u}] ERROR {1}", DateTime.Now, message);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                writer.Flush();
                writer.Dispose();
            }
        }
    }
}