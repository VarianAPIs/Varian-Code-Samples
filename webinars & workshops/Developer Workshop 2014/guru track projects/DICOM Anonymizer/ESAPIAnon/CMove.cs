using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FetchDicom
{
    public class CMove
    {
        public const string DCMTK_BIN_PATH = @"C:\variandeveloper\tools\dcmtk-3.6.0-win32-i386\bin\"; // path to DCMTK binaries
        public const string AET = "Anon";                 // local AE title
        //public const string AEC = "\"DB Daemon\"";               // AE title of VMS DB Daemon

        public static bool GenerateDicomFiles(string patientMRN, string IPAddress, string port, string AEC, string path)
        {
            if (String.IsNullOrEmpty(patientMRN) || String.IsNullOrEmpty(IPAddress) ||
                String.IsNullOrEmpty(port) || String.IsNullOrEmpty(AEC))
            {
                MessageBox.Show("patient Id or IP address or Port or AEC is missing ");
                return false;
            }

            string move = "movescu.exe -S -aet " + "\"" + AET + "\"" + " -aec " + "\"" + AEC + "\"" +
                   " -od " + "\"" + path + "\"" + " " + "-k " + "\"0010,0020=" + patientMRN + "\"" + " -k 0008,0052=STUDY " +
                   "--port 106 " +
                   IPAddress + " " + port;
            try
            {

                System.Diagnostics.ProcessStartInfo procStartInfo =
                      new System.Diagnostics.ProcessStartInfo("cmd", "/c " + move);

                //The following commands are needed to redirect the standard output.
                //This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                //Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                //Display the command output.
                Console.WriteLine(result);
                return true;
            }
            catch (Exception objException)
            {
                // Log the exception
                return false;
            }

        }
    }
}
