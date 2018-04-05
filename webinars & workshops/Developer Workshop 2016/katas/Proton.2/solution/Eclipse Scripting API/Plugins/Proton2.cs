////////////////////////////////////////////////////////////////////////////////
// Proton2.cs
//
// proton.2)
//  Starting from the point of completing the Proton.1 task.  Write a function 
//  that gets the dose at a given volume from the Plan and Uncertainty plan 
//  curves.  You should use GetDoseAtVolume as a guide.  Then for the 
//  structure "CTV_5200" use that function to display the Min, Max and 
//  Average D95%[cGy] for the plan and uncertainty plans collection.  
//
//  Patient ABDOMEN, TG244 has a proton plan called Proton Uncert in course 
//  C2 that can be used for testing and solution verification.
//
// Applies to:
//      Eclipse Scripting API
//          13.7, 15.0,15.1
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

        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            Structure ST = context.StructureSet.Structures.Where(x => x.Id == "CTV_5200").First();
            List<curve> Uncertainty_Curves = new List<curve>();

            string msg = "Not a proton Plan";
            if (context.PlanSetup.PlanType.ToString() == "ExternalBeam_Proton")
            {
                Uncertainty_Curves = GetDVHDataWithUncertainties(context.IonPlanSetup, ST, VolumePresentation.Relative, DoseValuePresentation.Relative, 0.1);
                List<double> D95percent_percent = new List<double>();
                foreach (curve UC in Uncertainty_Curves)
                {
                    D95percent_percent.Add(GetDoseAtVolumeForUncertainties(UC, 95.0, VolumePresentation.Relative, DoseValuePresentation.Absolute));
                }
                msg = string.Format("Minimum D95%[cGy] = {0:0.00}, Maximum D95%[cGy] = {1:0.00}, AverageD95%[cGy] = {2:0.00}", D95percent_percent.Min(), D95percent_percent.Max(), D95percent_percent.Average());
            }
            MessageBox.Show(msg);
        }

        public double GetDoseAtVolumeForUncertainties(curve DVH, double Volume, VolumePresentation VolPres, DoseValuePresentation RequestedDosePresentation)
        {
            double dose;

            if (DVH.VolumePresentation == VolumePresentation.AbsoluteCm3 && VolPres == VolumePresentation.Relative)
            {
                Volume = Volume * DVH.AbsoluteStructureVolume / 100.0;
            }

            if (DVH.VolumePresentation == VolumePresentation.Relative && VolPres == VolumePresentation.AbsoluteCm3)
            {
                Volume = Volume / DVH.AbsoluteStructureVolume * 100.0;
            }

            int ind1 = DVH.CurveData.Select(x => x.Volume).Count() - DVH.CurveData.Where(x => x.Volume < Volume).Select(x => x.Volume).Count() - 1;
            int ind2 = DVH.CurveData.Select(x => x.Volume).Count() - DVH.CurveData.Where(x => x.Volume < Volume).Select(x => x.Volume).Count();

            DVHPoint DVP1 = DVH.CurveData.ElementAt(ind1);
            DVHPoint DVP2 = DVH.CurveData.ElementAt(ind2);

            dose = Interpolate(DVP1.Volume, DVP2.Volume, DVP1.DoseValue.Dose, DVP2.DoseValue.Dose, Volume);

            if (DVH.DoseValuePresentation == DoseValuePresentation.Absolute && RequestedDosePresentation == DoseValuePresentation.Relative)
            {
                dose = dose / DVH.PrescribedDose * 100.0;
            }

            if (DVH.DoseValuePresentation == DoseValuePresentation.Relative && RequestedDosePresentation == DoseValuePresentation.Absolute)
            {
                dose = dose * DVH.PrescribedDose / 100.0;
            }

            return dose;
        }

        public double GetVolumeAtDoseForUncertainties(curve DVH, double input_dose, VolumePresentation RequestedVolPres, DoseValuePresentation DosePresentation)
        {
            double return_volume;

            if (DVH.DoseValuePresentation == DoseValuePresentation.Absolute && DosePresentation == DoseValuePresentation.Relative)
            {
                input_dose = input_dose / 100.0 * DVH.PrescribedDose;
            }

            if (DVH.DoseValuePresentation == DoseValuePresentation.Relative && DosePresentation == DoseValuePresentation.Absolute)
            {
                input_dose = input_dose / DVH.PrescribedDose * 100.0;
            }


            int ind1 = DVH.CurveData.Select(x => x.DoseValue.Dose).Count() - DVH.CurveData.Where(x => x.DoseValue.Dose > input_dose).Select(x => x.DoseValue.Dose).Count() - 1;
            int ind2 = DVH.CurveData.Select(x => x.DoseValue.Dose).Count() - DVH.CurveData.Where(x => x.DoseValue.Dose > input_dose).Select(x => x.DoseValue.Dose).Count();

            DVHPoint DVP1 = DVH.CurveData.ElementAt(ind1);
            DVHPoint DVP2 = DVH.CurveData.ElementAt(ind2);

            return_volume = Interpolate(DVP1.DoseValue.Dose, DVP2.DoseValue.Dose, DVP1.Volume, DVP2.Volume, input_dose);

            if (DVH.VolumePresentation == VolumePresentation.AbsoluteCm3 && RequestedVolPres == VolumePresentation.Relative)
            {
                return_volume = return_volume / DVH.AbsoluteStructureVolume * 100.0;
            }

            if (DVH.VolumePresentation == VolumePresentation.Relative && RequestedVolPres == VolumePresentation.AbsoluteCm3)
            {
                return_volume = return_volume / 100.0 * DVH.AbsoluteStructureVolume;
            }

            return return_volume;
        }

        /// <summary>
        /// Interpolates between b1 and b2 given A between a1 and a2
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <param name="A"></param>
        /// <returns></returns>
        public double Interpolate(double a1, double a2, double b1, double b2, double A)
        {
            double result = ((a1 - A) / (a1 - a2)) * Math.Abs(b1 - b2) + b1;
            return result;
        }

        /// <summary>
        /// Function used to group Plan DVH with Uncertainty DVHs and retun the set as a list of curves.
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="ST"></param>
        /// <returns></returns>
        public List<curve> GetDVHDataWithUncertainties(IonPlanSetup IP, Structure ST, VolumePresentation VP, DoseValuePresentation DP, double BinWidth)
        {
            List<curve> Curves = new List<curve>();

            curve temp_curve = new curve();
            temp_curve.Id = "U0";
            temp_curve.DoseValuePresentation = DP;
            temp_curve.VolumePresentation = VP;
            temp_curve.StructureName = ST.Id;
            temp_curve.AbsoluteStructureVolume = ST.Volume;
            temp_curve.PrescribedDose = IP.TotalDose.Dose / IP.TreatmentPercentage;
            temp_curve.CurveData = IP.GetDVHCumulativeData(ST, temp_curve.DoseValuePresentation, temp_curve.VolumePresentation, BinWidth).CurveData;
            Curves.Add(temp_curve);


            if (IP.PlanUncertainties.Count() != 0)
            {
                foreach (PlanUncertainty PU in IP.PlanUncertainties)
                {
                    try
                    {
                        temp_curve = new curve();
                        temp_curve.Id = PU.Id;
                        temp_curve.IsocenterShift = PU.IsocenterShift;
                        temp_curve.DoseValuePresentation = DP;
                        temp_curve.VolumePresentation = VP;
                        temp_curve.StructureName = ST.Id;
                        temp_curve.AbsoluteStructureVolume = ST.Volume;
                        temp_curve.PrescribedDose = IP.TotalDose.Dose / IP.TreatmentPercentage;
                        temp_curve.CalibrationCurveError = PU.CalibrationCurveError;
                        temp_curve.DisplayName = PU.DisplayName;
                        temp_curve.CurveData = PU.GetDVHCumulativeData(ST, temp_curve.DoseValuePresentation, temp_curve.VolumePresentation, BinWidth).CurveData;
                        Curves.Add(temp_curve);
                    }
                    catch
                    {

                    }

                }
            }

            return Curves;
        }
    }

    public class curve
    {
        public DVHPoint[] CurveData { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public VVector IsocenterShift { get; set; }
        public double CalibrationCurveError { get; set; }
        public DoseValuePresentation DoseValuePresentation { get; set; }
        public VolumePresentation VolumePresentation { get; set; }
        public string StructureName { get; set; }
        public double AbsoluteStructureVolume { get; set; }
        public double PrescribedDose { get; set; }
    }
}
