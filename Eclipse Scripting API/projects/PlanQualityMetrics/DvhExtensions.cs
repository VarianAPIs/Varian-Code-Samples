////////////////////////////////////////////////////////////////////////////////
// DvhExtensions.cs
//
//  Adds PlanningItem.GetDoseAtVolume and PlanningItem.GetVolumeAtDose to
//  VMS.TPS.PlanningItem class via the .NET extension mechanism.
//  
// Applies to:  ESAPI v11, ESAPI v13.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
  public static class DvhExtensions
  {
    public static DoseValue GetDoseAtVolume(this PlanningItem pitem, Structure structure, double volume, VolumePresentation volumePresentation, DoseValuePresentation requestedDosePresentation)
    {
      if (pitem is PlanSetup)
      {
        return ((PlanSetup)pitem).GetDoseAtVolume(structure, volume, volumePresentation, requestedDosePresentation);
      }
      else
      {
        if (requestedDosePresentation != DoseValuePresentation.Absolute)
          throw new ApplicationException("Only absolute dose supported for Plan Sums");
        DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, volumePresentation, 0.001);
        return DvhExtensions.DoseAtVolume(dvh, volume);
      }
    }
    public static double GetVolumeAtDose(this PlanningItem pitem, Structure structure, DoseValue dose, VolumePresentation requestedVolumePresentation)
    {
      if (pitem is PlanSetup)
      {
        return ((PlanSetup)pitem).GetVolumeAtDose(structure, dose, requestedVolumePresentation);
      }
      else
      {
        DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, requestedVolumePresentation, 0.001);
        return DvhExtensions.VolumeAtDose(dvh, dose.Dose);
      }
    }

    public static DoseValue DoseAtVolume(DVHData dvhData, double volume)
    {
      if (dvhData == null || dvhData.CurveData.Count() == 0)
        return DoseValue.UndefinedDose();
      double absVolume = dvhData.CurveData[0].VolumeUnit == "%" ? volume * dvhData.Volume * 0.01 : volume;
      if (volume < 0.0 || absVolume > dvhData.Volume)
        return DoseValue.UndefinedDose();

      DVHPoint[] hist = dvhData.CurveData;
      for (int i = 0; i < hist.Length; i++)
      {
        if (hist[i].Volume < volume)
          return hist[i].DoseValue;
      }
      return DoseValue.UndefinedDose();
    }

    public static double VolumeAtDose(DVHData dvhData, double dose)
    {
      if (dvhData == null)
        return Double.NaN;

      DVHPoint[] hist = dvhData.CurveData;
      int index = (int)(hist.Length * dose / dvhData.MaxDose.Dose);
      if (index < 0 || index > hist.Length)
        return 0.0;//Double.NaN;
      else
        return hist[index].Volume;
    }
  }
}
