///////////////////////////////////////////////////
// DoseMetricsExample
//
// Looking up Calculated DVH Metrics. This application is combined with
// a DVH Plot view and DVH tabular data.
//
// Applies to Eclipse V115.6 MR3
// Built by Matthew Schmidt 
// for questions: (matthew.schmidt@wustl.edu)
// Current calculations
//  -Utilizes built-in ESAPI method GetDoseAtVolume and GetVolumeAtDose
//  
//
//DoseMetricExample Copyright(c) 2019 Washington University. 
//Matthew Schmidt (matthew.schmidt@wustl.edu). Washington University hereby grants to you a non-transferable, non-exclusive, 
//royalty-free, non-commercial, research license to use and copy the computer code that may be downloaded 
//within this site (the �Software�).  You agree to include this license and the above copyright notice in all copies of the Software.  
//The Software may not be distributed, shared, or transferred to any third party.  
//This license does not grant any rights or licenses to any other patents, copyrights, 
//or other forms of intellectual property owned or controlled by Washington University.  
//If interested in obtaining a commercial license, please contact Washington University's Office of Technology Management (otm@wustl.edu).

///Note: This application was built as an example during a Webinar sponsored by Varian Medical Systems.
///It is intended for educational purposes only, specifically to serve as an example for MVVM pattern building in 
///.NET Framework applications.



//YOU AGREE THAT THE SOFTWARE PROVIDED HEREUNDER IS EXPERIMENTAL AND IS PROVIDED �AS IS�, 
//WITHOUT ANY WARRANTY OF ANY KIND, EXPRESSED OR IMPLIED, INCLUDING WITHOUT LIMITATION WARRANTIES 
//OF MERCHANTABILITY OR FITNESS FOR ANY PARTICULAR PURPOSE, OR NON-INFRINGEMENT OF ANY THIRD-PARTY PATENT, 
//COPYRIGHT, OR ANY OTHER THIRD-PARTY RIGHT.  IN NO EVENT SHALL THE CREATORS OF THE SOFTWARE OR WASHINGTON UNIVERSITY BE LIABLE 
//FOR ANY DIRECT, INDIRECT, SPECIAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF OR IN ANY WAY CONNECTED WITH THE SOFTWARE, 
//THE USE OF THE SOFTWARE, OR THIS AGREEMENT, WHETHER IN BREACH OF CONTRACT, TORT OR OTHERWISE, 
//EVEN IF SUCH PARTY IS ADVISED OF THE POSSIBILITY OF SUCH DAMAGES
using Autofac;
using DoseMetricExample.Startup;
using DoseMetricExample.ViewModels;
using DoseMetricExample.Views;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DoseMetricExample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var app = VMS.TPS.Common.Model.API.Application.CreateApplication();
            var patient = app.OpenPatientById(e.Args[0].Split(';').First().Trim('"'));
            var course = patient.Courses.First(x => x.Id == e.Args[0].Split(';')[1]);
            var plan = course.PlanSetups.First(x => x.Id == e.Args[0].Split(';').Last().Trim('"'));
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap(plan);

            var mainView = container.Resolve<MainView>();
            mainView.DataContext = container.Resolve<MainViewModel>();
            //var dMSView = new DoseMetricSelectionView();
            //dMSView.DataContext = new DoseMetricSelectionViewModel(plan);
            //mainView.Content = dMSView;
            mainView.Show();
        }
    }
}
