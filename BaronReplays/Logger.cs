using System;
using System.IO;
using System.Text;
using System.Threading;

namespace BaronReplays
{
    class Logger
    {
        private static Logger instance;
        private StreamWriter logWriter;
        private StringBuilder builder;

        public string LogContent
        {
            get
            {
                return builder.ToString();
            }
        }
        public string FileName
        {
            get;
        }

        private Logger()
        {
            Utilities.IfDirectoryNotExitThenCreate("Logs");
            FileName = @"Logs\" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + "Log.txt";
            builder = new StringBuilder();
            logWriter = new StreamWriter(FileName, false);
        }

        private object LogLock = new Object();
        public void WriteLog(String message)
        {
            lock (LogLock)
            {
#if (DEBUG)
                Console.WriteLine(message);
#endif
                var str = String.Format("{0,-12} {1,3} [{2}] ", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId, DateTime.Now) + message;
                builder.AppendLine(str);
                logWriter.WriteLine(str);
                logWriter.Flush();
            }
        }

        //~Logger()
        //{
        //    logWriter.Close();
        //}

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }
                return instance;
            }
        }


    }
}
