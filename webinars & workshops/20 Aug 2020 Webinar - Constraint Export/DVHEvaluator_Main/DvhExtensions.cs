using System;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// Helper class with methods to calculate DVH parameters.
    /// Currently only "IsDoseValid" is used.
    /// </summary>
    public static class DvhExtensions
    {
        // Calculate Dose at Volume.
        public static DoseValue GetDoseAtVolume(this PlanningItem pitem, Structure structure, double volume, VolumePresentation volumePresentation, DoseValuePresentation requestedDosePresentation)
        {
            if (pitem is PlanSetup)
            {
                return ((PlanSetup)pitem).GetDoseAtVolume(structure, volume, volumePresentation, requestedDosePresentation);
            }
            else
            {
                if (requestedDosePresentation != DoseValuePresentation.Absolute)
                {
                    throw new ApplicationException("Only absolute dose supported for Plan Sums");
                }

                DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, volumePresentation, 0.001);
                return DvhExtensions.DoseAtVolume(dvh, volume);
            }
        }

        // Calculate Volume at Dose.
        public static double GetVolumeAtDose(this PlanningItem pitem, Structure structure, DoseValue dose, VolumePresentation requestedVolumePresentation)
        {
            if (pitem is PlanSetup)
            {
                //try catch statement to switch dose units to system presentation. Otherwise exception "Dose Units do not match to system settings
                try
                {
                    return ((PlanSetup)pitem).GetVolumeAtDose(structure, dose, requestedVolumePresentation);
                }
                catch
                {
                    if (dose.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0)
                    {
                        return ((PlanSetup)pitem).GetVolumeAtDose(structure, new DoseValue(dose.Dose / 100, DoseValue.DoseUnit.Gy), requestedVolumePresentation);
                    }
                    else if (dose.Unit.CompareTo(DoseValue.DoseUnit.Gy) == 0)
                    {
                        return ((PlanSetup)pitem).GetVolumeAtDose(structure, new DoseValue(dose.Dose * 100, DoseValue.DoseUnit.cGy), requestedVolumePresentation);
                    }
                    else
                    {
                        return ((PlanSetup)pitem).GetVolumeAtDose(structure, dose, requestedVolumePresentation);
                    }
                }
            }
            else
            {
                DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, requestedVolumePresentation, 0.001);

                // Convert dose unit to system unit: otherwise false output without warning.
                try
                {
                    ((PlanSum)pitem).PlanSetups.First().GetVolumeAtDose(structure, dose, requestedVolumePresentation);
                    return DvhExtensions.VolumeAtDose(dvh, dose.Dose);
                }
                catch
                {
                    if (dose.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0)
                    {
                        return DvhExtensions.VolumeAtDose(dvh, dose.Dose / 100);
                    }
                    else if (dose.Unit.CompareTo(DoseValue.DoseUnit.Gy) == 0)
                    {
                        return DvhExtensions.VolumeAtDose(dvh, dose.Dose * 100);
                    }
                    else
                    {
                        return DvhExtensions.VolumeAtDose(dvh, dose.Dose);
                    }
                }
            }
        }

        // Calculate Dose at Volume.
        public static DoseValue DoseAtVolume(DVHData dvhData, double volume)
        {
            if (dvhData == null || dvhData.CurveData.Count() == 0)
            {
                return DoseValue.UndefinedDose();
            }

            double absVolume = dvhData.CurveData[0].VolumeUnit == "%" ? volume * dvhData.Volume * 0.01 : volume;
            if (volume < 0.0 || absVolume > dvhData.Volume)
            {
                return DoseValue.UndefinedDose();
            }

            DVHPoint[] hist = dvhData.CurveData;
            for (int i = 0; i < hist.Length; i++)
            {
                if (hist[i].Volume < volume)
                {
                    return hist[i].DoseValue;
                }
            }
            return dvhData.MaxDose;
        }

        // Calculate Volume at Dose.
        public static double VolumeAtDose(DVHData dvhData, double dose)
        {
            if (dvhData == null)
            {
                return Double.NaN;
            }

            DVHPoint[] hist = dvhData.CurveData;
            int index = (int)(hist.Length * dose / dvhData.MaxDose.Dose);
            if (index < 0 || index >= hist.Length)
            {
                return 0.0;
            }
            else
            {
                return hist[index].Volume;
            }
        }

        // Determines if dose is valid in a plan.
        public static bool IsDoseValid(this PlanningItem pitem)
        {
            if (pitem is PlanSetup)
            {
                return ((PlanSetup)pitem).IsDoseValid;
            }
            else if (pitem is PlanSum)
            {
                PlanSum psum = (PlanSum)pitem;
                var plans = (from p in psum.PlanSetups where p.IsDoseValid == false select p);
                return plans.Count() <= 0;
            }
            else
            {
                throw new ApplicationException("Unknown PlanningItem type " + pitem.ToString());
            }
        }
    }
}



