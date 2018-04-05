////////////////////////////////////////////////////////////////////////////////
// DvhLookups.cs
//
//  Simple DVH Lookup tool.
//  
// Applies to:  ESAPI v11 and later.
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
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Controls;

namespace VMS.TPS
{
    public static class DvhExtensions
    {
        public static DoseValue GetDoseAtVolume(this PlanningItem pitem, Structure structure, double volume, VolumePresentation volumePresentation, DoseValuePresentation requestedDosePresentation)
        {
            if (pitem is PlanSetup)
            {
                return ((PlanSetup)pitem).GetDoseAtVolume(structure, volume, volumePresentation, requestedDosePresentation);
            }
            else
            {
                if (requestedDosePresentation != DoseValuePresentation.Absolute)
                    throw new ApplicationException("Only absolute dose supported for Plan Sums");
                DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, volumePresentation, 0.001);
                return DvhExtensions.DoseAtVolume(dvh, volume);
            }
        }
        public static double GetVolumeAtDose(this PlanningItem pitem, Structure structure, DoseValue dose, VolumePresentation requestedVolumePresentation)
        {
            if (pitem is PlanSetup)
            {
                return ((PlanSetup)pitem).GetVolumeAtDose(structure, dose, requestedVolumePresentation);
            }
            else
            {
                DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, requestedVolumePresentation, 0.001);
                return DvhExtensions.VolumeAtDose(dvh, dose.Dose);
            }
        }

        public static DoseValue DoseAtVolume(DVHData dvhData, double volume)
        {
            if (dvhData == null || dvhData.CurveData.Count() == 0)
                return DoseValue.UndefinedDose();
            double absVolume = dvhData.CurveData[0].VolumeUnit == "%" ? volume * dvhData.Volume * 0.01 : volume;
            if (volume < 0.0 || absVolume > dvhData.Volume)
                return DoseValue.UndefinedDose();

            DVHPoint[] hist = dvhData.CurveData;
            for (int i = 0; i < hist.Length; i++)
            {
                if (hist[i].Volume < volume)
                    return hist[i].DoseValue;
            }
            return DoseValue.UndefinedDose();
        }

        public static double VolumeAtDose(DVHData dvhData, double dose)
        {
            if (dvhData == null)
                return Double.NaN;

            DVHPoint[] hist = dvhData.CurveData;
            int index = (int)(hist.Length * dose / dvhData.MaxDose.Dose);
            if (index < 0 || index > hist.Length)
                return 0.0;//Double.NaN;
            else
                return hist[index].Volume;
        }
    }

    public class Script
    {
        public Script()
        {
        }

        //---------------------------------------------------------------------------------------------  
        public void Execute(ScriptContext context, Window window)
        {
            PlanSetup plan = context.PlanSetup;
            PlanSum psum = context.PlanSumsInScope.FirstOrDefault();
            if (plan == null && psum == null)
                return;

            window.Closing += new System.ComponentModel.CancelEventHandler(OnWindowClosing);
            window.Background = System.Windows.Media.Brushes.Cornsilk;
            window.Height = 120;
            window.Width = 1024;

            SelectedPlanningItem = plan != null ? (PlanningItem)plan : (PlanningItem)psum;
            // Plans in plansum can have different structuresets but here we only use structureset to allow chosing one structure
            SelectedStructureSet = plan != null ? plan.StructureSet : psum.PlanSetups.First().StructureSet;

            window.Title = "DVH Lookups for " + SelectedPlanningItem.Id + " / " + SelectedStructureSet.Id;

            if (SelectedPlanningItem.Dose == null)
                return;

            InitializeUI(window);
        }

        PlanningItem SelectedPlanningItem { get; set; }
        StructureSet SelectedStructureSet { get; set; }
        Structure SelectedStructure { get; set; }

        bool m_closing = false;
        //---------------------------------------------------------------------------------------------  
        void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_closing = true;
        }

        //---------------------------------------------------------------------------------------------  
        void InitializeUI(Window window)
        {
            StackPanel rootPanel = new StackPanel();
            rootPanel.Orientation = Orientation.Horizontal;

            // Structure selection and info
            {
                GroupBox structureGroup = new GroupBox();
                structureGroup.Header = "Structure";
                rootPanel.Children.Add(structureGroup);

                StackPanel structurePanel = new StackPanel();
                structurePanel.Orientation = Orientation.Horizontal;

                ComboBox structureCombo = new ComboBox();
                structureCombo.ItemsSource = SelectedStructureSet.Structures;
                structureCombo.SelectionChanged += new SelectionChangedEventHandler(OnComboSelectionChanged);
                structureCombo.MinWidth = 160.0;

                Label volumeLabel = new Label();
                volumeLabel.Content = "Volume (cm3)";
                m_structureVolume.Height = 25.0;
                m_structureVolume.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                structureGroup.Content = structurePanel;

                structurePanel.Children.Add(structureCombo);
                structurePanel.Children.Add(volumeLabel);
                structurePanel.Children.Add(m_structureVolume);

                m_absDoseCheckbox.Content = "AbsDose";
                m_absDoseCheckbox.VerticalAlignment = VerticalAlignment.Center;
                m_absDoseCheckbox.Checked += new RoutedEventHandler(CheckBoxChanged);
                m_absDoseCheckbox.Unchecked += new RoutedEventHandler(CheckBoxChanged);
                if (SelectedPlanningItem is PlanSum)
                {
                    // only absolute dose for plansums
                    m_absDoseCheckbox.IsChecked = true;
                    m_absDoseCheckbox.IsEnabled = false;
                }
                structurePanel.Children.Add(m_absDoseCheckbox);

                m_absVolCheckbox.Content = "AbsVol";
                m_absVolCheckbox.VerticalAlignment = VerticalAlignment.Center;
                m_absVolCheckbox.Checked += new RoutedEventHandler(CheckBoxChanged);
                m_absVolCheckbox.Unchecked += new RoutedEventHandler(CheckBoxChanged);
                structurePanel.Children.Add(m_absVolCheckbox);
            }

            // DVH lookup controls
            {
                GroupBox dvhGroup = new GroupBox();
                dvhGroup.Header = "DVH";
                rootPanel.Children.Add(dvhGroup);

                StackPanel dvhPanel = new StackPanel();
                dvhPanel.Orientation = Orientation.Horizontal;

                m_volumeTextBox.TextChanged += new TextChangedEventHandler(OnInputChanged);
                m_doseTextBox.TextChanged += new TextChangedEventHandler(OnInputChanged);

                dvhGroup.Content = dvhPanel;

                m_volumeAtDoseLabel.Content = "Volume at Dose";
                m_doseAtVolumeLabel.Content = "Dose at Volume";

                dvhPanel.Children.Add(m_volumeAtDoseLabel);
                dvhPanel.Children.Add(m_doseTextBox);
                dvhPanel.Children.Add(m_volumeAtDoseResultLabel);
                dvhPanel.Children.Add(m_resultVolumeAtDose);

                dvhPanel.Children.Add(m_doseAtVolumeLabel);
                dvhPanel.Children.Add(m_volumeTextBox);
                dvhPanel.Children.Add(m_doseAtVolumeResultLabel);
                dvhPanel.Children.Add(m_resultDoseAtVolume);
            }

            // Layout
            {
                m_structureVolume.MinWidth = 60.0;
                m_volumeTextBox.MinWidth = 60.0;
                m_doseTextBox.MinWidth = 60.0;

                m_resultVolumeAtDose.MinWidth = 60.0;
                m_resultDoseAtVolume.MinWidth = 60.0;
                rootPanel.Height = 60.0;
            }
            window.Content = rootPanel;
        }

        void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            UpdateDvhLookup();
        }

        TextBlock m_structureVolume = new TextBlock();
        TextBox m_volumeTextBox = new TextBox();
        Label m_resultVolumeAtDose = new Label();
        TextBox m_doseTextBox = new TextBox();
        Label m_resultDoseAtVolume = new Label();
        CheckBox m_absDoseCheckbox = new CheckBox();
        CheckBox m_absVolCheckbox = new CheckBox();
        Label m_doseAtVolumeLabel = new Label();
        Label m_volumeAtDoseLabel = new Label();

        Label m_doseAtVolumeResultLabel = new Label();
        Label m_volumeAtDoseResultLabel = new Label();

        //---------------------------------------------------------------------------------------------  
        void OnInputChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDvhLookup();
        }
        static double s_binWidth = 0.001;

        //---------------------------------------------------------------------------------------------  
        void UpdateDvhLookup()
        {
            if (m_closing || SelectedStructure == null)
                return;

            bool doseAbsolute = m_absDoseCheckbox.IsChecked.Value;
            bool volAbsolute = m_absVolCheckbox.IsChecked.Value;

            m_volumeAtDoseResultLabel.Content = "";
            m_doseAtVolumeResultLabel.Content = "";

            m_resultVolumeAtDose.Content = "";
            m_resultDoseAtVolume.Content = "";
            m_structureVolume.Text = "";

            double inputVolume = Double.NaN;
            if (m_volumeTextBox.Text != null)
            {
                Double.TryParse(m_volumeTextBox.Text, out inputVolume);
            }

            double inputDose = Double.NaN;
            if (m_doseTextBox.Text != null)
            {
                Double.TryParse(m_doseTextBox.Text, out inputDose);
            }

            DoseValuePresentation dosePres = doseAbsolute ? DoseValuePresentation.Absolute : DoseValuePresentation.Relative;
            VolumePresentation volPres = volAbsolute ? VolumePresentation.AbsoluteCm3 : VolumePresentation.Relative;

            DVHData dvhData = SelectedPlanningItem.GetDVHCumulativeData(SelectedStructure, dosePres, volPres, s_binWidth);
            m_structureVolume.Text = dvhData.Volume.ToString("F5");

            if (SelectedPlanningItem != null && SelectedPlanningItem.Dose != null && SelectedStructure != null)
            {
                if (!Double.IsNaN(inputVolume))
                {
                    DoseValue val = SelectedPlanningItem.GetDoseAtVolume(SelectedStructure, inputVolume, volPres, dosePres);
                    DoseValue controlVal = DvhExtensions.DoseAtVolume(dvhData, inputVolume);
                    double err = Math.Abs((val.Dose - controlVal.Dose) / val.Dose);
                    if (err > 0.001)
                    {
                        MessageBox.Show("Value : " + val.ToString() + " Control Val : " + controlVal.ToString());
                    }
                    m_resultDoseAtVolume.Content = val.ToString();

                    string doseAtVolumeResultType = volAbsolute ? "cm3 D(" : "% D(";
                    doseAtVolumeResultType += inputVolume.ToString("F1") + (volAbsolute ? "cm3" : "%") + ") =";
                    m_doseAtVolumeResultLabel.Content = doseAtVolumeResultType;
                }
                if (!Double.IsNaN(inputDose))
                {
                    DoseValue.DoseUnit doseUnit = dvhData.MaxDose.Unit;
                    double vol = SelectedPlanningItem.GetVolumeAtDose(SelectedStructure, new DoseValue(inputDose, doseUnit), volPres);

                    double controlVal = DvhExtensions.VolumeAtDose(dvhData, inputDose);
                    double err = Math.Abs((vol - controlVal) / vol);
                    if (err > 0.001)
                    {
                        MessageBox.Show("Value : " + vol.ToString("F3") + " Control Val : " + controlVal.ToString("F3"));
                    }

                    m_resultVolumeAtDose.Content = vol.ToString("F5") + (volAbsolute ? "cm3" : "%");

                    string volumeAtDoseResultType = doseUnit.ToString() + " V(";
                    volumeAtDoseResultType += inputDose.ToString("F1") + doseUnit.ToString() + " ) =";
                    m_volumeAtDoseResultLabel.Content = volumeAtDoseResultType;
                }
            }
        }
        //---------------------------------------------------------------------------------------------  
        private void OnComboSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                Structure structure = e.AddedItems[0] as Structure;
                if (structure != null)
                {
                    SelectedStructure = structure;
                    UpdateDvhLookup();
                }
            }
        }
        //---------------------------------------------------------------------------------------------  
        private void OnPercentageTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateDvhLookup();
        }
    }
}
