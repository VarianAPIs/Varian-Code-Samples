////////////////////////////////////////////////////////////////////////////////
// TextDVHMetrics v2.1.cs
//
//  A ESAPI v11+ script that uses a CSV file as a DVH metrics template and
//  generates a result file in the same format.  Uses DVH analysis description 
//  language as defined in the paper by Mayo, et al. 
//  "Establishment of practice standards in nomenclature and prescription to 
//  enable construction of software and databases for knowledge-based 
//  practice review" (http://dx.doi.org/10.1016/j.prro.2015.11.001).
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
// REVISION HISTORY
//
// July 12, 2016 Steve Thompson
// -Added 2 add'l checks for cGy vs Gy in template vs Eclipse for minmaxmean pattern
// -Note: Add references to solution:
//      -VMS.TPS.Common.Model.API
//      -VMS.TPS.Common.Model.Types
//      -Presentation Framework
//      -Microsoft.VisualBasic
//
// June 23, 2016 Tomasz Morgas
//-Revisions to handle minmaxmean pattern better (from NRG testing)
//
//
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;


namespace VMS.TPS
{
    public class Script
    {
        // define your workbook template here.

        string WORKBOOK_TEMPLATE_DIR = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string WORKBOOK_RESULT_DIR = System.IO.Path.GetTempPath();
        // these map to the defined columns in the CSV template file
        public class StructureObjective
        {
            public string ID { get; set; }
            public string Code { get; set; }
            public string[] Aliases { get; set; }
            public string DVHObjective { get; set; }
            public string Evaluator { get; set; }
            public string Priority { get; set; }
            public string Variation { get; set; }
            public string Met { get; set; }
            public string Achieved { get; set; }
            public string FoundStructureID { get; set; }
        }
        StructureObjective[] m_objectives; //all the dose values are internally in Gy.


        public Script()
        {
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            PlanningItem pitem = null;

            StructureSet ss = context.StructureSet;
            List<string[]> CSVSheet = new List<string[]>();
            List<string[]> DataOut = new List<string[]>();
            string[] header;

            // if a plansum is loaded take the first plansum to work on, otherwise take the active plansetup.

            if (context.PlanSetup == null && context.PlanSumsInScope == null)
            {
                throw new ApplicationException("Please load a plan or plansum.");
            }
            if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
            {
                throw new ApplicationException("Please close all but one plan sum.");
            }
            else if (context.PlanSetup == null && context.PlanSumsInScope.Count() == 1)
            {
                PlanSum plansum = context.PlanSumsInScope.Single();
                pitem = plansum;
                ss = plansum.StructureSet;
            }
            //PlanSetup plan = context.PlanSetup;
            if (pitem == null)
                pitem = context.PlanSetup;
            if (context.Patient == null || ss == null || pitem == null)
            {
                MessageBox.Show("Please load a patient and plan or plansum before running this script.");
                return;
            }
            if (!pitem.IsDoseValid())
            {
                MessageBox.Show("Please calculate dose for the active plan before running this script.");
                return;
            }

            // make sure the workbook directory exists
            if (!System.IO.Directory.Exists(WORKBOOK_TEMPLATE_DIR))
            {
                MessageBox.Show(string.Format("The default template file directory '{0}' defined by the script does not exist.", WORKBOOK_TEMPLATE_DIR));
                return;
            }

            // have user choose the template to use.

            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.DefaultExt = "csv";
            fileDialog.InitialDirectory = WORKBOOK_TEMPLATE_DIR;
            fileDialog.Multiselect = false;
            fileDialog.Title = "Choose the PQM YOU template file";
            fileDialog.ShowReadOnly = true;
            fileDialog.Filter = "CSV files (*.csv)|*.csv";
            fileDialog.FilterIndex = 0;
            fileDialog.CheckFileExists = true;
            if (fileDialog.ShowDialog() == false)
            {
                return;    // user canceled
            }

            var file = fileDialog.FileName;

            // make sure the workbook template exists
            if (!System.IO.File.Exists(file))
            {
                MessageBox.Show(string.Format("The template file '{0}' chosen does not exist.", file));
                return;
            }

            CSVSheet = parseCSV(file);
            //extract header and modify to indicate output values
            header = CSVSheet.First();
            header[2] = "Patient Structure";
            CSVSheet.RemoveAt(0);
            int numFoundObjectives = ReadObjectives(CSVSheet, out m_objectives);
            EvaluateMetrics(ss, pitem);
            DataOut = UpdateWorkbook(CSVSheet, m_objectives);
            //Store data to file
            string outputpath = System.IO.Path.Combine(WORKBOOK_RESULT_DIR, context.Patient.Id.ToString() + "-" + pitem.Id.ToString() + ".csv");
            CSVWrite(outputpath, DataOut, header);
            //Open default app associated with .csv to show the results

            System.Diagnostics.Process.Start(outputpath);
            System.Threading.Thread.Sleep(3000);

        }

