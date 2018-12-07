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
using VMS.TPS.Common.Model.Types;
using Example_DVH.Models;

namespace Example_DVH
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        public MainControl()
        {
            InitializeComponent();
        }
        public PlanSetup ps;
        public void DrawDVH(DVHData dvhData, Structure s)
        {
            // Calculate multipliers for scaling DVH to canvas.
            ps.DoseValuePresentation = DoseValuePresentation.Absolute;
            double xCoeff = MainCanvas.Width / ps.Dose.DoseMax3D.Dose;
            double yCoeff = MainCanvas.Height / 100;

            // Set Y axis label
            DoseMaxLabel.Content = string.Format("{0:F1}", ps.Dose.DoseMax3D.ToString());
            TextBlock tb = new TextBlock();
            tb.Text = string.Format("{0}: Max Dose: {1}; Min Dose: {2}; Mean Dose: {3}",
                s.Id, dvhData.MaxDose.ToString(), dvhData.MinDose.ToString(), dvhData.MeanDose.ToString());
            stats_sp.Children.Add(tb);
            // Draw histogram 
            for (int i = 0; i < dvhData.CurveData.Length - 1; i++)
            {
                // Set drawing line parameters
                var line = new Line() { Stroke = new SolidColorBrush(s.Color), StrokeThickness = 4.0 };

                // Set line coordinates
                line.X1 = dvhData.CurveData[i].DoseValue.Dose * xCoeff;
                line.X2 = dvhData.CurveData[i + 1].DoseValue.Dose * xCoeff;
                // Y axis start point is top-left corner of window, convert it to bottom-left.
                line.Y1 = MainCanvas.Height - dvhData.CurveData[i].Volume * yCoeff;
                line.Y2 = MainCanvas.Height - dvhData.CurveData[i + 1].Volume * yCoeff;

                // Add line to the existing canvas
                MainCanvas.Children.Add(line);
            }
        }
        public void Cb_Checked(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            Structure s = ps.StructureSet.Structures.First(x => x.Id == (sender as CheckBox).Content.ToString());
            DVHData dvhData = ps.GetDVHCumulativeData(s,
                                          DoseValuePresentation.Absolute,
                                          VolumePresentation.Relative,1);
            DrawDVH(dvhData, s);
        }

        private void dqmTemplate_cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //dqm_dg.ItemsSource = ps.Beams;
            List<DQM> dqms = DQM.get_Vals(dqmTemplate_cmb.SelectedItem.ToString().Split(':').Last().Trim());
            foreach(DQM dqm in dqms)
            {
                if(dqm.dqmType)
                {
                    dqm.dosValue = ps.GetDoseAtVolume(
                        ps.StructureSet.Structures.First(x => dqm.structureNames.Contains(x.Id)),
                        Convert.ToDouble(dqm.volValue),
                        dqm.volType ? VolumePresentation.AbsoluteCm3 : VolumePresentation.Relative,
                        dqm.dosType ? DoseValuePresentation.Absolute : DoseValuePresentation.Relative).ToString();
                }
                else
                {
                    //volume at dose.
                }
            }
            dqm_dg.ItemsSource = dqms;
        }
    }
}
