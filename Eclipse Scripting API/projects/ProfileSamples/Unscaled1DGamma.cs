using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileSamples
{
    class Unscaled1DGamma : IDifferenceCalculator
    {
        public int Neighborhood { get; private set; } = 5;

        private double[] nhoodValues;

        public Unscaled1DGamma(int nhood = 5)
        {
            nhoodValues = new double[(Neighborhood = nhood)*2+1];
        }

        private double DotProd(double[] v1, double[] v2)
        {
            return v1.Zip(v2, (a, b) => (a * b)).Aggregate((a, b) => (a + b));
        }

        private double MinDistance(double[] profilePoint1, double[] profilePoint2, double[] measurement)
        {
            //
            // a = profilePoint2 - profilePoint1
            //
            var a = profilePoint1.Zip(profilePoint2, (av, bv) => (bv - av)).ToArray();

            //
            // b = measurement - profilePoint1
            //
            var b = profilePoint1.Zip(measurement, (av, bv) => (bv - av)).ToArray();

            //
            // |a|
            //
            var aNorm = Math.Sqrt(DotProd(a, a));

            //
            // |b|
            //
            var bNorm = Math.Sqrt(DotProd(b, b));

            //
            // a * b
            //
            var aDotb = DotProd(a, b);

            //
            // scalarProjection = cos(alpha) * |b| = ((a*b)/(|a||b|))*|b| = (a*b)/|a|
            //
            var proj = aDotb / aNorm;

            //
            // measurement closest to segment start (profilePoint 1) 
            //
            if (proj < 0)
            {
                return bNorm;
            }
            //
            // projection > |a| => closest point is segment end point (profilePoint2)
            //
            if (proj > aNorm)
            {
                var c = profilePoint2.Zip(measurement, (av, bv) => (bv - av)).ToArray();
                return Math.Sqrt(DotProd(c, c));
            }
            //
            // Projection falls on line segment between profilePoint1 and profilePoint 2;
            //
            return Math.Sqrt(bNorm * bNorm - proj * proj);
        }
        /// <summary>
        /// Calculates shortest distance between measured dose point and calculated profile segment (linear)
        /// The method is akin to 1D gamma without distance and dose difference scaling. Degenerate cases are not handled.
        /// </summary>
        /// <param name="profilePoint1"> double[], (location, doseValue) start of calculated profile segment</param>
        /// <param name="profilePoint2"> end of calculated profile segment </param>
        /// <param name="measurement">measured dose point</param>
        /// <returns> 
        /// Smallest (Euclidian) distance from line segement profilePoint1 -> profilePoint2 to measurement point
        /// </returns>
        public double Calculate(double[] measuredPoint, double[][] profile, int profileIndex)
        {
            var lInd = 0;
            var startInd = (int)Math.Max(profileIndex - Neighborhood, 1);
            var endInd = (int)Math.Min(profileIndex + Neighborhood, profile.Length);
            endInd = Math.Max(endInd, startInd+1);

            for (var nInd = startInd; nInd < endInd; nInd++, lInd++)
            {
                nhoodValues[lInd] = MinDistance(profile[nInd - 1], profile[nInd], measuredPoint);
            }
            var minVal = nhoodValues.Take(lInd).Min((v) => { return Double.IsNaN(v) ? Double.PositiveInfinity : Math.Abs(v); });
            return double.IsPositiveInfinity(minVal) ? double.NaN : minVal;
        }
    }
}