        void CSVWrite(string path, List<string[]> CSVSheet, string[] Header)
        {
            var csv = new StringBuilder();

            string line = String.Join(",", Header);
            csv.AppendLine(line);
            foreach (string[] row in CSVSheet)
            {
                line = String.Join(",", row);
                csv.AppendLine(line);
            }
            System.IO.File.WriteAllText(path, csv.ToString());
        }

        public List<string[]> parseCSV(string path)
        {
            List<string[]> parsedData = new List<string[]>();
            string[] fields;

            try
            {
                TextFieldParser parser = new TextFieldParser(path);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    fields = parser.ReadFields();
                    parsedData.Add(fields);
                }

                parser.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return parsedData;
        }
        List<string[]> UpdateWorkbook(List<string[]> CSVsheet, StructureObjective[] objectives)
        {
            int row = 0;
            List<string[]> Calculated = new List<string[]>();
            foreach (var objective in objectives)
            {
                // Achieved
                string[] line = CSVsheet.ElementAt(row);
                line[2] = objective.FoundStructureID;
                line[7] = objective.Met;
                string achieved = objective.Achieved;
                if (line[3].Contains("cGy")) //CSVsheet has the original values from template. Here convert only 'achieved' back to original units if needed.
                {
                  if (line[3].Contains("D") || line[3].Contains("M"))
                  {
                    ConvertValueTocGy(ref achieved);
                    ConvertUnitTocGy(ref achieved);
                  }
                }
                line[8] = achieved;
                Calculated.Add(line);
                row++;
            }
            return Calculated;
        }

        // read an Excel sheet that has the following columns defined:
        // Structure ID	    Structure Code	    Aliases	    DVH Objective	    Evaluator	    Priority
        int ReadObjectives(List<string[]> CSVsheet, out StructureObjective[] objectives)
        {
            int numFoundObjectives = CSVsheet.Count();

            objectives = new StructureObjective[numFoundObjectives];
            int i = 0;
            foreach (string[] line in CSVsheet)
            {
                objectives[i] = new StructureObjective();
                // Structure ID
                objectives[i].ID = line[0];
                // Structure Code
                objectives[i].Code = line[1];
                // Aliases : extract individual aliases using "|" as separator.  If blank, use the ID.
                string aliases = line[2];
                objectives[i].Aliases = (aliases.Length > 0) ? aliases.Split('|') : new string[] { objectives[i].ID };
                // DVH Objective
                string obj = line[3];
                string evaluator = line[4];
                string variation = line[5];
                if (obj.Contains("cGy")) //convert so that internally always handle Gy.
                {
                  ConvertUnitToGy(ref obj);
                  if (obj.Contains("D") || obj.Contains("M"))
                  {
                    ConvertValueToGy(ref evaluator);
                    ConvertValueToGy(ref variation);
                  }
                  else
                  {
                    ConvertValueToGy(ref obj);
                  }
                }
                objectives[i].DVHObjective = obj;
                // Evaluator
                objectives[i].Evaluator = evaluator;
                //Variation
                objectives[i].Variation = variation;
                // Priority
                objectives[i].Priority = line[6];
                // Met (calculate this later, check if meeting - Goal, Variation, Not met)
                objectives[i].Met = "";
                // Achieved (calculate this later)
                objectives[i].Achieved = "";
                i++;
            }
            return numFoundObjectives;
        }

