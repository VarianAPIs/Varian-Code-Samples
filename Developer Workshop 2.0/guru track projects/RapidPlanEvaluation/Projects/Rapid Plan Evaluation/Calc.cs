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

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows;

namespace RapidPlanEvaluation
{
    public class Calc
    {
        public static double DMean(Structure structure, PlanSetup plan, VMMetric metric, bool useRapPlanEstimate)
        {
            MyDVHData cumDVH = new MyDVHData();
            cumDVH.LoadDVH(structure, plan, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, useRapPlanEstimate);


            if (cumDVH != null)
                return cumDVH.MeanDose;
            else
                return 0;
        }

        public static double DMax(Structure structure, PlanSetup plan, VMMetric metric, bool useRapPlanEstimate)
        {
            MyDVHData cumDVH = new MyDVHData();
            cumDVH.LoadDVH(structure, plan, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, useRapPlanEstimate);
            if (cumDVH != null)
                return cumDVH.MaxDose;
            else
                return 0;
        }

        public static double D0_1cc(Structure structure, PlanSetup plan, VMMetric metric, bool useRapPlanEstimate)
        {
            return GetDoseToVolume(structure, plan, 0.1, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, metric, useRapPlanEstimate);
        }

        public static double D95p(Structure structure, PlanSetup plan, VMMetric metric, bool useRapPlanEstimate)
        {
            return GetDoseToVolume(structure, plan, 95, DoseValuePresentation.Absolute, VolumePresentation.Relative, metric, useRapPlanEstimate);
        }

        public static double D5p(Structure structure, PlanSetup plan, VMMetric metric, bool useRapPlanEstimate)
        {
            return GetDoseToVolume(structure, plan, 5, DoseValuePresentation.Absolute, VolumePresentation.Relative, metric, useRapPlanEstimate);
        }

        public static double V33p(Structure structure, PlanSetup plan, VMMetric metric, bool useRapPlanEstimate)
        {
            return GetVolumeWithDose(structure, plan, 33, DoseValuePresentation.Relative, VolumePresentation.AbsoluteCm3, metric, useRapPlanEstimate);
        }

        public static double NTCP(Structure structure, PlanSetup plan, VMMetric metric, bool useRapPlanEstimate)
        {
            MyDVHData StdDVH = new MyDVHData();
            StdDVH.LoadDVH(structure, plan, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, useRapPlanEstimate);

            if (StdDVH.CurveData == null)
            {
                //missing upper or lower dvh estimate
                return 0;
            }
            //Convert DHV to direct
            MyDVHData.DVHPoint[] dirDVH = new MyDVHData.DVHPoint[StdDVH.CurveData.Count()];
            dirDVH[dirDVH.Count() - 1] = new MyDVHData.DVHPoint
            {
                Dose = StdDVH.CurveData[StdDVH.CurveData.Count() - 1].Dose,
                Volume = StdDVH.CurveData[StdDVH.CurveData.Count() - 1].Volume
            };

            for (int i = 0; i < StdDVH.CurveData.Count() - 1; i++)
            {
                dirDVH[i] = new MyDVHData.DVHPoint
                {
                    Dose = StdDVH.CurveData[i].Dose,
                    Volume = StdDVH.CurveData[i].Volume - StdDVH.CurveData[i + 1].Volume
                };
            }

            //Biocorrect DVH
            foreach (var point in dirDVH)
            {
                point.Dose = BioDose.bioCorrectEQD2(point.Dose, plan, structure, metric.NTCPParameters.AlphaBeta);
            }

            double EUD_a = 1.0 / metric.NTCPParameters.LKBn;

            double sum_bEUD = 0;
            for (int i = 0; i < dirDVH.Count(); i++)
            {
                double tEUD = (dirDVH[i].Volume / (structure.Volume)) * Math.Pow(dirDVH[i].Dose, EUD_a);
                sum_bEUD = sum_bEUD + tEUD;
            }

            double gEUD = Math.Pow(sum_bEUD, metric.NTCPParameters.LKBn);
            //NORMSDIST((gEUD-lkb_d50)/(lkb_m*lkb_d50))
            double NTCPOPER = (gEUD - metric.NTCPParameters.LKBD50) / (metric.NTCPParameters.LKBm * metric.NTCPParameters.LKBD50);
            if (NTCPOPER > 4.89)//ANYTHING OVER THIS RESULTS IN AN NTCP OF 100%, BUT CAN GIVE THE STATS PACKAGE AN ERROR
            {
                NTCPOPER = 4.89;
            }
            return 100 * Statistics.NormSDist(NTCPOPER);
        }

