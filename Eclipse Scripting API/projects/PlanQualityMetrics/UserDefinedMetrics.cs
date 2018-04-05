////////////////////////////////////////////////////////////////////////////////
// UserDefinedMetrics.cs
//
//  User-defined plan quality metrics for structures.  Only metrics for
//  Head & Neck and Prostate are present now.
//  
// Many of these metrics come from QUANTEC data. See 
// http://en.wikibooks.org/wiki/Radiation_Oncology/Toxicity/QUANTEC
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
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// To make it into the final PQM report, all quality metrics need to be defined here, then made use of 
// in PQMReporter.WriteDoseStatisticsXML_Prostate2GyOrLess or WriteDoseStatisticsXML_HeadAndNeck2GyOrLess
// to evaluate using the metric.  When a structure with an ID matching one defined in "searchIds" is
// found, metric reporting for that structure will be performed and included in the PQM report.
namespace UserDefinedMetrics
{
  namespace NonHypoFractionated
  {
    // quality metrics for the target
    public class Target
    {
      public static PlanQualityMetric[] getPQMs(DoseValue totalPrescribedDose)
      {
        PlanQualityMetric[] PQMs = 
          {
            new VolumeAtDose("V[95%(Rx) > 95%]", new DoseValue(totalPrescribedDose.Dose*.95, totalPrescribedDose.Unit),   95.0, 1.0, VolumePresentation.Relative, PQMUtilities.LimitType.lower)
          };
        return PQMs;
      }
    }
    // quality metrics for the rectum structure
    public static class Rectum
    {
      public static PlanQualityMetric[] PQMs = 
    {
        new VolumeAtDose("V50 < 50%", new DoseValue(50.0, DoseValue.DoseUnit.Gy), 50.0, 1.05),
        new VolumeAtDose("V60 < 35%", new DoseValue(60.0, DoseValue.DoseUnit.Gy), 35.0, 1.05),
        new VolumeAtDose("V65 < 25%", new DoseValue(65.0, DoseValue.DoseUnit.Gy), 25.0, 1.05),
        new VolumeAtDose("V70 < 20%", new DoseValue(70.0, DoseValue.DoseUnit.Gy), 20.0, 1.0),
        new VolumeAtDose("V75 < 15%", new DoseValue(75.0, DoseValue.DoseUnit.Gy), 15.0, 1.0)
    };
      public static string[] searchIds = { "Rectum", "CT_RECTUM" };
        // Put a comma separated list here of all variations of the IDs used for the "Rectum"
        // structure in your clinic. The search for matching IDs is case insensitive, the PQM 
        // script searches for all case variations of "rectum", "RECTUM", etc.
    };
    // quality metrics for the bladder structure
    public static class Bladder
    {
      public static PlanQualityMetric[] PQMs = 
    {
      new VolumeAtDose("V65 < 50%",new DoseValue(65.0, DoseValue.DoseUnit.Gy), 50.0, 1.05),
      new VolumeAtDose("V70 < 35%",new DoseValue(70.0, DoseValue.DoseUnit.Gy), 35.0, 1.05),
      new VolumeAtDose("V75 < 25%",new DoseValue(75.0, DoseValue.DoseUnit.Gy), 25.0, 1.0),
      new VolumeAtDose("V80 < 15%",new DoseValue(80.0, DoseValue.DoseUnit.Gy), 15.0, 1.0)
    };
      public static string[] searchIds = { "Bladder" };
    };
    // quality metrics for the penile bulb structure
    public static class PenileBulb
    {
      public static PlanQualityMetric[] PQMs = 
      { 
        new MeanDoseLimit(new DoseValue(50.0,DoseValue.DoseUnit.Gy), 1.05),
        new DoseAtVolume("D90 < 50 Gy", 90.0, new DoseValue(50, DoseValue.DoseUnit.Gy), 1.0)
      };
      public static string[] searchIds = { "Penile Bulb", "PENILE_BULB", "penilebulb" };
    }
    // quality metrics for the small bowel
    public static class SmallBowel
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new VolumeAtDose("V15 < 120 cc (single loop)",new DoseValue(15.0, DoseValue.DoseUnit.Gy), 120.0, 1.05,VolumePresentation.AbsoluteCm3),
          new VolumeAtDose("V45 < 195 cc (entire bowel)",new DoseValue(45.0, DoseValue.DoseUnit.Gy), 195.0, 1.05,VolumePresentation.AbsoluteCm3)
        };
      public static string[] searchIds = { "Bowel", "Small bowel" };
    };
    // quality metrics for femurs
    public static class FemurLeft
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new VolumeAtDose("V50 < 5%",new DoseValue(50.0, DoseValue.DoseUnit.Gy), 5.0, 1.05),
        };
      public static string[] searchIds = { "Lt Fem Head", "LT FEMUR", "Head of Femur-LT", "FEMUR_L" }; // , "LT FEM HEAD"
    }
    public static class FemurRight
    {
      public static PlanQualityMetric[] PQMs = FemurLeft.PQMs;
      public static string[] searchIds = { "Rt Fem Head", "RT FEMUR", "Head of Femur-RT", "FEMUR_R" }; // "RT FEM HEAD"
    }


    // quality metrics for the Brachial Plexus structure
    public static class BrachialPlexus
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MaxDoseLimit(new DoseValue(66.0,DoseValue.DoseUnit.Gy), 1.0)
        };
      public static string[] searchIds = { "Brachial Plexus" };
    }

    // quality metrics for the Brain structure
    public static class Brain
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MaxDoseLimit(new DoseValue(72.0,DoseValue.DoseUnit.Gy), 1.0),
          new MaxDoseLimit(new DoseValue(60.0,DoseValue.DoseUnit.Gy), 1.0)
        };
      public static string[] searchIds = { "Brain" };
    }

    // quality metrics for the Brainstem structure
    public static class Brainstem
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MaxDoseLimit(new DoseValue(64.0,DoseValue.DoseUnit.Gy), 1.0),
          new MaxDoseLimit(new DoseValue(54.0,DoseValue.DoseUnit.Gy), 1.0),
          new MaxDoseLimit(new DoseValue(50.0,DoseValue.DoseUnit.Gy), 1.0),
          new VolumeAtDose("V59 < 1cc", new DoseValue(59.0, DoseValue.DoseUnit.Gy), 1.0, 1.05, VolumePresentation.AbsoluteCm3)
        };
      public static string[] searchIds = { "Brainstem" };
    }

    // quality metrics for the Cochlea structure
    public static class Cochlea
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MeanDoseLimit(new DoseValue(35.0,DoseValue.DoseUnit.Gy), 1.05),
          new MeanDoseLimit(new DoseValue(45.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Cochlea" };
    }

    // quality metrics for the Esophagus structure
    public static class Esophagus
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new VolumeAtDose("V35 < 50%",new DoseValue(35.0, DoseValue.DoseUnit.Gy), 50.0, 1.05),
          new VolumeAtDose("V50 < 40%",new DoseValue(50.0, DoseValue.DoseUnit.Gy), 40.0, 1.05),
          new VolumeAtDose("V70 < 20%",new DoseValue(70.0, DoseValue.DoseUnit.Gy), 20.0, 1.05),
          new MeanDoseLimit(new DoseValue(34.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Esophagus" };
    }

    // quality metrics for the Eye lens structures
    public static class LensLeft
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MeanDoseLimit(new DoseValue(10.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Lens LT" };
    }
    public static class LensRight
    {
      public static PlanQualityMetric[] PQMs = LensLeft.PQMs;
      public static string[] searchIds = { "Lens RT" };
    }

    // quality metrics for the Eye structures
    public static class OrbitLeft
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MeanDoseLimit(new DoseValue(45.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Orbit LT" };
    }
    public static class OrbitRight
    {
      public static PlanQualityMetric[] PQMs = OrbitLeft.PQMs;
      public static string[] searchIds = { "Orbit RT" };
    }

    // quality metrics for the Optic Nerve structures
    public static class OpticNerveLeft
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MaxDoseLimit(new DoseValue(60.0,DoseValue.DoseUnit.Gy), 1.0),
          new MaxDoseLimit(new DoseValue(55.0,DoseValue.DoseUnit.Gy), 1.0),
          new MaxDoseLimit(new DoseValue(50.0,DoseValue.DoseUnit.Gy), 1.0)
        };
      public static string[] searchIds = { "Optic Nerve LT" };
    }

    public static class OpticNerveRight
    {
      public static PlanQualityMetric[] PQMs = OpticNerveLeft.PQMs;
      public static string[] searchIds = { "Optic Nerve RT" };
    }

    public static class OpticChiasm
    {
      public static PlanQualityMetric[] PQMs = OpticNerveLeft.PQMs;
      public static string[] searchIds = { "Optic Chiasm" };
    }

    // quality metrics for the Larynx structures
    public static class Larynx
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MeanDoseLimit(new DoseValue(44.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Larynx", "Glottic Larynx", "GlotticLarynx" };
    }

    // quality metrics for the Lung structures (Lung (whole organ; target volume is NOT within the lung))
    public static class LungLeft
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MeanDoseLimit(new DoseValue(7.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Lung LT" };
    }
    public static class LungRight
    {
      public static PlanQualityMetric[] PQMs = LungLeft.PQMs;
      public static string[] searchIds = { "Lung RT" };
    }

    // quality metrics for parotids
    public static class ParotidLeft
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MeanDoseLimit(new DoseValue(10.0,DoseValue.DoseUnit.Gy), 1.05),
          new MeanDoseLimit(new DoseValue(20.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Parotid LT" };
    }
    public static class ParotidRight
    {
      public static PlanQualityMetric[] PQMs = ParotidLeft.PQMs;
      public static string[] searchIds = { "Parotid RT" };
    }
    /// ParotidsCombined is created by the script and then evaluated
    public static class ParotidsCombined
    {
      public static PlanQualityMetric[] PQMs = 
        {
            new MeanDoseLimit(new DoseValue(10.0,DoseValue.DoseUnit.Gy), 1.05),
            new MeanDoseLimit(new DoseValue(25.0,DoseValue.DoseUnit.Gy), 1.05),
            new MeanDoseLimit(new DoseValue(40.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Parotid Combined" };
    }

    // quality metrics for the spinal cord structure
    public static class SpinalCord
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MaxDoseLimit(new DoseValue(60.0,DoseValue.DoseUnit.Gy), 1.0),
          new MaxDoseLimit(new DoseValue(50.0,DoseValue.DoseUnit.Gy), 1.0)
        };
      public static string[] searchIds = { "Spinal Cord", "Cord" };
    }

    // quality metrics for the Mandible structure
    public static class Mandible
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MaxDoseLimit(new DoseValue(70.0,DoseValue.DoseUnit.Gy), 1.0)
        };
      public static string[] searchIds = { "Mandible" };
    }

    // quality metrics for the Oral Cavity structure
    public static class OralCavity
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MaxDoseLimit(new DoseValue(40.0,DoseValue.DoseUnit.Gy), 1.0),
          new MeanDoseLimit(new DoseValue(40.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Oral Cavity" };
    }

    // quality metrics for the Pharynx structure
    public static class Pharynx
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MeanDoseLimit(new DoseValue(50.0,DoseValue.DoseUnit.Gy), 1.00)
        };
      public static string[] searchIds = { "Pharynx" };
    }

    // quality metrics for the Pharyngeal Constrictor structure
    public static class PharyngealConstrictor
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MeanDoseLimit("Naso and Sinus Cases", new DoseValue(50.0,DoseValue.DoseUnit.Gy), 1.05),
          new MeanDoseLimit("Post op", new DoseValue(45.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Pharyngeal Con", "Phar Con", "Pharyngeal Constrictor" };
    }

    // quality metrics for the submandibular structure
    public static class Submandibular
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MaxDoseLimit(new DoseValue(39.0,DoseValue.DoseUnit.Gy), 1.05)
        };
      public static string[] searchIds = { "Submandibular", "Subman LT", "Subman RT" };
    }

    // quality metrics for the Thyroid structure
    public static class Thyroid
    {
      public static PlanQualityMetric[] PQMs = 
        {
          new MaxDoseLimit(new DoseValue(35.0,DoseValue.DoseUnit.Gy), 1.0)
        };
      public static string[] searchIds = { "Thyroid" };
    }
  }
};