        void ConvertUnitToGy(ref string expression)
        {
          expression = expression.Replace("cGy", "Gy");
        }

        void ConvertUnitTocGy(ref string expression)
        {
          expression = expression.Replace("Gy", "cGy");
        }

        void ConvertValueToGy(ref string expression)
        {
          var resultString = Regex.Match(expression, @"\d+\p{P}\d+|\d+").Value;
          double newValue = double.Parse(resultString) / 100;
          expression = expression.Replace(resultString, newValue.ToString());
        }

        void ConvertValueTocGy(ref string expression)
        {
          var resultString = Regex.Match(expression, @"\d+\p{P}\d+|\d+").Value;
          double newValue = double.Parse(resultString) * 100;
          expression = expression.Replace(resultString, newValue.ToString());
        }

        Structure FindStructureFromAlias(StructureSet ss, string ID, string[] aliases)
        {
            // search through the list of alias ids until we find an alias that matches an existing structure.
            Structure oar = null;
            string actualStructId = "";
            oar = (from s in ss.Structures
                   where s.Id.ToUpper().CompareTo(ID.ToUpper()) == 0
                   select s).FirstOrDefault();
            if (oar == null)
            {
                foreach (string volumeId in aliases)
                {
                    oar = (from s in ss.Structures
                           where s.Id.ToUpper().CompareTo(volumeId.ToUpper()) == 0
                           select s).FirstOrDefault();
                    if (oar != null)
                    {
                        actualStructId = oar.Id;
                        break;
                    }
                }
            }
            return oar;
        }

