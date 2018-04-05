////////////////////////////////////////////////////////////////////////////////
// PQMReporter.cs
//
//  A ESAPI PQMReporter that generates a report for the selected patient and
//  plan with various calculated Plan Quality Metrics for defined structures.
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
//
// The flag v13_ESAPI adds reporting on combined parotids,
// which is done by creating a new volume "Parotid Combined", 
// reporting on it, then removing it. V11 ESAPI doesn't support this.
//#define v13_ESAPI  // comment this line for version 11 ESAPI

using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Xml.Linq;
using UserDefinedMetrics.NonHypoFractionated;

namespace VMS.TPS
{
  public abstract class PQMReporter
  {
    protected string eclipseVersion, scriptVersion;
    protected string currentuser;
    protected string rootPath;
    protected string stylesheet;
    protected XslCompiledTransform myXslTransform;
    
    // override these methods
    abstract protected void dumpReportXML(Patient patient, StructureSet ss, PlanningItem plan, string xmlFilePath);

    // implementation
    public PQMReporter(string rootPath_, string userId, string eclipseVersion_, string scriptVersion_, string planSetupStylesheet_ = @"gen_report.xsl")
    {
      rootPath = rootPath_;
      currentuser = userId;
      stylesheet = planSetupStylesheet_;
      eclipseVersion = eclipseVersion_;
      scriptVersion = scriptVersion_;
      // load the stylesheet into memory, transform report XML into HTML, show that to the user.
      using (XmlReader stylesheetReader = XmlReader.Create(stylesheet))
      {
        stylesheetReader.ReadToDescendant("xsl:stylesheet");

        try
        {
          myXslTransform = new XslCompiledTransform();
          myXslTransform.Load(stylesheetReader);
        }
        catch (System.Xml.Xsl.XsltCompileException e)
        {
          throw new ApplicationException("Failed to load xsl stylesheet '" + stylesheet + "'. details:\n" + e.ToString());
        }
        catch (System.Xml.Xsl.XsltException e)
        {
          throw new ApplicationException("General stylesheet problem when applying '" + stylesheet + "'. details:\n" + e.ToString());
        }
      }
    }

