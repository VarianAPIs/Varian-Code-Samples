////////////////////////////////////////////////////////////////////////////////
// PQMs.cs
//
//  Plan Quality Metric calculators.  Requires DvhExtensions.GetDoseAtVolume
//  and DvhExtensions.GetVolumeAtDose that add these methods to PlanningItem.
//  
//  These calculators support the user defined plan quality metrics
//  in UserDefinedMetrics.cs.
//  
//  Each of these generate XML for each given metric, which is appended to the 
//  report that gets built by PQMReporter.
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
using System.Xml;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Xml.Linq;

namespace VMS.TPS
{
    public interface PlanQualityMetric 
    {
      void addPQMInfo(PlanningItem plan, Structure organ, XmlWriter writer);
    }

    public static class PQMUtilities
    {
      public static XElement getDVXML(DoseValue dv)
      {
        return 
          new XElement("DoseValue",
            new XAttribute("units", dv.UnitAsString),
            new XAttribute("calculated", true.ToString()),
            dv.ValueAsString);
      }
      public static XElement getEvaluateXML(DoseValue dv, DoseValue doseConstraint, double upperLimit)
      {
        double upperLimitMax = (doseConstraint.Dose * upperLimit);
        string pfw = "PASS";
        if (dv.Dose > doseConstraint.Dose && dv.Dose <= upperLimitMax)
          pfw = "WARN";
        else if (dv.Dose > upperLimitMax)
          pfw = "FAIL";

        return
          new XElement("Evaluate",
            new XElement("Limit",
               new XAttribute("type", "upper"),
               doseConstraint.ValueAsString),
            new XElement("Tolerance",
               new XAttribute("type", "leq"),
               upperLimit.ToString("0.000")),
            new XElement("Result",
              new XElement("MaxLimit", upperLimitMax.ToString("0.000")),
              new XElement("PFW", pfw)));
      }
      public enum DosePQMType { MaxDose = 0, MeanDose };
      public static string ToString(this DosePQMType type) { if(type == DosePQMType.MaxDose) return "MaxDose"; if(type==DosePQMType.MeanDose)return "MeanDose"; return "unknown";}
      public static void addDosePQMInfo(DosePQMType type, DoseValue dv, DoseValue doseConstraint, double upperLimit, XmlWriter writer)
      {
        string dosePQMName = string.Format("{0} < {1}", (type == DosePQMType.MaxDose) ? "Dmax" : "Dmean", doseConstraint.ToString());

        XElement pqm = new XElement("PQM",
          new XAttribute("type", type.ToString()),
          new XAttribute("name", dosePQMName),
          getDVXML(dv),
          getEvaluateXML(dv, doseConstraint, upperLimit));

        pqm.WriteTo(writer);
      }
      public static void addDosePQMInfo(string dosePQMName, DosePQMType type, DoseValue dv, DoseValue doseConstraint, double upperLimit, XmlWriter writer)
      {
        XElement pqm = new XElement("PQM",
          new XAttribute("type", type.ToString()),
          new XAttribute("name", dosePQMName),
          getDVXML(dv),
          getEvaluateXML(dv, doseConstraint, upperLimit));

        pqm.WriteTo(writer);
      }
      public enum LimitType { upper = 0, lower };
      public static string ToString(this LimitType t)
      {
        switch (t)
        {
          case LimitType.upper:
            return "upper";
          case LimitType.lower:
            return "lower";
          default:
            return "unknown";
        };
      }
    }
    struct VolumeAtDose : PlanQualityMetric
    {
      public double volumeConstraint;
      public double upperLimit;
      public VolumePresentation vp;
      public DoseValue dv;
      public string name;
      PQMUtilities.LimitType lt;

