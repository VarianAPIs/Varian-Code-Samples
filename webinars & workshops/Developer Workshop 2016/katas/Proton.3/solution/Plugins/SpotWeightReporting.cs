////////////////////////////////////////////////////////////////////////////////
// SpotWeightReporting.cs
//
//  A ESAPI v13.7+ script for reporting minimum and maximum raw spot weights in 
//  each layer of a proton spot scanning plan.
//
// Kata Proton.3)    
//  Create a script that goes through all the layers of a proton spot scanning 
//  plan and reports the lowest and highest MU per spot for each layer.
//
// Applies to:
//      Eclipse Scripting API
//      13.7, 15.0, 15.1
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
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            if (context.Patient != null)
            {
                MessageBox.Show("Patient id is " + context.Patient.Id);
                Patient patient = context.Patient;
                foreach (IonBeam ionBeam in context.IonPlanSetup.IonBeams)
                {
                    IonBeamParameters beamParameters = ionBeam.GetEditableParameters();
                    IonControlPointPairCollection layers = beamParameters.IonControlPointPairs;
                    List<List<double>> allLayersParams = new List<List<double>>();

                    foreach (IonControlPointPair layer in layers)
                    {
                        List<double> thisLayerParams = new List<double>();
                        double energyForThisLayer = layer.NominalBeamEnergy;
                        double minSpotWeightFromOptimization = 1000;
                        double maxSpotWeightFromOptimization = 0;

                        foreach (IonSpotParameters spot in layer.RawSpotList)
                        {
                            if (spot.Weight < minSpotWeightFromOptimization) minSpotWeightFromOptimization = spot.Weight;
                            if (spot.Weight > maxSpotWeightFromOptimization) maxSpotWeightFromOptimization = spot.Weight;
                        }

                        thisLayerParams.Add(energyForThisLayer);
                        thisLayerParams.Add(minSpotWeightFromOptimization);
                        thisLayerParams.Add(maxSpotWeightFromOptimization);
                        allLayersParams.Add(thisLayerParams);
                    }
                    string report = "";
                    report += "Energy Raw min Raw max\n";
                    foreach (List<double> line in allLayersParams)
                    {
                        report += String.Format("{0,8:F1} {1,12:f2} {2,13:f2}\n", line[0], line[1], line[2]);
                    }
                    MessageBox.Show(report);
                }
            }
        }
    }
}