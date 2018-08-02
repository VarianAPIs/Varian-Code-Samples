using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using EclipsePlugInRunner.Scripting;
using ProfileSamples;



// TODO: uncomment the line below if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script : IScriptRunnerRunable
    {
        public Script()
        {
        }

        public void Execute(ScriptContext context, Window window /*ScriptEnvironment environment*/)
        {
            Run(context.CurrentUser, context.Patient, context.Image, context.StructureSet, context.PlanSetup, null, null, window);
        }

        public void Run(User user,
                 Patient patient,
                 Image image,
                 StructureSet structureSet,
                 PlanSetup planSetup,
                 IEnumerable<PlanSetup> planSetups,
                 IEnumerable<PlanSum> planSums,
                 Window window)
        {
            //
            //Dose  profiles
            //
            var prof = Profile.getBeamDoseProfile(planSetup.Beams.First(), new VVector(1.0, 0, 0), distanceFromIsocenterTowardSourceInmm: 30);
         
            Helpers.DumpIntoMatlab(prof.Select(val => double.IsNaN(val.Value) ? 0 : val.Value).ToArray(), new int[] { prof.Count }, "orthogonalDoseProfile");
         
            var dprof = Profile.getBeamDepthDoseProfileAlongBeamAxis(structureSet.Structures.Single(st => st.Id == "BODY"), planSetup.Beams.ToArray()[4]);
            Helpers.DumpIntoMatlab(dprof.Select(val => double.IsNaN(val.Value) ? 0 : val.Value).ToArray(), new int[] { dprof.Count }, "depthdoseProfile");

            var dprof2 = Profile.getBeamDoseProfile(planSetup.Beams.First(), new VVector(0, -1.0, 0));
            Helpers.DumpIntoMatlab(dprof2.Select(val => double.IsNaN(val.Value) ? 0 : val.Value).ToArray(), new int[] { dprof2.Count }, "depthdoseProfile2");

            //
            // Dose plane
            //
            
            var plane = Plane.OrthogonalDoseCrossSection(planSetup.Beams.ToArray()[4]);
            var planeAsArray = new double[plane.Length];
            Buffer.BlockCopy(plane, 0, planeAsArray, 0, plane.Length * sizeof(double));
            Helpers.DumpIntoMatlab(planeAsArray, new int[] { plane.GetLength(1), plane.GetLength(0) }, "orthogonalPlane");

            //
            // Image profiles 
            //
            var (xProf, yProf, zProf) = Profile.getImageProfilesThroughIsocenter(planSetup);
            Helpers.DumpIntoMatlab(xProf.Select(val => double.IsNaN(val.Value) ? 0 : val.Value).ToArray(), new int[] { xProf.Count }, "XProfile");
            Helpers.DumpIntoMatlab(xProf.Select(val => double.IsNaN(val.Value) ? 0 : val.Value).ToArray(), new int[] { xProf.Count }, "XProfile");
            Helpers.DumpIntoMatlab(zProf.Select(val => double.IsNaN(val.Value) ? 0 : val.Value).ToArray(), new int[] { zProf.Count }, "ZProfile");

            //
            // DVH with dose profiles.
            //
            planSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
            var p2 = patient.Courses.Single(cr => cr.Id == planSetup.Course.Id).PlanSetups.Single(pl => pl.Id == "Plan2");
            p2.DoseValuePresentation = DoseValuePresentation.Absolute;

            var doses = new Dose[] {
                planSetup.Dose 
                //p2.Dose
            };
            var (cDVH, dDVH, sampleCoverage) = DVH.StructureDVH(doses, structureSet.Structures.Single(st => st.Id == "CTV"), new DVH.NoConversion());
            Helpers.DumpIntoMatlab(Helpers.into1DArray(dDVH.Select(val => new double[] { val.doseValue, val.Volume }).ToArray()), new int[] {2, dDVH.Length}, "dDVH");
            Helpers.DumpIntoMatlab(Helpers.into1DArray(cDVH.Select(val => new double[] { val.doseValue, val.Volume}).ToArray()), new int[] {2, cDVH.Length}, "cDVH");
            return;
        }
    }
}