      public VolumeAtDose(string name_, DoseValue doseValue, double volConstraint, double upperLimitTol,
              VolumePresentation vpp = VolumePresentation.Relative, PQMUtilities.LimitType lt_ = PQMUtilities.LimitType.upper)
      {
        volumeConstraint = volConstraint;
        upperLimit = upperLimitTol;
        vp = vpp;
        dv = doseValue;
        name = name_;
        lt = lt_;
      }
      public void addPQMInfo(PlanningItem plan, Structure organ, XmlWriter writer)
      {
        writer.WriteStartElement("PQM");
        writer.WriteAttributeString("type", "VolumeAtDose");
        writer.WriteAttributeString("name", name);

        writer.WriteStartElement("DoseValue");
        writer.WriteAttributeString("units", dv.UnitAsString);
        writer.WriteString(dv.ValueAsString);
        writer.WriteEndElement(); // </DoseValue>

        double volume = plan.GetVolumeAtDose(organ, dv, vp);
        writer.WriteStartElement("Volume");
        writer.WriteAttributeString("units", vp.ToString());
        writer.WriteAttributeString("calculated", true.ToString());
        writer.WriteString(volume.ToString("0.000"));
        writer.WriteEndElement(); // </Volume>

        string limType = lt.ToString(), tolType;
        if (lt == PQMUtilities.LimitType.upper)
        {
          tolType = "leq";
        }
        else
        {
          tolType = "geq";
        }
        writer.WriteStartElement("Evaluate");
        writer.WriteStartElement("Limit");
        writer.WriteAttributeString("type", limType);
        writer.WriteString(volumeConstraint.ToString("0.000"));
        writer.WriteEndElement(); // </Limit>
        writer.WriteStartElement("Tolerance");
        writer.WriteAttributeString("type", tolType);
        writer.WriteString(upperLimit.ToString("0.000"));
        writer.WriteEndElement(); // </Tolerance>
        writer.WriteStartElement("Result");
        double limitMax = (volumeConstraint * upperLimit);
        writer.WriteElementString("MaxLimit", limitMax.ToString("0.000"));
        string pfw = "PASS";
        if (lt == PQMUtilities.LimitType.upper)
        {
          if (volume > volumeConstraint && volume <= limitMax)
            pfw = "WARN";
          else if (volume > limitMax)
            pfw = "FAIL";
        }
        else 
        {
          if (volume < volumeConstraint && volume >= limitMax)
            pfw = "WARN";
          else if (volume < limitMax)
            pfw = "FAIL";
        }
        writer.WriteElementString("PFW", pfw);
        writer.WriteEndElement(); // </Result>
        writer.WriteEndElement(); // </Evaluate>

        writer.WriteEndElement(); // </PQM>
      }
    }
    struct DoseAtVolume : PlanQualityMetric
    {
      public double volume;
      public double upperLimit;
      public VolumePresentation vp;
      DoseValuePresentation dvp;
      string name;
      public DoseValue doseConstraint;
      PQMUtilities.LimitType lt;

      public DoseAtVolume(string name_, double volumeValue, DoseValue doseLimit, double upperLimitTol,
              VolumePresentation vpp = VolumePresentation.Relative,
              DoseValuePresentation dvp_ = DoseValuePresentation.Absolute,
              PQMUtilities.LimitType lt_ = PQMUtilities.LimitType.upper)
      {
        name = name_;
        volume = volumeValue;
        upperLimit = upperLimitTol;
        vp = vpp;
        doseConstraint = doseLimit;
        dvp = dvp_;
        lt = lt_;
      }
      public void addPQMInfo(PlanningItem plan, Structure organ, XmlWriter writer)
      {
        writer.WriteStartElement("PQM");
        writer.WriteAttributeString("type", "DoseAtVolume");
        writer.WriteAttributeString("name", name);

        DoseValue dv = plan.GetDoseAtVolume(organ, volume, vp, dvp);
        writer.WriteStartElement("DoseValue");
        writer.WriteAttributeString("units", dv.UnitAsString);
        writer.WriteAttributeString("calculated", true.ToString());
        writer.WriteString(dv.ValueAsString);
        writer.WriteEndElement(); // </DoseValue>

        writer.WriteStartElement("Volume");
        writer.WriteAttributeString("units", vp.ToString());
        writer.WriteAttributeString("calculated", false.ToString());
        writer.WriteString(volume.ToString("0.000"));
        writer.WriteEndElement(); // </Volume>

        string limType = lt.ToString(), tolType;
        if (lt == PQMUtilities.LimitType.upper)
          tolType = "leq";
        else
          tolType = "geq";

        writer.WriteStartElement("Evaluate");
        writer.WriteStartElement("Limit");
        writer.WriteAttributeString("type", limType);
        writer.WriteString(doseConstraint.ValueAsString);
        writer.WriteEndElement(); // </Limit>
        writer.WriteStartElement("Tolerance");
        writer.WriteAttributeString("type", tolType);
        writer.WriteString(upperLimit.ToString("0.000"));
        writer.WriteEndElement(); // </Tolerance>
        writer.WriteStartElement("Result");
        double limitMax = (doseConstraint.Dose * upperLimit);
        writer.WriteElementString("MaxLimit", limitMax.ToString("0.000"));
        string pfw = "PASS";
        if (lt == PQMUtilities.LimitType.upper)
        {
          if (dv.Dose > doseConstraint.Dose && dv.Dose <= limitMax)
            pfw = "WARN";
          else if (dv.Dose > limitMax)
            pfw = "FAIL";
        }
        else
        {
          if (dv.Dose < doseConstraint.Dose && dv.Dose >= limitMax)
            pfw = "WARN";
          else if (dv.Dose < limitMax)
            pfw = "FAIL";
        }
        writer.WriteElementString("PFW", pfw);
        writer.WriteEndElement(); // </Result>
        writer.WriteEndElement(); // </Evaluate>

        writer.WriteEndElement(); // </PQM>
      }
    }
    struct MaxDoseLimit : PlanQualityMetric
    {
      public double upperLimit;
      DoseValuePresentation dvp;
      public DoseValue doseConstraint;

