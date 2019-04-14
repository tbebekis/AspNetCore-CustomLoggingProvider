using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Logging
{
    public class LogFileListener : ILogListener
    {
        string fFolder = "./Logs";
        int fMaxFileSizeInMB = 2;
        int fRetainPolicyFileCount = 5;

        int Counter = 0;
        string FilePath;
        Dictionary<string, int> Lengths = new Dictionary<string, int>();

        void ApplyRetainPolicy()
        {
            FileInfo FI;
            try
            {
                List<FileInfo> FileList = new DirectoryInfo(Folder)
                .GetFiles("*.log", SearchOption.TopDirectoryOnly)
                .OrderBy(fi => fi.CreationTime)
                .ToList();

                while (FileList.Count >= RetainPolicyFileCount)
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
        void WriteLine(string Text)
        {
            // check the file size after any 100 writes
            Counter++;
            if (Counter % 100 == 0)
            {
                FileInfo FI = new FileInfo(FilePath);
                if (FI.Length > (1024 * 1024 * MaxFileSizeInMB))
                {
                    BeginFile();
                }
            }

            File.AppendAllText(FilePath, Text);
        }
        string Pad(string Text, int MaxLength)
        {
            if (string.IsNullOrWhiteSpace(Text))
                return "".PadRight(MaxLength);

            if (Text.Length > MaxLength)
                return Text.Substring(0, MaxLength);

            return Text.PadRight(MaxLength);
        }
        void PrepareLengths()
        {
            // prepare the lengs table
            Lengths["Time"] = 24;
            Lengths["Host"] = 16;
            Lengths["User"] = 16;
            Lengths["Level"] = 10;
            Lengths["EventId"] = 12;
            Lengths["Source"] = 32;
        }
        void BeginFile()
        {
            Directory.CreateDirectory(Folder);
            FilePath = Path.Combine(Folder, LogEntry.StaticHostName + "-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".log");

            // titles
            StringBuilder SB = new StringBuilder();
            SB.Append(Pad("Time", Lengths["Time"]));
            SB.Append(Pad("Host", Lengths["Host"]));
            SB.Append(Pad("User", Lengths["User"]));
            SB.Append(Pad("Level", Lengths["Level"]));
            SB.Append(Pad("EventId", Lengths["EventId"]));
            SB.Append(Pad("Source", Lengths["Source"]));
            SB.AppendLine("Text");

            File.WriteAllText(FilePath, SB.ToString());

            ApplyRetainPolicy();
        }
 
        public LogFileListener()
        {
            PrepareLengths();            
            BeginFile();        // create the first file
        }
        public void ProcessLog(LogEntry Info)
        {
            bool IsActive = Info.Level != LogLevel.None
                            && this.Level != LogLevel.None
                            && Convert.ToInt32(this.Level) <= Convert.ToInt32(Info.Level);

            if (IsActive)
            {
                StringBuilder SB = new StringBuilder();
                SB.Append(Pad(Info.TimeStampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.ff"), Lengths["Time"]));
                SB.Append(Pad(Info.HostName, Lengths["Host"]));
                SB.Append(Pad(Info.UserName, Lengths["User"]));
                SB.Append(Pad(Info.Level.ToString(), Lengths["Level"]));
                SB.Append(Pad(Info.EventId > 0 ? Info.EventId.ToString() : "", Lengths["EventId"]));
                SB.Append(Pad(Info.Source, Lengths["Source"])); 

                string Text = Info.Exception != null? Info.Exception.Message: Info.Text; 

                if (!string.IsNullOrWhiteSpace(Text))
                {
                    SB.Append(Text.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " "));
                }

                SB.AppendLine();
                WriteLine(SB.ToString());
            }
        }

        public LogLevel Level { get; set; } = LogLevel.Info;
        public string Folder
        {
            get { return !string.IsNullOrWhiteSpace(fFolder) ? fFolder : System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location); }
            set { fFolder = value; }
        }
        public int MaxFileSizeInMB
        {
            get { return fMaxFileSizeInMB > 0 ? fMaxFileSizeInMB : 2; }
            set { fMaxFileSizeInMB = value; }
        }
        public int RetainPolicyFileCount
        {
            get { return fRetainPolicyFileCount < 5 ? 5 : fRetainPolicyFileCount; }
            set { fRetainPolicyFileCount = value; }
        }
    }
}
