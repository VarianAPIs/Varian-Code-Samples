using System.Collections;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DvhBioCorrection.DvhMetric
{
    internal class DoseExtractor
    {
        private const double DoseSamplingDistanceInMm = 2;

        private readonly Dose _dose;

        private readonly double _xStart;
        private readonly double _yStart;
        private readonly double _zStart;

        private readonly double _xEnd;
        private readonly double _yEnd;
        private readonly double _zEnd;

        public DoseExtractor(Dose dose)
        {
            _dose = dose;

            _xStart = _dose.Origin.x;
            _yStart = _dose.Origin.y;
            _zStart = _dose.Origin.z;

            _xEnd = _xStart + _dose.XSize * _dose.XRes;
            _yEnd = _yStart + _dose.YSize * _dose.YRes;
            _zEnd = _zStart + _dose.ZSize * _dose.ZRes;
        }

        public double[] GetDoseForStructure(Structure structure)
        {
            var doseList = new List<double>();

            for (double z = _zStart; z < _zEnd; z += DoseSamplingDistanceInMm)
            {
                for (double y = _yStart; y < _yEnd; y += DoseSamplingDistanceInMm)
                {
                    VVector start = new VVector(_xStart, y, z);
                    VVector stop = new VVector(_xEnd, y, z);

                    BitArray bitArray = new BitArray(_dose.XSize);
                    var segmentProfile = structure.GetSegmentProfile(start, stop, bitArray);

                    double[] doseArray = new double[_dose.XSize];
                    var doseProfile = _dose.GetDoseProfile(start, stop, doseArray);

                    for (int i = 0; i < segmentProfile.Count; i++)
                    {
                        if (segmentProfile[i].Value)
                        {
                            doseList.Add(doseProfile[i].Value);
                        }
                    }
                }
            }

            return doseList.ToArray();
        }
    }
}