using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VMS.TPS.Common.Model.API;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// Class for simple logging of usage and errors.
    /// Prints data to a .log and .csv file at the location specified below.
    /// </summary>
    public class Logger
    {
        // Private fields
        readonly private string LogPath = ConfigurationInfo.LogLocation;
        readonly private string LogFile = string.Format("logfile_{0}.log", System.DateTime.Now.ToString("yyyyMMdd"));
        private string LogFileCSV = string.Format("logfile_{0}.csv", System.DateTime.Now.ToString("yyyyMMdd"));
        private List<string> LogData = new List<string>();
        private string printFormat = "{0,-20} {1}";
        private List<string> HeaderInfo = new List<string>() { "RecordNumber", "StartTime", "EndTime", "UserName", "ProgramMode", "PatientId", "PlanSetupId", "PlansInScopeId", "PlanSumsInScopeId", "ShortErrors" };

        // Public fields. These must be public to allow reflection to work properly when printing to CSV.
        public string RecordNumber;
        public string StartTime;
        public string EndTime;
        public string UserName;
        public string PatientId;
        public string PlanSetupId;
        public string PlansInScopeId;
        public string PlanSumsInScopeId;
        public string ShortErrors;
        public List<string> FullErrors = new List<string>();
        public string ProgramMode;

        // Constructors
        public Logger(ScriptContext context)
        {
            UserName = (context.CurrentUser != null) ? context.CurrentUser.ToString() : "";
            PatientId = (context.Patient != null) ? context.Patient.ToString() : "";
            PlanSetupId = (context.PlanSetup != null) ? string.Format("[{0}:{1}]", context.PlanSetup.Course.ToString(), context.PlanSetup.ToString()) : "";
            PlansInScopeId = string.Join("; ", context.PlansInScope.Select(x => string.Format("[{0}:{1}]", x.Course.ToString(), x.ToString()))) ?? "Unknown";
            PlanSumsInScopeId = string.Join("; ", context.PlanSumsInScope.Select(x => string.Format("[{0}:{1}]", x.Course.ToString(), x.ToString()))) ?? "Unknown";
            StartProgram();
        }
        public Logger() { }

        // Program begins
        public void StartProgram()
        {
            DateTime currentTime = System.DateTime.Now;
            StartTime = currentTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            RecordNumber = currentTime.ToString("yyyyMMddHHmmssfff");
        }

        // Generic error
        public void Error(Exception error)
        {
            FullErrors.Add(error.ToString());
            if (string.IsNullOrEmpty(ShortErrors))
                ShortErrors += error.Message;
            else
                ShortErrors += "; " + error.Message;
        }

        // Exit the program and write to files
        public void EndProgram()
        {
            EndTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            WriteLog();
            WriteCSVLog();
        }

        // Write the text log
        public void WriteLog()
        {
            LogData.Add("");
            LogData.Add("-----------------DVH Analysis Start-----------------");
            LogData.Add(string.Format(printFormat, "Start Time:", StartTime));
            LogData.Add(string.Format(printFormat, "Record Number:", RecordNumber));
            LogData.Add(string.Format(printFormat, "ProgramMode:", ProgramMode));
            LogData.Add(string.Format(printFormat, "User:", UserName));
            LogData.Add(string.Format(printFormat, "Patient ID:", PatientId));
            LogData.Add(string.Format(printFormat, "Loaded Plan:", PlanSetupId));
            LogData.Add(string.Format(printFormat, "PlansInScope:", PlansInScopeId));
            LogData.Add(string.Format(printFormat, "PlanSumsInScope:", PlanSumsInScopeId));
            if (FullErrors != null && FullErrors.Count > 0)
                LogData.Add(string.Join("\n", FullErrors.Select(x => string.Format(printFormat, "Error:", x))));
            LogData.Add(string.Format(printFormat, "End Time:", EndTime));
            LogData.Add("");

            // Try to write to file. If it's in use, try again a few times before giving up.
            int retries = 0;
            int sleepTime = 50; //number of milliseconds to wait between tries.
            int maxRetries = 1000 / sleepTime; //If we can't write after one second, give up.
            while (true)
            {
                try
                {
                    File.AppendAllText(LogPath + LogFile, string.Join("\n", LogData));
                    break;
                }
                catch
                {
                    if (retries < maxRetries)
                    {
                        System.Threading.Thread.Sleep(sleepTime);
                        retries++;
                    }
                    else
                    {
                        Console.WriteLine("Could not write to text log");
                        break;
                    }
                }
            }
        }

        // Write the CSV log
        private void WriteCSVLog()
        {
            if (string.IsNullOrEmpty(ShortErrors))
                ShortErrors = "";

            // Verify existing data has the same header. Append a number to the end of the file name, if we don't.
            int i = 0;
            while (true)
            {
                // If file doesn't exist, we don't have to worry about merging data
                if (!File.Exists(LogPath + LogFileCSV))
                    break;

                i++;
                List<string> oldHeader = new List<string>(ParseCSV(LogPath + LogFileCSV).First());
                if (!HeaderInfo.SequenceEqual(oldHeader))
                    LogFileCSV = string.Format("logfile_{0}_{1}.csv", System.DateTime.Now.ToString("yyyyMMdd"), i);
                else
                    break;
            }

            // Build our CSV data
            List<string> CSVData = new List<string>();
            foreach (string header in HeaderInfo)
            {
                try
                {
                    string str = (this.GetType().GetField(header).GetValue(this) as string);
                    str = str.Replace(",", ";");
                    str = str.Replace("\n", "");
                    CSVData.Add(str); //Find each property that matches the header. Replace any commas and newlines
                }
                catch
                {
                    CSVData.Add(string.Format("Error: No {0} property found", header)); //If no match, mark an error
                }
            }

            // Try to write to file. If it's in use, try again a few times before giving up.
            int retries = 0;
            int sleepTime = 50; //number of milliseconds to wait between tries.
            int maxRetries = 1000 / sleepTime; //If we can't write after one second, give up.
            while (true)
            {
                try
                {
                    // If CSV doesn't exist, create it and write header
                    if (!File.Exists(LogPath + LogFileCSV))
                        File.WriteAllText(LogPath + LogFileCSV, string.Join(",", HeaderInfo));
                    File.AppendAllText(LogPath + LogFileCSV, "\n"+string.Join(",", CSVData));
                    break;
                }
                catch
                {
                    if (retries < maxRetries)
                    {
                        System.Threading.Thread.Sleep(sleepTime);
                        retries++;
                    }
                    else
                    {
                        Console.WriteLine("Could not write to CSV log");
                        break;
                    }
                }
            }
        }

        // Read in CSV file and convert to List of Strings
        private List<string[]> ParseCSV(string path)
        {
            List<string[]> parsedData = new List<string[]>();
            string[] fields;

            try
            {
                var parser = new StreamReader(File.OpenRead(path));
                while (!parser.EndOfStream)
                {
                    fields = parser.ReadLine().Split(',');
                    if (fields.All(x => x == "")) // Skip empty lines
                        continue;
                    parsedData.Add(fields);
                }
                parser.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
            return parsedData;
        }
    }
}