        public static double CustomMetric(Structure structure, VMMetric metric, PlanSetup plan, bool useRapPlanEstimate)
        {

            CustomMetricsConfiguration config = Myconfig.GetCustomMetricsSection();
            foreach (CustomMetricElement custMetric in config.Metrics)
                if (metric.Name == custMetric.Name)
                {
                    if (custMetric.Base == "D#%(Gy)")
                        return GetDoseToVolume(structure, plan, custMetric.Parameter, DoseValuePresentation.Absolute, VolumePresentation.Relative, metric, useRapPlanEstimate);
                    else if (custMetric.Base == "D#%(%)")
                        return GetDoseToVolume(structure, plan, custMetric.Parameter, DoseValuePresentation.Relative, VolumePresentation.Relative, metric, useRapPlanEstimate);
                    else if (custMetric.Base == "D#cc(Gy)")
                        return GetDoseToVolume(structure, plan, custMetric.Parameter, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, metric, useRapPlanEstimate);
                    else if (custMetric.Base == "D#cc(%)")
                        return GetDoseToVolume(structure, plan, custMetric.Parameter, DoseValuePresentation.Relative, VolumePresentation.AbsoluteCm3, metric, useRapPlanEstimate);
                    else if (custMetric.Base == "V#%(cc)")
                        return GetVolumeWithDose(structure, plan, custMetric.Parameter, DoseValuePresentation.Relative, VolumePresentation.AbsoluteCm3, metric, useRapPlanEstimate);
                    else if (custMetric.Base == "V#%(%)")
                        return GetVolumeWithDose(structure, plan, custMetric.Parameter, DoseValuePresentation.Relative, VolumePresentation.Relative, metric, useRapPlanEstimate);
                    else if (custMetric.Base == "V#Gy(cc)")
                        return GetVolumeWithDose(structure, plan, custMetric.Parameter, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, metric, useRapPlanEstimate);
                    else if (custMetric.Base == "V#Gy(%)")
                        return GetVolumeWithDose(structure, plan, custMetric.Parameter, DoseValuePresentation.Absolute, VolumePresentation.Relative, metric, useRapPlanEstimate);
                }

            //not one of the defined custom metric bases
            return -1;
        }

        //-----------------------------------------------------------------------------------------------
        // Different combination of absolute/relative dose/volume metrics

        private static double GetDoseToVolume(Structure structure, PlanSetup plan, double vol, DoseValuePresentation dosePresentation, VolumePresentation volPresentation, VMMetric metric, bool useRapPlanEstimate)
        {
            double dose = 0;

            MyDVHData dvh = new MyDVHData();
            dvh.LoadDVH(structure, plan, dosePresentation, volPresentation, useRapPlanEstimate);

            if (dvh != null)
            {
                if (dvh.CurveData == null)
                {
                    //missing upper or lower dvh estimate
                    return dose;
                }
                MyDVHData.DVHPoint[] points = dvh.CurveData;
                for (int i = 0; i < points.Count(); i++)
                {
                    if (points[i].Volume < vol)
                    {
                        if (i == 0)
                        {
                            dose = points[0].Dose;
                            break;
                        }
                        //interpolate dose
                        dose = points[i - 1].Dose + (points[i].Dose - points[i - 1].Dose) *
                            (vol - points[i - 1].Volume) / (points[i].Volume - points[i - 1].Volume);
                        break;
                    }
                }

                return dose;

            }
            else
                return 0;
        }


        private static double GetVolumeWithDose(Structure structure, PlanSetup plan, double dose, DoseValuePresentation dosePresentation, VolumePresentation volPresentation, VMMetric metric, bool useRapPlanEstimate)
        {
            double volume = 0;

            MyDVHData dvh = new MyDVHData();
            dvh.LoadDVH(structure, plan, dosePresentation, volPresentation, useRapPlanEstimate);

            if (dvh != null)
            {
                if (dvh.CurveData == null)
                {
                    //missing upper or lower dvh estimate
                    return 0;
                } MyDVHData.DVHPoint[] points = dvh.CurveData;
                for (int i = 0; i < points.Count(); i++)
                {
                    if (points[i].Dose > dose)
                    {
                        if (i == 0)
                        {
                            volume = points[0].Volume;
                            break;
                        }

                        //interpolate volume
                        volume = points[i - 1].Volume + (points[i].Volume - points[i - 1].Volume) *
                            (dose - points[i - 1].Dose) / (points[i].Dose - points[i - 1].Dose);
                        break;
                    }
                }

                return volume;
            }
            else
                return 0;

        }

        public class Statistics
        {
            // returns the probability that the observed value of a standard normal random variable will be less than or equal to d
            public static double NormSDist(double d)
            {
                double erfHolder = Erf(d / Math.Sqrt(2.0));
                return (1 + erfHolder) / 2;
            }
            private static double Erf(double z)
            {
                double erfValue = z;
                double currentCoefficient = 1.0;
                int termCount = 100;
                for (int n = 1; n < termCount; n++)
                {
                    currentCoefficient *= -1.0 * (2.0 * (double)n - 1.0) / ((double)n * (2.0 * (double)n + 1.0));
                    erfValue += currentCoefficient * Math.Pow(z, (2 * n + 1));
                }
                return erfValue * (2.0 / Math.Sqrt(Math.PI));
            }
        }

        //private static DVHData GetCummulativeDVH(string structId, PlanningItem pItem, DoseValuePresentation dosePresentation, VolumePresentation volumePresentation)
        //{
        //    DVHData cumDVH = null;
        //    double binSize = 0.1;
        //    if (pItem is PlanSetup)
        //    {
        //        PlanSetup plan = pItem as PlanSetup;
        //        Structure varStruct = plan.StructureSet.Structures.FirstOrDefault(s => s.Id == structId);
        //        cumDVH = plan.GetDVHCumulativeData(varStruct, dosePresentation, volumePresentation, binSize);
        //    }
        //    else if (pItem is PlanSum)
        //    {
        //        PlanSum psum = pItem as PlanSum;
        //        Structure varStruct = psum.StructureSet.Structures.FirstOrDefault(s => s.Id == structId);
        //        cumDVH = psum.GetDVHCumulativeData(varStruct, dosePresentation, volumePresentation, binSize);
        //    }

        //    return cumDVH;
        //}

    }
}
