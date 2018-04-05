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
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Annotations;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.IO;
using System.Xml.XPath;
using System.Windows;
using System.Collections.ObjectModel;//Needed for ObservableCollections class
using System.Windows.Input;//Needed for the ICommandInterface

using VMS.TPS.Common.Model.API;//Needed to be able to use  Eclipse API objects 
using VMS.TPS.Common.Model.Types;//Needed to be able to use  Eclipse API objects 

using MVVM_Demo;
using System.Threading;
using System.Threading.Tasks;

namespace DVHPlot.ViewModel
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private VMS.TPS.Common.Model.API.Application cur_app;
        
        private PlotModel plotModel;
        public PlotModel PlotModel
        {
            get { return plotModel; }
            set { plotModel = value; OnPropertyChanged("PlotModel"); }
        }

        private TPReportData rd1 = new TPReportData();
        private TPReportData rd2 = new TPReportData();

        private string patient1 = "PatientID1,CourseID1,PlanSetupID1";
        private string patient2 = "PatientID2,CourseID2,PlanSetupID2";

        private Dictionary<string, OxyPlot.OxyColor> colors = new Dictionary<string, OxyPlot.OxyColor>();        
        
        private Cursor curcursor = Cursors.Arrow;
        public Cursor CurCursor{
            get{return curcursor;}
            set{curcursor = value;OnPropertyChanged("CurCursor");}
        }

        

        public string Patient1
        {
            get { return patient1; }
            set
            {
                patient1 = value;
                OnPropertyChanged("Patient1");
                if (patient1.Split(',').Count() == 3)
                {
                    PatID_Pat1 = patient1.Split(',')[0];

                    rd1 = GetReportData(patient1);
                }
            }
        }

        public string Patient2
        {
            get { return patient2; }
            set
            {
                patient2 = value;
                OnPropertyChanged("Patient2");
                if (patient2.Split(',').Count() == 3)
                {
                    PatID_Pat2 = patient2.Split(',')[0];
                    rd2 = GetReportData(patient2);
                }
            }
        }

        public string patid_pat1 = "test";
        public string PatID_Pat1
        {
            get { return patid_pat1; }
            set
            {
                patid_pat1 = value;
                OnPropertyChanged("PatID_Pat1");
            }
        }

        public string patid_pat2 = "test2";
        public string PatID_Pat2
        {
            get { return patid_pat2; }
            set { patid_pat2 = value; OnPropertyChanged("PatID_Pat2"); }
        }

        private void DrawDVHs()
        {
            this.plotModel.Series.Clear();

            string ptid1 = patient1.Split(',')[0];
            string ptid2 = patient2.Split(',')[0];

            try
            {
            // search for patient in database
                foreach (var patientSummary in cur_app.PatientSummaries)
                {
                    if (patientSummary.Id == ptid1)   //this is the first patient
                    {
                        VMS.TPS.Common.Model.API.Patient pt1 = cur_app.OpenPatient(patientSummary);
                        if (pt1 == null)
                            throw new ApplicationException("Cannot open patient No.1" + patientSummary.Id);
                        Console.WriteLine("Open patient No.1");

                        //get patient info
                        rd1.CurPatientInfo.LastName = pt1.LastName;
                        rd1.CurPatientInfo.FirstName = pt1.FirstName;
                        rd1.CurPatientInfo.PatientID = pt1.Id;

                        //loop through courses
                        foreach (var course in pt1.Courses)
                        {
                            Console.WriteLine("course=" + course.Id);  //print course name and send to UI
                            rd1.CurPlanInfo.CourseID = course.Id;

                            foreach (var planSetup in course.PlanSetups)
                            {
                                Console.WriteLine("planID=" + planSetup.Id);
                                rd1.CurPlanInfo.PlaneID = planSetup.Id;
                                rd1.CurDosePrescription.Fractionation = planSetup.UniqueFractionation.Id;
                                rd1.CurDosePrescription.TotalPrescribedDose = planSetup.TotalPrescribedDose.ToString();
                                rd1.CurDosePrescription.PlanNormalization = planSetup.PlanNormalizationValue.ToString();
                                rd1.CurDosePrescription.PrescribedDoseFractination = planSetup.UniqueFractionation.PrescribedDosePerFraction.ToString();
                                rd1.CurDosePrescription.NumberOfFraction = planSetup.UniqueFractionation.NumberOfFractions.ToString();

                                foreach (var beam in planSetup.Beams)
                                {
                                    FieldInfo fieldInfo1 = new FieldInfo();
                                    fieldInfo1.BeamID = beam.Id;
                                    fieldInfo1.Collimator = beam.ControlPoints[0].CollimatorAngle.ToString();
                                    fieldInfo1.Gantry = beam.ControlPoints[0].GantryAngle.ToString();
                                    fieldInfo1.Couch = beam.ControlPoints[0].PatientSupportAngle.ToString();
                                    fieldInfo1.DoseRate = beam.DoseRate.ToString();
                                    fieldInfo1.Energy = beam.EnergyModeDisplayName;
                                    fieldInfo1.Machine = beam.TreatmentUnit.Id;
                                    fieldInfo1.MU = beam.Meterset.ToString();
                                    fieldInfo1.X1 = beam.ControlPoints[0].JawPositions.X1.ToString();
                                    fieldInfo1.Y1 = beam.ControlPoints[0].JawPositions.Y1.ToString();
                                    fieldInfo1.X2 = beam.ControlPoints[0].JawPositions.X2.ToString();
                                    fieldInfo1.Y2 = beam.ControlPoints[0].JawPositions.Y2.ToString();

                                    rd1.CurFieldInfoList.Add(fieldInfo1);
                                }

                                // get structure Set
                                StructureSet structureSet = planSetup.StructureSet;

                                foreach (var structure in structureSet.Structures)
                                {
                                    //StructureData structureData1 = new StructureData();


                                    if (colors.ContainsKey(structure.Id.ToLower()) || colors.ContainsKey(structure.DicomType.ToLower()))
                                    {
                                        DVHData dvhData = planSetup.GetDVHCumulativeData(structure,
                                          DoseValuePresentation.Absolute,
                                          VolumePresentation.Relative, 0.2);

                                        if (dvhData == null)
                                        {
                                            continue;
                                            throw new ApplicationException("DVH data does not exist. Script execution cancelled.");
                                        }



                                        //To print and anlyze the DVH 
                                        //
                                        if (colors.ContainsKey(structure.Id.ToLower()))
                                        {
                                            OxyPlot.OxyColor color = colors[structure.Id.ToLower()];
                                            Console.WriteLine("maximum dose of" + structure.Id + " is:" + dvhData.MaxDose.ToString());
                                            AddData(dvhData, structure.Id, color, OxyPlot.MarkerType.Circle);
                                        }
                                        else if (colors.ContainsKey(structure.DicomType.ToLower()))
                                        {
                                            OxyPlot.OxyColor color = colors[structure.DicomType.ToLower()];
                                            Console.WriteLine("maximum dose of" + structure.DicomType + " is:" + dvhData.MaxDose.ToString());
                                            AddData(dvhData, structure.Id, color, OxyPlot.MarkerType.Circle);
                                        }
                                    }
                                }
                            }

                        }
                        cur_app.ClosePatient(); //close the first patient
                    }


                    //Read second patient
                    if (patientSummary.Id == ptid2)   //this is the second patient 
                    {
                        VMS.TPS.Common.Model.API.Patient pt2 = cur_app.OpenPatient(patientSummary);
                        if (pt2 == null)
                            throw new ApplicationException("Cannot open patient No.2" + patientSummary.Id);
                        Console.WriteLine("===========================");

                        Console.WriteLine("Open patient No.2");
                        //loop through courses
                        foreach (var course in pt2.Courses)
                        {
                            Console.WriteLine("course=" + course.Id);  //print course name and send to UI
                            foreach (var planSetup in course.PlanSetups)
                            {
                                Console.WriteLine("planID=" + planSetup.Id);

                                // get structure Set
                                StructureSet structureSet = planSetup.StructureSet;

                                foreach (var structure in structureSet.Structures)
                                {
                                    if (colors.ContainsKey(structure.Id.ToLower()) || colors.ContainsKey(structure.DicomType.ToLower()))
                                    {
                                        DVHData dvhData2 = planSetup.GetDVHCumulativeData(structure,
                                                DoseValuePresentation.Absolute,
                                                VolumePresentation.Relative, 1);

                                        if (dvhData2 == null)
                                        {
                                            continue;
                                            throw new ApplicationException("DVH data does not exist. Script execution cancelled.");
                                        }


                                        //To print and anlyze the DVH 
                                        //

                                        if (colors.ContainsKey(structure.Id.ToLower()))
                                        {
                                            OxyPlot.OxyColor color = colors[structure.Id.ToLower()];
                                            Console.WriteLine("maximum dose of" + structure.Id + " is:" + dvhData2.MaxDose.ToString());
                                            AddData(dvhData2, structure.Id, color, OxyPlot.MarkerType.Cross);
                                        }
                                        else if (colors.ContainsKey(structure.DicomType.ToLower()))
                                        {
                                            OxyPlot.OxyColor color = colors[structure.DicomType.ToLower()];
                                            Console.WriteLine("maximum dose of" + structure.Id + " is:" + dvhData2.MaxDose.ToString());
                                            AddData(dvhData2, structure.Id, color, OxyPlot.MarkerType.Cross);
                                        }
                                    }
                                }
                            }
                        }
                        cur_app.ClosePatient(); //close the second patient
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                cur_app.ClosePatient();
            }

            plotModel.InvalidatePlot(true);
        }

        private TPReportData GetReportData(string patientstring)
        {
            CurCursor = Cursors.Wait;

            TPReportData returnvalue = new TPReportData();
            VMS.TPS.Common.Model.API.Patient curpat = null;
            VMS.TPS.Common.Model.API.PlanSetup curps = null;

            if (patientstring.Split(',').Count() == 3)
            {
                string patient_id = patientstring.Split(',')[0];
                string course_id = patientstring.Split(',')[1];
                string plansetup_id = patientstring.Split(',')[2];
                curpat = cur_app.OpenPatientById(patient_id);
                if (curpat != null) curps = curpat.Courses.Where(x => x.Id == course_id).Single().PlanSetups.Where(x => x.Id == plansetup_id).Single();
                if (curps != null)
                {
                    returnvalue.CurPatientInfo.FirstName = curpat.FirstName;
                    returnvalue.CurPatientInfo.LastName = curpat.LastName;
                    returnvalue.CurPatientInfo.PatientID = curpat.Id;


                    returnvalue.CurPlanInfo.CourseID = curps.Course.Id;
                    returnvalue.CurPlanInfo.PlaneID = curps.Id;


                    returnvalue.CurDosePrescription.NumberOfFraction = curps.UniqueFractionation.NumberOfFractions.Value.ToString();
                    returnvalue.CurDosePrescription.PrescribedDoseFractination = Math.Round(curps.UniqueFractionation.PrescribedDosePerFraction.Dose, 1).ToString();
                    returnvalue.CurDosePrescription.PlanNormalization = Math.Round(curps.PlanNormalizationValue, 1).ToString();
                    returnvalue.CurDosePrescription.TotalPrescribedDose = Math.Round(curps.TotalPrescribedDose.Dose, 1).ToString();


                    FieldInfo curfi = new FieldInfo();
                    foreach (Beam b in curps.Beams)
                    {
                        curfi = new FieldInfo();
                        curfi.BeamID = b.Id;
                        curfi.Collimator = Math.Round(b.ControlPoints[0].CollimatorAngle, 1).ToString();
                        curfi.Gantry = Math.Round(b.ControlPoints[0].GantryAngle, 1).ToString();
                        curfi.Couch = Math.Round(b.ControlPoints[0].PatientSupportAngle, 1).ToString();
                        curfi.Energy = b.EnergyModeDisplayName;
                        curfi.Machine = b.TreatmentUnit.Id;
                        curfi.MU = Math.Round(b.Meterset.Value, 1).ToString();
                        curfi.X1 = Math.Round(b.ControlPoints[0].JawPositions.X1 / 10.0f, 1).ToString();
                        curfi.X2 = Math.Round(b.ControlPoints[0].JawPositions.X2 / 10.0f, 1).ToString();
                        curfi.Y1 = Math.Round(b.ControlPoints[0].JawPositions.Y1 / 10.0f, 1).ToString();
                        curfi.Y2 = Math.Round(b.ControlPoints[0].JawPositions.Y2 / 10.0f, 1).ToString();
                        returnvalue.CurFieldInfoList.Add(curfi);
                    }

                    StructureData cursd = new StructureData();
                    DVHData dvhd = null;
                    foreach (Structure s in curps.StructureSet.Structures)
                    {
                        if (colors.ContainsKey(s.Id.ToLower()) || colors.ContainsKey(s.DicomType.ToLower()))
                        {
                            if (!s.IsEmpty && s.Volume > 0.0f)
                            {
                                cursd = new StructureData();
                                cursd.StructureID = s.Id;
                                cursd.Type = s.DicomType;
                                cursd.Volume = Math.Round(s.Volume, 1).ToString();
                                dvhd = curps.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 1.0f);
                                if (dvhd != null)
                                {
                                    cursd.MaxDose = Math.Round(dvhd.MaxDose.Dose, 1).ToString();
                                    cursd.MinDose = Math.Round(dvhd.MinDose.Dose, 1).ToString();
                                }
                                returnvalue.CurStructureDataList.Add(cursd);
                            }
                        }
                    }
                }

            }
            cur_app.ClosePatient();
            CurCursor = Cursors.Arrow;
            return returnvalue;
        }
        private void SetUpModel()
        {
            plotModel.LegendTitle = "Legend";
            plotModel.LegendOrientation = LegendOrientation.Horizontal;
            plotModel.LegendPlacement = LegendPlacement.Outside;
            plotModel.LegendPosition = LegendPosition.TopRight;
            plotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            plotModel.LegendBorder = OxyColors.Black;

            var dateAxis = new OxyPlot.Axes.LinearAxis(OxyPlot.Axes.AxisPosition.Bottom, "Dose") { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, IntervalLength = 80 };
            plotModel.Axes.Add(dateAxis);
            var valueAxis = new OxyPlot.Axes.LinearAxis(OxyPlot.Axes.AxisPosition.Left, 0, 100, "Volume") { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Volume" };
            plotModel.Axes.Add(valueAxis);
        }

        public void AddData(VMS.TPS.Common.Model.API.DVHData dvh, string name, OxyColor color, MarkerType marker)
        {
            var lineSerie = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 3,
                MarkerStroke = color,
                MarkerType = marker,
                Color = color,
                CanTrackerInterpolatePoints = false,
                Title = name,
                Smooth = false,
            };

            dvh.CurveData.ToList().ForEach(d => lineSerie.Points.Add(new DataPoint(d.DoseValue.Dose, d.Volume)));
            PlotModel.Series.Add(lineSerie);
        }

        public void ExportPDF()
        {
            const string DestinationDirectory = @"c:\variandeveloper";
            if (!Directory.Exists(DestinationDirectory))
            {
                Directory.CreateDirectory(DestinationDirectory);
            }

            // A4
            const double Width = 297 / 25.4 * 72;
            const double Height = 210 / 25.4 * 72;


            if (plotModel == null)
            {
                return;
            }

            var path = Path.Combine(DestinationDirectory, StringHelper.CreateValidFileName("test", ".pdf"));
            using (var s = File.Create(path))
            {
                try
                {
                    PdfExporter.Export(plotModel, s, Width, Height);
                }
                catch (Exception ex)
                {
                    //Debug.Assert.Fail(ex.Message);
                }
            }

            //Debug.Assert.IsTrue(File.Exists(path));
        }

        private void InitParams()
        {            
            // Initialise to zero...

            plotModel = new PlotModel();
            SetUpModel();

            colors["rectum"] = OxyPlot.OxyColor.FromRgb(255, 0, 0);
            colors["bladder"] = OxyPlot.OxyColor.FromRgb(0, 255, 0);
            colors["ptv"] = OxyPlot.OxyColor.FromRgb(0, 0, 255);
            Patient1 = "B_002,C1,REPLAN";
            Patient2 = "B_001,C1,IMRT";
        }

        public MainWindowModel()
        {
            InitParams();
        }

        public MainWindowModel(VMS.TPS.Common.Model.API.Application app)
        {
            cur_app = app;
            
            InitParams();
        }

        

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        //The XAML interfaces to methods that are interfaces of the ICommand interface. We have to wire up the method that does the work with the delegate and interface
        private DelegateCommand printreport;
        public ICommand PrintReport
        {
            get
            {
                if (printreport == null) printreport = new DelegateCommand(PrintReport_Method);
                return printreport;
            }

        }
        private void PrintReport_Method(object obj)
        {
            System.Windows.Controls.PrintDialog printDlg = new System.Windows.Controls.PrintDialog();
            if (printDlg.ShowDialog() == true)
            {
                //PrintReportDocumentPaginator prp = new PrintReportDocumentPaginator(eclipsedatamodel.CurrentPatient, eclipsedatamodel.CurrentPlanSetup,new Typeface("Calibri"),12,96*0.75,new Size(printDlg.PrintableAreaWidth,printDlg.PrintableAreaHeight));
                PrintReportDocumentPaginator prp = new PrintReportDocumentPaginator(rd1,rd2, new System.Windows.Media.Typeface("Calibri"), 12, 96 * 0.75, new Size(printDlg.PrintableAreaWidth, printDlg.PrintableAreaHeight));
                printDlg.PrintDocument(prp, "Print Report");
                Console.WriteLine("Here");

            }
        }

        //The XAML interfaces to methods that are interfaces of the ICommand interface. We have to wire up the method that does the work with the delegate and interface
        private DelegateCommand printDVH;
        public ICommand PrintDVH
        {
            get
            {
                if (printDVH == null) printDVH = new DelegateCommand(PrintDVH_Method);
                return printDVH;
            }

        }
        private void PrintDVH_Method(object obj)
        {
            DrawDVHs();
        }

        private DelegateCommand pat1changed; //Declare a delegate command
        public ICommand Pat1Changed  //wire up the method CalcTotalMU_Method to the delegate command as an ICommandInterface that can be used by the XAML
        {
            get
            {
                if (pat1changed == null) pat1changed = new DelegateCommand(OnPat1Changed);

                return pat1changed;

            }


        }
        private void OnPat1Changed(object obj)
        {
            Console.WriteLine("Pat1 has changed");
        }//This is the business end, use it to carry out the actual calcualtion
    }

    //This is a generic class that you need to include in the namespace of the classes using the INotifyProperty changed interface. It inherits from the ICommand interface so that the XAML can bind to it
    public class DelegateCommand : ICommand
    {
        readonly Action<object> _execute;
        readonly Predicate<object> _canExecute;

        public DelegateCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