      public MaxDoseLimit(DoseValue doseLimit, double upperLimit_,
              DoseValuePresentation dvp_ = DoseValuePresentation.Absolute)
      {
        upperLimit = upperLimit_;
        doseConstraint = doseLimit;
        dvp = dvp_;
      }
      public void addPQMInfo(PlanningItem plan, Structure organ, XmlWriter writer)
      {
        DVHData dvh = plan.GetDVHCumulativeData(organ, dvp, VolumePresentation.Relative, 0.1);
        if (dvh != null)
        {
          DoseValue dv = dvh.MaxDose;
          PQMUtilities.addDosePQMInfo(PQMUtilities.DosePQMType.MaxDose, dv, doseConstraint, upperLimit, writer);
        }
        else 
        {
            XElement pqm = new XElement("PQM",
              new XAttribute("type", "MaxDose"),
              new XAttribute("name", "Dmax < " + doseConstraint.ToString()),
              new XElement("Error", string.Format("Error calculating DVH for structure '{0}'.", organ.Id))
              );
            pqm.WriteTo(writer);
        }
      }
    }
    struct MeanDoseLimit : PlanQualityMetric
    {
      public double upperLimit;
      DoseValuePresentation dvp;
      public DoseValue doseConstraint;
      private string name;

      public MeanDoseLimit(string name_, DoseValue doseLimit, double upperLimit_,
              DoseValuePresentation dvp_ = DoseValuePresentation.Absolute)
      {
        name = name_;
        upperLimit = upperLimit_;
        doseConstraint = doseLimit;
        dvp = dvp_;
      }
      public MeanDoseLimit(DoseValue doseLimit, double upperLimit_,
              DoseValuePresentation dvp_ = DoseValuePresentation.Absolute)
      {
        name = "";
        upperLimit = upperLimit_;
        doseConstraint = doseLimit;
        dvp = dvp_;
      }
      public void addPQMInfo(PlanningItem plan, Structure organ, XmlWriter writer)
      {
        DVHData dvh = plan.GetDVHCumulativeData(organ, dvp, VolumePresentation.Relative, 0.1);
        if (dvh != null)
        {
          DoseValue dv = dvh.MeanDose;
          if (name.Length > 0)
          {
            PQMUtilities.addDosePQMInfo(name, PQMUtilities.DosePQMType.MeanDose, dv, doseConstraint, upperLimit, writer);
          }
          else
          {
            PQMUtilities.addDosePQMInfo(PQMUtilities.DosePQMType.MeanDose, dv, doseConstraint, upperLimit, writer);
          }
        }
        else
        {
          XElement pqm = new XElement("PQM",
            new XAttribute("type", "MeanDose"),
            new XAttribute("name", "Dmean < " + doseConstraint.ToString()),
            new XElement("Error", string.Format("Error calculating DVH for structure '{0}'.", organ.Id))
            );
          pqm.WriteTo(writer);
        }
      }
    }
}
