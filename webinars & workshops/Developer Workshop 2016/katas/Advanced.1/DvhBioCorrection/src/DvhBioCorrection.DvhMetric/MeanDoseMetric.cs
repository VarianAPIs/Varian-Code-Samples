using System.Linq;

namespace DvhBioCorrection.DvhMetric
{
    internal class MeanDoseMetric : IDoseMetric
    {
        public double Calculate(double[] dose)
        {
            return dose.Average();
        }
    }
}