    public PQMReporter(string rootPath_, string userId, string eclipseVersion_, string scriptVersion_, XmlReader stylesheetReader)
    {
      rootPath = rootPath_;
      currentuser = userId;
      stylesheet = "internal";
      eclipseVersion = eclipseVersion_;
      scriptVersion = scriptVersion_;

      // load the PQM stylesheet into memory, transform report XML into HTML, show that to the user.
      stylesheetReader.ReadToDescendant("xsl:stylesheet");

      try
      {
        myXslTransform = new XslCompiledTransform();
        myXslTransform.Load(stylesheetReader);
      }
      catch (System.Xml.Xsl.XsltCompileException e)
      {
        throw new ApplicationException("Failed to load xsl stylesheet '" + stylesheet + "'. details:\n" + e.ToString());
      }
      catch (System.Xml.Xsl.XsltException e)
      {
        throw new ApplicationException("General stylesheet problem when applying '" + stylesheet + "'. details:\n" + e.ToString());
      }
    }
    string MakeFilenameValid(string s)
    {
      char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
      foreach (char ch in invalidChars)
      {
        s = s.Replace(ch, '_');
      }
      return s;
    }
    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method creates a Plan Quality Metric report for the specified plan.
    /// </summary>
    /// <param name="patient">loaded patient</param>
    /// <param name="ss">structure set to use while generating Plan Quality Metrics</param>
    /// <param name="plan">Plan for which the report is going to be generated.</param>
    /// <param name="rootPath">root directory for the report.  This method creates a subdirectory
    ///  'patientid' under the root, then creates the xml and html reports in the subdirectory.</param>
    /// <param name="userId">User whose id will be stamped on the report.</param>
    /// <param name="xmlFilePath">Raw unformatted XML report.</param>
    //---------------------------------------------------------------------------------------------
    public string generateReport(Patient patient, StructureSet ss, PlanningItem plan, out string xmlFilePath)
    {
      string rootDir =  string.Format(@"{0}\{1}", rootPath, MakeFilenameValid(patient.Id));
      Directory.CreateDirectory(rootDir);
      string fileRoot = string.Format(@"{0}\PQMReport-{1}", rootDir,  MakeFilenameValid(plan.Id));
      // build exported XML filename, put it in the root path (likely is users temp directory)
      xmlFilePath = string.Format(@"{0}.xml", fileRoot);
      string htmlFilePath = string.Format(@"{0}.html", fileRoot);

      dumpReportXML(patient, ss, plan, xmlFilePath);

      // PQM stylesheet should already be loaded into memory, transform report XML into HTML, show that to the user.
      try
      {
        myXslTransform.Transform(xmlFilePath, htmlFilePath);
        return htmlFilePath;
      }
      catch (System.Xml.Xsl.XsltCompileException e)
      {
        throw new ApplicationException("Failed to load xsl stylesheet '" + stylesheet + "'. details:\n" + e.ToString());
      }
      catch (System.Xml.Xsl.XsltException e)
      {
        throw new ApplicationException("General stylesheet problem when applying '" + stylesheet + "'. details:\n" + e.ToString());
      }
    }

    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method generates plan quality metrics (PQMs) for the target of the passed plansum. 
    /// </summary>
    /// <param name="patient">loaded patient</param>
    /// <param name="ss">structure set to use while generating Plan Quality Metrics</param>
    /// <param name="psum">plansum for which the report is going to be generated.</param>
    /// <param name="writer">XML document where the evaluated PQMs will be written.</param>
    //---------------------------------------------------------------------------------------------
    protected void WriteDoseStatisticsXML_Target(Patient patient, StructureSet ss, PlanSum psum, XmlWriter writer)
    {
      // select all plan target structures and shove them in a list.
      var targets = from ps in psum.PlanSetups
                    from s in ss.Structures
                    where (s.Id == ps.TargetVolumeID)
                    select s.Id;
      // check whether all targets are the same
      bool uniform = !targets.Any(id => id != psum.PlanSetups.First().TargetVolumeID);

      // if all targets are the same, report on the first one.
      if (uniform)
      {
        // linq query finds the target structure
        Structure target = (from s in ss.Structures
                            where s.Id == targets.First()
                            select s).FirstOrDefault();
        if (target == null)
        {
          // linq query finds the first PTV structure
          target = (from s in ss.Structures
                    where s.DicomType == "PTV"
                    select s).FirstOrDefault();
        }

        // add target stastics if a target was found.
        if (target != null)
        {
          var doses = from PlanSetup p in psum.PlanSetups
                        select p.TotalPrescribedDose;
          double totalPrescribedDose = doses.Sum(x => (x.Unit == DoseValue.DoseUnit.Gy) ? x.Dose : x.Dose/100.0);

          PlanQualityMetric[] PQMs = Target.getPQMs(new DoseValue(totalPrescribedDose, DoseValue.DoseUnit.Gy));

          string[] search = { target.Id };
          addStructurePQM(psum, ss, search, PQMs, writer);
        }

      }
      else
      {
        // write warning that the component plans have different targets
        XElement plansSetupsX = new XElement("PlanSetups");
        XElement pstw = new XElement("PlanSumTargetWarning",
         new XAttribute("planSumId", psum.Id),
         plansSetupsX);
        foreach (PlanSetup plan in psum.PlanSetups)
        {
          plansSetupsX.Add(
            new XElement("PlanSetup",
              new XAttribute("Id", plan.Id),
              new XElement("TargetVolumeID", plan.TargetVolumeID)));
        }
        pstw.WriteTo(writer);
        // write dose statistics for all targets
        foreach (PlanSetup plan in psum.PlanSetups)
        {
          WriteDoseStatisticsXML_Target(patient, plan.StructureSet, plan, writer);
//          WriteDoseStatisticsXML_Target(patient, ss, plan, writer); // which structure set to use, the plansum, or plan
        }

      }
    }
    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method generates plan quality metrics (PQMs) for the target of the passed plan. 
    /// </summary>
    /// <param name="patient">loaded patient</param>
    /// <param name="ss">structure set to use while generating Plan Quality Metrics</param>
    /// <param name="plan">Plan for which the report is going to be generated.</param>
    /// <param name="writer">XML document where the evaluated PQMs will be written.</param>
    //---------------------------------------------------------------------------------------------
    protected void WriteDoseStatisticsXML_Target(Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer)
    {
      // linq query finds the target structure
      Structure target = (from s in ss.Structures
                          where s.Id == plan.TargetVolumeID
                          select s).FirstOrDefault();
      if (target == null)
      {
        // linq query finds the first PTV structure
        target = (from s in ss.Structures
                  where s.DicomType == "PTV"
                  select s).FirstOrDefault();
      }

      // add target stastics if a target was found.
      if (target != null)
      {
        PlanQualityMetric[] PQMs = Target.getPQMs(plan.TotalPrescribedDose);
        string[] search = { target.Id };
        addStructurePQM(plan, ss, search, PQMs, writer);
      }
    }
    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method generates plan quality metrics (PQMs) of type Prostate2GyOrLess for the passed
    /// plan. If the plan is not a Prostate plan nothing is added.
    /// </summary>
    /// <param name="patient">loaded patient</param>
    /// <param name="ss">structure set to use while generating Plan Quality Metrics</param>
    /// <param name="plan">Plan or plansum for which the report is going to be generated.</param>
    /// <param name="writer">XML document where the evaluated PQMs will be written.</param>
    //---------------------------------------------------------------------------------------------
    protected void WriteDoseStatisticsXML_Prostate2GyOrLess(Patient patient, StructureSet ss, PlanningItem plan, XmlWriter writer)
    {
      // find stats for the rectum structure
      addStructurePQM(plan, ss, Rectum.searchIds, Rectum.PQMs, writer);

      // find stats for the bladder structure
      addStructurePQM(plan, ss, Bladder.searchIds, Bladder.PQMs, writer);

      // find stats for the penile bulb structure
      addStructurePQM(plan, ss, PenileBulb.searchIds, PenileBulb.PQMs, writer);

      // find stats for the small bowel
      addStructurePQM(plan, ss, SmallBowel.searchIds, SmallBowel.PQMs, writer);

      // find stats for femurs
      addStructurePQM(plan, ss, FemurLeft.searchIds, FemurLeft.PQMs, writer);
      addStructurePQM(plan, ss, FemurRight.searchIds, FemurRight.PQMs, writer);
    }
    protected void WriteDoseStatisticsXML_HeadAndNeck2GyOrLess(Patient patient, StructureSet ss, PlanningItem plan, XmlWriter writer)
    {
      // find stats for the Brachial Plexus structure
      addStructurePQM(plan, ss, BrachialPlexus.searchIds, BrachialPlexus.PQMs, writer);

      // find stats for the Brain structure
      addStructurePQM(plan, ss, Brain.searchIds, Brain.PQMs, writer);

      // find stats for the Brainstem structure
      addStructurePQM(plan, ss, Brainstem.searchIds, Brainstem.PQMs, writer);

      // find stats for the Cochlea structure
      addStructurePQM(plan, ss, Cochlea.searchIds, Cochlea.PQMs, writer);

      // find stats for the Esophagus structure
      addStructurePQM(plan, ss, Esophagus.searchIds, Esophagus.PQMs, writer);

      // find stats for the Eye lens structures
      addStructurePQM(plan, ss, LensLeft.searchIds, LensLeft.PQMs, writer);
      addStructurePQM(plan, ss, LensRight.searchIds, LensRight.PQMs, writer);

      // find stats for the Eye structures
      addStructurePQM(plan, ss, OrbitLeft.searchIds, OrbitLeft.PQMs, writer);
      addStructurePQM(plan, ss, OrbitRight.searchIds, OrbitRight.PQMs, writer);

      // find stats for the Optic Nerve structures
      addStructurePQM(plan, ss, OpticNerveLeft.searchIds, OpticNerveLeft.PQMs, writer);
      addStructurePQM(plan, ss, OpticNerveRight.searchIds, OpticNerveRight.PQMs, writer);
      addStructurePQM(plan, ss, OpticChiasm.searchIds, OpticChiasm.PQMs, writer);

      // find stats for the Larynx structures
      addStructurePQM(plan, ss, Larynx.searchIds, Larynx.PQMs, writer);

      // find stats for the Lung structures (Lung (whole organ; target volume is NOT within the lung))
      addStructurePQM(plan, ss, LungLeft.searchIds, LungLeft.PQMs, writer);
      addStructurePQM(plan, ss, LungRight.searchIds, LungRight.PQMs, writer);

      // parotids
      addStructurePQM(plan, ss, ParotidLeft.searchIds, ParotidLeft.PQMs, writer);
      addStructurePQM(plan, ss, ParotidRight.searchIds, ParotidRight.PQMs, writer);

      // combined parotids (cut this out for version 11, won't work)
#if v13_ESAPI
      if (patient.CanModifyData())
      {
        int count = 0;
        patient.BeginModifications();   // enable writing with this script.
        // create the empty "Parotid Combined" structure
        string ID = "Parotid Combined";
        Structure parotid_combined = ss.AddStructure("AVOIDANCE", ID);

        // search for left parotid, if found combine it into 'parotid_combined'
        foreach (string volumeId in ParotidLeft.searchIds)
        {
          Structure oar = (from s in ss.Structures
                           where s.Id.ToUpper().CompareTo(volumeId.ToUpper()) == 0
                           select s).FirstOrDefault();
          if (oar != null)
          {
            count++;
            parotid_combined.SegmentVolume = parotid_combined.Or(oar);
            break;
          }
        }

        // search for right parotid, if found combine it into 'parotid_combined'
        foreach (string volumeId in ParotidRight.searchIds)
        {
          Structure oar = (from s in ss.Structures
                           where s.Id.ToUpper().CompareTo(volumeId.ToUpper()) == 0
                           select s).FirstOrDefault();
          if (oar != null)
          {
            count++;
            parotid_combined.SegmentVolume = parotid_combined.Or(oar);
            break;
          }
        }
        if (count > 0)
        {
          string[] searchIds = { ID };
          addStructurePQM(plan, ss, searchIds, ParotidsCombined.PQMs, writer);
        }
        ss.RemoveStructure(parotid_combined);
      }
#endif
      // find stats for the spinal cord structure
      addStructurePQM(plan, ss, SpinalCord.searchIds, SpinalCord.PQMs, writer);

      // find stats for the Mandible structure
      addStructurePQM(plan, ss, Mandible.searchIds, Mandible.PQMs, writer);

      // find stats for the Oral Cavity structure
      addStructurePQM(plan, ss, OralCavity.searchIds, OralCavity.PQMs, writer);

      // find stats for the Pharynx structure
      addStructurePQM(plan, ss, Pharynx.searchIds, Pharynx.PQMs, writer);

      // find stats for the PharyngealConstrictor structure
      addStructurePQM(plan, ss, PharyngealConstrictor.searchIds, PharyngealConstrictor.PQMs, writer);

      // find stats for the submandibular structure
      addStructurePQM(plan, ss, Submandibular.searchIds, Submandibular.PQMs, writer);

      // find stats for the Thyroid structure
      addStructurePQM(plan, ss, Thyroid.searchIds, Thyroid.PQMs, writer);

      //      writer.WriteEndElement(); // </DoseStatistics>
    }
    private void addStructurePQM(PlanningItem plan, StructureSet ss, string[] structureIds, PlanQualityMetric[] pqmStats, XmlWriter writer)
    {
      // search through the list of structure ids until we find one
      Structure oar = null;
      string actualStructId = "";
      foreach (string volumeId in structureIds)
      {
        oar = (from s in ss.Structures
               where s.Id.ToUpper().CompareTo(volumeId.ToUpper()) == 0
               select s).FirstOrDefault();
        if (oar != null)
        { actualStructId = oar.Id;
          break;
        }
      }
      writer.WriteStartElement("Structure");
      writer.WriteAttributeString("Id", actualStructId);

      bool bStructureFound = (oar != null);
      writer.WriteAttributeString("present", bStructureFound.ToString());
      if (bStructureFound)
      {
        writer.WriteElementString("Volume", oar.Volume.ToString("0.00000"));
        writer.WriteStartElement("PQMs"); // PQM = Plan Quality Metric

        foreach (PlanQualityMetric pqm in pqmStats)
        {
          pqm.addPQMInfo(plan, oar, writer);
        }

        writer.WriteEndElement(); // </PQMs>
      }
      writer.WriteEndElement(); // </Structure>
    }
    public void WritePatientXML(Patient patient, XmlWriter writer)
    {
      writer.WriteAttributeString("Id", patient.Id);
      writer.WriteElementString("CreationDateTime", patient.CreationDateTime.ToString());
      writer.WriteElementString("Id2", patient.Id2);
      writer.WriteElementString("SSN", patient.SSN);
      writer.WriteElementString("FirstName", patient.FirstName);
      writer.WriteElementString("MiddleName", patient.MiddleName);
      writer.WriteElementString("LastName", patient.LastName);
      writer.WriteElementString("Sex", patient.Sex);
      writer.WriteElementString("PrimaryOncologistId", patient.PrimaryOncologistId);
    }
    public enum CtrlPtSelector { NoControlPoints = 0, IncludeControlPoints};
    public enum MLCSelector { NoMLC = 0, IncludeMLC };
    public void WritePlanXML(PlanSetup plan, XmlWriter writer, CtrlPtSelector writeCtrlPts = CtrlPtSelector.NoControlPoints, MLCSelector mlc = MLCSelector.NoMLC)
    {
      Course course = plan.Course;
      writer.WriteStartElement("Courses");
      writer.WriteStartElement("Course");
      writer.WriteAttributeString("Id", course.Id);
      writer.WriteAttributeString("Name", course.Name);
      writer.WriteAttributeString("Comment", course.Comment);
      writer.WriteElementString("StartDateTime", course.StartDateTime.ToString());
      writer.WriteStartElement("PlanSetups");
      writer.WriteStartElement("PlanSetup");
      plan.WriteXml(writer);
      if (writeCtrlPts == CtrlPtSelector.IncludeControlPoints)
      {
        WriteBeamAndControlPoints(plan, writer, mlc);
      }
      writer.WriteEndElement(); // </PlanSetup>
      writer.WriteEndElement(); // </PlanSetups>
      writer.WriteEndElement(); // </Course>
      writer.WriteEndElement(); // </Courses>
    }
    protected void WriteBeamAndControlPoints(PlanSetup plan, XmlWriter writer, MLCSelector mlc)
    {
      writer.WriteStartElement("BeamAndControlPoints");
      foreach (var beam in plan.Beams)
      {
        writer.WriteStartElement("Beam");
        beam.WriteXml(writer);
        // ... and controlpoints
        writer.WriteStartElement("ControlPoints");
        beam.ControlPoints.WriteXml(writer);
        foreach (var controlPoint in beam.ControlPoints)
        {
          writer.WriteStartElement("ControlPoint");
          controlPoint.WriteXml(writer);
          if (mlc == MLCSelector.IncludeMLC)
          {
            float[,] lp = controlPoint.LeafPositions;
            if (lp.Length > 1)
            {
              writer.WriteStartElement("LeafPositions");
              for (int i = 0; i <= lp.GetUpperBound(0); i++)
              {
                writer.WriteStartElement("Bank");
                writer.WriteAttributeString("number", i.ToString());
                for (int j = 0; j <= lp.GetUpperBound(1); j++)
                {
                  float f = lp[i, j];
                  writer.WriteString(f.ToString() + " ");
                }
                writer.WriteEndElement();//</Bank>
              }
              writer.WriteEndElement();//</LeafPositions>
            }
          }
          writer.WriteEndElement();//</ControlPoint>
        }
        writer.WriteEndElement();//</ControlPoints>
        writer.WriteEndElement(); // </Beam>
      }
      writer.WriteEndElement(); // </BeamAndControlPoints>
    }
  }

