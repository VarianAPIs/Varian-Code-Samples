#if FOO
#undef FOO
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ProfileSamples
{
    class Profile
    {
        static public VVector Lateral => new VVector(1, 0, 0);
        static public VVector Axial => new VVector(0, 1, 0);
        static public double SAD => 1000;

        /// <summary>
        /// Extract dose profile centered at beam axis 
        /// </summary>
        /// <param name="beam">TPS.NET beam object with valid dose</param>
        /// <param name="OrientationInGantry">profile orientation vector in gantry</param>
        /// <param name="profileMaxLength">maximum profile length</param>
        /// <param name="distanceFromIsocenterTowardSourceInmm">profile offset from isocenter toward source</param>
        /// <param name="stepSizeInmm">profile resolution in mm</param>
        /// <returns>
        /// TPS.NET.DoseProfile in DICOM coordinates centered around beam axis such that 
        /// profile orientation in gantry coordinates is as specified.
        /// </returns>
        public static DoseProfile getBeamDoseProfile(
            Beam beam,
            VVector OrientationInGantry,
            double profileMaxLength = 600,
            double distanceFromIsocenterTowardSourceInmm = 0,
            double stepSizeInmm = 2.5)
        {

            if (OrientationInGantry.Length < 1e-8 || OrientationInGantry.Length > 1e8)
            {
                throw new ArgumentException("Invalid direction vector for profile");
            }
            OrientationInGantry.ScaleToUnitLength();
         
            var start = -(profileMaxLength * .5) * OrientationInGantry;
            start.z += distanceFromIsocenterTowardSourceInmm;

            var stop =  (profileMaxLength *.5) * OrientationInGantry;
            stop.z += distanceFromIsocenterTowardSourceInmm;

            return getBeamDoseProfile(beam, start, stop, stepSizeInmm);
        }
        /// <summary>
        /// Extract dose profile between 'start' and 'stop' defined in gantry coordinates (for HFS patient)
        /// </summary>
        /// <param name="beam">Beam with valid dose</param>
        /// <param name="startInGantry">profile start in gatry</param>
        /// <param name="stopInGantry">profile stop in gantry</param>
        /// <param name="stepSizeInmm">profile resolution in mm</param>
        /// <returns></returns>
        public static DoseProfile getBeamDoseProfile(
           Beam beam,
           VVector startInGantry,
           VVector stopInGantry,
           double stepSizeInmm = 2.5)
        {
            //
            //Convert limits from gantry to DICOM
            //
            var start = Helpers.GantryToDICOM(startInGantry, beam.ControlPoints.First().GantryAngle, beam.ControlPoints.First().PatientSupportAngle, beam.IsocenterPosition);
            var stop = Helpers.GantryToDICOM(stopInGantry, beam.ControlPoints.First().GantryAngle, beam.ControlPoints.First().PatientSupportAngle, beam.IsocenterPosition);

            var dose = beam.Dose;

            //
            // Get the profile
            //

            return dose.GetDoseProfile(start, stop, new double[(int)Math.Ceiling((stop - start).Length / stepSizeInmm)]);
        }

        /// <summary>
        /// 
        /// Returns beam depth dose profile limited by structure, usually 'BODY'
        /// 
        /// </summary>
        /// <param name="body"> Structure with which to clip dose profile</param>
        /// <param name="beam"> TPS.NET beam object with valid dose</param>
        /// <param name="stepSizeInmm"> Profile resolution in mm</param>
        /// <returns></returns>

        public static DoseProfile getBeamDepthDoseProfileAlongBeamAxis(Structure body, Beam beam, double stepSizeInmm = 2.5)
        {
            var dose = beam.Dose;
            var normalVec = Helpers.directionTowardSource(beam);
            (var startPoint, var endPoint) = Helpers.GetStructureEntryAndExit(body, normalVec, beam.IsocenterPosition, stepSizeInmm);
            return dose.GetDoseProfile(startPoint, endPoint, new double[(int)(Math.Ceiling((endPoint - startPoint).Length / stepSizeInmm))]);
        }

        /// <summary>
        /// Return image profiles through plan isocenter in primary axis direction
        /// </summary>
        /// <param name="plan">TPS.NET.Plan</param>
        /// <param name="profileLengthInmm"> length of the profiles in mm</param>
        /// <returns></returns>
        public static (ImageProfile, ImageProfile, ImageProfile) getImageProfilesThroughIsocenter(PlanSetup plan)
        {
            var image = plan.StructureSet.Image;
            var dirVecs = new VVector[] {
                plan.StructureSet.Image.XDirection,
                plan.StructureSet.Image.YDirection,
                plan.StructureSet.Image.ZDirection
            };

            var steps = new double[] {
                plan.StructureSet.Image.XRes,
                plan.StructureSet.Image.YRes,
                plan.StructureSet.Image.ZRes
            };


            var planIso = plan.Beams.First().IsocenterPosition;
            var tmpRes = new ImageProfile[3];
           
            //
            // Throws if plan does not have 'BODY'
            //
            var body = plan.StructureSet.Structures.Single(st => st.Id == "BODY");
           
            for (int ind = 0; ind < 3; ind++)
            {
                (var startPoint, var endPoint) = Helpers.GetStructureEntryAndExit(body, dirVecs[ind], planIso, steps[ind]);
                var samples = (int) Math.Ceiling((endPoint - startPoint).Length / steps[ind]);
                tmpRes[ind] = image.GetImageProfile(startPoint, endPoint, new double[samples]);
            }
            return (tmpRes[0], tmpRes[1], tmpRes[2]);
        }
    }
}
