////////////////////////////////////////////////////////////////////////////////
// TRE.cs
//
//  A SmartAdapt v13+ script that calculates TRE and displays a 
//  registration report for the selected registration.
//
// Copyright (c) 2014 Varian Medical Systems, Inc.
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
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.CA.Scripting;

namespace VMS.IRS.Scripting {

    public class Script
    {

        public Script()
        {
        }
        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            // snippet 1
            // assume a registration is in context.
            MIRSRegistration mirsReg = context.Registration;
            if (mirsReg == null)
            {
                MessageBox.Show("This script requires a registration to be selected.", "Varian Developer");
                return;
            }

            // look for MatchPoints structure on source and target image.
            MIRSImage registeredImage = mirsReg.RegisteredImage;
            MIRSImage sourceImage = mirsReg.SourceImage;

            MessageBox.Show("Fixed image ID = " + registeredImage.Id);
            MessageBox.Show("Moving image ID = " + sourceImage.Id);
            // snippet 1

            // get the match points from the fixed image
            var listSS = registeredImage.Image.StructureSets;
            PointsStructure matchPtsRegistered = null;
            foreach (Structure s in listSS.First().Structures)
            {
                if (s is PointsStructure && s.StructureType == StructureType.Registration)
                {
                    matchPtsRegistered = (PointsStructure)s;
                    break;
                }
            }
#if false
        // Equivalent LINQ query:
        PointsStructure matchPtsFixed = 
            (from s in listSS.First().Structures 
             where (s is PointsStructure) && (s.StructureType == StructureType.Registration)
                select s).FirstOrDefault();
#endif

            // get the match points from the moving image
            listSS = sourceImage.Image.StructureSets;
            PointsStructure matchPtsSource = null;
            foreach (Structure s in listSS.First().Structures)
            {
                if (s is PointsStructure && s.StructureType == StructureType.Registration)
                {
                    matchPtsSource = (PointsStructure)s;
                    break;
                }
            }

            // transform points through the registration matrix.
            VVector[] transformed = mirsReg.TransformPoints(matchPtsSource.Points);

            // compute some TRE statistics
            RegStats stats = new RegStats();
            stats.computeStats(matchPtsSource.Points, matchPtsRegistered.Points, transformed);

            // generate a report
            string htmlReportPath =
                GenerateReport(context, mirsReg, matchPtsSource.Points, matchPtsRegistered.Points, transformed, stats);


            // 'Start' generated HTML file to launch browser window
            System.Diagnostics.Process.Start(htmlReportPath);
            // Sleep for a few seconds to let internet browser window to start
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
        }

