namespace DvhBioCorrection.DvhMetric
{
    internal class LqBioDoseConverter : IDoseConverter
    {
        private readonly int _fractions;
        private readonly double _alphaBeta;

        public LqBioDoseConverter(int fractions, double alphaBeta)
        {
            _fractions = fractions;
            _alphaBeta = alphaBeta;
        }

        public double[] Convert(double[] dose)
        {
            var bioDose = new double[dose.Length];

            for (int i = 0; i < dose.Length; i++)
            {
                bioDose[i] = ConvertToBioDose(dose[i], _fractions, _alphaBeta);
            }

            return bioDose;
        }

        private double ConvertToBioDose(double d, int n, double alphaBeta)
        {
            return d * (d / n + alphaBeta) / (2.0 + alphaBeta);
        }
    }
}