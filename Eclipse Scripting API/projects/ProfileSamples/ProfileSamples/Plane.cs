using System;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ProfileSamples
{
   
    class Plane
    {
        /// <summary>
        ///  Samples 3D beam dose for orthgonal plane cross section at 'isocenterOffSet' from beam isocenter.   
        ///  
        /// Offerrd as is, take with massive grain of salt, script has been tried not tested.
        /// 
        /// </summary>
        /// <param name="calcVol"> Plan calculation volume limits, plane is cut with calcualtion volume.</param>
        /// <param name="beam"> TPS.NET beam, with valid dose</param>
        /// <param name="isocenterOffset"> Displacement from beam isocenter in mm, positive toward source, default = 0mm</param>
        /// <param name="xSize"> Maximum plane size in X,Y in mm (rotated about Z with gantry angle), default = 200mm</param>
        /// <param name="zSize"> Maximum plane size in Z in mm (gantry rotates about Z axis), default = 200mm</param>
        /// <param name="pixelSize"> Square pixel size in mm</param>
        /// <returns> double[,], two dimensional array of dose values in set dose representation (relative/absolute)</returns>
       
        public static double[,] OrthogonalDoseCrossSection(Beam beam,  double isocenterOffset= 0, double xSize = 200, double zSize = 200, double pixelSize = 2.5)
        {

            //
            // Plane X axis orientation
            //

            var xDir = Helpers.GantryToDICOM(new VVector(1, 0, 0), beam.ControlPoints.ElementAt(0).GantryAngle, beam.ControlPoints.ElementAt(0).PatientSupportAngle, new VVector(0,0,0));
         
            //
            // Alternate method
            //
            //var nDir = Helpers.RotateZ(new VVector(0, -1, 0), beam.ControlPoints.ElementAt(0).GantryAngle);
            //nDir = Helpers.RotateY(nDir, beam.ControlPoints.ElementAt(0).PatientSupportAngle);

            //
            // Alternate method (since with Gantry at 0 degress direction toward source is [0,-1,0], this is opposite to result of cross product.) 
            //
            //var zDir = Helpers.RotateZ(new VVector(0, 0, 1), beam.ControlPoints.ElementAt(0).GantryAngle);
            //zDir = Helpers.RotateY(zDir, beam.ControlPoints.ElementAt(0).PatientSupportAngle); 
            
            //
            // Beam axis direction
            //

            var normVec = Helpers.directionTowardSource(beam);

            //
            // Individual dose profile X start and end positions on plane
            // 
            var start = beam.IsocenterPosition - xSize * xDir * 0.5;
            var end = beam.IsocenterPosition + xDir * 0.5;

            start = start + normVec * isocenterOffset;
            end = end + normVec * isocenterOffset;

            //
            // Plane Z axis orientation
            //
            var zDir = Helpers.CrossProduct(xDir, normVec); 

            //
            // These ought to be zero
            //
            var a = zDir.ScalarProduct(xDir);
            var b = xDir.ScalarProduct(normVec);
            //
            // Z 
            //
            var zStart = beam.IsocenterPosition + normVec * isocenterOffset  - zDir * zSize * 0.5;
            var zEnd = zStart + zDir * zSize;

            var dose = beam.Dose;

            var xSamples = (int) Math.Ceiling((end-start).Length/ pixelSize);
            var zSamples = (int) Math.Ceiling((zEnd - zStart).Length/pixelSize);
            var tmpResult = new double[zSamples * xSamples];

            var rowIndex = 0;

            start -= zSamples / 2 * zDir * pixelSize;
            end -= zSamples / 2 * zDir * pixelSize;

            for (double zPos = 0;  zPos < zSamples; zPos++) 
            {
                
                start += zDir * pixelSize;
                end += zDir * pixelSize;
                var prof = dose.GetDoseProfile(start, end, new double[xSamples]);
                //
                // Replacing NaN's with 0's may not be smartest thing, NaN's are for unknown not 0 dose
                //
                var pvals = prof.Select(point => Double.IsNaN(point.Value) ? 0 : point.Value).ToArray();
                Array.ConstrainedCopy(pvals, 0, tmpResult, rowIndex * xSamples, xSamples);
                rowIndex++;
            }
            var result = new double[zSamples, xSamples];
            Buffer.BlockCopy(tmpResult, 0, result, 0, zSamples * xSamples * sizeof(double));
            return result;
        }
    }
}
