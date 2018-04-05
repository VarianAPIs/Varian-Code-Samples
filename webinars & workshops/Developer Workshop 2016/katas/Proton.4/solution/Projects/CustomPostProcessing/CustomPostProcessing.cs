////////////////////////////////////////////////////////////////////////////////
// CustomPostProcessing.cs
//
// A ESAPI v15+ script for custom post processing of a spot scanning proton plan
//
// Kata Proton.4)    
//  Create a custom proton post processing script: go through all the layers of a 
//  proton spot scanning plan and implement energy-dependent minimum and/or maximum 
//  spot MU limits. The energy-dependence can be whatever you like, for example you 
//  can give the values for 70 and 250 MeV and interpolate the intermediate ones.
//
// Applies to:
//      Eclipse Scripting API
//      15.1
//
//      Eclipse Scripting API for Research Users
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

[assembly: ESAPIScript(IsWriteable = true)]

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
                patient.BeginModifications();
                foreach (IonBeam ionBeam in context.IonPlanSetup.IonBeams)
                {
                    IonBeamParameters beamParameters = ionBeam.GetEditableParameters();
                    IonControlPointPairCollection layers = beamParameters.IonControlPointPairs;
                    List<List<double>> allLayersParams = new List<List<double>>();

                    double minEnergy = 70;
                    double maxEnergy = 250;
                    double spotWeightLowerLimitAtMinEnergy = 0.1;
                    double spotWeightLowerLimitAtMaxEnergy = 2;
                    double spotWeightUpperLimitAtMinEnergy = 15;
                    double spotWeightUpperLimitAtMaxEnergy = 30;

                    foreach (IonControlPointPair layer in layers)
                    {
                        List<double> thisLayerParams = new List<double>();
                        double energyForThisLayer = layer.NominalBeamEnergy;
                        double minSpotWeightFromOptimization = 1000;
                        double maxSpotWeightFromOptimization = 0;
                        double minSpotWeightAfterProcessing = 1000;
                        double maxSpotWeightAfterProcessing = 0;

                        double spotWeightLowerLimitForThisLayer = spotWeightLowerLimitAtMinEnergy + (spotWeightLowerLimitAtMaxEnergy - spotWeightLowerLimitAtMinEnergy) * (energyForThisLayer - minEnergy) / (maxEnergy - minEnergy);
                        double spotWeightUpperLimitForThisLayer = spotWeightUpperLimitAtMinEnergy + (spotWeightUpperLimitAtMaxEnergy - spotWeightUpperLimitAtMinEnergy) * (energyForThisLayer - minEnergy) / (maxEnergy - minEnergy);

                        foreach (IonSpotParameters spot in layer.RawSpotList)
                        {
                            if (spot.Weight < minSpotWeightFromOptimization) minSpotWeightFromOptimization = spot.Weight;
                            if (spot.Weight > maxSpotWeightFromOptimization) maxSpotWeightFromOptimization = spot.Weight;

                            if (spot.Weight < spotWeightLowerLimitForThisLayer)
                            {
                                if (spot.Weight < 0.5 * spotWeightLowerLimitForThisLayer) spot.Weight = 0;
                                else spot.Weight = (float)spotWeightLowerLimitForThisLayer;
                            }

                            if (spot.Weight > spotWeightUpperLimitForThisLayer) spot.Weight = (float)spotWeightUpperLimitForThisLayer;

                            if (spot.Weight < minSpotWeightAfterProcessing) minSpotWeightAfterProcessing = spot.Weight;
                            if (spot.Weight > maxSpotWeightAfterProcessing) maxSpotWeightAfterProcessing = spot.Weight;
                        }

                        thisLayerParams.Add(energyForThisLayer);
                        thisLayerParams.Add(minSpotWeightFromOptimization);
                        thisLayerParams.Add(maxSpotWeightFromOptimization);
                        thisLayerParams.Add(minSpotWeightAfterProcessing);
                        thisLayerParams.Add(maxSpotWeightAfterProcessing);
                        allLayersParams.Add(thisLayerParams);
                    }

                    ionBeam.ApplyParameters(beamParameters);
                    string report = "";
                    report += "Energy Raw min Raw max PP min PP max\n";
                    foreach (List<double> line in allLayersParams)
                    {
                        report += String.Format("{0,8:F1} {1,12:f2} {2,13:f2} {3,10:f2} {4,10:f2} \n", line[0], line[1], line[2], line[3], line[4]);
                    }
                    MessageBox.Show(report);
                }
            }
        }
    }
}