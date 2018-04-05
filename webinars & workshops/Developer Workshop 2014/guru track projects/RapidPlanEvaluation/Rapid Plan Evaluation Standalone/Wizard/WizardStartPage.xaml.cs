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

namespace Rapid_Plan_Evaluation_Standalone.Wizard
{
    /// <summary>
    /// Interaction logic for WizardStartPage.xaml
    /// </summary>
    public partial class WizardStartPage : Page
    {
        private JobParameters prams;

        public WizardStartPage(ref JobParameters jobParameters)
        {
            InitializeComponent();
            this.prams = jobParameters;
            lstPatients.ItemsSource = Globals.eclipseAPI.PatientSummaries;
            
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            Patient patient = Globals.eclipseAPI.OpenPatient((VMS.TPS.Common.Model.API.PatientSummary) lstPatients.SelectedItem);
            Globals.patientCtx = patient;
            this.prams.patientID = patient.Id;
            NavigationService.Navigate(new CourseSelectionPage(ref prams));
        }
    }
}
