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
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using VMS.TPS.Common.Model.API;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace RBEReport
{
  public class RBEViewModel : INotifyPropertyChanged
  {
    public DataGrid Grid = null;
    public ListBox ListBox = null;
    public string PatientId { get { return patientId; } set { patientId = value; OnPropertyChanged("PatientId"); } }
    public string PatientName { get { return patientName; } set { patientName = value; OnPropertyChanged("PatientName"); } }
    public string DOB { get { return dob; } set { dob = value; OnPropertyChanged("DOB"); } }
    public string CourseId { get { return courseId; } set { courseId = value; OnPropertyChanged("CourseId"); } }
    public string PlanId { get { return planId; } set { planId = value; OnPropertyChanged("PlanId"); } }
    public string Approval { get { return approval; } set { approval = value; OnPropertyChanged("Approval"); } }
    public string Modification { get { return modification; } set { modification = value; OnPropertyChanged("Modification"); } }
    public string Date { get { return date; } set { date = value; OnPropertyChanged("Date"); } }
    public string TotalEQD2Dose { get { return totaleqd2dose; } set { totaleqd2dose = value; OnPropertyChanged("TotalEQD2Dose"); } }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(name));
      }
    }

    private string patientId;
    private string patientName;
    private string dob;
    private string courseId;
    private string planId;
    private string approval;
    private string modification;
    private string date;
    private string totaleqd2dose;

    System.Collections.ObjectModel.ObservableCollection<Columns> Table = new System.Collections.ObjectModel.ObservableCollection<Columns>();

    public System.Collections.ObjectModel.ObservableCollection<PlanDetails> Details = new System.Collections.ObjectModel.ObservableCollection<PlanDetails>();

    public void SetupConnections()
    {
      Grid.DataContext = Table;
      ListBox.DataContext = Details;
      Grid.CellEditEnding += new EventHandler<DataGridCellEditEndingEventArgs>(Grid_CellEditEnding);
    }

    public void AddPlanningItem(PlanSetup ps)
    {
      string planType = ps is ExternalPlanSetup ? "External Beam" : "Brachytherapy";
      string doseFormat = "0.00 " + ps.TotalPrescribedDose.UnitAsString;
      string technique = GetTechnique(ps);

      if (planType == "Brachytherapy" && technique != "HDR")
      {
        string message;
        if (technique.Length < 1)
        {
          message = "This script does not support this type of plans.";
        }
        else
        {
          message = string.Format("This script does not support {0} plans.", technique);
        }
        throw new ApplicationException(message);
      }

      double defaultAB = 10;
      double eqd2Dose = CalculateEQ2Dose(ps, defaultAB);
      Table.Add(new Columns()
      {
        CourseID = ps.Course.Id,
        PlanID = ps.Id,
        PlanType = planType,
        TargetVolumeID = ps.TargetVolumeID,
        TargetAB = defaultAB.ToString("0.00"),
        PrescPercentage = (ps.PrescribedPercentage * 100).ToString() + "%",
        DosePerFraction = ps.UniqueFractionation.PrescribedDosePerFraction.Dose.ToString(doseFormat),
        NumberOfFractions = ps.UniqueFractionation.NumberOfFractions.ToString(),
        TotalDosePlanned = ps.TotalPrescribedDose.Dose.ToString(doseFormat),
        EQD2DosePlanned = eqd2Dose.ToString(doseFormat) 
      });
      Details.Add(new PlanDetails()
      {
        CourseID = ps.Course.Id,
        PlanID = "Plan ID: " + ps.Id,
        PlanType = planType,
        TargetVolumeID = ps.TargetVolumeID,
        TargetAB = defaultAB.ToString(),
        PrescPercentage = (ps.PrescribedPercentage * 100).ToString() + "%",
        DosePerFraction = ps.UniqueFractionation.PrescribedDosePerFraction.Dose.ToString(doseFormat),
        NumberOfFractions = ps.UniqueFractionation.NumberOfFractions.ToString(),
        TotalDosePlanned = ps.TotalPrescribedDose.Dose.ToString(doseFormat),
        EQD2DosePlanned = eqd2Dose.ToString(doseFormat),
        Technique = technique,
        Status = ps.ApprovalStatus.ToString(),
        Modified = ps.HistoryUserName,
        Date = ps.HistoryDateTime.ToString()
      });
      Plans.Add(new KeyValuePair<PlanSetup, double>(ps, defaultAB));
      TotalEQD2Dose = SumTotal().ToString(doseFormat);
    }

    public string GetTechnique(PlanSetup ps)
    {
      if (ps is BrachyPlanSetup)
      {
        BrachyPlanSetup brachy = ps as BrachyPlanSetup;
        if (brachy.NumberOfPdrPulses != null)
        {
          return "PDR";
        }
        else
        {
          Catheter c = brachy.Catheters.FirstOrDefault();
          if (c != null)
          {
            return c.TreatmentUnit.DoseRateMode;
          }
        }
      }
      else
      {
        Beam beam = ps.Beams.FirstOrDefault();
        if (beam != null)
        {
          if (beam.GantryDirection != VMS.TPS.Common.Model.Types.GantryDirection.None)
          {
            return (beam.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.VMAT) ? "VMAT" : "ARC";
          }
          else
          {
            return (beam.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.DoseDynamic) ? "IMRT" : "STATIC";
          }
        }
      }

      return "";
    }

    double CalculateEQ2Dose(PlanSetup ps, double targetAB)
    {
      double dosePerFraction = ps.UniqueFractionation.PrescribedDosePerFraction.Dose;
      double dosePerFractionInGy = dosePerFraction;
      if (ps.UniqueFractionation.PrescribedDosePerFraction.Unit == VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.cGy)
      {
        dosePerFractionInGy = dosePerFraction * 0.01;
      }
      int numberOfFractions = ps.UniqueFractionation.NumberOfFractions.Value;
      double bed = numberOfFractions * dosePerFraction * (1 + dosePerFractionInGy / targetAB);
      double eq2 = bed / (1 + 2 / targetAB);
      return eq2;
    }

    double SumTotal()
    {
      return Plans.Sum(p => CalculateEQ2Dose(p.Key, p.Value));
    }

    public List<KeyValuePair<PlanSetup,double>> Plans = new List<KeyValuePair<PlanSetup,double>>();

    void Grid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
      if (e.EditAction == DataGridEditAction.Commit)
      {
        double value;
        TextBox tb = e.EditingElement as TextBox;
        try
        {
          value = double.Parse(tb.Text);
        }
        catch
        {
          return;
        }
        var col = e.Column;
        var row = e.Row;
        int ix = row.GetIndex();
        if (ix >= Plans.Count) return; //empty last row in DataGrid
        Columns item = row.Item as Columns;
        PlanSetup ps = Plans[ix].Key;
        string doseFormat = "0.00 " + ps.TotalPrescribedDose.UnitAsString; 
        double oldAB = Plans[ix].Value;
        KeyValuePair<PlanSetup, double> pair = Plans[ix];
        Plans.Remove(pair);
        Plans.Insert(ix, new KeyValuePair<PlanSetup, double>(ps, value));
        Details[ix].TargetAB = value.ToString("0.00");
        double newEQ2Dose = CalculateEQ2Dose(ps, value);
        item.EQD2DosePlanned = newEQ2Dose.ToString(doseFormat);
        Details[ix].EQD2DosePlanned = newEQ2Dose.ToString(doseFormat);
        TotalEQD2Dose = SumTotal().ToString(doseFormat);
       }
    }

    public void ExportToPDF(string pdfFile)
    {
      Document migraDoc = new Document();
      Section section = migraDoc.AddSection();
      section.PageSetup.Orientation = MigraDoc.DocumentObjectModel.Orientation.Landscape;
      //Paragraph paragraph = section.AddParagraph();
      Table table = new Table();
      table.Borders.Width = 1;
      table.Borders.Color = Colors.White;
      table.AddColumn(Unit.FromCentimeter(6));
      table.AddColumn(Unit.FromCentimeter(6));
      Row row = table.AddRow();
      Cell cell = row.Cells[0];
      cell.AddParagraph("Patient ID:");
      cell = row.Cells[1];
      Paragraph paragraph = cell.AddParagraph();
      paragraph.AddFormattedText(PatientId, TextFormat.Bold);
      row = table.AddRow();
      cell = row.Cells[0];
      cell.AddParagraph("Patient Name:");
      cell = row.Cells[1];
      paragraph = cell.AddParagraph();
      paragraph.AddFormattedText(PatientName, TextFormat.Bold);
      row = table.AddRow();
      cell = row.Cells[0];
      cell.AddParagraph("Date of Birth:");
      cell = row.Cells[1];
      if (!string.IsNullOrEmpty(DOB))
      {
        cell.AddParagraph(DOB);
      }
      row = table.AddRow();
      cell = row.Cells[0];
      cell.AddParagraph("Course ID:");
      cell = row.Cells[1];
      cell.AddParagraph(CourseId);
      row = table.AddRow();
      cell = row.Cells[0];
      cell.AddParagraph("Plan ID:");
      cell = row.Cells[1];
      cell.AddParagraph(PlanId);
      row = table.AddRow();
      cell = row.Cells[0];
      cell.AddParagraph("Approval Status:");
      cell = row.Cells[1];
      cell.AddParagraph(Approval);
      row = table.AddRow();
      cell = row.Cells[0];
      cell.AddParagraph("Modification by:");
      cell = row.Cells[1];
      cell.AddParagraph(Modification);
      row = table.AddRow();
      cell = row.Cells[0];
      cell.AddParagraph("Modification Date/Time:");
      cell = row.Cells[1];
      cell.AddParagraph(Date);

      section.Add(table);

      Paragraph paragraph2 = section.AddParagraph("\n\n");
      paragraph2.AddFormattedText("Plan Summary", TextFormat.Bold);

      table = new Table();
      table.Borders.Width = 1;
      table.Borders.Color = Colors.Olive;
      for (int c = 0; c < 10; c++)
      {
        table.AddColumn(Unit.FromCentimeter(2.6));
      }
      row = table.AddRow();
      row.Shading.Color = Colors.PaleGoldenrod;
      cell = row.Cells[0];
      cell.AddParagraph("Course ID");
      cell = row.Cells[1];
      cell.AddParagraph("Plan ID");
      cell = row.Cells[2];
      cell.AddParagraph("Plan Type");
      cell = row.Cells[3];
      cell.AddParagraph("Target Volume ID");
      cell = row.Cells[4];
      cell.AddParagraph("Target a/b");
      cell = row.Cells[5];
      cell.AddParagraph("Presc. %");
      cell = row.Cells[6];
      cell.AddParagraph("Dose / Fraction");
      cell = row.Cells[7];
      cell.AddParagraph("Number of Fractions");
      cell = row.Cells[8];
      cell.AddParagraph("Total Dose Planned");
      cell = row.Cells[9];
      cell.AddParagraph("EQD2 Dose Planned");

      foreach (var plan in Table)
      {
        row = table.AddRow();
        cell = row.Cells[0];
        cell.AddParagraph(plan.CourseID);
        cell = row.Cells[1];
        cell.AddParagraph(plan.PlanID);
        cell = row.Cells[2];
        cell.AddParagraph(plan.PlanType);
        cell = row.Cells[3];
        cell.AddParagraph(plan.TargetVolumeID);
        cell = row.Cells[4];
        cell.AddParagraph(plan.TargetAB);
        cell = row.Cells[5];
        cell.AddParagraph(plan.PrescPercentage);
        cell = row.Cells[6];
        cell.AddParagraph(plan.DosePerFraction);
        cell = row.Cells[7];
        cell.AddParagraph(plan.NumberOfFractions);
        cell = row.Cells[8];
        cell.AddParagraph(plan.TotalDosePlanned);
        cell = row.Cells[9];
        cell.AddParagraph(plan.EQD2DosePlanned);
      }
      section.Add(table);

      Paragraph paragraph3 = section.AddParagraph("\n");
      paragraph3.AddFormattedText("Total Planned EQD2 Dose: " + TotalEQD2Dose);

      Paragraph paragraph4 = section.AddParagraph("\n\n");
      paragraph4.AddFormattedText("Plan Details", TextFormat.Bold);

      Table table2 = new Table();
      table2.Borders.Width = 1;
      table2.Borders.Color = Colors.White;
      for (int c = 0; c < 4; c++)
      {
        table2.AddColumn(Unit.FromCentimeter(6));
      }
      foreach (var plan in Details)
      {
        row = table2.AddRow();
        row = table2.AddRow();
        cell = row.Cells[0];
        Paragraph p = cell.AddParagraph();
        p.AddFormattedText(plan.PlanID, TextFormat.Bold);
        row = table2.AddRow();
        cell = row.Cells[0];
        cell.AddParagraph("Course ID:");
        cell = row.Cells[1];
        cell.AddParagraph(plan.CourseID);

        row = table2.AddRow();
        cell = row.Cells[0];
        cell.AddParagraph("Plan Type:");
        cell = row.Cells[1];
        cell.AddParagraph(plan.PlanType);
        cell = row.Cells[2];
        cell.AddParagraph("Technique:");
        cell = row.Cells[3];
        cell.AddParagraph(plan.Technique);

        row = table2.AddRow();
        cell = row.Cells[0];
        cell.AddParagraph("Dose / Fraction:");
        cell = row.Cells[1];
        cell.AddParagraph(plan.DosePerFraction);
        cell = row.Cells[2];
        cell.AddParagraph("Prescribed Percentage:");
        cell = row.Cells[3];
        cell.AddParagraph(plan.PrescPercentage);

        row = table2.AddRow();
        cell = row.Cells[0];
        cell.AddParagraph("Number of Fractions:");
        cell = row.Cells[1];
        cell.AddParagraph(plan.NumberOfFractions);

        row = table2.AddRow();
        cell = row.Cells[0];
        cell.AddParagraph("Target Volume ID:");
        cell = row.Cells[1];
        cell.AddParagraph(plan.TargetVolumeID);
        cell = row.Cells[2];
        cell.AddParagraph("Target Volume a/b:");
        cell = row.Cells[3];
        cell.AddParagraph(plan.TargetAB);

        row = table2.AddRow();
        cell = row.Cells[0];
        cell.AddParagraph("Total Dose Planned:");
        cell = row.Cells[1];
        cell.AddParagraph(plan.TotalDosePlanned);
        cell = row.Cells[2];
        cell.AddParagraph("EQD2 Dose Planned:");
        cell = row.Cells[3];
        cell.AddParagraph(plan.EQD2DosePlanned);

        row = table2.AddRow();
        cell = row.Cells[0];
        cell.AddParagraph("Approval Status:");
        cell = row.Cells[1];
        cell.AddParagraph(plan.Status);

        row = table2.AddRow();
        cell = row.Cells[0];
        cell.AddParagraph("Modification by:");
        cell = row.Cells[1];
        cell.AddParagraph(plan.Modified);
        cell = row.Cells[2];
        cell.AddParagraph("Modification Date/Time:");
        cell = row.Cells[3];
        cell.AddParagraph(plan.Date);
      }
      section.Add(table2);

      PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.None);
      pdfRenderer.Document = migraDoc;
      pdfRenderer.RenderDocument();
      pdfRenderer.PdfDocument.Save(pdfFile);
    }
    
    public void PostToAria()
    {
      string pdfFile = @"C:\temp\RBEReport.pdf";
      ExportToPDF(pdfFile);
      AriaDocumentPost.Post(pdfFile, PatientId);
    }
  }

  public class Columns : INotifyPropertyChanged
  {
    public string CourseID { get { return courseid; } set { courseid = value; OnPropertyChanged("CourseID"); } }
    public string PlanID { get { return planid; } set { planid = value; OnPropertyChanged("PlanID"); } }
    public string PlanType { get { return plantype; } set { plantype = value; OnPropertyChanged("PlanType"); } }
    public string TargetVolumeID { get { return targetvolumeid; } set { targetvolumeid = value; OnPropertyChanged("TargetVolumeID"); } }
    public string TargetAB { get { return targetab; } set { targetab = value; OnPropertyChanged("TargetAB"); } }
    public string PrescPercentage { get { return prescpercentage; } set { prescpercentage = value; OnPropertyChanged("PrescPercentage"); } }
    public string DosePerFraction { get { return doseperfraction; } set { doseperfraction = value; OnPropertyChanged("DosePerFraction"); } }
    public string NumberOfFractions { get { return numberoffractions; } set { numberoffractions = value; OnPropertyChanged("NumberOfFractions"); } }
    public string TotalDosePlanned { get { return totaldoseplanned; } set { totaldoseplanned = value; OnPropertyChanged("TotalDosePlanned"); } }
    public string EQD2DosePlanned { get { return eqd2doseplanned; } set { eqd2doseplanned = value; OnPropertyChanged("EQD2DosePlanned"); } }

    /// <summary>
    /// this event triggers the UI to update the table, when a row changes
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(name));
      }
    }

    private string courseid;
    private string planid;
    private string plantype;
    private string targetvolumeid;
    private string targetab;
    private string prescpercentage;
    private string doseperfraction;
    private string numberoffractions;
    private string totaldoseplanned;
    private string eqd2doseplanned;
  }

  public class PlanDetails : INotifyPropertyChanged
  {
    public string CourseID { get { return courseid; } set { courseid = value; OnPropertyChanged("CourseID"); } }
    public string PlanID { get { return planid; } set { planid = value; OnPropertyChanged("PlanID"); } }
    public string PlanType { get { return plantype; } set { plantype = value; OnPropertyChanged("PlanType"); } }
    public string TargetVolumeID { get { return targetvolumeid; } set { targetvolumeid = value; OnPropertyChanged("TargetVolumeID"); } }
    public string TargetAB { get { return targetab; } set { targetab = value; OnPropertyChanged("TargetAB"); } }
    public string PrescPercentage { get { return prescpercentage; } set { prescpercentage = value; OnPropertyChanged("PrescPercentage"); } }
    public string DosePerFraction { get { return doseperfraction; } set { doseperfraction = value; OnPropertyChanged("DosePerFraction"); } }
    public string NumberOfFractions { get { return numberoffractions; } set { numberoffractions = value; OnPropertyChanged("NumberOfFractions"); } }
    public string TotalDosePlanned { get { return totaldoseplanned; } set { totaldoseplanned = value; OnPropertyChanged("TotalDosePlanned"); } }
    public string EQD2DosePlanned { get { return eqd2doseplanned; } set { eqd2doseplanned = value; OnPropertyChanged("EQD2DosePlanned"); } }
    public string Technique { get { return technique; } set { technique = value; OnPropertyChanged("Technique"); } }
    public string Status { get { return status; } set { status = value; OnPropertyChanged("Status"); } }
    public string Modified { get { return modified; } set { modified = value; OnPropertyChanged("Modified"); } }
    public string Date { get { return date; } set { date = value; OnPropertyChanged("Date"); } }

    /// <summary>
    /// this event triggers the UI to update the table, when a row changes
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      PropertyChangedEventHandler handler = PropertyChanged;
      if (handler != null)
      {
        handler(this, new PropertyChangedEventArgs(name));
      }
    }

    private string courseid;
    private string planid;
    private string plantype;
    private string targetvolumeid;
    private string targetab;
    private string prescpercentage;
    private string doseperfraction;
    private string numberoffractions;
    private string totaldoseplanned;
    private string eqd2doseplanned;
    private string technique;
    private string status;
    private string modified;
    private string date;
  }
}
