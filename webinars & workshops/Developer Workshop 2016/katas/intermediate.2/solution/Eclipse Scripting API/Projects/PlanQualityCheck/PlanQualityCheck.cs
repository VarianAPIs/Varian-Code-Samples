////////////////////////////////////////////////////////////////////////////////
// PlanQualityCheck.cs
//
//  A ESAPI v11+ script that checks if the (Lung)plan dosimetry data meet the criterias and generate check result report in a csv file.
//   
//      
// Kata Intermediate.2:	
//      a) Extract D95 on PTV, MLD, V20 on total lung, V40 on heart, Dmax on spinal cord
//      b) Check if the dosimetry data meet the planning goal, then generate report in CSV file 
//  	
//
// Applies to:
//      Eclipse Scripting API
//      v11, 13, 13.5, 13.6, 13.7, 15.0
//
// Copyright (c) 2016 Varian Medical Systems, Inc.
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

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            // TODO : Add here your code that is called when the script is launched from Eclipse
            Patient patient = context.Patient;
            if (context.PlanSetup == null)
            {
                MessageBox.Show("Please load a patient plan before the calculation.", "Volume Calculation Results");
                return;
            }
            if (!context.PlanSetup.IsDoseValid)
            {
                MessageBox.Show("This plan has no dose calculated, Please load plan with 3D dose.", "Volume Calculation Results");
                return;
            }
            // get list of structures for loaded plan
            var listStructures = context.StructureSet.Structures;

            // search for target structure(PTV in this excercise)
            Structure ptv = listStructures.Where(x => !x.IsEmpty && x.Id.ToUpper().Contains("PTV")).FirstOrDefault();
            if (ptv == null)
            {
                MessageBox.Show("PTV is empty or not created for this plan.");
                return;
            }
            // calculate D95 on ptv
            double d95PTV = context.PlanSetup.GetDoseAtVolume(ptv, 95, VolumePresentation.Relative, DoseValuePresentation.Absolute).Dose;

            // search for total lung
            Structure totalLung = listStructures.Where(x => !x.IsEmpty && x.Id.ToUpper().Contains("TOTAL LUNG")).FirstOrDefault();
            if (totalLung == null)
            {
                MessageBox.Show("Total lung contour is empty or not created for this plan.");
                return;
            }
            // calculate V20 on total lung
            double v20Lung = context.PlanSetup.GetVolumeAtDose(totalLung, new DoseValue(20, DoseValue.DoseUnit.Gy), VolumePresentation.Relative);
            DVHData dvhLung = context.PlanSetup.GetDVHCumulativeData(totalLung, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.1);
            double meanLungDose = dvhLung.MeanDose.Dose;

            // search for Heart
            Structure heart = listStructures.Where(x => !x.IsEmpty && x.Id.ToUpper().Contains("HEART")).FirstOrDefault();
            if (heart == null)
            {
                MessageBox.Show("Heart contour is empty or not created for this plan.");
                return;
            }
            // calculate V40 on heart
            double v40Heart = context.PlanSetup.GetVolumeAtDose(heart, new DoseValue(40, DoseValue.DoseUnit.Gy), VolumePresentation.Relative);

            // search for spinal cord
            Structure cord = listStructures.Where(x => !x.IsEmpty && x.Id.ToUpper().Contains("CORD")).FirstOrDefault();
            if (cord == null)
            {
                MessageBox.Show("Spinal cord contour is empty or not created for this plan.");
                return;
            }
            // calculate Dmax on cord
            DVHData dvhCord = context.PlanSetup.GetDVHCumulativeData(cord, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.1);
            double dmaxCord = dvhCord.MaxDose.Dose;

            // search for esophagus
            Structure esoph = listStructures.Where(x => !x.IsEmpty && x.Id.ToUpper().Contains("ESOPHAGUS")).FirstOrDefault();
            if (esoph == null)
            {
                MessageBox.Show("Esophagus contour is empty or not created for this plan.");
                return;
            }
            // calculate V60 on esophagus
            double v60Esoph = context.PlanSetup.GetVolumeAtDose(esoph, new DoseValue(60, DoseValue.DoseUnit.Gy), VolumePresentation.Relative);
            // calculate Dmax 
            DVHData dvhEsoph = context.PlanSetup.GetDVHCumulativeData(esoph, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.1);
            double dmaxEsoph = dvhEsoph.MaxDose.Dose;

            // write the check results into csv file
            string filename = @"c:\temp\PlanQualityCheck_i2.csv";

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filename, false, Encoding.ASCII))
            {
                // header for the report
                sw.WriteLine(string.Format("Patient ID:, {0}", patient.Id));
                sw.WriteLine(string.Format("Plan ID:, {0}", context.PlanSetup.Id));
                sw.WriteLine("");
                sw.WriteLine("Structure, Parameters, Dosimetry Data, Criteria, Results");
                //get prescription dose
                var presDose = context.PlanSetup.TotalDose.Dose;
                // write the check results for each structures.
                string strPTV = string.Format("{0},D95(Gy),{1:0.000},>={2} Gy,{3}", ptv.Id, d95PTV, presDose, (d95PTV >= presDose) ? "Pass" : "Fail");
                sw.WriteLine(strPTV);
                string strLung = string.Format("{0},MLD(Mean Lung Dose),{1:0.000},< 20 Gy,{2}", totalLung.Id, meanLungDose, (meanLungDose < 20.0) ? "Pass" : "Fail");
                sw.WriteLine(strLung);
                string strLung2 = string.Format("{0},V20(%),{1:0.0}, < 40 % ,{2}", totalLung.Id, v20Lung, (v20Lung < 40.0) ? "Pass" : "Fail");
                sw.WriteLine(strLung2);
                string strCord = string.Format("{0},Dmax(Gy),{1:0.000},< 45 Gy,{2}", cord.Id, dmaxCord, (dmaxCord < 45.0) ? "Pass" : "Fail");
                sw.WriteLine(strCord);
                string strHeart = string.Format("{0},V40(%),{1:0.0},< 50%,{2}", heart.Id, v40Heart, (v40Heart < 50.0) ? "Pass" : "Fail");
                sw.WriteLine(strHeart);
                string strEsoph = string.Format("{0},V60(%),{1:0.0},< 50%,{2}", esoph.Id, v60Esoph, (v60Esoph < 50.0) ? "Pass" : "Fail");
                sw.WriteLine(strEsoph);
                string strEsoph2 = string.Format("{0},Dmax(Gy),{1:0.000},< 75 Gy,{2}", esoph.Id, dmaxEsoph, (dmaxEsoph < 75.0) ? "Pass" : "Fail");
                sw.WriteLine(strEsoph2);


                sw.Flush();
                sw.Close();

                var msg = string.Format(@"check report has been written to {0}", filename);
                MessageBox.Show(msg, "Plan Quality Check");
            }

        }
    }
}
