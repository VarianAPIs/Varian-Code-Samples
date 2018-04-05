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
using System.Linq;
using System.Text;
using System.Collections.Generic;


using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using System.Windows;//Need to include this to add a this to enable creating a window to display the userinput interface

namespace MVVM_Demo
{
  class Program
  {      
    [STAThread]
    static void Main(string[] args)
    {
      try
      {
          using (VMS.TPS.Common.Model.API.Application app = VMS.TPS.Common.Model.API.Application.CreateApplication(args[0], args[1]))
        {
          Execute(app);
        }
      }
      catch (Exception e)
      {
        Console.Error.WriteLine(e.ToString());
      }
    }

    static void DrawDVH(DVHPlot.ViewModel.MainWindowModel viewModel, VMS.TPS.Common.Model.API.PlanSetup curps)
    {
        StructureSet structureSet = curps.StructureSet;

        foreach (var structure in structureSet.Structures)
        {
            DVHData dvhData = curps.GetDVHCumulativeData(structure,
                                        DoseValuePresentation.Absolute,
                                        VolumePresentation.Relative, 1);
            OxyPlot.OxyColor color = new OxyPlot.OxyColor();
            OxyPlot.MarkerType marker = new OxyPlot.MarkerType();
            viewModel.AddData(dvhData, structure.Id, color, marker);
        }
    }

    static void Execute(VMS.TPS.Common.Model.API.Application app)
    {
      // This program demonstrates use of MVVM design with stand alone Eclipse Script API.  It includes binding of members and methods. In addition use of a class for 
      // multipage printing of a report from from the View class is demonstrated. A DocumentPaginator class (PrintReport.cs) includes generic code and hightlights
      // code added specifically for printing out a report from Eclipse. 


        //VMS.TPS.Common.Model.API.Patient curpat = app.OpenPatientById("ZBenchMark-TG166");

        //VMS.TPS.Common.Model.API.PlanSetup curps = curpat.Courses.Where(x => x.Id == "C1").Single().PlanSetups.Where(x => x.Id == "TG-166").Single();




        Window w = new Window();
       // View gui = new View(); //This is the default with nothing passed
        //View gui = new View("Pass string from the API"); //This only passes a string
        //View gui = new View("Pass Eclipse Info", curpat, curps);

        Console.WriteLine("Loading default patients ...");

        DVHPlot.MainWindow dvhgui = new DVHPlot.MainWindow(app);

        TPReportData pt1ReportData = new TPReportData();
        TPReportData pt2ReportData = new TPReportData();

        //DrawDVH(dvhgui.ViewModel, curps);
        //DrawDVHs(dvhgui.ViewModel, "B_002", "B_001", app, pt1ReportData, pt1ReportData);
        //dvhgui.ViewModel.ExportPDF();


        //w.Content = dvhgui;
        //w.SizeToContent = SizeToContent.WidthAndHeight;
        //w.ShowDialog();

        //dvhgui.content = gui;
        dvhgui.ShowDialog();
        
    }
  }
}
