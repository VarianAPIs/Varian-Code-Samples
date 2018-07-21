using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.2")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            // TODO : Add here the code that is called when the script is launched from Eclipse.
            Patient p = context.Patient;
            p.BeginModifications();//this line for scripting automation.

            //find the ctv in the structures.
            Structure ctv = context.StructureSet.Structures.First(o => o.DicomType == "CTV");
            //create on the structureset a PTV from the ctv.
            Structure ptv = context.StructureSet.AddStructure("PTV", "PTVAuto");
            ptv.SegmentVolume = ctv.Margin(8);

            //create a new plan.
            Course c_auto = p.AddCourse();
            c_auto.Id = "Course_Auto";
            ExternalPlanSetup plan = c_auto.AddExternalPlanSetup(context.StructureSet);
            plan.Id = "Plan_Auto";
            
            //create the fields.
            //define the externalBeam Parameters
            ExternalBeamMachineParameters ebmp = new ExternalBeamMachineParameters(
                "TrueBeam",
                "6X",
                600,
                "STATIC",
                null);

            //set gantry angles
            double[] g_angles = new double[] { 270, 0, 90, 180 };
            foreach(double ga in g_angles)
            {
                //add a new beam with intial parametres
                Beam b = plan.AddMLCBeam(
                    ebmp,
                    new float[2, 60],
                    new VRect<double>(-10, -10, 10, 10),
                    0,
                    ga,
                    0,
                    ptv.CenterPoint);
                //fit the MLC to the structure outline of the PTV.
                b.FitMLCToStructure(new FitToStructureMargins(10),
                    ptv,
                    false,
                    JawFitting.FitToRecommended,
                    OpenLeavesMeetingPoint.OpenLeavesMeetingPoint_Middle,
                    ClosedLeavesMeetingPoint.ClosedLeavesMeetingPoint_Center);
            }

            //Calculate Dose
            plan.CalculateDose();
            //Set Normalization
            plan.PlanNormalizationValue=plan.Beams.Count() * 100;
            //Set Prescription.
            plan.SetPrescription(30, new DoseValue(180, DoseValue.DoseUnit.cGy), 1);
        }
    }
}
