////////////////////////////////////////////////////////////////////////////////
// PlanQualityReporter.cs
//
// Utility class to create a report containing the plan quality metrics.
//  
// Applies to: ESAPI v11, v13, v13.5, v13.6.
//
// Copyright (c) 2015 Varian Medical Systems, Inc.
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
  public enum MetricType
  {
    Lower, Upper
  }

  public struct VolumeAtDoseMetric
  {
    public DoseValue DoseValue { get; private set; }
    public double Target { get; private set; }
    public MetricType Type { get; private set; }

    public VolumeAtDoseMetric(DoseValue value, double target, MetricType type) : this()
    {
      DoseValue = value;
      Target = target;
      Type = type;
    }
  }

  public struct DoseAtVolumeMetric
  {
    public double Volume { get; private set; }
    public double Target { get; private set; }
    public MetricType Type { get; private set; }

    public DoseAtVolumeMetric(double value, double target, MetricType type) : this()
    {
      Volume = value;
      Target = target;
      Type = type;
    }
  }

  public class PlanQualityReporter
  {

    private const string OutputFormat = "##0.00";
    private const string OutputFormatForConstraints = "##0";

    public static string CreateReport(ExternalPlanSetup plan, Dictionary<string, ModelStructure> structureMatches, string outputDir)
    {
      var outputFile = outputDir + "\\report_mco.xml";
      using (var writer = new XmlTextWriter(outputFile, Encoding.UTF8))
      {
        WriteHeader(writer, outputDir);
        const string rootElement = "PlanQualityMetrics";
        writer.WriteStartElement(rootElement);
        CreatePTVMetrics(writer, plan, structureMatches);
        CreateBladderMetrics(writer, plan, structureMatches);
        CreateRectumMetrics(writer, plan, structureMatches);
        //CreateFemurMetrics(writer, plan, structureMatches, "Femur_L");
        //CreateFemurMetrics(writer, plan, structureMatches, "Femur_R");
        writer.WriteEndElement();
        writer.WriteEndDocument();
        writer.Close();
      }
      return outputFile;
    }

    /// <summary>
    /// Metrics for femoral heads.
    /// </summary>
    private static void CreateFemurMetrics(XmlTextWriter writer, ExternalPlanSetup plan, Dictionary<string, ModelStructure> structureMatches, string femurId)
    {
      var structure = plan.StructureSet.Structures.Single(st => st.Id == structureMatches.Single(x => x.Value.ModelId == femurId).Key);
      writer.WriteStartElement("Structure");
      writer.WriteAttributeString("Id", structure.Id);
      writer.WriteAttributeString("Model_Id", structureMatches[structure.Id].ModelId);
      var metrics = new List<VolumeAtDoseMetric>
      {
        new VolumeAtDoseMetric(new DoseValue(50.0, DoseValue.DoseUnit.Gy), 10.0, MetricType.Upper),
        new VolumeAtDoseMetric(new DoseValue(53.0, DoseValue.DoseUnit.Gy), 5.0, MetricType.Upper)
      };
      CreateVolumeAtDoseMetrics(writer, plan, structure, metrics);
      writer.WriteEndElement();
    }

    /// <summary>
    /// Rectum metrics.
    /// </summary>
    private static void CreateRectumMetrics(XmlTextWriter writer, ExternalPlanSetup plan, Dictionary<string, ModelStructure> structureMatches)
    {
      var structure = plan.StructureSet.Structures.Single(st => st.Id == structureMatches.Single(x => x.Value.ModelId == "Rectum").Key);
      writer.WriteStartElement("Structure");
      writer.WriteAttributeString("Id", structure.Id);
      writer.WriteAttributeString("Model_Id", structureMatches[structure.Id].ModelId);
      var metrics = new List<VolumeAtDoseMetric> { new VolumeAtDoseMetric(new DoseValue(40.0, DoseValue.DoseUnit.Gy), 50.0, MetricType.Upper), 
                                                   new VolumeAtDoseMetric(new DoseValue(50.0, DoseValue.DoseUnit.Gy), 40.0, MetricType.Upper), 
                                                   new VolumeAtDoseMetric(new DoseValue(65.0, DoseValue.DoseUnit.Gy), 25.0, MetricType.Upper) };
      CreateVolumeAtDoseMetrics(writer, plan, structure, metrics);
      writer.WriteEndElement();
    }

    /// <summary>
    /// PTV metrics.
    /// </summary>
    private static void CreatePTVMetrics(XmlWriter writer, PlanSetup plan, IDictionary<string, ModelStructure> structureMatches)
    {
      var structure = plan.StructureSet.Structures.Single(st => st.Id == structureMatches.Single(x => x.Value.ModelId == "PTV").Key);
      writer.WriteStartElement("Structure");
      writer.WriteAttributeString("Id", structure.Id);
      writer.WriteAttributeString("Model_Id", structureMatches[structure.Id].ModelId);
     
      // Volume at dose metrics
      var metrics = new List<VolumeAtDoseMetric> { new VolumeAtDoseMetric(new DoseValue(95.0, DoseValue.DoseUnit.Percent), 99.0, MetricType.Lower), 
                                                   new VolumeAtDoseMetric(new DoseValue(100.0, DoseValue.DoseUnit.Percent), 98.0, MetricType.Lower),
                                                   new VolumeAtDoseMetric(new DoseValue(105.0, DoseValue.DoseUnit.Percent), 15.0, MetricType.Upper), 
                                                   new VolumeAtDoseMetric(new DoseValue(110.0, DoseValue.DoseUnit.Percent), 2.0, MetricType.Upper) };
      CreateVolumeAtDoseMetrics(writer, plan, structure, metrics);

      // Dose at volume metrics
      var metrics2 = new List<DoseAtVolumeMetric> {new DoseAtVolumeMetric(99.0, 95.0, MetricType.Lower)};
      CreateDoseAtVolumeMetrics(writer, plan, structure, metrics2);

      writer.WriteEndElement();
    }

    /// <summary>
    /// Bladder metrics
    /// </summary>
    private static void CreateBladderMetrics(XmlTextWriter writer, ExternalPlanSetup plan, Dictionary<string, ModelStructure> structureMatches)
    {
      var structure = plan.StructureSet.Structures.Single(st => st.Id == structureMatches.Single(x => x.Value.ModelId == "Bladder").Key);
      writer.WriteStartElement("Structure");
      writer.WriteAttributeString("Id", structure.Id);
      writer.WriteAttributeString("Model_Id", structureMatches[structure.Id].ModelId);
      var metrics = new List<VolumeAtDoseMetric> { new VolumeAtDoseMetric(new DoseValue(40.0, DoseValue.DoseUnit.Gy), 50.0, MetricType.Upper), 
                                                   new VolumeAtDoseMetric(new DoseValue(55.0, DoseValue.DoseUnit.Gy), 40.0, MetricType.Upper), 
                                                   new VolumeAtDoseMetric(new DoseValue(65.0, DoseValue.DoseUnit.Gy), 35.0, MetricType.Upper),
                                                   new VolumeAtDoseMetric(new DoseValue(75.0, DoseValue.DoseUnit.Gy), 30.0, MetricType.Upper)};
      CreateVolumeAtDoseMetrics(writer, plan, structure, metrics);
      writer.WriteEndElement();
    }

    private static void CreateVolumeAtDoseMetrics(XmlWriter writer, PlanSetup plan, Structure structure, IEnumerable<VolumeAtDoseMetric> metrics)
    {
      foreach (var dp in metrics)
      {
        var dv = dp.DoseValue;
        var volumeInRelativeUnits = plan.GetVolumeAtDose(structure, dv, VolumePresentation.Relative);
        writer.WriteStartElement("ClinicalPoint");
        writer.WriteAttributeString("Type", "VolumeAtDose");
        writer.WriteAttributeString("Dose", dv.Dose.ToString(OutputFormatForConstraints));
        var doseUnitAsString = (dv.Unit == DoseValue.DoseUnit.Percent) ? "%Rx" : dv.UnitAsString;
        writer.WriteAttributeString("DoseUnit", doseUnitAsString);
        writer.WriteAttributeString("Volume", volumeInRelativeUnits.ToString(OutputFormat));
        writer.WriteAttributeString("TargetUnit", "%");
        writer.WriteAttributeString("Target", dp.Target.ToString(OutputFormatForConstraints));
        writer.WriteAttributeString("MetricType", dp.Type == MetricType.Lower ? ">" : "<");
        bool isSatisfied;
        if (dp.Type == MetricType.Lower)
        {
          isSatisfied = volumeInRelativeUnits > dp.Target;
        }
        else
        {
          isSatisfied = volumeInRelativeUnits < dp.Target;
        }
        var violationInPercents = isSatisfied ? 0.0 : 100.0 * Math.Abs(volumeInRelativeUnits - dp.Target)/dp.Target;

        writer.WriteAttributeString("ConstraintSatisfied", isSatisfied.ToString());
        writer.WriteAttributeString("ConstraintViolationInPercents", violationInPercents.ToString(OutputFormat));
        writer.WriteEndElement();
      }
    }

    private static void CreateDoseAtVolumeMetrics(XmlWriter writer, PlanSetup plan, Structure structure, IEnumerable<DoseAtVolumeMetric> metrics)
    {
      foreach (var vp in metrics)
      {
        var vol = vp.Volume;
        var doseInRelativeUnits = plan.GetDoseAtVolume(structure, vol, VolumePresentation.Relative, DoseValuePresentation.Relative);
        writer.WriteStartElement("ClinicalPoint");
        writer.WriteAttributeString("Type", "DoseAtVolume");
        writer.WriteAttributeString("Volume", vol.ToString(OutputFormatForConstraints));
        writer.WriteAttributeString("VolumeUnit", "%");
        writer.WriteAttributeString("Dose", doseInRelativeUnits.Dose.ToString(OutputFormat));
        writer.WriteAttributeString("TargetUnit", "%");
        writer.WriteAttributeString("Target", vp.Target.ToString(OutputFormatForConstraints));
        writer.WriteAttributeString("MetricType", vp.Type == MetricType.Lower ? ">" : "<");
        bool isSatisfied;
        if (vp.Type == MetricType.Lower)
        {
          isSatisfied = doseInRelativeUnits.Dose > vp.Target;
        }
        else
        {
          isSatisfied = doseInRelativeUnits.Dose < vp.Target;
        }
        var violationInPercents = isSatisfied ? 0.0 : 100.0 * Math.Abs(doseInRelativeUnits.Dose - vp.Target) / vp.Target;

        writer.WriteAttributeString("ConstraintSatisfied", isSatisfied.ToString());
        writer.WriteAttributeString("ConstraintViolationInPercents", violationInPercents.ToString(OutputFormat));
        writer.WriteEndElement();
      }
    }

    private static void WriteHeader(XmlWriter writer, string outputDir)
    {
      writer.WriteStartDocument();
      writer.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"GenerateReport.xslt\"");
      
      // pull the stylesheet file from internal resources and craete in the report location.
      using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VMS.TPS.GenerateReport.xslt"))
      {
        using (var output = File.Create(outputDir + "\\GenerateReport_mco.xslt"))
        {
          if (stream != null)
          {
            stream.CopyTo(output);
          }
        }
      }
      
    }

  }


}