        void EvaluateMetrics(StructureSet ss, PlanningItem plan)
        {
            //start with a general regex that pulls out the metric type and the @ (evalunit) part.
            string pattern = @"^(?<type>[^\[\]]+)(\[(?<evalunit>[^\[\]]+)\])$";
            string minmaxmean_Pattern = @"^(M(in|ax|ean)|Volume)$";//check for Max or Min or Mean or Volume
            string d_at_v_pattern = @"^D(?<evalpt>\d+\p{P}\d+|\d+)(?<unit>(%|cc))$"; // matches D95%, D2cc
            string dc_at_v_pattern = @"^DC(?<evalpt>\d+)(?<unit>(%|cc))$"; // matches DC95%, DC700cc
            string v_at_d_pattern = @"^V(?<evalpt>\d+\p{P}\d+|\d+)(?<unit>(%|Gy|cGy))$"; // matches V98%, V40Gy
            string cv_at_d_pattern = @"^CV(?<evalpt>\d+)(?<unit>(%|Gy|cGy))$"; // matches CV98%, CV40Gy
            // Max[Gy] D95%[%] V98%[%] CV98%[%] D2cc[Gy] V40Gy[%]

            foreach (var objective in m_objectives)
            {
                // first find the structure for this objective
                Structure evalStructure = FindStructureFromAlias(ss, objective.ID, objective.Aliases);
                if (evalStructure == null)
                {
                    objective.Achieved = "Structure not found.";
                    objective.FoundStructureID = "";
                    continue;
                }
                objective.FoundStructureID = evalStructure.Id;

                //start with a general regex that pulls out the metric type and the [evalunit] part.
                var matches = Regex.Matches(objective.DVHObjective, pattern);

                if (matches.Count != 1)
                {
                    objective.Achieved =
                        string.Format("DVH Objective expression \"{0}\" is not a recognized expression type.",
                            objective.DVHObjective);
                    break;
                }
                Match m = matches[0];
                Group type = m.Groups["type"];
                Group evalunit = m.Groups["evalunit"];
                Console.WriteLine("expression {0} => type = {1}, unit = {2}", objective.DVHObjective, type.Value, evalunit.Value);

                //MessageBox.Show(type.Value+" " + minmaxmean_Pattern);


                // further decompose <type>
                var testMatch = Regex.Matches(type.Value, minmaxmean_Pattern);
                if (testMatch.Count != 1)
                {
                    testMatch = Regex.Matches(type.Value, v_at_d_pattern);
                    if (testMatch.Count != 1)
                    {
                        testMatch = Regex.Matches(type.Value, d_at_v_pattern);
                        if (testMatch.Count != 1)
                        {
                            testMatch = Regex.Matches(type.Value, cv_at_d_pattern);
                            if (testMatch.Count != 1)
                            {
                                testMatch = Regex.Matches(type.Value, dc_at_v_pattern);
                                if (testMatch.Count != 1)
                                {
                                    objective.Achieved =
                                        string.Format("DVH Objective expression \"{0}\" is not a recognized expression type.",
                                            objective.DVHObjective);
                                    //                                MessageBox.Show(objective.Achieved);
                                }
                                else
                                {
                                    // we have Covered Dose at Volume pattern
                                    System.Console.WriteLine("Covered Dose at Volume");
                                    objective.Achieved = "Not supported";
                                }
                            }
                            else
                            {
                                // we have Covered Volume at Dose pattern
                                System.Console.WriteLine("Covered Volume at Dose");
                                objective.Achieved = "Not supported";

                            }
                        }
                        else
                        {
                            // we have Dose at Volume pattern
                            //                        MessageBox.Show("Dose at Volume");
                            /*
              string d_at_v_pattern = @"^D(?<evalpt>\d+)(?<unit>(%|cc))$"; // matches D95%, D2cc
                             */
                            Group eval = testMatch[0].Groups["evalpt"];
                            Group unit = testMatch[0].Groups["unit"];
                            DoseValue.DoseUnit du = (unit.Value.CompareTo("%") == 0) ? DoseValue.DoseUnit.Percent :
                                    (unit.Value.CompareTo("Gy") == 0) ? DoseValue.DoseUnit.Gy : DoseValue.DoseUnit.Unknown;
                            VolumePresentation vp = (unit.Value.CompareTo("%") == 0) ? VolumePresentation.Relative : VolumePresentation.AbsoluteCm3;
                            DoseValue dv = new DoseValue(double.Parse(eval.Value), du);
                            double volume = double.Parse(eval.Value);
                            VolumePresentation vpFinal = (evalunit.Value.CompareTo("%") == 0) ? VolumePresentation.Relative : VolumePresentation.AbsoluteCm3;
                            DoseValuePresentation dvpFinal = (evalunit.Value.CompareTo("%") == 0) ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute;

                            DoseValue dvAchieved = plan.GetDoseAtVolume(evalStructure, volume, vp, dvpFinal);
                            //checking dose output unit and adapting to template
                            if (dvAchieved.UnitAsString.CompareTo(evalunit.Value.ToString()) != 0)
                            {
                                if ((evalunit.Value.CompareTo("Gy") == 0) && (dvAchieved.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0))
                                {
                                    dvAchieved = new DoseValue(dvAchieved.Dose / 100, DoseValue.DoseUnit.Gy);
                                }
                                else
                                {
                                  throw new ApplicationException("internal error");
                                }
                            }

                            objective.Achieved = dvAchieved.ToString();
                        }
                    }
                    else
                    {
                        // we have Volume at Dose pattern
                        //                    MessageBox.Show("Volume at Dose");
                        /*
            string v_at_d_pattern = @"^V(?<evalpt>\d+)(?<unit>(%|Gy|cGy))$"; // matches V98%, V40Gy, V4000cGy
                         */
                        Group eval = testMatch[0].Groups["evalpt"];
                        Group unit = testMatch[0].Groups["unit"];
                        DoseValue.DoseUnit du = (unit.Value.CompareTo("%") == 0) ? DoseValue.DoseUnit.Percent :
                                (unit.Value.CompareTo("Gy") == 0) ? DoseValue.DoseUnit.Gy : DoseValue.DoseUnit.Unknown;
                        VolumePresentation vp = (unit.Value.CompareTo("%") == 0) ? VolumePresentation.Relative : VolumePresentation.AbsoluteCm3;
                        DoseValue dv = new DoseValue(double.Parse(eval.Value), du);
                        double volume = double.Parse(eval.Value);
                        VolumePresentation vpFinal = (evalunit.Value.CompareTo("%") == 0) ? VolumePresentation.Relative : VolumePresentation.AbsoluteCm3;
                        DoseValuePresentation dvpFinal = (evalunit.Value.CompareTo("%") == 0) ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute;
                        double volumeAchieved = plan.GetVolumeAtDose(evalStructure, dv, vpFinal);
                        objective.Achieved = string.Format("{0:0.00} {1}", volumeAchieved, evalunit.Value);   // todo: better formatting based on VolumePresentation
#if false
                        string message = string.Format("{0} - Dose unit = {1}, Volume Presentation = {2}, vpFinal = {3}, dvpFinal ={4}", 
                            objective.DVHObjective, du.ToString(), vp.ToString(), vpFinal.ToString(), dvpFinal.ToString());
                        MessageBox.Show(message);
#endif
                    }
                }
                else
                {

                    // we have Min, Max, Mean, or Volume
                    if (type.Value.CompareTo("Volume") == 0)
                    {
                        objective.Achieved = string.Format("{0:0.00} {1}", evalStructure.Volume, evalunit.Value);
                    }
                    else
                    {
                        DoseValuePresentation dvp = (evalunit.Value.CompareTo("%") == 0) ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute;
                        DVHData dvh = plan.GetDVHCumulativeData(evalStructure, dvp, VolumePresentation.Relative, 0.1);
                        if (type.Value.CompareTo("Max") == 0)
                        {
                            //checking dose output unit and adapting to template
                            //Gy to cGy
                            if ((evalunit.Value.CompareTo("Gy") == 0) && (dvh.MaxDose.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0))
                            {
                              objective.Achieved = new DoseValue(dvh.MaxDose.Dose / 100, DoseValue.DoseUnit.Gy).ToString();
                            }
                            //Gy to Gy or % to %
                            else
                            {
                              objective.Achieved = dvh.MaxDose.ToString();
                            }                            
                        }
                        else if (type.Value.CompareTo("Min") == 0)
                        {
                            //checking dose output unit and adapting to template
                            //Gy to cGy
                            if ((evalunit.Value.CompareTo("Gy") == 0) && (dvh.MinDose.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0))
                            {
                              objective.Achieved = new DoseValue(dvh.MinDose.Dose / 100, DoseValue.DoseUnit.Gy).ToString();
                            }
                            //Gy to Gy or % to %
                            else
                            {
                              objective.Achieved = dvh.MinDose.ToString();
                            }      
                        }
                        else
                        {
                            //checking dose output unit and adapting to template
                            //Gy to cGy
                            if ((evalunit.Value.CompareTo("Gy") == 0) && (dvh.MeanDose.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0))
                            {
                              objective.Achieved = new DoseValue(dvh.MeanDose.Dose / 100, DoseValue.DoseUnit.Gy).ToString();
                            }
                            //Gy to Gy or % to %
                            else
                            {
                              objective.Achieved = dvh.MeanDose.ToString();
                            }    
                        }
                    }
                }
            }
            // further decompose <ateval>
            //look at the evaluator and compare to goal or variation
            foreach (var objective in m_objectives)
            {

                string evalpattern = @"^(?<type><|<=|=|>=|>)(?<goal>\d+\p{P}\d+|\d+)$";
                if (!String.IsNullOrEmpty(objective.Evaluator))
                {
                    var matches = Regex.Matches(objective.Evaluator, evalpattern);
                    if (matches.Count != 1)
                    {
                        MessageBox.Show("Eval pattern not recognized");
                        objective.Met =
                        string.Format("Evaluator expression \"{0}\" is not a recognized expression type.",
                            objective.Evaluator);
                    }
                    Match m = matches[0];
                    Group goal = m.Groups["goal"];
                    Group evaltype = m.Groups["type"];


                    if (String.IsNullOrEmpty(Regex.Match(objective.Achieved, @"\d+\p{P}\d+|\d+").Value))
                    {
                        objective.Met = "Not evaluated";
                    }
                    else
                    {
                        double evalvalue = Double.Parse(Regex.Match(objective.Achieved, @"\d+\p{P}\d+|\d+").Value);
                        if (evaltype.Value.CompareTo("<") == 0)
                        {
                            if ((evalvalue - Double.Parse(goal.ToString())) < 0)
                            {
                                objective.Met = "Goal";
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(objective.Variation))
                                {
                                    objective.Met = "Not met";
                                }
                                else
                                {
                                    if ((evalvalue - Double.Parse(objective.Variation)) < 0)
                                    {
                                        objective.Met = "Variation";
                                    }
                                    else
                                    {
                                        objective.Met = "Not met";
                                    }
                                }
                            }
                        }
                        else if (evaltype.Value.CompareTo("<=") == 0)
                        {
                            //MessageBox.Show("evaluating <= " + evaltype.ToString());
                            if ((evalvalue - Double.Parse(goal.ToString())) <= 0)
                            {
                                objective.Met = "Goal";
                            }
                            else
                            {
                                //MessageBox.Show("Evaluating variation");
                                if (String.IsNullOrEmpty(objective.Variation))
                                {
                                    //MessageBox.Show(String.Format("Empty variation condition Achieved: {0} Variation: {1}", objective.Achieved.ToString(), objective.Variation.ToString()));
                                    objective.Met = "Not met";
                                }
                                else
                                {
                                    //MessageBox.Show(String.Format("Non Empty variation condition Achieved: {0} Variation: {1}", objective.Achieved.ToString(), objective.Variation.ToString()));
                                    if ((evalvalue - Double.Parse(objective.Variation)) <= 0)
                                    {
                                        objective.Met = "Variation";
                                    }
                                    else
                                    {
                                        objective.Met = "Not met";
                                    }
                                }
                            }
                        }
                        else if (evaltype.Value.CompareTo("=") == 0)
                        {
                            if ((evalvalue - Double.Parse(goal.ToString())) == 0)
                            {
                                objective.Met = "Goal";
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(objective.Variation))
                                {
                                    objective.Met = "Not met";
                                }
                                else
                                {
                                    if ((evalvalue - Double.Parse(objective.Variation)) == 0)
                                    {
                                        objective.Met = "Variation";
                                    }
                                    else
                                    {
                                        objective.Met = "Not met";
                                    }
                                }
                            }
                        }
                        else if (evaltype.Value.CompareTo(">=") == 0)
                        {
                            if ((evalvalue - Double.Parse(goal.ToString())) >= 0)
                            {
                                objective.Met = "Goal";
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(objective.Variation))
                                {
                                    objective.Met = "Not met";
                                }
                                else
                                {
                                    if ((evalvalue - Double.Parse(objective.Variation)) >= 0)
                                    {
                                        objective.Met = "Variation";
                                    }
                                    else
                                    {
                                        objective.Met = "Not met";
                                    }
                                }
                            }
                        }
                        else if (evaltype.Value.CompareTo(">") == 0)
                        {
                            if ((evalvalue - Double.Parse(goal.ToString())) > 0)
                            {
                                objective.Met = "Goal";
                            }
                            else
                            {
                                if (String.IsNullOrEmpty(objective.Variation))
                                {
                                    objective.Met = "Not met";
                                }
                                else
                                {
                                    if ((evalvalue - Double.Parse(objective.Variation)) > 0)
                                    {
                                        objective.Met = "Variation";
                                    }
                                    else
                                    {
                                        objective.Met = "Not met";
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }


    }
    public static class DvhExtensions
    {
        public static DoseValue GetDoseAtVolume(this PlanningItem pitem, Structure structure, double volume, VolumePresentation volumePresentation, DoseValuePresentation requestedDosePresentation)
        {
            if (pitem is PlanSetup)
            {
                return ((PlanSetup)pitem).GetDoseAtVolume(structure, volume, volumePresentation, requestedDosePresentation);
            }
            else
            {
                if (requestedDosePresentation != DoseValuePresentation.Absolute)
                    throw new ApplicationException("Only absolute dose supported for Plan Sums");
                DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, volumePresentation, 0.001);
                return DvhExtensions.DoseAtVolume(dvh, volume);
            }
        }
        public static double GetVolumeAtDose(this PlanningItem pitem, Structure structure, DoseValue dose, VolumePresentation requestedVolumePresentation)
        {
            if (pitem is PlanSetup)
            {
                //try catch statement to switch dose units to system presentation. Otherwise exception "Dose Units do not match to system settings
                try
                {
                    return ((PlanSetup)pitem).GetVolumeAtDose(structure, dose, requestedVolumePresentation);
                }
                catch
                {
                    if (dose.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0)
                    {
                        return ((PlanSetup)pitem).GetVolumeAtDose(structure, new DoseValue(dose.Dose / 100, DoseValue.DoseUnit.Gy), requestedVolumePresentation);
                    }
                    else if (dose.Unit.CompareTo(DoseValue.DoseUnit.Gy) == 0)
                    {
                        return ((PlanSetup)pitem).GetVolumeAtDose(structure, new DoseValue(dose.Dose * 100, DoseValue.DoseUnit.cGy), requestedVolumePresentation);
                    }
                    else
                    {
                        return ((PlanSetup)pitem).GetVolumeAtDose(structure, dose, requestedVolumePresentation);
                    }
                }
            }
            else
            {
                DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, requestedVolumePresentation, 0.001);
                //convert dose unit to system unit: otherwise false output without warning
                try
                {
                    ((PlanSum)pitem).PlanSetups.First().GetVolumeAtDose(structure, dose, requestedVolumePresentation);
                    return DvhExtensions.VolumeAtDose(dvh, dose.Dose);
                }
                catch
                {
                    if (dose.Unit.CompareTo(DoseValue.DoseUnit.cGy) == 0)
                    {
                        return DvhExtensions.VolumeAtDose(dvh, dose.Dose / 100);
                    }
                    else if (dose.Unit.CompareTo(DoseValue.DoseUnit.Gy) == 0)
                    {
                        return DvhExtensions.VolumeAtDose(dvh, dose.Dose * 100);
                    }
                    else
                    {
                        return DvhExtensions.VolumeAtDose(dvh, dose.Dose);
                    }
                }
            }
        }

        public static DoseValue DoseAtVolume(DVHData dvhData, double volume)
        {
            if (dvhData == null || dvhData.CurveData.Count() == 0)
                return DoseValue.UndefinedDose();
            double absVolume = dvhData.CurveData[0].VolumeUnit == "%" ? volume * dvhData.Volume * 0.01 : volume;
            if (volume < 0.0 || absVolume > dvhData.Volume)
                return DoseValue.UndefinedDose();

            DVHPoint[] hist = dvhData.CurveData;
            for (int i = 0; i < hist.Length; i++)
            {
                if (hist[i].Volume < volume)
                    return hist[i].DoseValue;
            }
            return dvhData.MaxDose;
        }

        public static double VolumeAtDose(DVHData dvhData, double dose)
        {
            if (dvhData == null)
                return Double.NaN;

            DVHPoint[] hist = dvhData.CurveData;
            int index = (int)(hist.Length * dose / dvhData.MaxDose.Dose);
            if (index < 0 || index >= hist.Length)
                return 0.0;//Double.NaN;
            else
                return hist[index].Volume;
        }
        public static bool IsDoseValid(this PlanningItem pitem)
        {
            if (pitem is PlanSetup)
            {
                return ((PlanSetup)pitem).IsDoseValid;
            }
            else if (pitem is PlanSum)
            {   // scan for plans with invalid dose, if there are none then we can assume plansum dose is valid.
                PlanSum psum = (PlanSum)pitem;
                var plans = (from p in psum.PlanSetups where p.IsDoseValid == false select p);
                return plans.Count() <= 0;
            }
            else
            {
                throw new ApplicationException("Unknown PlanningItem type " + pitem.ToString());
            }
        }
    }
}
