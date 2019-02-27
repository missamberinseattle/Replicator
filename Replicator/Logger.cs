using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace Replicator
{
    public static class Logger
    {
        private static readonly object Padlock = new object();
        private static string _logFileDirectory;
        private static string _logFileNameRoot;
        private static string _logFileName;
        private static int _maxLogAge = -1;
        private static DateTime _lastLogWrite = DateTime.MinValue;
        private static TraceLevel? _logVerbosity;

        public static int MaxLogAge
        {
            get
            {
                if (_maxLogAge < 0)
                {
                    _maxLogAge = 14; // Configuration.Logging.MaxLogAge;
                }

                return _maxLogAge;
            }
        }

        public static string LogFileNameRoot
        {
            get
            {
                if (_logFileNameRoot == null)
                {
                    _logFileNameRoot = "Replicator"; // Configuration.Logging.FileNameRoot;
                }

                return _logFileNameRoot;
            }
        }

        public static TraceLevel LogVerbosity
        {
            get
            {
                if (_logVerbosity == null)
                {
                    _logVerbosity = TraceLevel.Verbose; // Configuration.Logging.Verbosity;
                }

                return (TraceLevel)_logVerbosity;
            }
            set { _logVerbosity = value; }
        }


        public static void Info(string message)
        {
            Log(string.Empty, message, TraceLevel.Info);
        }

        public static void Info(string prefix, string message)
        {
            Log(prefix, message, TraceLevel.Info);
        }

        public static void Warning(string message)
        {
            Log(string.Empty, message, TraceLevel.Warning);
        }

        public static void Warning(string prefix, string message)
        {
            Log(prefix, message, TraceLevel.Warning);
        }

        public static void Error(string message)
        {
            Log(string.Empty, message, TraceLevel.Error);
        }

        public static void Error(string prefix, string message)
        {
            Log(prefix, message, TraceLevel.Error);
        }


        public static void Verbose(string message)
        {
            Log(string.Empty, message);
        }

        public static void Verbose(string prefix, string message)
        {
            Log(prefix, message);
        }


        private static void Log(string prefix, string message, TraceLevel traceLevel = TraceLevel.Verbose)
        {
            // Off (0), Error (1), Warning (2), Info (3), Verbosity (4)

            if (traceLevel == TraceLevel.Off || traceLevel > LogVerbosity)
            {
                return;
            }

            MainForm.AddToLogView(traceLevel, prefix, message);

            var text = prefix + "\t" + message;
            Debug.WriteLine(text);

            try
            {
                var logFilePath = GetLogFilePath();

                lock (Padlock)
                {
                    using (TextWriter logWriter = new StreamWriter(logFilePath, true))
                    {
                        logWriter.Write(DateTime.Now.ToString("HH:mm:ss.ffff"));
                        logWriter.Write("\t");
                        logWriter.Write(traceLevel);
                        logWriter.Write("\t");
                        logWriter.WriteLine(text);
                        logWriter.Close();
                    }
                }
                _lastLogWrite = DateTime.UtcNow.Date;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private static string GetLogFilePath()
        {
            if (_logFileDirectory == null)
            {
                _logFileDirectory = Environment.CurrentDirectory; // Configuration.Logging.Directory;
            }

            //if (_logFileDirectory == null)
            //{
            //    throw new ConfigurationErrorsException("Could not load logging directory from web.config");
            //}

            var name = GetLogFileName();

            var path = Path.Combine(_logFileDirectory, name);
            Debug.WriteLine(path);
            return path;
        }

        private static string GetLogFileName()
        {
            if (_lastLogWrite.CompareTo(DateTime.UtcNow.Date) == 0 && _logFileName != null)
            {
                return _logFileName;
            }

            CleanOldLogs();

            _logFileName = LogFileNameRoot + "_" + DateTime.Now.Date.ToString("yyyyMMdd") + ".log";
            Debug.WriteLine(_logFileName);
            return _logFileName;
        }

        private static void CleanOldLogs()
        {
            var files = Directory.GetFiles(_logFileDirectory, "*.log");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var age = DateTime.Now.Subtract(fileInfo.LastWriteTime);

                if (!(age.TotalDays > MaxLogAge)) continue;

                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Could not delete " + file + ", Message: " + ex.Message);
                }
            }
        }
    }
}
