using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileSamples
{
    class PointDifference:IDifferenceCalculator
    {
        private double linInterp(double v1, double v2, double x)
        {
            return (1 - x) * v1 + x * v2;
        }
        private double DoseDiff(double[] profilePoint1, double[] profilePoint2, double[] measurement)
        {
            var len = (profilePoint2[0] - profilePoint1[0]);
            //
            // degenerate case, segment has zero length,
            //
            if (Math.Abs(len) < Double.Epsilon)
            {
                if (Math.Abs(profilePoint2[1] - profilePoint1[1]) < Double.Epsilon && Math.Abs(measurement[0] - profilePoint1[0]) < Double.Epsilon)
                {
                    return profilePoint1[1];
                }
                //
                // Profile is bad, it has two values for the same argument 
                //
                return Double.NaN;
            }
            var x = (measurement[0] - profilePoint1[0]) / len;
            //
            // does measurement fall in segment, if not return NaN
            //
            if (x < 0 || x > 1)
            {
                return Double.NaN;
            }
            var interpolatedDose = linInterp(profilePoint1[1], profilePoint2[1], x);
            return measurement[1] - interpolatedDose;
        }
        /// <summary>
        ///  Difference between measured and calculated dose at measurement location (1D)
        /// </summary>
        /// <param name="profilePoint1"> double[2], segment start (x , doseValue)</param>
        /// <param name="profilePoint2"> segment end</param>
        /// <param name="measurement"> measured dose</param>
        /// <returns> MeasuredDose - interpolated dose if measurement falls in segment, NaN otherwise</returns>
        public double Calculate(double[] measurement, double[][] profile, int profileIndex)
        {
            return DoseDiff(profile.ElementAt(profileIndex - 1), profile.ElementAt(profileIndex), measurement);
        }
    }
}
