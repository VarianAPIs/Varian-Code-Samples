////////////////////////////////////////////////////////////////////////////////
/*
 * *
ADVANCED 8 KATA
1.Open a patient with the first name "Advanced8a"
2.Using the only structure set, find all 3D images with the same frame of reference (like in a gating study)
	**3D images have a defined origin, single images do not
3.Using the body structure, find the HU at the center point of the body in each image

		//CT_RP_30 = ?
                //CT_RP_40 = ?
                //CT_RP_50 = ?
                //CT_RP_60 = ?
                //CT_RP_70 = ?
                //CT_RP_Mip = ?
                //CT_RP_Ave = ?

4. Find the actual maximum AND average voxel value across the scans RP_30-RP70 (phases 30-70).
5. Do they match the values for the RP_Mip (maximum intensity projection) and RP_Ave (average)?
 */
//
// Applies to:
//      Eclipse Scripting API
//          11, 13.6, 13.7, 15.0,15.1
//
// Copyright (c) 2016 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace Advanced8Kata
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var app = Application.CreateApplication())
            {
                //Open Patient
                var summary = app.PatientSummaries.First(p => p.FirstName.Contains("Advanced8a"));
                var pat = app.OpenPatient(summary);

                //Get planning image (average)
                var im1 = pat.StructureSets.First().Image;
                var study = im1.Series.Study;

                //Find all images with the same frame of reference
                //Filter images with no origin (single images)-> We only want 3D images
                var sameFOR = study.Series
                    .SelectMany(s => s.Images.Where(i => i.FOR == im1.FOR))
                    .Where(im => !double.IsNaN(im.Origin.x));
                var body = pat.StructureSets.First().Structures.First(s => s.Id == "BODY");
                var center = body.CenterPoint;

                List<double> hus = new List<double>();
                foreach (var im in sameFOR)
                {
                    var x = center.x;
                    var y = center.y;
                    var z = center.z;
                    var hu = GetHUAtLocation(im, x, y, z);
                    hus.Add(hu);
                    Console.WriteLine("{0} = {1}", im.Id, hu.ToString("F2"));
                }

                var avg = hus.Average(); //39.71
                var max = hus.Max(); //81

                //Pause console to write answer
                //CT_RP_30 = 81.00
                //CT_RP_40 = 14.00
                //CT_RP_50 = 30.00
                //CT_RP_60 = 30.00
                //CT_RP_70 = 9.00
                //CT_RP_Mip = 81.00
                //CT_RP_Ave = 33.00

                Console.Read();
            }
        }

        private static double GetHUAtLocation(Image image, double x0, double y0, double z0)
        {
            int[,] buffer = new int[image.XSize, image.YSize];
            double[,] hu = new double[image.XSize, image.YSize];

            //Calculate the voxel locations (integers) from actual 3D spatial coordinates
            var dx = (x0 - image.Origin.x) / (image.XRes * image.XDirection.x);
            var dy = (y0 - image.Origin.y) / (image.YRes * image.YDirection.y);
            var dz = (z0 - image.Origin.z) / (image.ZRes * image.ZDirection.z);

            //Fill buffer with voxels from current slice
            image.GetVoxels((int)dz, buffer);
            for (int x = 0; x < image.XSize; x++)
            {
                for (int y = 0; y < image.YSize; y++)
                {
                    //Set HU from "voxel value" - have to convert
                    hu[x, y] = image.VoxelToDisplayValue(buffer[x, y]);
                }
            }
            return hu[(int)Math.Round(dx), (int)Math.Round(dy)];
        }
    }

}
