////////////////////////////////////////////////////////////////////////////////
// PlanQualityMetrics.cs
//
//  A ESAPI v11/v13 Script that generates a report for the selected patient and
//  plan with various calculated Plan Quality Metrics for defined structures.
//
//  The plan quality metrics that will be generated are defined in 
//  UserDefinedMetrics.NonHypoFractionated (UserDefinedMetrics.cs).
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
//

using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;
using System.Reflection;
using System.Xml;

namespace VMS.TPS
{
  public class Script
  {
    /// <summary>
    /// The PlanQualityMetrics script generates a report of plan quality, using evaluation metrics defined in UserDefinedMetrics.cs.
    /// The script first computes metrics for all found structures, evaluates whether the metrics are in or out of tolerance and 
    /// generates an xml report of that information and saves it to %temp\<patientid>\PQMReport-<planid>.xml.  The XML report is
    /// then transformed into an HTML report with an XSLT stylesheet, using either the gen_report.xsl stylesheet for Plans, or 
    /// the gen_report_plansum.xsl stylesheet for PlanSums.  The HTML report is then shown to the user.
    /// 
    /// Metric evaluation and XML & HTML report generation is driven by PQMReporter.
    /// 
    // Many of these metrics used in this script come from QUANTEC data. See 
    // http://en.wikibooks.org/wiki/Radiation_Oncology/Toxicity/QUANTEC
    //
    /// Add your own evaluators and change structure ids to match those used in your clinic in UserDefinedMetrics.cs.
    /// If new evaluators are added, you need to also add a call to use those evaluators in 
    /// PQMReporter.WriteDoseStatisticsXML_Prostate2GyOrLess, or WriteDoseStatisticsXML_HeadAndNeck2GyOrLess.
    /// 
    /// This script currently supports metric reporting for normally fractionated Prostate and Head and Neck.
    /// </summary>
    /// <param name="context"></param>
    public void Execute(ScriptContext context /*, System.Windows.Window window*/)
    {
      string temp = System.Environment.GetEnvironmentVariable("TEMP");
      string eclipseVersion = System.Reflection.Assembly.GetAssembly
        (typeof(VMS.TPS.Common.Model.API.Application)).GetName().Version.ToString();
      string scriptVersion = "1.0.10";

      if (context.PlanSumsInScope.Count() > 0)
      {
        foreach (PlanSum plansum in context.PlanSumsInScope)
        {
          if (plansum.StructureSet == null)
          {
            MessageBox.Show(  "Plansum has no structure set.", 
                              "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
          }
          if (plansum.Dose == null)
          {
            MessageBox.Show("Loaded plansum has no dose: Please select an external beam plan or plansum with dose calculated that you would like evaluate.", 
                            "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
          }
          // Load the plansum stylesheet into memory, transform report XML into HTML, show that to the user.
          // gen_report_plansum.xsl is automatically compiled in when the build action for the VS project is set to 
          // "Embedded Resource".  See online help for Build Action.  Read the embedded stylesheet file from the DLL.
          Stream stylesheet = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlanQualityMetrics.gen_report_plansum.xsl");
          XmlReader stylesheetReader = XmlReader.Create(new StreamReader(stylesheet));
          VMS.TPS.PlanSumReporter reporter = new PlanSumReporter(temp, context.CurrentUser.Id, eclipseVersion, scriptVersion, stylesheetReader);
          Patient patient = context.Patient;

          string xmlReportPath;
          string htmlReportPath = reporter.generateReport(patient, plansum.StructureSet, plansum, out xmlReportPath);

          // 'Start' generated HTML file to launch browser window
          System.Diagnostics.Process.Start(htmlReportPath);
          // Sleep for a few seconds to let internet browser window to start
          System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
        }
      }
      else
      {
        if (context.Patient == null  || context.PlanSetup == null)
        {
          MessageBox.Show("Please select the external beam plan that you would like evaluate.", 
                          "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        if (context.PlanSetup.StructureSet == null)
        {
          MessageBox.Show("Loaded plan does not have a structure set.", 
                         "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        if (context.PlanSetup.Dose == null)
        {
          MessageBox.Show("Loaded plan has no dose: Please select an external beam plan with dose calculated that you would like evaluate.", 
                          "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
          return;
        }
        // gen_report.xsl is automatically compiled in when the build action for the VS project is set to 
        // "Embedded Resource".  See online help for Build Action.  Read the embedded stylesheet file from the DLL.
        Stream stylesheet = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlanQualityMetrics.gen_report.xsl");
        XmlReader stylesheetReader = XmlReader.Create(new StreamReader(stylesheet));
        VMS.TPS.PlanSetupReporter reporter = new PlanSetupReporter(temp, context.CurrentUser.Id, eclipseVersion, scriptVersion, stylesheetReader);
        Patient patient = context.Patient;
        PlanSetup plan = context.PlanSetup;
        StructureSet ss = context.StructureSet;

        string xmlReportPath;
        string htmlReportPath = reporter.generateReport(patient, plan.StructureSet, plan, out xmlReportPath);

        // 'Start' generated HTML file to launch browser window
        System.Diagnostics.Process.Start(htmlReportPath);
        // Sleep for a few seconds to let internet browser window to start
        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
      }
    }
  }
}
