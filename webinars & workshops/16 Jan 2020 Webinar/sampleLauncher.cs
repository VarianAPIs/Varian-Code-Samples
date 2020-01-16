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
//within this site (the ?Software?).  You agree to include this license and the above copyright notice in all copies of the Software.  
//The Software may not be distributed, shared, or transferred to any third party.  
//This license does not grant any rights or licenses to any other patents, copyrights, 
//or other forms of intellectual property owned or controlled by Washington University.  
//If interested in obtaining a commercial license, please contact Washington University's Office of Technology Management (otm@wustl.edu).

///Note: This application launcher is a mimic of the application launcher provided by Carlos Anderson at http://www.carlosjanderson.com/an-easy-way-to-launch-stand-alone-apps-from-eclipse/
///small changes implemented to reflect a static location of an executable application.



//YOU AGREE THAT THE SOFTWARE PROVIDED HEREUNDER IS EXPERIMENTAL AND IS PROVIDED ?AS IS?, 
//WITHOUT ANY WARRANTY OF ANY KIND, EXPRESSED OR IMPLIED, INCLUDING WITHOUT LIMITATION WARRANTIES 
//OF MERCHANTABILITY OR FITNESS FOR ANY PARTICULAR PURPOSE, OR NON-INFRINGEMENT OF ANY THIRD-PARTY PATENT, 
//COPYRIGHT, OR ANY OTHER THIRD-PARTY RIGHT.  IN NO EVENT SHALL THE CREATORS OF THE SOFTWARE OR WASHINGTON UNIVERSITY BE LIABLE 
//FOR ANY DIRECT, INDIRECT, SPECIAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF OR IN ANY WAY CONNECTED WITH THE SOFTWARE, 
//THE USE OF THE SOFTWARE, OR THIS AGREEMENT, WHETHER IN BREACH OF CONTRACT, TORT OR OTHERWISE, 
//EVEN IF SUCH PARTY IS ADVISED OF THE POSSIBILITY OF SUCH DAMAGES
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Diagnostics;
using System.IO;

namespace VMS.TPS
{

    public class Script
    {

        public Script()
        {
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            // TODO : Add here your code that is called when the script is launched from Portal Dosimetry
            try
            {
                Process.Start(AppExePath(), String.Format("\"{0};{1};{2}\"",context.Patient.Id, context.Course.Id,context.PlanSetup.Id));
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to start application.");
            }
        }

        private string AppExePath()
        {
            return @"C:\Users\vicadmin\Desktop\Webinar\AppsCombined\DoseMetricExample\DoseMetricExample\bin\Debug\DoseMetricExample.exe";
        }
    }

}