        // RegStats holds registration statistics.
        public class RegStats
        {
            public double min, max, mean, standard_deviation;
            public RegStats()
            {
                min = double.MaxValue;
                max = double.MinValue;
                mean = 0.0;
                standard_deviation = double.PositiveInfinity;
            }
            public void computeStats(IEnumerable<VVector> sourcePoints,
                                          IEnumerable<VVector> registeredPoints,
                                          IEnumerable<VVector> transformedPoints)
            {
                // ensure that all lists have same number of points.
                if (sourcePoints.Count() != registeredPoints.Count() || sourcePoints.Count() != transformedPoints.Count())
                {
                    throw new ApplicationException("source and registered match points don't have the same # of points!");
                }
                // compute statistics
                double sum = 0;
                int numPoints = sourcePoints.Count();
                for (int i = 0; i < numPoints; i++)
                {
                    VVector source = sourcePoints.ElementAt(i);
                    VVector registered = registeredPoints.ElementAt(i);
                    VVector derived = transformedPoints.ElementAt(i);
                    double distance = VVector.Distance(registered, derived);
                    min = Math.Min(min, distance);
                    max = Math.Max(max, distance);
                    sum += distance;
                }
                mean = sum / (double)numPoints;
            }
        };
        public string GenerateReport(ScriptContext context, MIRSRegistration mirsReg,
                                        IEnumerable<VVector> sourcePoints,
                                        IEnumerable<VVector> registeredPoints,
                                        IEnumerable<VVector> transformedPoints,
                                        RegStats stats)
        {
            // userid {0}, report time {1}, patient {2}, source dataset {3}, registered dataset {4}.
            string htmlStartFmt = @"
                <HTML>
                  <BODY text=""black"" bgColor=""white"">
                    <H2>Registration Report</H2>
                    Created by {0} on {1}
                    <br/><br/>
                    <table>
                      <tr><td>Patient:</td><td>{2}</td></tr>
                      <tr><td>Source Dataset:</td><td>{3}</td></tr>
                      <tr><td>Registered Dataset:</td><td>{4}</td></tr>
                    </table>";
            string htmlEnd = "</BODY></HTML>";

            // reg id {0}, reg type {1}, min {2}, max{3}, mean {4}
            string htmlRegHeaderFmt = @"
                <H3>Registration:     {0} (type: {1})</H3>
                <H4>Error Statistics:</H4>
                <table>
                  <tr><td>Min:</td><td>{2:0.0}</td><td>mm</td></tr>
                  <tr><td>Max:</td><td>{3:0.0}</td><td>mm</td></tr>
                  <tr><td>Mean:</td><td>{4:0.0}</td><td>mm</td></tr>
                </table>
                <table border=""1"">
                  <tr>
                    <th>point #</th>
                    <th>Source Match Point</th>
                    <th>Registered Match Point</th>
                    <th>Derived Match Point</th>
                    <th>Distance (mm)</th>
                  </tr>";
            string htmlRegFooter = "</table>";

            // point # {0}, source match point {1}, registered match point {2}, derived match point {3}, distance {4}
            string htmlPointRowFmt = @"
                <tr>
                <td>{0}</td>
                <td>{1:0.0}</td>
                <td>{2:0.0}</td>
                <td>{3:0.0}</td>
                <td>{4:0.0}</td>
                </tr>";

            // point.x {0}, point.y {1}, point.z {2}
            string htmlFirstIndividualPointFmt = @"
                <table>
                    <tr><td>x: {0:0.0}</td></tr>
                    <tr><td>y: {1:0.0}</td></tr>
                    <tr><td>z: {2:0.0}</td></tr>
                </table>";

            string htmlIndividualPointFmt = @"
                <table>
                    <tr><td>{0:0.0}</td></tr>
                    <tr><td>{1:0.0}</td></tr>
                    <tr><td>{2:0.0}</td></tr>
                </table>";

            string temp = System.Environment.GetEnvironmentVariable("TEMP");
            string htmlPath = string.Format("{0}\\reg_report.html", temp);
            // open file "%temp%\points.html"
            using (System.IO.TextWriter writer = new System.IO.StreamWriter(htmlPath))
            {
                // build the HTML header and write that
                // userid {0}, report time {1}, patient {2}, source dataset {3}, registered dataset {4}.
                writer.Write(string.Format(htmlStartFmt, context.CurrentUser.Name, DateTime.Now,
                    context.Patient.Id, mirsReg.SourceImage.Id, mirsReg.RegisteredImage.Id));

                // write the reg header
                writer.Write(string.Format(htmlRegHeaderFmt, mirsReg.Id, mirsReg.ToString(), stats.min, stats.max, stats.mean));

                // list points for the registration
                int numPoints = sourcePoints.Count();
                for (int i = 0; i < numPoints; i++)
                {
                    VVector source = sourcePoints.ElementAt(i);
                    VVector registered = registeredPoints.ElementAt(i);
                    VVector derived = transformedPoints.ElementAt(i);
                    double distance = VVector.Distance(registered, derived);
                    string oneRow =
                        string.Format(htmlPointRowFmt, i + 1,
                            string.Format(htmlFirstIndividualPointFmt, source.x, source.y, source.z),
                            string.Format(htmlIndividualPointFmt, registered.x, registered.y, registered.z),
                            string.Format(htmlIndividualPointFmt, derived.x, derived.y, derived.z),
                            distance.ToString(".0"));
                    writer.Write(oneRow);
                }
                // write the registration footer.
                writer.Write(htmlRegFooter);
                // end the HTML document
                writer.Write(htmlEnd);
            }
            return htmlPath;
        }
    };
    // extends class MIRSRegistration and adds new method TransformPoints.
    public static class MirsRegExtensions
    {
        public static VVector[] TransformPoints(this MIRSRegistration mirsReg, PointCollection sourcePoints)
        {
            VVector[] transformedPoints = new VVector[sourcePoints.Count()];
            int index = 0;
            if (mirsReg is MIRSRigidRegistration)
            {
                foreach (VVector p in sourcePoints)
                    transformedPoints[index++] = ((MIRSRigidRegistration)mirsReg).RigidRegistration.TransformPoint(p);
            }
            else if (mirsReg is MIRSNonRigidRegistration)
            {
                foreach (VVector p in sourcePoints)
                    transformedPoints[index++] = ((MIRSNonRigidRegistration)mirsReg).NonRigidRegistration.TransformPoint(p);
            }
            return transformedPoints;
        }
    };
}