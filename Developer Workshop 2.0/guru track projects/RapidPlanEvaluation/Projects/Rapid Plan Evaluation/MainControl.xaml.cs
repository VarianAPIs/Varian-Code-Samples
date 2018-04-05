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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using VMS.TPS.Common.Model.API;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Configuration;

namespace RapidPlanEvaluation
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {

        private ViewModel _viewModel;
        private Random rnd = new Random();

        //Implemented dose metrics
        public enum DoseMetrics
        {
            [DescriptionAttribute("Mean Dose")]
            DMean,
            [DescriptionAttribute("DMax(Gy)")]
            DMax,
            [DescriptionAttribute("D0.1cc(Gy)")]
            D0_1cc,
            [DescriptionAttribute("D95%(Gy)")]
            D95p,
            [DescriptionAttribute("D5%(Gy)")]
            D5p,
            [DescriptionAttribute("V33%(cc)")]
            V33p,
            [DescriptionAttribute("NTCP")]
            NTCP
        };

        public static readonly DependencyProperty MetricListPropety =
            DependencyProperty.Register("MetricList", typeof(List<string>), typeof(MainControl));
        public List<string> MetricList
        {
            get { return (List<string>)GetValue(MetricListPropety); }
            set { SetValue(MetricListPropety, value); }
        }

        //Public properties used to receive data from main program
        private StructureSet _structureSet;
        public StructureSet StructureSet
        {
            get { return _structureSet; }

            set
            {
                _structureSet = value;
                foreach (Structure structure in _structureSet.Structures.OrderBy(s => s.Id))
                {
                    VMStructure myStruc = new VMStructure();
                    myStruc.Id = structure.Id;
                    myStruc.IsSelected = false;
                    myStruc.UpdatingNow = false;
                    myStruc.Color = structure.Color;

                    myStruc.IsTarget = (structure.DicomType == "CTV" || structure.DicomType == "GTV" || structure.DicomType == "PTV");

                    //add a metric per strucure
                    VMMetric metric = new VMMetric();
                    myStruc.DoseMetrics.Add(metric);

#if false
                    //If planning items in scope was received, initialize dose metrics and volume lists
                    if (PItemsInScope != null)
                        foreach (PlanningItem pitem in PItemsInScope)
                        {
                            metric.BeamMetrics.Add(0);

                            Structure apiStruc = null;
                            if (pitem is PlanSetup)
                            {
                                apiStruc = (pitem as PlanSetup).StructureSet.Structures.FirstOrDefault(s => s.Id == myStruc.Id);
                            }
                            else if (pitem is PlanSum)
                            {
                                apiStruc = (pitem as PlanSum).StructureSet.Structures.FirstOrDefault(s => s.Id == myStruc.Id);
                            }
                            if (apiStruc != null)
                                if (!double.IsNaN(apiStruc.Volume))
                                    myStruc.Volume.Add(0.01 * Convert.ToInt32(apiStruc.Volume * 100));
                            myStruc.FieldNames.Add(pitem.Id);
                        }
#endif
                    _viewModel.Structures.Add(myStruc);
                }

                //DVHDataModel.Instance.StructureSet = _structureSet;
            }
        }

        public PlanSetup StdPlan { get; set; }
        public PlanSetup RapidPlan { get; set; }

        //private List<PlanningItem> _pItemsInScope;
        //public List<PlanningItem> PItemsInScope
        //{
        //    get { return _pItemsInScope; }
        //    set
        //    {
        //        _pItemsInScope = value;

        //        //Fill list of dose metrics and volume table column titles
        //        int i = 0;
        //        foreach (var pitem in _pItemsInScope)
        //        {
        //            //create template column with ItemsControl to display all metrics for this beam in a give structure
        //            DataGridTemplateColumn tmpCol = new DataGridTemplateColumn();
        //            tmpCol.Header = pitem.Id;

        //            FrameworkElementFactory itmCtrlFactory = new FrameworkElementFactory(typeof(ItemsControl));
        //            itmCtrlFactory.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("DoseMetrics"));

        //            DataTemplate itmCtrlTemplate = new DataTemplate();
        //            itmCtrlTemplate.VisualTree = itmCtrlFactory;

        //            //add textblock to itemscontrol
        //            FrameworkElementFactory txtFactory = new FrameworkElementFactory(typeof(TextBlock));
        //            txtFactory.SetBinding(TextBlock.TextProperty, new Binding("BeamMetrics[" + i + "]")
        //            {
        //                Converter = new NoZeroConverter()
        //            });
        //            txtFactory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        //            txtFactory.SetValue(TextBlock.MarginProperty, new Thickness(5, 0, 0, 0));
        //            txtFactory.SetValue(TextBlock.HeightProperty, 25d);
        //            DataTemplate txtTemplate = new DataTemplate();
        //            txtTemplate.VisualTree = txtFactory;

        //            itmCtrlFactory.SetValue(ItemsControl.ItemTemplateProperty, txtTemplate);

        //            //add ItemsControl to column and column to datagrid
        //            tmpCol.CellTemplate = itmCtrlTemplate;
        //            dtaMetrics.Columns.Add(tmpCol);

        //            i++;

        //            //if strutureset has been received, initialize dose metric and volume lists
        //            if (StructureSet == null)
        //                throw new ApplicationException("StructureSet property not set");

        //            foreach (var myStruc in _viewModel.Structures)
        //            {
        //                myStruc.DoseMetrics[0].BeamMetrics.Add(0);
        //                Structure apiStruc = null;
        //                if (pitem is PlanSetup)
        //                {
        //                    apiStruc = (pitem as PlanSetup).StructureSet.Structures.FirstOrDefault(s => s.Id == myStruc.Id);
        //                }
        //                else if (pitem is PlanSum)
        //                {
        //                    apiStruc = (pitem as PlanSum).StructureSet.Structures.FirstOrDefault(s => s.Id == myStruc.Id);
        //                }
        //                if (apiStruc != null)
        //                    if (!double.IsNaN(apiStruc.Volume))
        //                        myStruc.Volume.Add(0.01 * Convert.ToInt32(apiStruc.Volume * 100));
        //                myStruc.FieldNames.Add(pitem.Id);
        //            }
        //        }

        //        //Add planning items to the data model
        //        DVHDataModel.Instance.PlanningItems = _pItemsInScope;
        //    }
        //}

        private Patient _patient;
        public Patient patient
        {
            get { return _patient; }
            set
            {
                _patient = value;

               // DVHDataModel.Instance.Patient = _patient;
            }
        }

        private Course _course;
        public Course Course
        {
            get { return _course; }
            set
            {
                _course = value;

                //DVHDataModel.Instance.Course = _course;
            }
        }


        private User _user;
        public User user
        {
            get { return _user; }
            set
            {
                _user = value;
            }
        }


        //MainControl constructor. This is where execution of this control starts
        public MainControl()
        {
            InitializeComponent();


            _viewModel = new ViewModel();
            this.DataContext = _viewModel;

            //populate default metric list
            MetricList = new List<string>();
            foreach (Enum dmetric in Enum.GetValues(typeof(DoseMetrics)))
                MetricList.Add(EnumUtils.stringValueOf(dmetric));

            //add custom defined metrics
            CustomMetricsConfiguration config = Myconfig.GetCustomMetricsSection();
            foreach (CustomMetricElement metric in config.Metrics)
                MetricList.Add(metric.Name);
        }


        private void chkStructures_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            TextBlock txt = check.Content as TextBlock;
            string strucId = txt.Text;

            VMStructure struc = _viewModel.Structures.FirstOrDefault(s => s.Id == strucId);

            DefaultMetricsConfiguration config = Myconfig.GetDefaultMetricsSection();
            if (config != null)
            {
                foreach (DefaultMetricElement met in config.Metrics)
                {
                    if (struc.Id.ToUpper().Contains(met.Structure.ToUpper()))
                    {
                        struc.DoseMetrics[0].Name = met.Metric;
                        UpdateDoseMetrics(struc, struc.DoseMetrics[0]);
                        break;
                    }
                }
            }

        }

        private void chkStructures_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = sender as CheckBox;
            TextBlock txt = check.Content as TextBlock;
            string strucId = txt.Text;

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {

                Cursor = Cursors.Wait;

                VMMetric selMetric = ((FrameworkElement)sender).DataContext as VMMetric;
                ComboBox cmb = sender as ComboBox;
                DataGridRow row = dtaMetrics.ContainerFromElement(cmb) as DataGridRow;
                if (row == null)
                    return;

                int rowIndex = row.GetIndex();
                VMStructure struc = dtaMetrics.Items[rowIndex] as VMStructure;
                if (cmb.SelectedValue != null)
                {
                    selMetric.Name = cmb.SelectedValue.ToString();

                    if (selMetric.Name == "NTCP")
                    {
                        if (selMetric.NTCPParameters.LKBn == 0)
                        {
                            SiteChooser siteChoose = new SiteChooser();
                            siteChoose.ShowDialog();
                            selMetric.NTCPParameters.AlphaBeta = siteChoose.AlphaBeta;
                            selMetric.NTCPParameters.LKBn = siteChoose.LKBn;
                            selMetric.NTCPParameters.LKBm = siteChoose.LKBm;
                            selMetric.NTCPParameters.LKBD50 = siteChoose.LKBD50;
                        }
                    }

                    UpdateDoseMetrics(struc, selMetric);

                    selMetric.UseBioDose = false;
                    if (MetricUsesGy(selMetric.Name))
                        selMetric.CanUseBiodose = true;
                    else if (selMetric.Name == "NTCP")
                        selMetric.UseBioDose = true;
                    else
                        selMetric.CanUseBiodose = false;

                }

                Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateDoseMetrics(VMStructure struc, VMMetric selMetric)
        {
            Cursor = Cursors.Wait;
            Structure structure = StructureSet.Structures.FirstOrDefault(s => s.Id == struc.Id);
            if (structure == null)
                throw new ApplicationException("Could not find Structure");
            int i = 0;
            selMetric.StdPlanMetric = CalculateDoseMetric(StdPlan, selMetric, structure);
            selMetric.RapidPlanMetric = CalculateDoseMetric(RapidPlan, selMetric, structure);
            selMetric.RapidPlanEstimateMetric = CalculateDoseMetric(RapidPlan, selMetric, structure, true);

            Cursor = Cursors.Arrow;
        }


        private double CalculateDoseMetric(PlanSetup plan, VMMetric metric, Structure structure, bool UseRapPlanEstimate=false)
        {
            double metricValue = -1;
            try
            {
                if (metric.Name == EnumUtils.stringValueOf(DoseMetrics.DMean))
                    metricValue = Calc.DMean(structure, plan, metric,UseRapPlanEstimate);
                else if (metric.Name == EnumUtils.stringValueOf(DoseMetrics.DMax))
                    metricValue = Calc.DMax(structure, plan, metric,UseRapPlanEstimate);
                else if (metric.Name == EnumUtils.stringValueOf(DoseMetrics.D0_1cc))
                    metricValue = Calc.D0_1cc(structure, plan, metric,UseRapPlanEstimate);
                else if (metric.Name == EnumUtils.stringValueOf(DoseMetrics.D95p))
                    metricValue = Calc.D95p(structure, plan, metric,UseRapPlanEstimate);
                else if (metric.Name == EnumUtils.stringValueOf(DoseMetrics.D5p))
                    metricValue = Calc.D5p(structure, plan, metric,UseRapPlanEstimate);
                else if (metric.Name == EnumUtils.stringValueOf(DoseMetrics.V33p))
                    metricValue = Calc.V33p(structure, plan, metric,UseRapPlanEstimate);
                else if (metric.Name == EnumUtils.stringValueOf(DoseMetrics.NTCP))
                    metricValue = Calc.NTCP(structure, plan, metric,UseRapPlanEstimate);
                else
                {
                    //check if custom metric
                    metricValue = Calc.CustomMetric(structure, metric, plan, UseRapPlanEstimate);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (metricValue == -1)
                throw new ApplicationException("The Dose Metric " + metric.Name + " is not implemented");

            return metricValue;
        }

//        private void btnExcel_Click(object sender, RoutedEventArgs e)
//        {
//            Cursor = Cursors.Wait;
//            List<string> displayHeaders = new List<string>();

//            for (int i = 0; i < dtaMetrics.Columns.Count; i++)
//            {
//                DataGridColumn col = dtaMetrics.Columns.FirstOrDefault(s => s.DisplayIndex == i);
//                if (col.Header.ToString() != "ROI" && col.Header.ToString() != "Dose Point")
//                    displayHeaders.Add(col.Header.ToString());
//            }

//            ExcelDocument.CreateDocument(displayHeaders, _viewModel.Structures.ToList<VMStructure>(), PItemsInScope, (bool)chkDVH.IsChecked);
//            Cursor = Cursors.Arrow;
//#if(false)
//            //get the order of the columns in the display
//            List<DataGridColumn> dtgCols = dtaComparison.Columns.ToList();
//            List<string> displayHeaders = new List<string>();

//            for (int i = 0; i < dtaComparison.Columns.Count; i++)
//            {
//                DataGridColumn col = dtaComparison.Columns.FirstOrDefault(s => s.DisplayIndex == i);
//                if (col.Header.ToString() != "ROI" && col.Header.ToString() != "Dose Point")
//                    displayHeaders.Add(col.Header.ToString());
//            }

            
//            string path = string.Empty;
//            var dialog = new Microsoft.Win32.SaveFileDialog();
//            dialog.Filter = "Excel Docuents|*.csv";
//            while (path == string.Empty)
//            {
//                Nullable<bool> res = dialog.ShowDialog();
//                if (res == true)
//                    path = dialog.FileName;
//                else
//                    return;
//            }

//            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(path,false,Encoding.ASCII))
//            {
//                string headers = "ROI, Dose Point";
//                foreach (var header in displayHeaders)
//                    headers += ", " + header;
//                sw.WriteLine(headers);

//                foreach (MyStructure structure in _viewModel.Structures.Where(s=>s.IsSelected))
//                {
//                    string line = structure.Id + ", " + structure.Metric;
//                    foreach (var header in displayHeaders)
//                    {
//                        //find field
//                        int fieldIndex = PItemsInScope.FindIndex(s => s.Id == header);
//                        if (fieldIndex == -1) continue;

//                        line += ", " + structure.DoseMetric[fieldIndex];
//                    }
//                    sw.WriteLine(line);
//                }
//            }
//#endif
//        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void imgAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var struc in _viewModel.Structures)
                struc.IsSelected = true;
        }

        private void imgNone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var struc in _viewModel.Structures)
                struc.IsSelected = false;
        }

        //private void btnDVH_Click(object sender, RoutedEventArgs e)
        //{
        //    UMRO.Utils.DVHViewer.DVHViewer viewer = new UMRO.Utils.DVHViewer.DVHViewer();
        //    viewer.PrescribedDose = (pItemOpen as PlanSetup).TotalPrescribedDose.Dose;

        //    foreach (var structure in _viewModel.Structures.Where(s => s.IsSelected))
        //    {
        //        foreach (var plan in PItemsInScope)
        //        {
        //            MyDVHData dvh = DVHDataModel.Instance.GetStdDVH(structure.Id, plan.Id, VMS.TPS.Common.Model.Types.DoseValuePresentation.Absolute, VMS.TPS.Common.Model.Types.VolumePresentation.AbsoluteCm3);
        //            if (dvh != null)
        //            {
        //                Point[] dvhPoints = new Point[dvh.CurveData.Count()];
        //                for (int i = 0; i < dvh.CurveData.Count(); i++)
        //                {
        //                    dvhPoints[i].X = dvh.CurveData[i].Dose;
        //                    dvhPoints[i].Y = dvh.CurveData[i].Volume;
        //                }

        //                viewer.AddCumulativeDVH(structure.Id + "-" + plan.Id, structure.Color, dvhPoints, "Gy");
        //            }
        //            //check if a biological corrected DVH has beem calculated
        //            VMMetric metric = structure.DoseMetrics.FirstOrDefault(s => s.UseBioDose);
        //            if (metric != null)
        //            {
        //                MyDVHData bioDvh = DVHDataModel.Instance.GetBioDVH(structure.Id, plan.Id, VMS.TPS.Common.Model.Types.VolumePresentation.AbsoluteCm3, metric);
        //                if (bioDvh != null)
        //                {
        //                    Point[] dvhPoints = new Point[bioDvh.CurveData.Count()];
        //                    for (int i = 0; i < bioDvh.CurveData.Count(); i++)
        //                    {
        //                        dvhPoints[i].X = bioDvh.CurveData[i].Dose;
        //                        dvhPoints[i].Y = bioDvh.CurveData[i].Volume;
        //                    }

        //                    viewer.AddCumulativeDVH(structure.Id + "-" + plan.Id + "(EQD2)", structure.Color, dvhPoints, "Gy");
        //                }

        //            }
        //        }
        //    }

        //    Window window = new Window();
        //    window.Content = viewer;
        //    window.ShowDialog();
        //}


        private void imgAddMetric_MouseDown(object sender, MouseButtonEventArgs e)
        {
            VMStructure stru = ((FrameworkElement)sender).DataContext as VMStructure;
            VMMetric metric = new VMMetric();

            stru.DoseMetrics.Add(metric);
        }

