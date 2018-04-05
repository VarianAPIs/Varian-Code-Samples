////////////////////////////////////////////////////////////////////////////////
// GetDicomCollection.cs
//
//  A ESAPI v11/v13 Script that generates a DCMTK script that is then executed
//  to enact a C-MOVE of a plan, related structure set and CT series, and
//  calculated dose.
//
//  See the article "Scripting the Varian DICOM DB Daemon with ESAPI + DCMTK"
//  for details on configuring your system to use this code.
//
//  Change the configuration items starting on Line 54 to those for your local
//  configuration.
//  
// Copyright (c) 2014 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Reflection;
using System.IO;
using System.Diagnostics;


namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public const string DCMTK_BIN_PATH= @"C:\variandeveloper\tools\dcmtk-3.6.0-win32-i386\bin"; // path to DCMTK binaries
    public const string AET = @"DCMTK";                 // local AE title
    public const string AEC = @"VMSDBD1";               // AE title of VMS DB Daemon
    public const string AEM = @"VMSFD";                 // AE title of VMS File Daemon
    public const string IP_PORT = @" 192.168.15.1 5678";// IP address of server hosting the DB Daemon, port daemon is listening to
    public const string CMD_FILE_FMT = @"move-{0}({1})-{2}.cmd";

    // holds standard error output collected during run of the DCMTK script
    private static StringBuilder stdErr = new StringBuilder("");

    public void Execute(ScriptContext context /*, System.Windows.Window window*/)
    {
      if (context.PlanSetup == null)
      {
        MessageBox.Show("please select a plan"); return;
      }

      string temp = System.Environment.GetEnvironmentVariable("TEMP");
      string filename = MakeFilenameValid(
          string.Format(CMD_FILE_FMT, context.Patient.LastName, context.Patient.Id, context.PlanSetup.Id)
        );
      filename = temp + @"\" + filename;
      GenerateDicomMoveScript(context.Patient, context.PlanSetup, filename);

      string stdErr1;
      string logFile = filename + "-log.txt";
      using (Process process = new Process())
      {

        // this powershell command allows us to see the standard output and also log it.
        string command = string.Format(@"&'{0}' | tee-object -filepath '{1}'", filename, logFile);
        // Configure the process using the StartInfo properties.
        process.StartInfo.FileName = "PowerShell.exe";
        process.StartInfo.Arguments = command;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = true;

        // Set our event handler to asynchronously accumulate std err
        process.ErrorDataReceived += new DataReceivedEventHandler(stdErrHandler);

        process.Start();

        // Read the error stream first and then wait.
        stdErr1 = process.StandardError.ReadToEnd();
//        process.BeginErrorReadLine();
        process.WaitForExit();
        process.Close();
      }

      // dump out the standard error file, show them to user if they exist, and exit with a nice message.
      string stdErrFile = "";

      if (stdErr1.Length > 0)
      {
        stdErrFile = filename + "-err.txt";
        System.IO.File.WriteAllText(stdErrFile, stdErr1);
      }

      string message = string.Format("Done processing. \n\nCommand File = {0}\n\nLog file: {1}\nStandard error log: {2}", filename, logFile, stdErrFile);
      MessageBox.Show(message, "Varian Developer");

      // 'Start' generated log file to launch Notepad
      System.Diagnostics.Process.Start(logFile);
      // 'Start' generated text file to launch Notepad
      if (stdErr1.Length > 0)
        System.Diagnostics.Process.Start(stdErrFile);
      // Sleep for a few seconds to let notepad start
      System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
    }

    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// Callback for accumulating standard error while the DCMTK command script is running.
    /// </summary>
    //---------------------------------------------------------------------------------------------
    private static void stdErrHandler(object sendingProcess, 
        DataReceivedEventArgs outLine)
    {
      if (!String.IsNullOrEmpty(outLine.Data))
      {
          stdErr.Append(Environment.NewLine + outLine.Data);
      }
    }

    string MakeFilenameValid(string s)
    {
      char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
      foreach (char ch in invalidChars)
      {
        s = s.Replace(ch, '_');
      }
      return s;
    }
    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method generates a DCMTK DOS Command script that when run will execute a C-MOVE on 
    /// the DB Daemon configured above on line 54+.  The move will include the DICOM objects for
    /// the passed plan and its related CT image, Structure Set, and Dose.
    /// </summary>
    /// <param name="patient">Active Eclipse patient</param>
    /// <param name="plan">Active Eclipse RT Plan</param>
    /// <param name="filename">Active Eclipse patient</param>
    //---------------------------------------------------------------------------------------------
    public void GenerateDicomMoveScript(Patient patient, PlanSetup plan, string filename)
    {
      string move = "movescu -v -aet " + AET + " -aec " + AEC + " -aem " + AEM + " -S -k ";

      StreamWriter sw = new StreamWriter(filename, false, Encoding.ASCII);

      sw.WriteLine(@"@set PATH=%PATH%;" + DCMTK_BIN_PATH);

      // write the command to move the 3D image data set
      if (plan.StructureSet != null && plan.StructureSet.Image != null)
      {
        sw.WriteLine("rem move 3D image " + plan.StructureSet.Image.Id);
        string cmd = move + '"' + "0008,0052=SERIES" + '"' + " -k " + '"' + "0020,000E=" + plan.StructureSet.Image.Series.UID + '"' + IP_PORT;
        sw.WriteLine(cmd);
      }

      // write the command to move the structure set
      if (plan.StructureSet != null)
      {
        sw.WriteLine("rem move StructureSet " + plan.StructureSet.Id);
        string cmd = move + '"' + "0008,0052=IMAGE" + '"' + " -k " + '"' + "0008,0018=" + plan.StructureSet.UID + '"' + IP_PORT;
        sw.WriteLine(cmd);
      }

      // write the command to move the plan
      {
        sw.WriteLine("rem move RTPlan " + plan.Id);
        string cmd = move + '"' + "0008,0052=IMAGE" + '"' + " -k " + '"' + "0008,0018=" + plan.UID + '"' + IP_PORT;
        sw.WriteLine(cmd);
      }

      // write the command to move all RT Dose objects (we can't tell from the scripting API which RTDose to use, send them all).
      foreach (Study study in patient.Studies)
      {
        if ((from s in study.Series where (s.Modality == SeriesModality.RTDOSE) select s).Count() > 0)
        {
          sw.WriteLine("rem move all RTDose in study " + study.Id);
          // Study instance UID and RTDoseStorage SOP Class UID
          string cmd = move + "\"0008,0052=IMAGE\" -k \"0008,0016=1.2.840.10008.5.1.4.1.1.481.2\" -k \"0020,000D=" + study.UID + '"' + IP_PORT;
          sw.WriteLine(cmd);
        }
      }
      sw.Flush();
      sw.Close();
    }
  }
}
