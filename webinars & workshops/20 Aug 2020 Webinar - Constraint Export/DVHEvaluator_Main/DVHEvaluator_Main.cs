/*
////////////////////////////////////////////////////////////////////////////////
//  Plan Comparison Tool and DVH Evaluator
//  
//  Originally adapted from Varian's DVHMetric v1.0.cs
//
//  A ESAPI v11+ script that uses a CSV file as a DVH metrics template and
//  generates a result file in the HTML format.  Uses DVH analysis description 
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
// October 5, 2016 Tomasz Morgas
// - Change of script type from Binary plugin to Single File Plug-in
// - CSV parsing doesn't need Microsoft.VisualBasic reference
// - Added checks for empty structures (NRG feedback)
// - Added checks for sufficient sampling and dose coverage for evaluation of DHV goals (NRG feedback)
// - Fortmated HTML output
//
// October 4, 2016 Steve Thompson
// - Changed output format to HTML
// - Added color coding to achieved results
//
// July 12, 2016 Steve Thompson
// - Added 2 add'l checks for cGy vs Gy in template vs Eclipse for minmaxmean pattern
// - Note: Add references to solution:
//      - VMS.TPS.Common.Model.API
//      - VMS.TPS.Common.Model.Types
//      - Presentation Framework
//      - Microsoft.VisualBasic
//
// June 23, 2016 Tomasz Morgas
// - Revisions to handle minmaxmean pattern better (from NRG testing)
//
// August 2, 2018 Matt Schmidt
// - Line PlanSetup plan = planItem as PlanSetup was updated from context.PlanSetup because 
//      context does not exist in the Run method.
// - Run method implemented so that the plug-in tester can be used for testing. Will possibly leave as implementation for this reason.
// - Removed the mandatory unit conversion to Gy.
// - If a volume at dose is wanted, the system checks the dose unit and converts to the prescription dose unit before evaluating getvolumeatdose
// - For plansums, the script forces GetDVHCumulativeData to be absolute.
//
// November 8, 2018 Ryan Scheuermann
// - Converted from single file plug-in to binary plug-in format
// - Support for patient-specific or standard EBRT evaluation added
// - Changed default file load directory for csv to \\uphappndc050\DVH Analysis Script
//    - Updated regex for DC and CV objective types to recognize decimals
//    - Changed FindStructureFromAlias to search aliases if primary structure id is either not found or primary structure is empty
//    - EvaluateMetrics updated to convert relative dose objectives to Gy for plan sums and display a warning message that conversion performed
//    - EvaluateMetrics updated so that relative dose objectives with plan sums show 'Not Evaluated' in Met column
//
// November 13, 2018 Ryan Scheuermann
// - Evaluate Metrics updated to handle dose at volume with undefined dose type due to insufficient dose coverage
//
// November 14, 2018 Ryan Scheuermann
// - Updated output HTML filename to replace ":" with "_" to handle use on plan revisions
// - Changed Warning message for unrecognized DVH Objective pattern so that Goal will show as 'Not Evaluated'
//
// November 19, 2018 Ryan Scheuermann
// - Edit edits to UI to change button appearance
// - Added help button
//
// December 06, 2018 Ryan Scheuermann
// - Added error handling for invalid Evaluator and VariationAcceptable formats
// - Changed to yellow highlight for all 'Warning messages'
// - Changed error handing for DVH Objective formatting to continue execution rather than break
//
// March 4, 2019 Ryan Scheuermann
// - Fixed bug for plan sums and VatD constraints with units of 'cGy'
//
// May 22, 2020 Brandon Koger
// - Added "Plan Comparison Tool" mode, to allow analysis on several plans / plansums at once
// - Added support for comformality index, 
// - Improved error handling and visuals
// - Improve backend code to be more robust
//
//////////////////////////////////////////////////////////////////////////////////////
*/

using System;
using System.Reflection;
using System.Windows;
using VMS.TPS.Common.Model.API;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.1.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
// [assembly: ESAPIScript(IsWriteable = true)]

/// <summary>
/// The main execution of the plugin. The general execution is as follows:
/// 1. User runs DVHEvaluator.esapi or PlanComparisonTool.esapi, each of which calls this method with a pre-defined "mode" parameter.
/// 2. This method creates a log file (Logger), chooses plan running mode, and executes the script.
/// 3. DataModel is called. This determines the plan(s) and evaluation criteria, and calculates pass / fail rates.
/// 4. The GUI is only launched if the program is in PlanComparison mode, allowing the user to choose which plan(s) to load.
/// 5. Results are shown in an HTML launched by the user's default HTML program (usually a web browser).
/// 
/// Preperation:
///  - ConfigurationInfo.cs stores the location of several files used by the Application.
///  - When implementing this script, users should edit ConfigurationInfo.ScriptDataLocation to their own location.
///  - This defaults to the ExampleData directory, which also includes an example CSV file.
/// </summary>
namespace DVHEvaluator_Main
{
    public class DVHEvaluator_Main
    {
        public DVHEvaluator_Main(ScriptContext context, string mode)
        {
            // Set up log file.
            Logger logger = new Logger(context);

            try
            {
                // Options for mode. Allows one code for PlanComparison and standard DVHEvaluator.
                // string mode = new string[] { "singlePlan", "planComparison", "debug" }[1];
                logger.ProgramMode = mode;
                var viewModel = new DataModel(context, mode);
            }
            catch (Exception e)
            {
                logger.Error(e);
                MessageBox.Show(e.Message);
            }
            logger.EndProgram();
        }
    }
}