#if(false)
        private void chkBioDose_Checked(object sender, RoutedEventArgs e)
        {
            VMMetric metric = ((FrameworkElement)sender).DataContext as VMMetric;
            CheckBox chkbox = sender as CheckBox;
            DataGridRow row = dtaMetrics.ContainerFromElement(chkbox) as DataGridRow;
            if (row == null)
                return;

            int rowIndex = row.GetIndex();
            VMStructure struc = dtaMetrics.Items[rowIndex] as VMStructure;

            //Get NTCP Parameters
            if (metric.NTCPParameters.AlphaBeta == 0)
            {
                BioDoseConfiguration bioConfig = Myconfig.GetBioDoseConfiguration();
                if (struc.IsTarget)
                    metric.NTCPParameters.AlphaBeta = bioConfig.TargetAlphaBeta;
                else
                    metric.NTCPParameters.AlphaBeta = bioConfig.OrganAlphaBeta;
            }

            UpdateDoseMetrics(struc, metric);

        }

        private void chkBioDose_Unchecked(object sender, RoutedEventArgs e)
        {
            VMMetric metric = ((FrameworkElement)sender).DataContext as VMMetric;

            CheckBox chkbox = sender as CheckBox;
            DataGridRow row = dtaMetrics.ContainerFromElement(chkbox) as DataGridRow;
            if (row == null)
                return;

            int rowIndex = row.GetIndex();
            VMStructure struc = dtaMetrics.Items[rowIndex] as VMStructure;

            UpdateDoseMetrics(struc, metric);

            metric.ShowNTCPParams = false;
        }