  public class PlanSumReporter : PQMReporter
  {
    public PlanSumReporter(string rootPath_, string userId, string eclipseVersion_, string scriptVersion_, string planSetupStylesheet_ = @"gen_report_plansum.xsl")
      : base(rootPath_, userId, eclipseVersion_, scriptVersion_, planSetupStylesheet_)
    {
    }
    public PlanSumReporter(string rootPath_, string userId, string eclipseVersion_, string scriptVersion_, XmlReader stylesheetReader)
      : base(rootPath_, userId, eclipseVersion_, scriptVersion_, stylesheetReader)
    {
    }
    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method creates a Plan Quality Metric report for the specified plansum.
    /// </summary>
    /// <param name="patient">loaded patient</param>
    /// <param name="ss">structure set to use while generating Plan Quality Metrics</param>
    /// <param name="psum">Plansum for which the report is going to be generated.</param>
    /// <param name="rootPath">root directory for the report.  This method creates a subdirectory
    ///  'patientid' under the root, then creates the xml and html reports in the subdirectory.</param>
    /// <param name="userId">User whose id will be stamped on the report.</param>
    //---------------------------------------------------------------------------------------------
    override protected void dumpReportXML(Patient patient, StructureSet ss, PlanningItem plan_, string sXMLPath)
    {
      if (!(plan_ is PlanSum))
        throw new ApplicationException("PlanSumReporter should be used only for PlanSum types!");

      PlanSum psum = (PlanSum)plan_;

      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      settings.IndentChars = ("\t");
      System.IO.MemoryStream mStream = new System.IO.MemoryStream();
      XmlWriter writer = XmlWriter.Create(mStream, settings);
      writer.WriteStartDocument(true);
      writer.WriteStartElement("PlanQualityReport");
      writer.WriteAttributeString("created", DateTime.Now.ToString());
      writer.WriteAttributeString("userid", currentuser);
      writer.WriteAttributeString("eclipseVersion", eclipseVersion);
      writer.WriteAttributeString("scriptVersion", scriptVersion);
      writer.WriteStartElement("Patient");
      WritePatientXML(patient, writer);
      writer.WriteStartElement("PlanSum");
      psum.WriteXml(writer);
      foreach (PlanSetup plan in psum.PlanSetups)
        WritePlanXML(plan, writer, CtrlPtSelector.IncludeControlPoints);
      writer.WriteEndElement(); // </PlanSum>
      writer.WriteEndElement(); // </Patient>

      writer.WriteStartElement("DoseStatistics");
      WriteDoseStatisticsXML_Target(patient, ss, psum, writer);
      WriteDoseStatisticsXML_Prostate2GyOrLess(patient, ss, psum, writer);
      WriteDoseStatisticsXML_HeadAndNeck2GyOrLess(patient, ss, psum, writer);
      writer.WriteEndElement(); // </DoseStatistics>

      writer.WriteEndElement(); // </PlanQualityReport>
      writer.WriteEndDocument();
      writer.Flush();
      mStream.Flush();

      // write the XML file report.
      using (System.IO.FileStream file = new System.IO.FileStream(sXMLPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
      {
        // Have to rewind the MemoryStream in order to read its contents. 
        mStream.Position = 0;
        mStream.CopyTo(file);
        file.Flush();
        file.Close();
      }

      writer.Close();
      mStream.Close();
    }
  }
  public class PlanSetupReporter : PQMReporter
  {
    public PlanSetupReporter(string rootPath_, string userId, string eclipseVersion_, string scriptVersion_, string planSetupStylesheet_ = @"gen_report.xsl")
      : base(rootPath_, userId, eclipseVersion_, scriptVersion_, planSetupStylesheet_)
    {}
    public PlanSetupReporter(string rootPath_, string userId, string eclipseVersion_, string scriptVersion_, XmlReader stylesheetReader)
      : base(rootPath_, userId, eclipseVersion_, scriptVersion_, stylesheetReader)
    {
    }
    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method creates a Plan Quality Metric report for the specified plan.
    /// </summary>
    /// <param name="patient">loaded patient</param>
    /// <param name="ss">structure set to use while generating Plan Quality Metrics</param>
    /// <param name="plan">Plan for which the report is going to be generated.</param>
    /// <param name="rootPath">root directory for the report.  This method creates a subdirectory
    ///  'patientid' under the root, then creates the xml and html reports in the subdirectory.</param>
    /// <param name="userId">User whose id will be stamped on the report.</param>
    //---------------------------------------------------------------------------------------------
    override protected void dumpReportXML(Patient patient, StructureSet ss, PlanningItem plan_, string sXMLPath)
    {
      if (!(plan_ is PlanSetup))
        throw new ApplicationException("PlanSetupReporter should be used only for PlanSetup types!");

      PlanSetup plan = (PlanSetup)plan_;


      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      settings.IndentChars = ("\t");
      System.IO.MemoryStream mStream = new System.IO.MemoryStream();
      XmlWriter writer = XmlWriter.Create(mStream, settings);
      writer.WriteStartDocument(true);
      writer.WriteStartElement("PlanQualityReport");
      writer.WriteAttributeString("created", DateTime.Now.ToString());
      writer.WriteAttributeString("userid", currentuser);
      writer.WriteAttributeString("eclipseVersion", eclipseVersion);
      writer.WriteAttributeString("scriptVersion", scriptVersion);
      writer.WriteStartElement("Patient");
      WritePatientXML(patient, writer);
      WritePlanXML(plan, writer, CtrlPtSelector.IncludeControlPoints);
      writer.WriteEndElement(); // </Patient>

      writer.WriteStartElement("DoseStatistics");
      WriteDoseStatisticsXML_Target(patient, ss, plan, writer);
      WriteDoseStatisticsXML_Prostate2GyOrLess(patient, ss, plan, writer);
      WriteDoseStatisticsXML_HeadAndNeck2GyOrLess(patient, ss, plan, writer);
      writer.WriteEndElement(); // </DoseStatistics>

      writer.WriteEndElement(); // </PlanQualityReport>
      writer.WriteEndDocument();
      writer.Flush();
      mStream.Flush();

      // write the XML file report.
      using (System.IO.FileStream file = new System.IO.FileStream(sXMLPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
      {
        // Have to rewind the MemoryStream in order to read its contents. 
        mStream.Position = 0;
        mStream.CopyTo(file);
        file.Flush();
        file.Close();
      }

      writer.Close();
      mStream.Close();
    }

  }
}
