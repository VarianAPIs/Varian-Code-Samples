using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileSamples
{
    interface IDifferenceCalculator
    {
        double Calculate(double[] measuredPoint, double[][] profile, int profileIndex);
    }
}