#endif
        public static bool MetricUsesGy(string metricName)
        {
            if (metricName == null)
                return false;

            //first find if in custom metrics
            CustomMetricsConfiguration config = Myconfig.GetCustomMetricsSection();
            foreach (CustomMetricElement custMetric in config.Metrics)
                if (custMetric.Name == metricName)
                {
                    if (custMetric.Base.Contains("Gy"))
                        return true;
                    else
                        return false;
                }

            //not a custom metric
            if (metricName.Contains("Gy") || metricName.Contains("Mean"))
                return true;
            else
                return false;
        }

        private void imgBioParams_MouseDown(object sender, MouseButtonEventArgs e)
        {
            VMMetric metric = ((FrameworkElement)sender).DataContext as VMMetric;
            metric.ShowNTCPParams = !metric.ShowNTCPParams;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            double temp;
            e.Handled = !double.TryParse(e.Text, out temp);
        }

        //private void TextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    VMMetric metric = ((FrameworkElement)sender).DataContext as VMMetric;
        //    TextBox txtBox = sender as TextBox;
        //    string paramName = txtBox.Name;

        //    DataGridRow row = dtaMetrics.ContainerFromElement(txtBox) as DataGridRow;
        //    if (row == null)
        //        return;

        //    int rowIndex = row.GetIndex();
        //    VMStructure struc = dtaMetrics.Items[rowIndex] as VMStructure;
        //}

        //private void TextBox_KeyDown(object sender, KeyEventArgs e)
        //{
        //    TextBox txtBox = sender as TextBox;
        //    if (e.Key == Key.Enter)
        //        Keyboard.ClearFocus();
        //}

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Clear metric results to force recalc
            VMMetric metric = ((FrameworkElement)sender).DataContext as VMMetric;
            metric.ClearMetrics();
        }

        private void btnRecalc_Click(object sender, RoutedEventArgs e)
        {
            VMMetric metric = ((FrameworkElement)sender).DataContext as VMMetric;
            Button button = sender as Button;
            DataGridRow row = dtaMetrics.ContainerFromElement(button) as DataGridRow;
            if (row == null)
                return;

            int rowIndex = row.GetIndex();
            VMStructure struc = dtaMetrics.Items[rowIndex] as VMStructure;
            UpdateDoseMetrics(struc, metric);

            metric.ShowNTCPParams = false;
        }
    }

    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EnumUtils
    {
        public static string stringValueOf(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }
    }

    public class NoZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double input = (double)value;
            if (input == 0)
                return "";
            else
                return input;
        }

        public object ConvertBack(object value, Type tragetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

    }

    //public class BioDoseAllowedConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        string metricName = (string)value;
    //        if (metricName == null)
    //            return false;
    //        return MainControl.MetricUsesGy(metricName);
    //    }

    //    public object ConvertBack(object value, Type tragetType, object parameter, CultureInfo culture)
    //    {
    //        return DependencyProperty.UnsetValue;
    //    }
    //}

}
