using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ProfileSamples
{
    class Helpers
    {
        public static double maxPatientThickness => 600;
        public enum Direction { CCW = 1, CW = -1 };
        //
        // Cut profile with calculation volume. Could actaully be easier to simply prune out NaN's which is assiugned to points outside of the dose.
        //
        // Math.Abs(direction.x) > 1e-4 is a crude check for profile running parallel to axis (and thus beigbn outside of the calculation volume)
        //
        public static void cutWithCalculationVolume(VoiBox calculationVolume, ref VVector point, VVector direction)
        {
            var xDiff = Math.Max(point.x - calculationVolume.xmax, 0.0);
            xDiff = (xDiff == 0) ? Math.Min(point.x - calculationVolume.xmin, 0) : xDiff;

            if (xDiff != 0 && Math.Abs(direction.x) > 1e-4)
            {
                var l = xDiff / direction.x;
                point.x -= xDiff;
                point.y -= l * direction.y;
                point.z -= l * direction.z;
            }

            var yDiff = Math.Max(point.y - calculationVolume.ymax, 0.0);
            yDiff = (yDiff == 0) ? Math.Min(point.y - calculationVolume.ymin, 0) : yDiff;
            if (yDiff != 0 && Math.Abs(direction.y) > 1e-4)
            {
                var l = yDiff / direction.y;
                point.x -= l * direction.x;
                point.y -= yDiff;
                point.z -= l * direction.z;

            }
            var zDiff = Math.Max(point.z - calculationVolume.zmax, 0.0);
            zDiff = (zDiff == 0) ? Math.Min(point.z - calculationVolume.zmin, 0) : zDiff;
            if (zDiff != 0 && Math.Abs(direction.z) > 1e-4)
            {
                var l = zDiff / direction.z;
                point.x -= l * direction.x;
                point.y -= l * direction.y;
                point.z -= zDiff;
            }
        }
        /// <summary>
        /// Get first structure entry and last exit point along line defined with point and direction (f(x) = point + x * direction)
        /// </summary>
        /// <param name="structure"></param>
        /// <param name="direction"> direction vector </param>
        /// <param name="point"> point on the line</param>
        /// <param name="stepSizeInmm"> resolution of internal segment profile, ~accuracy of bound </param>
        /// <returns>
        /// first structure entry and last exit position along line 
        /// </returns>
        public static (VVector, VVector) GetStructureEntryAndExit(Structure structure, VVector direction, VVector point, double stepSizeInmm = 2.5)
        {
            var tDir = direction;
            tDir.ScaleToUnitLength();
            var startPoint = point + tDir * (maxPatientThickness / 2);
            var endPoint = point - tDir * (maxPatientThickness / 2);
            var profile = structure.GetSegmentProfile(startPoint, endPoint, new System.Collections.BitArray((int)Math.Ceiling((endPoint - startPoint).Length / stepSizeInmm)));

            startPoint = profile.First(spp => spp.Value == true).Position;
            endPoint = profile.Last(spp => spp.Value == true).Position;

            return (startPoint, endPoint);
        }
        /// <summary>
        /// Alternate way to get get field normal.
        /// 
        /// (source - isocenter)/|source - isocenter| 
        /// 
        /// </summary>
        /// <param name="beam"> beam for wih</param>
        /// <returns>
        /// Unit vector pointing from isocenter toward source 
        /// 
        /// The reusult shoudl be the as with: GantryToDicom(new double[]{0,0,-1}, gantryAngle, patientSupportAngle, isoCenter)
        /// </returns>
        public static VVector directionTowardSource(Beam beam)
        {
            var sourceLocation = beam.GetSourceLocation(beam.ControlPoints.First().GantryAngle);
            var dirVec = sourceLocation - beam.IsocenterPosition;
            dirVec.ScaleToUnitLength();
            return dirVec;
        }

        /// <summary>
        /// Rotate point about Z axis (patient support)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="angleInDeg"></param>
        /// <param name="dir">Positive angle direction, default CCW</param>
        /// <returns>point rotated about z axis</returns>
        public static VVector RotateZ(VVector point, double angleInDeg, Direction dir = Direction.CW)
        {
            var angleInRad = angleInDeg * 2 * Math.PI / 360;
            var c = Math.Cos(angleInRad);
            var s = ((int)dir) * Math.Sin(angleInRad);
            var x = point.x * c - point.y * s;
            var y = point.x * s + point.y * c;
            return new VVector(x, y, point.z);
        }
        /// <summary>
        /// Rotate point about Y axis (gantry)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="angleInDeg">Patient support angle</param>
        /// <param name="dir">Positive angle direction default clockwise</param>
        /// <returns>point rotated about y axis</returns>
        public static VVector RotateY(VVector point, double angleInDeg, Direction dir = Direction.CW)
        {
            var angleInRad = angleInDeg * 2 * Math.PI / 360;
            var c = Math.Cos(angleInRad);
            var s = ((int)dir) * Math.Sin(angleInRad);
            var x = point.x * c - point.z * s;
            var z = point.x * s + point.z * c;
            return new VVector(x, point.y, z);
        }
        /// <summary>
        /// Transforms point in gantry co-ordinate system into DICOM (HFS patient only) 
        /// </summary>
        /// <param name="point"> Point to transform</param>
        /// <param name="gantryInDegrees"> Gantry angle b</param>
        /// <param name="patientSupportInDegrees">Patient support angle</param>
        /// <param name="isoCenter"> Isocenter position</param>
        /// <returns></returns>
        public static VVector GantryToDICOM(VVector point, double gantryInDegrees, double patientSupportInDegrees, VVector isoCenter)
        {
            //
            // Account for gantry
            //
            var retval = RotateY(point, gantryInDegrees);
            //
            // Account for patient support
            //
            retval = RotateZ(retval, patientSupportInDegrees);
            //
            // Add beam isocenter
            //
            return new VVector(retval.x, -retval.z, retval.y) + isoCenter;
        }

        /// <summary>
        /// Transforms point in DICOM (HFS patient only) into gantry coordinates (invert GantryToDICOM)
        /// </summary>
        /// <param name="point"> Point to transform</param>
        /// <param name="gantryInDegrees"> Gantry angle b</param>
        /// <param name="patientSupportInDegrees">Patient support angle</param>
        /// <param name="isoCenter"> Isocenter position</param>
        /// <returns></returns>
        public static VVector DICOMToGantry(VVector point, double gantryInDegrees, double patientSupportInDegrees, VVector isoCenter)
        {
            var retval = point - isoCenter;
            retval = new VVector(retval.x, retval.z, -retval.y);
            retval = RotateZ(retval, -patientSupportInDegrees);
            return RotateY(retval, -gantryInDegrees);
        }
        /// <summary>
        /// cross product between two 3-vectors a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static VVector CrossProduct(VVector a, VVector b)
        {
            return new VVector()
            {
                x = a.y * b.z - a.z * b.y,
                y = -a.x * b.z + b.x * a.z,
                z = a.x * b.y - a.y * b.x
            };
        }
        /// <summary>
        /// Convert array of arrays ([][]), where inner arrays all have the same length into one diemnsional arrray of values array; 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns>1D array</returns>
        //
        public static T[] into1DArray<T>(T[][] input)
        {
            var rlen = input.First().Length;
            var rval = new T[input.Length * input.First().Length];
            var rowInd = 0;
            foreach (var row in input)
            {
                Array.Copy(row, 0, rval, rowInd * rlen, rlen);
                rowInd++;
            }
            return rval;
        }

        public static void DumpIntoMatlab<T>(T[] imageData, int[] dims, string name)
        {
            //
            // Do mat file
            //
            try
            {
                string path = @"C:\Users\mvarsta\MatlabTMP\";
                var sw = new StringWriter();
                sw.Write(string.Format("{0} = reshape(csvread('{0}_data.csv')", name));
                foreach (var dim in dims)
                    sw.Write("," + dim.ToString());
                //
                // If one dimensional add singleton dimension to keep reshape happy.
                //
                if (dims.Length == 1)
                {
                    sw.Write(",1");
                }
                sw.Write(");");
                using (var of = new StreamWriter(path + name + "_script.m"))
                {
                    of.WriteLine(sw.ToString());
                    of.Close();
                }
                //
                // Do data
                //
                var swd = new StringWriter();
                foreach (var value in imageData)
                {
                    swd.Write(value.ToString() + ",");
                }
                using (var ofd = new StreamWriter(path + name + "_data.csv"))
                {
                    ofd.WriteLine(swd.ToString().Trim(','));
                    ofd.Close();
                }
            }
            catch
            {
                // do nothing
            }
        }
        /// <summary>
        /// Return collapsed 1D dose profile 
        /// </summary>
        /// <param name="profile"> DoseProfile to collapse</param>
        /// <returns>Array of double[2] of location and doseValue. 
        /// Location at profile start is set to 0 with subsequent values set at distance from start.
        /// </returns>
        public static double[][] flattenProfile(DoseProfile profile)
        {
            var start = profile.ElementAt(0).Position;
            return profile.Select(pp => new double[] { (pp.Position - start).Length, pp.Value }).ToArray();
        }

        /// <summary>
        /// 'Unflatten' previously flattened profile
        /// </summary>
        /// <param name="flatProfile"></param>
        /// <param name="start">'start' point of the original profile</param>
        /// <param name="stop">'stop' point on the original pofile 
        /// (actually any point along the line defined by start and stop toward stop from start would do)</param>
        /// <returns>Array of tuples containing VVector for 3D position and dosevalue.</returns>
        public static (VVector position, double value)[] unflattenProfile(double[][] flatProfile, VVector start, VVector stop)
        {
            var dirVec = (stop - start);
            dirVec.ScaleToUnitLength();
            return flatProfile.Select(val => (start + val[0] * dirVec, val[1])).ToArray();
        }

        private class MyCMP : IComparer
        {
            public int Compare(object _a, object _b)
            {
                var a = _a as double[];
                var b = _b as double[];
                return a[0].CompareTo(b[0]);
            }
        }
        /// <summary>
        /// Calculate differences from measurements to calculated 
        /// 
        /// This is more efficient but a bit messy and not always accurate as it only looks at the 
        /// immediate neighborhood of the interval where measurement point location falls
        /// </summary>
        /// <param name="doseProfile">calculated profile</param>
        /// <param name="measurements">measured points</param>
        /// <param name="diffCalc">operator for calculating distance for each measured datapoint</param>
        /// <returns>Returns measurement point wise distance vector from calculated</returns>
        public static double[] distanceVec(DoseProfile doseProfile, double[][] measurements, IDifferenceCalculator diffCalc)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            //
            // If measurements are in 3D flatten into 1D.
            // Note it is assumed measurrmetns are taken along the same line as profile.
            //
            var tmpMeas = measurements;
            if (measurements[0].Length == 4)
            {
                tmpMeas = measurements.Select(val => new double[] { (new VVector(val[0], val[1], val[2]) - doseProfile[0].Position).Length, val[3] }).ToArray();
            }
            //
            // Put measurments in increasing order on position along profile, permutation is returned in indices 
            //
            var indices = Enumerable.Range(0, tmpMeas.Length).ToArray();
            Array.Sort(tmpMeas, indices, new MyCMP());
            //
            // Flatten dose profile into 1D
            //
            var flatProfile = flattenProfile(doseProfile);
            //
            // Since measurement points are in increasing order, we only need to march through profile once 
            //
            var distVec = new double[tmpMeas.Length].Select(v => v = Double.NaN).ToArray();
            var profileIndex = 0;
            var measIndex = 0;
            foreach (var meas in tmpMeas)
            {
                //
                // step along profile until we have passed location of current measurement (meas)
                //
                // Comparison of point locations (flatProfile[profileIndex][0] < meas[0]) is susceptible to numerical issues if 
                // calcualted dose profile limits are set exactly at measurment limits.
                //
                for (; (profileIndex < flatProfile.Length) && (flatProfile[profileIndex][0] < meas[0]); profileIndex++) ;
                //
                // We have moved beyond calcualted profile
                //
                if (profileIndex == flatProfile.Length)
                    break;
                //
                // Calculate distance values with provided calculator
                //
                distVec[indices[measIndex++]] = diffCalc.Calculate(meas, flatProfile, profileIndex);
            }
            watch.Stop();
            Console.WriteLine("Time: " + watch.ElapsedTicks);
            return distVec;
        }

        //
        // This is less efficient but far easier to understand
        //
        public static double[] distanceVecSimple(DoseProfile doseProfile, double[][] measurements, Func<double[], double[], double[], double> pointDistance)
        {
           
            //
            // If measuremnts are in 3D flatten into 1D.
            // Note it is assumed measuermetns are taken along the same line as profile.
            //
            var tmpMeas = measurements;
            if (measurements[0].Length == 4)
            {
                tmpMeas = measurements.Select(val => new double[] { (new VVector(val[0], val[1], val[2]) - doseProfile[0].Position).Length, val[3] }).ToArray();
            }
            //
            // Flatten dose profile into 1D
            //
            var flatProfile = flattenProfile(doseProfile);
            var distVec = new double[tmpMeas.Length].Select(v => v = Double.NaN).ToArray();
            var mIndex = 0;
            foreach (var meas in tmpMeas)
            {
                var minSoFar = double.MaxValue;
                for (var pInd = 1; pInd < flatProfile.Length; pInd++)
                {
                    minSoFar = Math.Min(minSoFar, pointDistance(flatProfile[pInd - 1], flatProfile[pInd], meas));
                }
                distVec[mIndex++] = minSoFar;
            }
            return distVec;
        }
    }
}
