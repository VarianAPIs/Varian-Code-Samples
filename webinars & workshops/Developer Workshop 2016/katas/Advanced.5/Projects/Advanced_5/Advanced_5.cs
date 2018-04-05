////////////////////////////////////////////////////////////////////////////////
// Advanced_5.cs - Automated VMAT Planning
//
//  A ESAPI v11+ script that demonstrates DVH extraction.
//
// Kata Advanced.5)    
//  Checks that structures IDs correspond to user defined standard values. Creates target optimization structures and dose limiting annuli of to tune optimization.
//  Adds course and treatment plan, checking first if there are pre-existing objects
//  Adds VMAT beams, setting postion to middle of PTV_High structure, rounding to the nearest cm
//  Sets optimizaiton constraints
//  Carries out VMAT optimization, dose calculation , and demonstrates displaying DVH metrics
//
// Applies to:
//      Eclipse Scripting API
//      v11, 13, 13.5, 13.6, 13.7, 15.0
//
// Copyright (c) 2016 Charles Mayo, University of Michigan
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

// TODO: uncomment the line below if the script requires write access.
 [assembly: ESAPIScript(IsWriteable = true)]

namespace Advanced_5
{
  class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      try
      {
        using (Application app = Application.CreateApplication())
        {
          Execute(app);

        }
      }
      catch (Exception e)
      {
        Console.Error.WriteLine(e.ToString());
        System.Windows.MessageBox.Show(e.ToString());
        Console.WriteLine();
        Console.WriteLine("Press any key to continue");
        Console.ReadKey();
      }
    }
    static void Execute(Application app)
    {
      // TODO: add here your code


        Patient curpat = app.OpenPatientById("002441");
        Course curcourse = curpat.Courses.Where(x => x.Id == "advanced.3").Single();
        //ExternalPlanSetup cureps = curcourse.ExternalPlanSetups.Where(x => x.Id == "NCPTestScript").Single();
        StructureSet curstructset = curpat.StructureSets.Where(x => x.Id == "StdNomenclature").First();

            
        //Create a list of Tuples to pass target information:Target Type,Structure ID, Minimum dose constraint, dose units (Gy v cGy)
        List<Tuple<string, string, float, string>> targetstructureconstraints = new List<Tuple<string, string, float, string>>();


        targetstructureconstraints.Add(new Tuple<string, string, float, string>("PTV_High", "PTV_6800", 68.0f, "Gy"));//This maps the contoured PTV_6800 structure to the generic PTV_High type and assigns prescribed dose
        targetstructureconstraints.Add(new Tuple<string, string, float, string>("PTV_Low", "PTV_5600", 56.0f, "Gy"));//This maps the contoured PTV_5600 structure to the generic PTV_Low type and assigns prescribed dose

        List<string> listallowednontargetStructures = new List<string>() { "Rectum", "Bladder", "Femur_Head_L", "Femur_Head_R", "Femur_Heads", "LN", "PenileBulb" };

        Planning(app, curpat, curstructset, targetstructureconstraints, listallowednontargetStructures, 68,34);
    }

    static void Planning(Application curapp, Patient curpat, StructureSet curstructset, List<Tuple<string, string, float, string>> TargetStructures, List<string> AllowedNonTargetStructures, float RxDose, int NFractions)
    {
        curpat.BeginModifications();

        string IDofptv_low = string.Empty;
        string IDofptv_high = string.Empty; //

        StringBuilder sb = new StringBuilder();

        //Check structure nameing
        sb.AppendLine("Check Structure Naming");
        sb.AppendLine("Structure ID        \tIs Standard Name?");
        foreach (Structure curstruct in curstructset.Structures.OrderBy(x=>x.Id))
        {
            if (curstruct.DicomType == "PTV" | curstruct.DicomType == "CTV" | curstruct.DicomType == "GTV")
            {
                if (TargetStructures.Where(x => x.Item1 == curstruct.Id).Any() || TargetStructures.Where(x => curstruct.DicomType.ToString() + "_" + x.Item3 + (x.Item4 == "Gy" ? "00" : string.Empty) == curstruct.Id).Any()) sb.AppendLine(curstruct.Id.PadRight(curstruct.Id.Length < 20 ? 20 - curstruct.Id.Length : 0) + "\tYes");
                else sb.AppendLine(curstruct.Id.PadRight(curstruct.Id.Length < 20 ? 20 - curstruct.Id.Length : 0) + "\tNo");

            }
            else
            {
                if (AllowedNonTargetStructures.Where(x => x == curstruct.Id).Any() || curstruct.Id.ToString().StartsWith("z")) sb.AppendLine(curstruct.Id.PadRight(curstruct.Id.Length < 20 ? 20 - curstruct.Id.Length : 0) + "\tYes");
                else sb.AppendLine(curstruct.Id.PadRight(curstruct.Id.Length < 20 ? 20 - curstruct.Id.Length : 0) + "\tNo");
            }
        }
        sb.AppendLine();
        sb.AppendLine("Press OK to continue with creating optimization structures");
        System.Windows.MessageBox.Show(sb.ToString());



        //Create optimization structures
        if (TargetStructures.Where(x => x.Item1 == "PTV_Low").Any()) IDofptv_low = TargetStructures.Where(x => x.Item1 == "PTV_Low").Select(x => x.Item2).First(); //Get the ID of the structure identified as PTV_Low
        if (TargetStructures.Where(x => x.Item1 == "PTV_High").Any()) IDofptv_high = TargetStructures.Where(x => x.Item1 == "PTV_High").Select(x => x.Item2).First();//Get the ID of the structure identified as PTV_High

        Structure ptv_low = null;
        Structure ptv_high = null;


        //Check that PT_High structure exists, issue warning if it does not. 
        if (curstructset.Structures.Where(x => x.Id == IDofptv_low).Any()) ptv_low = curstructset.Structures.Where(x => x.Id == IDofptv_low).First();
        else System.Windows.MessageBox.Show("Did not find a PTV_High Structure. Fix before proceeding");

        //Check that PT_Low structure exists, issue warning if it does not.
        if (curstructset.Structures.Where(x => x.Id == IDofptv_high).Any()) ptv_high = curstructset.Structures.Where(x => x.Id == IDofptv_high).First();
        else System.Windows.MessageBox.Show("Did not find a PTV_Low Structure. Fix before proceeding");


        //Creation of optimization structures. If a copy already exitst delete it first. 
        
        //Optimization structure for the PTV_Low volume is named zPTV_Low^Opt
        if (curstructset.Structures.Where(x => x.Id.Contains("zPTV_Low^Opt")).Any()) curstructset.RemoveStructure(curstructset.Structures.Where(x => x.Id.Contains("zPTV_Low^Opt")).First());
        Structure zptvlowopt = curstructset.AddStructure("ORGAN", "zPTV_Low^Opt");

        //Dose limiting annulus (DLA) structure is used to make the prescribed dose conformal. DLA for PTV_Low is named zDLA_Low
        if (curstructset.Structures.Where(x => x.Id.Contains("zDLA__Low")).Any()) curstructset.RemoveStructure(curstructset.Structures.Where(x => x.Id.Contains("zDLA__Low")).First());
        Structure zdlalow = curstructset.AddStructure("ORGAN", "zDLA__Low");

        //Optimization structure for the PTV_High volume is named zPTV_High^Opt
        if (curstructset.Structures.Where(x => x.Id.Contains("zPTV_High^Opt")).Any()) curstructset.RemoveStructure(curstructset.Structures.Where(x => x.Id.Contains("zPTV_High^Opt")).First());
        Structure zptvhighopt = curstructset.AddStructure("ORGAN", "zPTV_High^Opt");
       

        //Dose limiting annulus (DLA) structure is used to make the prescribed dose conformal. DLA for PTV_High is named zDLA_High
        if (curstructset.Structures.Where(x => x.Id.Contains("zDLA__High")).Any()) curstructset.RemoveStructure(curstructset.Structures.Where(x => x.Id.Contains("zDLA__High")).First());
        Structure zdlahigh = curstructset.AddStructure("ORGAN", "zDLA__High");

        //Make zPTV_High^Opt from PTV_High and boolean out the rectum
        zptvhighopt.SegmentVolume = ptv_high.Margin(0.0f);
        zptvhighopt.SegmentVolume = zptvhighopt.Sub(curstructset.Structures.Where(x => x.Id.Contains("Rectum")).Single());//Boolean the Rectum out of the high dose ptv optimization structure


        //Make zPTV_Low^Opt from PTV_Low and boolean out the PTV_High structure
        zptvlowopt.SegmentVolume = ptv_low.Margin(0.0f);
        zptvlowopt.SegmentVolume = zptvlowopt.Sub(ptv_high.Margin(1.0f));//Boolean the ptv_high out of ptv_low optimization structure


        //Make a dose limiting annulus arround the low dose ptv optimization structure
        zdlalow.SegmentVolume = zptvlowopt.SegmentVolume;
        zdlalow.SegmentVolume = zdlalow.Margin(10.0f);
        zdlalow.SegmentVolume = zdlalow.Sub(zptvlowopt.Margin(1.0f));
        zdlalow.SegmentVolume = zdlalow.Sub(zptvhighopt.Margin(5.0f));

        //Make a dose limiting annulus arround the high dose ptv optimization structure 
        zdlahigh.SegmentVolume = zptvhighopt.SegmentVolume;
        zdlahigh.SegmentVolume = zdlahigh.Margin(10.0f);
        zdlahigh.SegmentVolume = zdlahigh.Sub(zptvhighopt.Margin(1.0f));

        sb = new StringBuilder();
        sb.AppendLine("Done with creating optimization strutures");
        sb.AppendLine("Click OK to proceed with setting up course and VMAT plan");
        System.Windows.MessageBox.Show(sb.ToString());

        //Add course
        Course curcourse;
        if (curpat.Courses.Where(x => x.Id == "AutoPlan").Any()) curcourse = curpat.Courses.Where(x => x.Id == "AutoPlan").Single();
        else
        {
            curcourse = curpat.AddCourse();
            curcourse.Id = "AutoPlan";
        }

        //Remove PlanSetup if it exists then create new plan setup

        if (curcourse.PlanSetups.Where(x => x.Id == "AutoPlanVMAT").Any()) curcourse.RemovePlanSetup(curcourse.PlanSetups.Where(x => x.Id == "AutoPlanVMAT").Single());
        ExternalPlanSetup cureps = curcourse.AddExternalPlanSetup(curstructset);
        cureps.Id = "AutoPlanVMAT";




        //Add VMAT Beams
        VVector isocenter = new VVector(Math.Round(ptv_high.CenterPoint.x / 10.0f) * 10.0f, Math.Round(ptv_high.CenterPoint.y / 10.0f) * 10.0f, Math.Round(ptv_high.CenterPoint.z / 10.0f) * 10.0f);
        ExternalBeamMachineParameters ebmp = new ExternalBeamMachineParameters("Truebeam", "6X", 600, "ARC", null);
        Beam VMAT1 = cureps.AddArcBeam(ebmp, new VRect<double>(-100, -100, 100, 100), 30, 181, 179, GantryDirection.Clockwise, 0, isocenter);
        Beam VMAT2 = cureps.AddArcBeam(ebmp, new VRect<double>(-100, -100, 100, 100), 330, 179, 181, GantryDirection.CounterClockwise, 0, isocenter);

        VMAT1.Id = "CW";
        VMAT2.Id = "CCW";

        VMAT1.FitCollimatorToStructure(new FitToStructureMargins(10), ptv_low, true, true, false);
        VMAT2.FitCollimatorToStructure(new FitToStructureMargins(10), ptv_low, true, true, false);



        cureps.SetCalculationModel(CalculationType.PhotonVMATOptimization, "PO_15014");
        cureps.SetPrescription(NFractions, new DoseValue(RxDose / NFractions, "Gy"), 1);

        curapp.SaveModifications();

        sb = new StringBuilder();
        sb.AppendLine("Done with setting up course and VMAT plan");
        sb.AppendLine("Click OK to proceed with plan optimization");
        System.Windows.MessageBox.Show(sb.ToString());

        float doseobjectivevalue_low = TargetStructures.Where(x => x.Item1 == "PTV_Low").Select(x => x.Item3).First();
        float doseobjectivevalue_high = TargetStructures.Where(x => x.Item1 == "PTV_High").Select(x => x.Item3).First();


        cureps.OptimizationSetup.AddPointObjective(zptvlowopt, OptimizationObjectiveOperator.Lower, new DoseValue(doseobjectivevalue_low, "Gy"), 100, 100);
        cureps.OptimizationSetup.AddPointObjective(zptvlowopt, OptimizationObjectiveOperator.Upper, new DoseValue(doseobjectivevalue_low+3.0f, "Gy"), 30, 50);
        cureps.OptimizationSetup.AddPointObjective(zdlalow, OptimizationObjectiveOperator.Upper, new DoseValue(doseobjectivevalue_low, "Gy"), 0, 50);

        cureps.OptimizationSetup.AddPointObjective(zptvhighopt, OptimizationObjectiveOperator.Lower, new DoseValue(doseobjectivevalue_high, "Gy"), 100, 120);
        cureps.OptimizationSetup.AddPointObjective(zptvhighopt, OptimizationObjectiveOperator.Upper, new DoseValue(doseobjectivevalue_high+2.0f,"Gy"), 0, 100);
        cureps.OptimizationSetup.AddPointObjective(zdlahigh, OptimizationObjectiveOperator.Upper, new DoseValue(doseobjectivevalue_high, "Gy"), 0, 50);

        cureps.OptimizationSetup.AddNormalTissueObjective(80.0f, 0.0f, 100.0f, 40.0f, 0.05f);
        OptimizerResult optresult=cureps.OptimizeVMAT(new OptimizationOptionsVMAT(OptimizationIntermediateDoseOption.NoIntermediateDose, string.Empty));



        sb = new StringBuilder();
        sb.AppendLine("VMAT optimization is done");
        sb.AppendLine("NIterattions:" + optresult.NumberOfIMRTOptimizerIterations.ToString() + " , ObjectiveFunctionValue: " + optresult.TotalObjectiveFunctionValue.ToString());
        sb.AppendLine("Click OK to proceed with optimization");

        cureps.OptimizeVMAT();

        curapp.SaveModifications();

        sb = new StringBuilder();
        sb.AppendLine("Done with optimization");
        sb.AppendLine("Click OK to proceed with dose calculation");
        System.Windows.MessageBox.Show(sb.ToString());

        cureps.CalculateDose();
        //cureps.PlanNormalizationValue = 99.0f;

        curapp.SaveModifications();
        sb = new StringBuilder();
        sb.AppendLine("Done with dose calculation");

        if (cureps.StructureSet.Structures.Where(x => x.Id=="Rectum").Any())
        {
            Structure reportstructure = cureps.StructureSet.Structures.Where(x => x.Id =="Rectum").First();
            sb.AppendLine("Rectum:V65Gy[%]:" + cureps.GetVolumeAtDose(reportstructure, new DoseValue(65.0f, DoseValue.DoseUnit.Gy), VolumePresentation.Relative).ToString());  
        }
    
        sb.AppendLine("PTV_High:D95%[Gy]:" + cureps.GetDoseAtVolume(ptv_high,95.0f, VolumePresentation.Relative,DoseValuePresentation.Absolute).ToString());

       
        sb.AppendLine("Click OK to finish script");
        System.Windows.MessageBox.Show(sb.ToString());
    }
  }
}
