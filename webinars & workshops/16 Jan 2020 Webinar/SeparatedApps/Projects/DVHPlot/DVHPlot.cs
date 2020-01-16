///////////////////////////////////////////////////
// DVHPlot
//
// Looking up properties from the plan's dose calculation and prescription
// This application is combined with
// a DVH Plot view and DoseMetrics.
//
// Applies to Eclipse V115.6 MR3
// Built by Matthew Schmidt
// for questions: (matthew.schmidt@wustl.edu)
// Current calculations
//  -Utilizes built-in ESAPI method GetDVHCumulativeData method
//  
//
//DoseMetricExample Copyright(c) 2019 Washington University.
//Matthew Schmidt (matthew.schmidt@wustl.edu). Washington University hereby grants to you a non-transferable, non-exclusive,
//royalty-free, non-commercial, research license to use and copy the computer code that may be downloaded
//within this site (the Software).  You agree to include this license and the above copyright notice in all copies of the Software.  
//The Software may not be distributed, shared, or transferred to any third party.  
//This license does not grant any rights or licenses to any other patents, copyrights,
//or other forms of intellectual property owned or controlled by Washington University.  
//If interested in obtaining a commercial license, please contact Washington University's Office of Technology Management (otm@wustl.edu).

///Note: This application was built as an example during a Webinar sponsored by Varian Medical Systems.
///It is intended for educational purposes only, specifically to serve as an example for MVVM pattern building in
///.NET Framework applications.

//YOU AGREE THAT THE SOFTWARE PROVIDED HEREUNDER IS EXPERIMENTAL AND IS PROVIDED AS IS,
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
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using DVHPlot.Startup;
using Autofac;
using DVHPlot.Views;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context, System.Windows.Window window, ScriptEnvironment environment)
        {
            // TODO : Add here the code that is called when the script is launched from Eclipse.
            if (context.PlanSetup != null)
            {
                var bootstrapper = new Bootstrapper();
                var builder = bootstrapper.Bootstrap(context.PlanSetup);
                var mainView = builder.Resolve<MainView>();
                window.Content = mainView;
                //dummy class instance to reference Oxyplot.wpf. Only needed for binary plugins.
                OxyPlot.Wpf.AngleAxis angleAxis = new OxyPlot.Wpf.AngleAxis();
            }
        }
    }
}
