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

namespace DVHPlot.ViewModel
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private PlotModel plotModel;
        public PlotModel PlotModel
        {
            get { return plotModel; }
            set { plotModel = value; OnPropertyChanged("PlotModel"); }
        }


        private string patient1 = "PatientID1,CourseID1,PlanSetupID1";
        private string patient2 = "PatientID2,CourseID2,PlanSetupID2";

        public string Patient1
        {
            get { return patient1; }
            set { patient1 = value;
            OnPropertyChanged("Patient1"); 
            if (patient1.Split(',').Count() == 3)
            {
                PatID_Pat1 = patient1.Split(',')[0];
            }
           
            
            }
        }

        public string Patient2
        {
            get { return patient2; }
            set { patient2 = value;
                OnPropertyChanged("Patient2");
                if (patient2.Split(',').Count() == 3)
                {
                    PatID_Pat2 = patient2.Split(',')[0];
                }
            }
        }

        public string patid_pat1="test";
        public string PatID_Pat1
        {
            get { return patid_pat1; }
            set{ patid_pat1 = value; 
                OnPropertyChanged("PatID_Pat1");
            }
        }

        public string patid_pat2 = "test2";
        public string PatID_Pat2
        {
            get { return patid_pat2; }
            set { patid_pat2 = value; OnPropertyChanged("PatID_Pat2"); }
        }

        private void SetUpModel()
        {
            plotModel.LegendTitle = "Legend";
            plotModel.LegendOrientation = LegendOrientation.Horizontal;
            plotModel.LegendPlacement = LegendPlacement.Outside;
            plotModel.LegendPosition = LegendPosition.TopRight;
            plotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            plotModel.LegendBorder = OxyColors.Black;

            var dateAxis = new OxyPlot.Axes.LinearAxis(OxyPlot.Axes.AxisPosition.Bottom,"Dose") { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, IntervalLength = 80 };
            plotModel.Axes.Add(dateAxis);
            var valueAxis = new OxyPlot.Axes.LinearAxis(OxyPlot.Axes.AxisPosition.Left, 0,100,"Volume") { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Volume" };
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

        public MainWindowModel()
        {
            plotModel = new PlotModel();
            SetUpModel();
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
                //PrintReportDocumentPaginator prp = new PrintReportDocumentPaginator(eclipsedatamodel, new Typeface("Calibri"), 12, 96 * 0.75, new Size(printDlg.PrintableAreaWidth, printDlg.PrintableAreaHeight));
                //printDlg.PrintDocument(prp, "Print Report");

            }
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
