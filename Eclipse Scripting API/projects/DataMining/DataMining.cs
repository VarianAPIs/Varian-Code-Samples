////////////////////////////////////////////////////////////////////////////////
// DataMining.cs
//
//  Example standalone Eclipse application that iterates over all patients
//  in the Aria database and generates a simple treatment report for each
//  in the file "%temp%\DataMiningOutput.txt", where %temp% is the windows
//  environment variable for the temporary directory.
//  
// Target Platform: ESAPI v11
//
// Copyright (c) 2013 Varian Medical Systems, Inc.
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
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;
using System.Text.RegularExpressions;

namespace DataMining
{
  class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      try
      {
        // create the application - passing (null,null) forces a manual sign in to Aria
        using (Application app = Application.CreateApplication(null, null))
        {
          Execute(app);
        }
      }
      catch (Exception e)
      {
        Console.Error.WriteLine(e.ToString());
      }
    }

    //-----------------------------------------------------------------------------------------------
    static void PrintStartupMsg(string dataFile)
    {
      Console.WriteLine("This data mining application queries the treatment history for each patient in the Aria database and writes");
      Console.WriteLine("the results to the simple report '"+dataFile+"'.");
      Console.WriteLine("Press any key to pause processing.");
      Console.WriteLine("============================================================================================================");
      System.Threading.Thread.Sleep(2000); // let user read message for 2 seconds
    }
    //-----------------------------------------------------------------------------------------------
    static void Execute(Application app)
    {
      // open the report file
      string temp = System.Environment.GetEnvironmentVariable("TEMP");
      string dataFilePath = temp + @"\DataMiningOutput.txt";

      PrintStartupMsg(dataFilePath);

      using (StreamWriter reportFile = new StreamWriter(dataFilePath, false))
      {
        // iterate over all patients in the database
        foreach (PatientSummary patsum in app.PatientSummaries)
        {
          // check if the user has hit a key
          if (StopNow())
            break;
          // open a patient, report on the treatment history for that patient, and close the patient.
          // If there is no treatment history, nothing is reported.
          Patient pat = app.OpenPatient(patsum);
          ReportOnePatient(pat, reportFile);
          app.ClosePatient();
        }
        reportFile.Flush();
      }
    }
    //-----------------------------------------------------------------------------------------------
    static void ReportOnePatient(Patient patient, StreamWriter reportFile)
    {
      if (patient == null)
        return;
      Console.WriteLine("Processing patient " + patient.Id);

      var approvedPlans = from Course c in patient.Courses
                          from PlanSetup ps in c.PlanSetups
                          where (ps.ApprovalStatus == PlanSetupApprovalStatus.TreatmentApproved || 
                                (ps.ApprovalStatus == PlanSetupApprovalStatus.Retired && ps.Id.Contains('#')) ||
                                (ps.IsTreated))
                          select new
                          {
                            Patient = patient,
                            Course = c,
                            Plan = ps
                          };

      if (!approvedPlans.Any())
        return;
      foreach (var p in approvedPlans)
      {
        if (StopNow())
          break;
        PlanSetup ps = p.Plan;
        ReportOnePlan(patient, ps, reportFile);
      }
    }
    //-----------------------------------------------------------------------------------------------
    static void ReportOnePlan(Patient patient, PlanSetup ps, StreamWriter reportFile)
    {
      ps.DoseValuePresentation = DoseValuePresentation.Absolute;
      string msg = patient.Id + "\t" + ps.Course.Id + "\t" + ps.Id + "\t";

      msg += (ps.IsTreated ? "Treated" : "-") + "\t";

      msg += ps.ApprovalStatus.ToString() + "\t";

      DateTime dt = ps.Course.StartDateTime.HasValue ? ps.Course.StartDateTime.Value : DateTime.MinValue;
      msg += dt.Year + "\t" + dt.Month + "\t" + dt.Day + "\t";

      string normalization = ps.PlanNormalizationMethod;
      msg += normalization + "\t";

      msg += (ps.StructureSet != null && ps.StructureSet.Structures.Any()) ? ps.StructureSet.Id : " ";
      msg += "\t";

      if (ps.UniqueFractionation.NumberOfFractions.HasValue)
      {
        msg += ps.UniqueFractionation.NumberOfFractions.ToString() + "\t" + ps.UniqueFractionation.PrescribedDosePerFraction.ToString() + "\t";
        msg += (ps.UniqueFractionation.NumberOfFractions.Value * ps.UniqueFractionation.PrescribedDosePerFraction).ToString() + "\t";
      }
      else
      {
        msg += " " + "\t" + ps.UniqueFractionation.PrescribedDosePerFraction.ToString() + "\t";
        msg += " " + "\t";
      }
      msg += ps.Dose != null ? ps.Dose.DoseMax3D.ToString() : " ";
      msg += "\t";

      int numberOfSetupFields = 0;
      int numberOfTreatmentFields = 0;
      HashSet<string> accessories = new HashSet<string>();
      HashSet<string> energiesTreatment = new HashSet<string>();
      HashSet<string> machines = new HashSet<string>();

      int isocenters = 0;
      VVector isoc = new VVector(Double.NaN, Double.NaN, Double.NaN);

      foreach (Beam b in ps.Beams)
      {
        if (!b.IsSetupField)
          energiesTreatment.Add(b.EnergyModeDisplayName);

        if (b.TreatmentUnit != null)
          machines.Add(b.TreatmentUnit.Id);

        if (!Double.IsNaN(b.IsocenterPosition.x) && Double.IsNaN(isoc.x))
        {
          isoc = b.IsocenterPosition;
          isocenters = 1;
        }
        else
        {
          if ((isoc - b.IsocenterPosition).Length > 0.00000000001)
            isocenters++;
        }

        if (b.IsSetupField)
        {
          numberOfSetupFields++;
          continue;
        }
        numberOfTreatmentFields++;
        if (b.Wedges.Any())
        {
          foreach (var wedge in b.Wedges)
          {
            string wedgeTypeString = wedge.GetType().Name;
            accessories.Add(wedgeTypeString);
          }
        }
        if (b.Compensator != null)
          accessories.Add("Compensator");
        if (b.Blocks.Any())
          accessories.Add("Block");
        if (b.Applicator != null)
          accessories.Add("Applicator");
        if (b.MLC != null)
          accessories.Add("MLC-" + b.MLCPlanType.ToString());
        if (b.Boluses.Any())
          accessories.Add("Bolus");
      }

      msg += numberOfTreatmentFields.ToString() + "\t" + numberOfSetupFields.ToString() + "\t";

      msg += String.Join("/", accessories) + "\t";
      msg += String.Join("/", machines) + "\t";
      msg += String.Join("/", energiesTreatment) + "\t";
      msg += isocenters.ToString() + "\t";

      string plansumId = "-";
      foreach (PlanSum psum in ps.Course.PlanSums)
      {
        if (psum.PlanSetups.Contains(ps))
          plansumId = psum.Id;
      }
      msg += plansumId + "\t";

      DVHData dvhStat = null;
      Structure targetStructure = null;
      if (ps.Dose != null && ps.StructureSet != null)
      {
        targetStructure = FindTargetStructure(ps);
        if (targetStructure != null)
        {
          dvhStat = ps.GetDVHCumulativeData(targetStructure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.1);
        }
      }
      if (dvhStat != null)
      {
        msg += targetStructure.Id + "\t" + dvhStat.MaxDose.ToString() + "\t" + dvhStat.MeanDose.ToString() + "\t" + dvhStat.MinDose.ToString() + "\t";
      }
      else
      {
        msg += "\t \t \t \t";
      }

      if (ps.Dose != null && ps.StructureSet != null)
      {
        msg += ps.PhotonCalculationModel + " " + ps.ElectronCalculationModel + " " + ps.ProtonCalculationModel;
      }
      else
      {
        msg += "-";
      }
      reportFile.WriteLine(msg);
    }

    static string[] s_targetIds = { "ITV", "PTV", "GTV", "CTV", "BLADDER", "CORD", "BRAINSTEM", "LUNG", "LIVER", "KIDNEY", "BREAST", "OPTIC", "PAROTID", "SPINE", "RECTUM", "BOWEL", "BRAIN" };

    //-----------------------------------------------------------------------------------------------
    private static Structure FindTargetStructure(PlanSetup ps)
    {
      Structure targetStructure = ps.StructureSet.Structures.Where(s => s.Id == ps.TargetVolumeID).FirstOrDefault();
      if (targetStructure == null)
      {
        targetStructure = ps.StructureSet.Structures.Where(s => s.DicomType.EndsWith("TV")).FirstOrDefault();
        if (targetStructure == null)
        {
          foreach (string id in s_targetIds)
          {
            targetStructure = ps.StructureSet.Structures.Where(s => Regex.IsMatch(s.Id, id, RegexOptions.IgnoreCase)).FirstOrDefault();
            if (targetStructure != null) break;
          }
          if (targetStructure == null)
          {
            targetStructure = ps.StructureSet.Structures.Where(s => s.DicomType == "ORGAN").FirstOrDefault();
            if (targetStructure == null)
            {
              targetStructure = ps.StructureSet.Structures.Where(s => s.DicomType == "EXTERNAL").FirstOrDefault();
            }
          }
        }
      }
      return targetStructure;
    }

    //-----------------------------------------------------------------------------------------------
    static bool stopped = false;
    static bool StopNow()
    {
      bool keyPressed = Console.KeyAvailable;
      if (keyPressed)
      {
        Console.ReadKey();
        Console.WriteLine("\n\nPress 'Y' and ENTER if you want to stop now");
        string line = Console.ReadLine();
        stopped = line.Contains('Y') || line.Contains('y');
      }
      return stopped;
    }
  }
}
