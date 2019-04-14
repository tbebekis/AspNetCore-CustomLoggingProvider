using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public enum LogLevel
    {
        None = 0,
        Info = 1,
        Warn = 2,
        Error = 3,
    }

    public class LogEntry
    {
        public LogEntry()
        {
            TimeStampUtc = DateTime.UtcNow;
            UserName = Environment.UserName;
        }
        public LogEntry(LogLevel Level, string Text, Exception Exception = null, string Source = "", int EventId = 0)
            : this()
        {
            this.Level = Level;
            this.Text = Text;
            this.Exception = Exception;
            this.Source = Source;
            this.EventId = EventId;
        }
 
        static public readonly string StaticHostName = System.Net.Dns.GetHostName();

        public string UserName { get; private set; }
        public string HostName { get { return StaticHostName; } }
        public DateTime TimeStampUtc { get; private set; }
        public string Source { get; set; }
        public LogLevel Level { get; set; }
        public string Text { get; set; }
        public Exception Exception { get; set; }
        public int EventId { get; set; } 
    }

    public interface ILogListener
    {
        void ProcessLog(LogEntry Info);
    }

    static public class Logger
    {
        static object syncLock = new object();
        static int fActive = 0;
        static List<ILogListener> listeners = new List<ILogListener>();

        static public void Log(LogEntry Info)
        {
            lock (syncLock)
            {
                if (Active)
                {
                    foreach (ILogListener listener in listeners)
                    {

                        Task.Run(() => {

                            try
                            {
                                listener.ProcessLog(Info);
                            }
                            catch // (Exception ex)
                            {
                            }

                        });

                    }

                }

            }
        }
        static public void Log(LogLevel Level, string Text, Exception Exception = null, string Source = "", int EventId = 0)
        {
            LogEntry Info = new LogEntry(Level, Text, Exception, Source, EventId);
            Log(Info);
        }

        static public void Info(string Text, string Source = "", int EventId = 0)
        {
            Log(LogLevel.Info, Text, null, Source, EventId);
        }
        static public void Warn(string Text, string Source = "", int EventId = 0)
        {
            Log(LogLevel.Warn, Text, null, Source, EventId);
        }
        static public void Error(Exception Exception, string Source = "", int EventId = 0)
        {
            Log(LogLevel.Error, null, Exception, Source, EventId);
        }

        static public void Add(ILogListener Listener)
        {
            lock (syncLock)
            {
                try
                {
                    if (!listeners.Contains(Listener))
                    {
                        listeners.Add(Listener);
                    }
                }
                catch
                {
                }
            }

        }
        static public void Remove(ILogListener Listener)
        {
            lock (syncLock)
            {
                try
                {
                    if (listeners.Contains(Listener))
                    {
                        listeners.Remove(Listener);
                    }
                }
                catch
                {
                }
            }
        }

        static public bool Active
        {
            get
            {
                lock (syncLock)
                {
                    return fActive == 0;
                }
            }
            set
            {
                lock (syncLock)
                {
                    if (!value)
                        fActive++;
                    else
                    {
                        fActive--;
                        if (fActive < 0)
                            fActive = 0;
                    }
                }
            }
        }
    }
}
