#region copyright
////////////////////////////////////////////////////////////////////////////////
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
//////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace RapidPlanEvaluation
{
    public class MyDVHData
    {
        public DVHPoint[] CurveData;
        public DoseUnits DoseUnit;
        public VolumeUnits VolumeUnit;
        public double MeanDose;
        public double MaxDose;
        public double VoxelVolume;
        public double BinSize;
        //public double TotalVolume;

        public enum DoseUnits
        {
            Unknown = 0,
            Gy = 1,
            cGy = 2,
            Percent = 3
        }

        public enum VolumeUnits
        {
            Unknown = 0,
            cc = 1,
            Percent = 2
        }

        public class DVHPoint
        {
            public double Volume;
            public double Dose;
        }

        public void LoadDVH(Structure structure, PlanSetup plan, DoseValuePresentation dosePresentation, VolumePresentation volPresentation, bool useRapPlanEstim = false)
        {
            BinSize = Convert.ToDouble(Myconfig.GetAppKey("DVHBinSize"));
            DVHData apiDVH = null;
            EstimatedDVH upEstDVH = null;
            EstimatedDVH lowEstDVH = null;
            if (volPresentation == VolumePresentation.AbsoluteCm3)
                VolumeUnit = MyDVHData.VolumeUnits.cc;
            else if (volPresentation == VolumePresentation.Relative)
                VolumeUnit = MyDVHData.VolumeUnits.Percent;
            else
                VolumeUnit = MyDVHData.VolumeUnits.Unknown;

            if (useRapPlanEstim)
            {
                upEstDVH = plan.DVHEstimates.FirstOrDefault(s => s.Structure.Id == structure.Id && s.Type == DVHEstimateType.Upper);
                lowEstDVH = plan.DVHEstimates.FirstOrDefault(s => s.Structure.Id == structure.Id && s.Type == DVHEstimateType.Lower);

                if (upEstDVH == null || lowEstDVH == null)
                {
                    CurveData = null;
                    return;
                }

                //TODO calc these from estimated DVH
                MaxDose = 0;
                MeanDose = 0;

                DoseUnit = (MyDVHData.DoseUnits)lowEstDVH.CurveData[0].DoseValue.Unit;
                CurveData = new DVHPoint[lowEstDVH.CurveData.Count()];
                for (int i = 0; i < lowEstDVH.CurveData.Count(); i++)
                {
                    double dose = lowEstDVH.CurveData[i].DoseValue.Dose;
                    double volume = 0.5 * (lowEstDVH.CurveData[i].Volume + upEstDVH.CurveData[i].Volume);
                    CurveData[i] = new DVHPoint
                    {
                        Dose = dose,
                        Volume = volume
                    };
                }
            }
            else
            {
                apiDVH = plan.GetDVHCumulativeData(structure, dosePresentation, volPresentation, BinSize);



                if (apiDVH == null)
                {
                    MessageBox.Show("Plan " + plan.Id + " contains no dose\nCannot calculate metrics", "Invalid data", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MaxDose = apiDVH.MaxDose.Dose;
                MeanDose = apiDVH.MeanDose.Dose;

                DoseUnit = (MyDVHData.DoseUnits)apiDVH.CurveData[0].DoseValue.Unit;
                CurveData = new DVHPoint[apiDVH.CurveData.Count()];
                for (int i = 0; i < apiDVH.CurveData.Count(); i++)
                {
                    CurveData[i] = new DVHPoint
                    {

                        Dose = apiDVH.CurveData[i].DoseValue.Dose,
                        Volume = apiDVH.CurveData[i].Volume
                    };
                }
            }
        }
    }


}
