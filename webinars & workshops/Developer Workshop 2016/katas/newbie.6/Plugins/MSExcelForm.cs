using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using Excel = Microsoft.Office.Interop.Excel;  // new lib for Excel connection


namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
    {
      // TODO : Add here your code that is called when the script is launched from Eclipse
     /* This program is to demonstrate how to extract data from Eclipse and record them into an Excel spreadsheet
      * To connect to MS Excel,  the Microsoft.Office.Interop.Excel needs to be added in reference (Right click refernce -> Add Reference-> COM -> Type Libraires->
      *                          Microsoft Excel 15.0 Object Library)
      * The goal is to read patient name, MRN and prescription dose and write them into a blank Excel spreadsheet.  The code can be changed to open
      * existing Excel template form and fill out info in the form. 
      *                            - developed by Minsong on 7/19/16
      */

        //initialize Excel application               
        Excel.Application oXL;
        Excel._Workbook oWB;
        Excel._Worksheet oSheet;
        oXL = new Excel.Application();
        oWB = oXL.Workbooks.Add();
        oSheet = oWB.ActiveSheet;

        // obtain current patient, plan in Eclipse 
        Patient patient = context.Patient;
        PlanSetup plansetup = context.PlanSetup;

        string value;  // value to write in Excel cell

        // define plan/patient parameters
        double prescbDose = 0.0; //total prescribed dose for plan 
        prescbDose = plansetup.TotalDose.Dose;

        //write patient name in Excel
        value = patient.LastName + "," + patient.FirstName;
        oSheet.get_Range("A2").Value = "Pt Name:";
        oSheet.get_Range("B2").Value = value;

        // write today's date and time in Excel
        value = System.DateTime.Today.ToString();
        oSheet.get_Range("A3").Value = "Date:";
        oSheet.get_Range("B3").Value = value;

        // write patent MRN in Excel
        value = patient.Id;
        oSheet.get_Range("A4").Value = "MRN";
        oSheet.get_Range("B4").Value = value;

        //write prescribed dose
        prescbDose = Math.Round(prescbDose, 2);
        value = prescbDose.ToString() + "Gy";
        oSheet.get_Range("A5").Value = "Prescription";
        oSheet.get_Range("B5").Value = value;

        // make the excel visible and controllable to user
        oXL.Visible = true;
        oXL.UserControl = true;
    }
  }
}
