using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ProfileSamples
{
    class DVH
    {
        public interface IDoseValueConverter
        {
            double Convert(double doseValue);
        }

        public struct DVHBin
        {
            public double doseValue { get; set; }
            public double Volume { get; set; }
                
        }

        /// <summary>
        /// Returns (dose)value as it was provided
        /// </summary>
        /// <param name="val"> Dose value</param>
        /// <returns>input value as is</returns>
        ///      
        public class NoConversion : IDoseValueConverter
        {
            public double Convert(double val)
            {
                return val;
            }
        }

        
        public class EQD2:IDoseValueConverter
        {
          
            public double alpha { get; set; }
           
            public double beta { get; set; }
            
            public int fractionNumber { get; set; }
            
            public double Convert(double val) {
                return val * ((val / fractionNumber) * (alpha / beta) / (2 + (alpha / beta)));
            }    
        }


        /// <summary>
        /// Calculates approximate structure DVH's by scanning the structure volume with dose profiles.
        /// The method allows user to plug in his/her own dose interpretation such as EQD2.
        /// Results for small strucutres are inaccurate due to relatively large number of voxles on the border
        /// </summary>
        /// <param name="doses">Array of dose matrices to sum, doses[0] is used to determine sampling resolution</param>
        /// <param name="structure">TPS.NET structure </param>
        /// <param name="doseValueConversion">Dose value conversion such as EQD2</param>
        /// <param name="bins">Number of histogram bins </param>
        /// <returns>DVH, dDVH and sample coverage for selected provided structure.</returns>
        public static (DVHBin[], DVHBin[], double) StructureDVH(Dose[] doses, Structure structure, IDoseValueConverter converter, int bins = 1024)
        {
            var ddvh = new int[bins];      
            //
            // Add epsilon to make sure every dose value is below upper limit of last bin.
            //
            var max = converter.Convert(doses.Sum(ds=>ds.DoseMax3D.Dose)) + Double.Epsilon;
            var step = max / bins;
            //
            // Contains structure, used to minimize scanned volume.
            //
            var boundingBox = structure.MeshGeometry.Bounds;
            //
            // Length of individual dose profile
            //
            var profileSamples = (int) Math.Ceiling(boundingBox.SizeX / doses[0].XRes);
            //
            // number of voxels in strucutre
            //

            var counter = 0;
            for (var z = boundingBox.Z; z < boundingBox.Z + boundingBox.SizeZ; z += doses[0].ZRes)
            {
                for (var y = boundingBox.Y; y < boundingBox.Y + boundingBox.SizeY; y += doses[0].YRes)
                {

                    var start = new VVector(boundingBox.X, y, z);
                    var stop = new VVector(boundingBox.X + boundingBox.SizeX, y, z);
                    //
                    // Sum converted dose values of profile
                    //
                    var sumOfConvertedDoseValues = new double[profileSamples];
                    foreach (var dose in doses)
                    {          
                        //
                        // get vector of converted dose values only on profile
                        //
                        var convertedDoseValues = dose.GetDoseProfile(start, stop, new double[profileSamples]).Select(dv => converter.Convert(dv.Value)).ToArray();
                        //
                        // 'Zip' arrays, add converted dose values to sum fancy but heavy
                        // 
                        //sumOfConvertedDoseValues = sumOfConvertedDoseValues.Zip(convertedDoseValues, (dv1, dv2) => dv1 + dv2).ToArray();

                        for (var index= 0; index < sumOfConvertedDoseValues.Length; index++)
                        {
                            sumOfConvertedDoseValues[index] += convertedDoseValues[index];
                        }
                    }
                   
                    var structureProfile = structure.GetSegmentProfile(start, stop, new BitArray(profileSamples)).Select(profilePoint => profilePoint.Value).ToArray();
                    for (var ind = 0; ind < structureProfile.Length; ind++)
                    {
                        if (true == structureProfile[ind])
                        {
                            counter++;
                            var bin = (int)Math.Floor(sumOfConvertedDoseValues[ind] / step);
                            ddvh[bin]++;
                        }
                    }                                     
                }
            }
            var voxelVol = doses[0].XRes * doses[0].YRes * doses[0].ZRes * 1e-3;
            var voxelVolOverStep = voxelVol / step;
           
            var sampleCoverage = (counter * voxelVol/structure.Volume);
          

            var diffDVH = new DVHBin[bins];
            var cumDVH = new DVHBin[bins];
          
            var binCenter = step / 2;
            var val = counter;
            for (var ind = 0; ind < bins; ind++, binCenter += step)
            {
                diffDVH[ind] = new DVHBin() { doseValue = binCenter, Volume = ddvh[ind] * voxelVolOverStep};
                cumDVH[ind] = new DVHBin() { doseValue = binCenter, Volume = (val -= ddvh[ind]) * voxelVol};
            }
            return (cumDVH, diffDVH, sampleCoverage);
        }
    }    
}
