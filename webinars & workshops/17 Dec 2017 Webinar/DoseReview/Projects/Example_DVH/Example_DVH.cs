using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Controls;


// Do not change namespace and class name
// otherwise Eclipse will not be able to run the script.
namespace VMS.TPS
{
    public class Script
    {
        public void Run(
    User user,
    Patient patient,
    VMS.TPS.Common.Model.API.Image image,
    StructureSet structureSet,
    PlanSetup planSetup,
    IEnumerable<PlanSetup> planSetupsInScope,
    IEnumerable<PlanSum> planSumsInScope,
    Window window)
        {
            // Your main code now goes here
            //PlanSetup planSetup = context.PlanSetup;

            // If there's no selected plan with calculated dose throw an exception
            if (planSetup == null || planSetup.Dose == null)
                throw new ApplicationException("Please open a calculated plan before using this script.");

            // Retrieve StructureSet
            structureSet = planSetup.StructureSet;
            if (structureSet == null)
                throw new ApplicationException("The selected plan does not reference a StructureSet.");

            // For this example we will retrieve first available structure of PTV type
            /*Structure target = null;
            foreach (var structure in structureSet.Structures)
            {
                if (structure.DicomType == "PTV")
                {
                    target = structure;
                    break;
                }

            }
            if (target == null)
                throw new ApplicationException("The selected plan does not have a PTV.");

            // Retrieve DVH data
            DVHData dvhData = planSetup.GetDVHCumulativeData(target,
                                          DoseValuePresentation.Relative,
                                          VolumePresentation.Relative, 0.1);

            if (dvhData == null)
                throw new ApplicationException("DVH data does not exist. Script execution cancelled.");
*/
            // Add existing WPF control to the script window.
            var mainControl = new Example_DVH.MainControl();
            window.Content = mainControl;
            window.Width = mainControl.Width;
            window.Height = mainControl.Height;
            foreach(Structure s in structureSet.Structures)
            {
                CheckBox cb = new CheckBox();
                cb.Content = s.Id;
                cb.Checked += mainControl.Cb_Checked;
                mainControl.structures_sp.Children.Add(cb);
            }
            mainControl.ps = planSetup;
            window.Title = "Plan : " + planSetup.Id;// + ", Structure : " + target.Id;

            // Draw DVH
            //mainControl.DrawDVH(dvhData);
        }

        

        public Script()
        {
        }

        public void Execute(ScriptContext scriptContext, Window mainWindow)
        {
            Run(scriptContext.CurrentUser,
              scriptContext.Patient,
              scriptContext.Image,
              scriptContext.StructureSet,
              scriptContext.PlanSetup,
              scriptContext.PlansInScope,
              scriptContext.PlanSumsInScope,
              mainWindow);
        }
    }
}
