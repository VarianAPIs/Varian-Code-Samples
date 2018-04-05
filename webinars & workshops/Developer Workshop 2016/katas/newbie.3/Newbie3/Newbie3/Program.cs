////////////////////////////////////////////////////////////////////////////////
//  A ESAPI v11+ script that demonstrates DVH Metric calculation.
//
// Kata newbie.3)	
/*
1. Open the patient with Id = "K_N3_Reporta", or K_N3_Reportb, etc
2. Using the first planning approved plan, write to the console the following items:

*Plan Id
*Patient name and Id

For each field, write out:
*Field Id
*Gantry Angle
*Collimator Angle
*Table Angle
*X1,X2,Y1,Y2 Jaws
*MU
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

namespace Newbie3
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (var app = Application.CreateApplication())
            {
                //Load patient K_N3_Reporta
                var pat = app.OpenPatientById("K_N3_Reporta");
                //*Plan Id
                Console.WriteLine("PLAN");
                Console.WriteLine("-----------------");
                var plan = pat.Courses.First().PlanSetups.First();
                Console.WriteLine(plan.Id);
                Console.WriteLine("");

                //*Patient name and Id
                Console.WriteLine("PATIENT");
                Console.WriteLine("-----------------");
                Console.WriteLine("{0}, {1} - {2}", pat.LastName, pat.FirstName, pat.Id);
                Console.WriteLine("");
                //For each field, write out:
                //*Field Id
                //*Gantry Angle
                //*Collimator Angle
                //*Table Angle
                //*X1,X2,Y1,Y2 Jaws
                //*MU
                foreach (var b in plan.Beams)
                {
                    Console.WriteLine("Field Id : {0}",b.Id);
                    Console.WriteLine("-----------------");
                    Console.WriteLine("Gantry Angle : {0}", b.ControlPoints.First().GantryAngle);
                    Console.WriteLine("Collimator Angle : {0}", b.ControlPoints.First().CollimatorAngle);
                    Console.WriteLine("Table Angle : {0}", b.ControlPoints.First().PatientSupportAngle);
                    Console.WriteLine("X1/X2 : {0}/{1}", b.ControlPoints.First().JawPositions.X1, b.ControlPoints.First().JawPositions.X2);
                    Console.WriteLine("Y1/Y2 : {0}/{1}", b.ControlPoints.First().JawPositions.Y1, b.ControlPoints.First().JawPositions.Y2);
                    Console.WriteLine("MU : {0}", b.ControlPoints.First().MetersetWeight);
                    Console.WriteLine("-----------------");
                    Console.WriteLine("");
                }

                Console.Read();
            }
            

            
        }
    }
}
