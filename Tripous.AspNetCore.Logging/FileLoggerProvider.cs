using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using System.Collections.Concurrent;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tripous.AspNetCore.Logging
{
    /// <summary>
    /// A logger provider that writes log entries to a text file.
    /// <para>"File" is the provider alias of this provider and can be used in the Logging section of the appsettings.json.</para>
    /// </summary>
    [Microsoft.Extensions.Logging.ProviderAlias("File")]
    public class FileLoggerProvider : LoggerProvider
    {
        /* private */
        bool Terminated;
        int Counter = 0;
        string FilePath;
        Dictionary<string, int> Lengths = new Dictionary<string, int>();
       
        ConcurrentQueue<LogEntry> InfoQueue = new ConcurrentQueue<LogEntry>();

        /// <summary>
        /// Applies the log file retains policy according to options
        /// </summary>
        void ApplyRetainPolicy()
        {
            FileInfo FI;
            try
            {
                List<FileInfo> FileList = new DirectoryInfo(Settings.Folder)
                .GetFiles("*.log", SearchOption.TopDirectoryOnly)
                .OrderBy(fi => fi.CreationTime)
                .ToList();

                while (FileList.Count >= Settings.RetainPolicyFileCount)
                {
                    FI = FileList.First();
                    FI.Delete();
                    FileList.Remove(FI);
                }
            }
            catch  
            { 
            }

        }
        /// <summary>
        /// Writes a line of text to the current file.
        /// If the file reaches the size limit, creates a new file and uses that new file.
        /// </summary>
        void WriteLine(string Text)
        {
            // check the file size after any 100 writes
            Counter++;
            if (Counter % 100 == 0)
            {
                FileInfo FI = new FileInfo(FilePath);
                if (FI.Length > (1024 * 1024 * Settings.MaxFileSizeInMB))
                {                    
                    BeginFile();
                }
            }

            File.AppendAllText(FilePath, Text);
        }
        /// <summary>
        /// Pads a string with spaces to a max length. Truncates the string to max length if the string exceeds the limit.
        /// </summary>
        string Pad(string Text, int MaxLength)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return "".PadRight(MaxLength);

            if (Text.Length > MaxLength)
                return Text.Substring(0, MaxLength);

            return Text.PadRight(MaxLength);
        }
        /// <summary>
        /// Prepares the lengths of the columns in the log file
        /// </summary>
        void PrepareLengths()
        {
            // prepare the lengs table
            Lengths["Time"] = 24;
            Lengths["Host"] = 16;
            Lengths["User"] = 16;
            Lengths["Level"] = 14;
            Lengths["EventId"] = 32;
            Lengths["Category"] = 92;
            Lengths["Scope"] = 64;
        }

        /// <summary>
        /// Creates a new disk file and writes the column titles
        /// </summary>
        void BeginFile()
        {
            Directory.CreateDirectory(Settings.Folder);
            FilePath = Path.Combine(Settings.Folder, LogEntry.StaticHostName + "-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".log");

            // titles
            StringBuilder SB = new StringBuilder();
            SB.Append(Pad("Time", Lengths["Time"]));
            SB.Append(Pad("Host", Lengths["Host"]));
            SB.Append(Pad("User", Lengths["User"]));
            SB.Append(Pad("Level", Lengths["Level"]));
            SB.Append(Pad("EventId", Lengths["EventId"]));
            SB.Append(Pad("Category", Lengths["Category"]));
            SB.Append(Pad("Scope", Lengths["Scope"]));
            SB.AppendLine("Text");

            File.WriteAllText(FilePath, SB.ToString());

            ApplyRetainPolicy();
        }
        /// <summary>
        /// Pops a log info instance from the stack, prepares the text line, and writes the line to the text file.
        /// </summary>
        void WriteLogLine()
        {
            LogEntry Info = null;
            if (InfoQueue.TryDequeue(out Info))
            {
                string S;
                StringBuilder SB = new StringBuilder();
                SB.Append(Pad(Info.TimeStampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.ff"), Lengths["Time"]));
                SB.Append(Pad(Info.HostName, Lengths["Host"]));
                SB.Append(Pad(Info.UserName, Lengths["User"]));
                SB.Append(Pad(Info.Level.ToString(), Lengths["Level"]));
                SB.Append(Pad(Info.EventId != null ? Info.EventId.ToString() : "", Lengths["EventId"]));
                SB.Append(Pad(Info.Category, Lengths["Category"]));

                S = "";
                if (Info.Scopes != null && Info.Scopes.Count > 0)
                {
                    LogScopeInfo SI = Info.Scopes.Last();
                    if (!string.IsNullOrWhiteSpace(SI.Text))
                    {
                        S = SI.Text;
                    }
                    else
                    {
                    }
                }
                SB.Append(Pad(S, Lengths["Scope"]));

                string Text = Info.Text;

                /* writing properties is too much for a text file logger
                if (Info.StateProperties != null && Info.StateProperties.Count > 0)
                {
                    Text = Text + " Properties = " + Newtonsoft.Json.JsonConvert.SerializeObject(Info.StateProperties);
                }                 
                 */

                if (!string.IsNullOrWhiteSpace(Text))
                {
                    SB.Append(Text.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " "));
                }

                SB.AppendLine();
                WriteLine(SB.ToString());
            }

        }
        void ThreadProc()
        {
            Task.Run(() => {

                while (!Terminated)
                {
                    try
                    {
                        WriteLogLine();
                        System.Threading.Thread.Sleep(100);
                    }
                    catch // (Exception ex)
                    {
                    }
                }

            });
        }

        /* overrides */
        /// <summary>
        /// Disposes the options change toker. IDisposable pattern implementation.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            Terminated = true;
            base.Dispose(disposing);
        }

        /* construction */
        /// <summary>
        /// Constructor.
        /// <para>The IOptionsMonitor provides the OnChange() method which is called when the user alters the settings of this provider in the appsettings.json file.</para>
        /// </summary>
        public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> Settings)
            : this(Settings.CurrentValue)
        {   
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/change-tokens
            SettingsChangeToken = Settings.OnChange(settings => {       
                this.Settings = settings;                   
            }); 
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public FileLoggerProvider(FileLoggerOptions Settings)
        {
            PrepareLengths();
            this.Settings = Settings;

            // create the first file
            BeginFile();

            ThreadProc();
        }

        /* public */
        /// <summary>
        /// Checks if the given logLevel is enabled. It is called by the Logger.
        /// </summary>
        public override bool IsEnabled(LogLevel logLevel)
        {
            bool Result = logLevel != LogLevel.None
               && this.Settings.LogLevel != LogLevel.None
               && Convert.ToInt32(logLevel) >= Convert.ToInt32(this.Settings.LogLevel);

            return Result;
        }
        /// <summary>
        /// Writes the specified log information to a log file.
        /// </summary>
        public override void WriteLog(LogEntry Info)
        {
            InfoQueue.Enqueue(Info);
        }

        /* properties */
        /// <summary>
        /// Returns the settings
        /// </summary>
        internal FileLoggerOptions Settings { get; private set; }


    }
}
