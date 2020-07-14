using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// Workhorse class for the program.
    /// Reads in CSV of objectives and evaluates against the selected plan(s).
    /// If being run in SinglePlan mode, the plan is the currently loaded plan.
    /// If being run in PlanComparison mode, a GUI is shown for the user to select plans.
    /// Results are shown in an HTML file for the user to view.
    /// </summary>
    public class DataModel
    {
        // CSV Constants
        public string WORKBOOK_TEMPLATE_DIR = ConfigurationInfo.ObjectiveCSVLocation;
        public string WORKBOOK_RESULT_DIR = System.IO.Path.GetTempPath();
        public string defaultFileFilter = null;
        public string CSVFileName = null;
        public bool CSVFileIsNotDefault = false;
        public string CSVSaveLocation = null;
        
        private List<string[]> CSVSheet = new List<string[]>();
        private List<string> CSVHeader;
        private List<string[]> DataOut = new List<string[]>();

        // Class properties
        public List<StructureObjective> Objectives; // List of DVH Objectives that will be checked. //all the dose values are internally in Gy.
        public List<string> UniqueStructures; // List of all structures in the objectives
        public List<PlanResult> Results = new List<PlanResult>(); // List that contains results for each plan.
        public Dictionary<string, string> WarningDictionary = new Dictionary<string, string>(); // Dictionary for warning numbers and definitions
        public Dictionary<StructureSet, Dictionary<string, Structure>> StructureDictionary = new Dictionary<StructureSet, Dictionary<string, Structure>>(); // Dictionary to map structure names and structure for each structure set
        public string Mode;

        // Parameters currently loaded in Context
        public User ContextUser { get; set; }
        public Patient ContextPatient { get; set; }
        public Image ContextImage { get; set; }
        public StructureSet ContextStructureSet { get; set; }
        public PlanSetup ContextPlanSetup { get; set; }
        public IEnumerable<PlanSetup> ContextPlanSetupsInScope { get; set; }
        public IEnumerable<PlanSum> ContextPlanSumsInScope { get; set; }

        // Constructors
        public DataModel(ScriptContext context, string mode) : this(context.CurrentUser, context.Patient, context.Image, context.StructureSet, context.PlanSetup, context.PlansInScope, context.PlanSumsInScope, mode) { }
        public DataModel(User user, Patient patient, Image image, StructureSet structureSet, PlanSetup planSetup, IEnumerable<PlanSetup> planSetupsInScope, IEnumerable<PlanSum> planSumsInScope, string mode)
        {
            ContextUser = user;
            ContextPatient = patient;
            ContextImage = image;
            ContextStructureSet = structureSet;
            ContextPlanSetup = planSetup;
            ContextPlanSetupsInScope = planSetupsInScope;
            ContextPlanSumsInScope = planSumsInScope;
            Mode = mode;

            InitializeViewModel();
        }

        // Main Program
        // Reads in the CSV file and saves as a list of StructureObjective objects
        // For each plan, creates a PlanResults object
        public void InitializeViewModel()
        {
            // Read in the objectives. The same objectives are used for all plans
            CreateObjectives();

            // Add plans to check
            // Determine whether this is a standard DVHEvaluator ("singlePlan"), Plan Comparison tool ("planComparison"), or debugging ("debug") situation
            switch (Mode)
            {
                case "singlePlan":
                    // Validate context and add it to plan.
                    PlanResult contextPlanResult = ValidateContext();
                    Results.Add(contextPlanResult);
                    break;
                case "planComparison":
                    // Open GUI to allow user to choose plans
                    PlanChooserViewModel planChooserViewModel = new PlanChooserViewModel(this);
                    if (planChooserViewModel.ChosenPlans.Any())
                    {
                        planChooserViewModel.ChosenPlans.ForEach(x => Results.Add(AddPlan(x)));
                    }
                    // If no plan, exit. Error-handling is dealth with in the GUI.
                    else
                    {
                        return;
                    }

                    break;
                case "debug":
                    // Load all plans in scope
                    ContextPlanSetupsInScope.ToList().ForEach(x => Results.Add(AddPlan(x)));
                    ContextPlanSumsInScope.ToList().ForEach(x => Results.Add(AddPlan(x)));
                    break;
                default:
                    throw new ApplicationException(string.Format("Chosen mode '{0}' does not exist.", Mode));
            }

            // Reorder results by given convention
            ReOrderResults();

            // Compute results for all plans
            Results.ForEach(x => x.ComputeResults(Objectives));

            // Find all unique warnings in the plan and assign codes to them.
            List<string> warn = (from res in Results
                                 from s in res.StructureObjectiveResults
                                 from w in s.Warnings
                                 select w).ToList().Distinct().ToList();
            foreach (int i in Enumerable.Range(1, warn.Count))
            {
                WarningDictionary.Add(warn[i - 1], i.ToString());
            }

            // Check plan ID for invalid filename characters and delete if present 
            string regexSearch = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            string strPlanId = r.Replace(Results.First().PlanningItem.Id.ToString(), "_");
            string strPatientId = r.Replace(Results.First().Patient.Id.ToString(), "_");
            string outputpath = System.IO.Path.Combine(WORKBOOK_RESULT_DIR, strPatientId + "-" + strPlanId + "-" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".html");

            // Print to HTML and open it
            DataOut = UpdateWorkbook(CSVSheet, Results);
            DataTable table = ConvertListToDataTable(DataOut, CSVHeader);
            string HtmlBody = ExportToHtml(table);
            System.IO.File.WriteAllText(outputpath, HtmlBody);
            System.Diagnostics.Process.Start(outputpath);
        }

        // Validate the context parameter to make sure patient and plan are loaded
        private PlanResult ValidateContext()
        {
            PlanResult planResult = new PlanResult();

            // Validate input
            // If a plansum is loaded take only a single plansum, otherwise take the active plansetup.

            // A plan is loaded
            if (ContextPlanSetup != null)
            {
                planResult = AddPlan(ContextPlanSetup);
            }
            else
            {
                // A plansum is loaded
                if (ContextPlanSumsInScope.Count() == 1)
                {
                    planResult = AddPlan(ContextPlanSumsInScope.Single());
                }
                // No plans or plansums are loaded
                else if (ContextPlanSumsInScope.Count() == 0)
                {
                    throw new ApplicationException("Please load a plan or plansum.");
                }
                // Too many plansums are loaded. Eclipse cannot determine which plansum is in scope if there is more than one.
                else if (ContextPlanSumsInScope.Count() > 1)
                {
                    throw new ApplicationException("Please close all but one plan sum.");
                }
            }

            return planResult;
        }

        // Read the CSV and create Objectives object
        public void CreateObjectives()
        {
            // Choose file if we haven't already
            string fileName = "";
            if (this.CSVFileName == null)
            {
                // Make sure the workbook directory exists
                if (!System.IO.Directory.Exists(WORKBOOK_TEMPLATE_DIR))
                {
                    throw new ApplicationException(string.Format("The default template file directory '{0}' defined by the script does not exist.", WORKBOOK_TEMPLATE_DIR));
                }

                // Find the csv file
                string patientId = ContextPatient.Id;
                defaultFileFilter = "*" + patientId + "*";
                string[] files = Directory.GetFiles(WORKBOOK_TEMPLATE_DIR, defaultFileFilter);

                // Check that there's only one
                if (files.Count() == 1)
                {
                    //MessageBox.Show("Loading File: " + files[0].ToString());
                    fileName = files[0];
                }
                else
                {
                    // If none, they have to export it from the EBRT
                    if (files.Count() == 0)
                    {
                        throw new ApplicationException(string.Format("No template files found for MRN: '{0}'.\nPlease run EBRT extraction script for this patient.", patientId));
                    }

                    // If too many, have them choose one
                    else
                    {
                        Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog
                        {
                            DefaultExt = "csv",
                            InitialDirectory = WORKBOOK_TEMPLATE_DIR,
                            Multiselect = false,
                            Title = "Multiple files found - select appropriate template file",
                            ShowReadOnly = true,
                            Filter = "CSV files (*.csv)|*.csv",
                            FilterIndex = 0,
                            CheckFileExists = true,
                            RestoreDirectory = true,
                            FileName = defaultFileFilter
                        };
                        if (fileDialog.ShowDialog() == false)
                        {
                            return;    // user canceled
                        }

                        var file = fileDialog.FileName;
                        fileName = file;
                    }
                }
            }
            else
            {
                fileName = CSVFileName;
            }

            // Make sure the CSV file exists
            if (!System.IO.File.Exists(fileName))
            {
                throw new ApplicationException(string.Format("The template file '{0}' chosen does not exist.", fileName));
            }

            // Read CSV to list of string arrays and pull out header
            CSVSheet = ParseCSV(fileName);
            CSVHeader = new List<string>(CSVSheet.First());
            CSVHeader[2] = "Patient Structure";
            CSVSheet.RemoveAt(0);
            CSVFileName = fileName;

            // Convert CSV to Array of StructureObjectives objects
            Objectives = ReadObjectives(CSVSheet);
            UniqueStructures = Objectives.Select(x => x.ID).ToList().Distinct().ToList();
        }

        // Convert CSVsheet to list of StructureObjective objects
        private List<StructureObjective> ReadObjectives(List<string[]> CSVsheet)
        {
            // Read an Excel sheet that has the following columns defined:
            // [0]Structure ID [1]Structure Code [2]Aliases [3]DVH Objective [4]Evaluator [5]VariationAcceptable [6]Priority [7]Met [8]Achieved

            List<StructureObjective> objectives = new List<StructureObjective>();
            for (int i = 0; i < CSVsheet.Count; i++)
            {
                string[] line = CSVsheet[i];

                // If the line is a different length (likely due to commas in the fields), insert an error we can recognize later.
                if (line.Length != CSVHeader.Count)
                {
                    line = new string[]{line[0],"","","Parsing Error","","","","",""};
                    CSVsheet[i] = line;
                }
                string id = line[0];
                string aliases = line[2];
                objectives.Add(new StructureObjective()
                {
                    ID = id,
                    Code = line[1],
                    Aliases = (aliases.Length > 0) ? aliases.Split('|') : new string[] { "" },
                    DVHObjective = line[3],
                    Evaluator = line[4],
                    Variation = line[5],
                    Priority = line[6],
                });
                objectives.Last().DetermineMetricType();
            }
            return objectives;
        }
 
        // Read in CSV file and convert to List of Strings.
        private List<string[]> ParseCSV(string path)
        {
            List<string[]> parsedData = new List<string[]>();
            string[] fields;

            try
            {
                var parser = new StreamReader(File.OpenRead(path));
                while (!parser.EndOfStream)
                {
                    fields = parser.ReadLine().Split(',');
                    parsedData.Add(fields);
                }
                parser.Close();

                // Remove empty lines at end and beginning. Stop when we find data.
                // Don't remove empty lines in the middle (some groups use them for logical separation of the objectives)
                for (int i = parsedData.Count-1; i>=0; i--)
                {
                    if (parsedData[i].All(x => x == ""))
                        parsedData.RemoveAt(i);
                    else
                        break;
                }
                for (int i = 1; i < parsedData.Count; i++) // Start at 1 because of header
                {
                    if (parsedData[i].All(x => x == ""))
                    {
                        parsedData.RemoveAt(i);
                        i--; //Reduce i since we removed it and everything has moved up an index
                    }
                    else
                        break;
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(e.Message);
            }
            return parsedData;
        }

        // Validate a PlanSetup or PlanSum and return a PlanResult item.
        private PlanResult AddPlan(PlanningItem planningItem)
        {
            Course course;
            StructureSet ss = planningItem.StructureSet;

            // Determine plan type as best as we can (Initial, CD, PlanSum, Other)
            string planType;
            if (planningItem is PlanSum)
            {
                planType = "PlanSum";
                course = (planningItem as PlanSum).Course;
            }
            else
            {
                if (planningItem.Id.ToUpper().Contains("INI"))
                {
                    planType = "Initial";
                }
                else if (planningItem.Id.ToUpper().Contains("CD"))
                {
                    planType = "CD";
                }
                else
                {
                    planType = "Other";
                }

                course = (planningItem as PlanSetup).Course;
            }

            Patient patient = course.Patient;

            // Validate the Plan
            if (planningItem == null)
            {
                throw new ApplicationException(string.Format("Plan or plansum {0}:{1} unable to be loaded: plan is invalid", course.Id, planningItem.Id));
            }

            if (patient == null)
            {
                throw new ApplicationException(string.Format("Plan or plansum {0}:{1} unable to be loaded: associated patient is invalid", course.Id, planningItem.Id));
            }

            if (ss == null)
            {
                throw new ApplicationException(string.Format("Plan or plansum {0}:{1} unable to be loaded: associated structure set is invalid", course.Id, planningItem.Id));
            }

            if (!planningItem.IsDoseValid())
            {
                throw new ApplicationException(string.Format("Please calculate dose for plan or plansum {0}:{1} before running this script.", course.Id, planningItem.Id));
            }

            Dictionary<string, Structure> ssDict;
            // Find structures for structure set
            if (StructureDictionary.Keys.Contains(ss))
            {
                ssDict = StructureDictionary[ss];
            }
            else
            {
                ssDict = AddStructureSetToDictionary(ss);
                StructureDictionary.Add(ss, ssDict);
            }

            // return Plan Result
            PlanResult planResult = (new PlanResult()
            {
                Patient = patient,
                PlanningItem = planningItem,
                StructureSet = ss,
                StructureSetDict = ssDict,
                PlanName = planningItem.Id,
                PlanType = planType,
                CourseName = course.Id
            });

            return planResult;

        }
        private PlanResult AddPlan(PlanSetup planSetup) { return AddPlan(planSetup as PlanningItem); }
        private PlanResult AddPlan(PlanSum planSum) { return AddPlan(planSum as PlanningItem); }

        // Used to map structure names to Structures. Speeds up processing.
        private Dictionary<string, Structure> AddStructureSetToDictionary(StructureSet ss)
        {
            Dictionary<string, Structure> ssDict = new Dictionary<string, Structure>();
            List<string> structureNames = ss.Structures.Select(x => x.Id.ToUpper()).ToList();

            foreach (string id in UniqueStructures)
            {
                Structure oar;
                int idx = structureNames.IndexOf(id.ToUpper());

                // If structure is found
                if (idx != -1)
                {
                    oar = ss.Structures.ElementAt(idx);
                }

                // Structure not found
                else
                {
                    // Also match "external" for body
                    if (id.ToUpper().CompareTo("EXTERNAL") == 0)
                    {
                        idx = structureNames.IndexOf("BODY");
                        if (idx != -1)
                        {
                            oar = ss.Structures.ElementAt(idx);
                        }
                        else
                        {
                            oar = null;
                        }
                    }
                    else
                    {
                        oar = null;
                    }
                }
                // If there is no contour drawn, we can't use it.
                if (oar != null && oar.IsEmpty)
                {
                    oar = null;
                }

                ssDict.Add(id, oar);
            }
            return ssDict;
        }

        // Reorder Results by course name(C1, C2, ... followed by all other courses) and then by plan type(Initial, CD, PlanSum, Other).
        private void ReOrderResults()
        {
            Regex c_numberRegex = new Regex(@"^(?i)(C)\d+"); // Match C### at beginning of course name
            Results = Results.
                      OrderBy(x => !c_numberRegex.IsMatch(x.CourseName)). //Must be ! (i.e.: not) because this evaluates to zeros and ones and we want the ones first
                      ThenBy(x => x.CourseName.ToUpper()).
                      ThenBy(x => x.PlanType != "Initial").
                      ThenBy(x => x.PlanType != "CD").
                      ThenBy(x => x.PlanType != "PlanSum").
                      ThenBy(x => x.PlanType != "Other").
                      ToList();
        }

        // Update Workbook and Header. Works for multiple plans.
        private List<string[]> UpdateWorkbook(List<string[]> CSVsheet, List<PlanResult> planResults)
        {
            // Update header: Remove StructureCode, Aliases, Met, Achieved. Rename Variation.
            CSVHeader = new List<string>{ "StructureName","DVHObjective","Evaluator","Variation","Priority"};
            foreach (PlanResult planResult in planResults) // Add headers for each plan for Met, Achieved, Warnings
            {
                CSVHeader.Add(string.Format("Met {0}:{1}", planResult.CourseName, planResult.PlanName));
                CSVHeader.Add(string.Format("Achieved {0}:{1}", planResult.CourseName, planResult.PlanName));
                CSVHeader.Add(string.Format("Warnings {0}:{1}", planResult.CourseName, planResult.PlanName));
            }

            // Headers are now:
            // [0]StructureName [1]DVH Objective [2]Evaluator [3]Variation [4] Priority [5]Met1 [6]Achieved1 [7]Met2 [8]Achieved2 .....

            int row = 0;
            List<string[]> Calculated = new List<string[]>();
            foreach (StructureObjective DVHObjective in Objectives)
            {
                List<string> line = new List<string>();
                line.Add(DVHObjective.ID);
                line.Add(DVHObjective.DVHObjective == "Parsing Error" ? "" : DVHObjective.DVHObjective);
                line.Add(DVHObjective.Evaluator);
                line.Add(DVHObjective.Variation);
                line.Add(DVHObjective.Priority);

                // If everything is blank except the first column, it's probably a subheading (e.g.: "Initial" or "CD" for breast). Ignore it.
                bool allIsBlank = string.IsNullOrEmpty(DVHObjective.DVHObjective) &&
                                  string.IsNullOrEmpty(DVHObjective.Evaluator) &&
                                  string.IsNullOrEmpty(DVHObjective.Variation) &&
                                  string.IsNullOrEmpty(DVHObjective.Priority);

                // Loop through Plans for Met/Achieved/Warnings
                foreach (PlanResult planResult in planResults)
                {
                    if (allIsBlank)
                    {
                        line.Add("");
                        line.Add("");
                        line.Add("");
                        continue;
                    }
                    StructureObjectiveResult objective = planResult.StructureObjectiveResults.Where(x => x.Objective.Equals(DVHObjective)).First();
                    line.Add(objective.Met);
                    line.Add(objective.Achieved);
                    line.Add(string.Join(",", objective.Warnings.Select(x => WarningDictionary[x]).OrderBy(x => int.Parse(x)))); //Add codes for all warnings
                }
                Calculated.Add(line.ToArray());
                row++;
            }
            // Save the CSV if necessary
            if (!string.IsNullOrEmpty(CSVSaveLocation))
            {
                // Make temporary changes to the CSV (that we don't want propogating to HTML)
                List<string[]> tmp = new List<string[]>(Calculated.Select(x => x.Clone() as string[]));
                tmp.Insert(0, CSVHeader.ToArray());
                string[] anEmptyLine = new string[] { "" };
                tmp.Add(anEmptyLine);
                tmp.Add(anEmptyLine);
                tmp.Add(new string[] { "Warnings: " });
                foreach (KeyValuePair<string, string> w in WarningDictionary)
                {
                    tmp.Add(new string[] { string.Format("{0}: {1}", w.Value, w.Key) });
                }

                File.WriteAllLines(CSVSaveLocation, tmp.Select(x => string.Join(",", x.Select(y => y.Replace(",",";")))));
                MessageBox.Show("Data successfully saved to " + CSVSaveLocation + ".");
            }

            return Calculated;
        }

        // Convert the results data to something that can be HTML-ified.
        private static DataTable ConvertListToDataTable(List<string[]> list, List<string> CSVHeader)
        {
            // New table.
            DataTable table = new DataTable();

            // Add columns.
            foreach (string header in CSVHeader)
                table.Columns.Add(header);

            // Add rows.
            foreach (var array in list)
                table.Rows.Add(array);

            return table;
        }
        
        // Function to convert the datatable to a formatted HTML (Goal=Green, Variation=yellow, Not met=red).
        private string ExportToHtml(DataTable dt)
        {
            // Read HTML template from an Embedded Resource in the root directory
            var resourceName = "DVH_Evaluator.Template_HTML.txt";
            StringBuilder strHTMLBuilder = new StringBuilder();
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                strHTMLBuilder.Append(reader.ReadToEnd());
            }

            // Build plan header
            List<string[]> planInfo = new List<string[]>();
            planInfo.Add(new string[]{ "Name (ID):", Results.First().Patient.ToString()});
            planInfo.Add(new string[] { "Plans/PlanSums:", string.Join(", ", Results.Select(x => string.Format("[{0}:{1}]", x.CourseName, x.PlanName))) });

            // Also display the EBRT file name if we use the non-default.
            if (CSVFileIsNotDefault)
            {
                planInfo.Add(new string[] { "EBRT File:", Path.GetFileName(CSVFileName) });
            }

            planInfo.Add(new string[] { "Printed:", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") });
            planInfo.Add(new string[] { "Disclaimer:", "For informational purposes only. Individual clinician is responsible for review of all doses in treatment plan(s)." });
            StringBuilder patientInfoTable = new StringBuilder();
            foreach (string[] info in planInfo)
            {
                patientInfoTable.AppendLine("<tr>");
                patientInfoTable.AppendLine("<td>" + info[0] + "</td>");
                patientInfoTable.AppendLine("<td>" + info[1] + "</td>");
                patientInfoTable.AppendLine("</tr>");
            }
            strHTMLBuilder.Replace("###PatientInfoTable", patientInfoTable.ToString().TrimEnd('\r', '\n'));

            // Build Result table
            StringBuilder resultTable = new StringBuilder();
            resultTable.AppendLine("<thead>"); //HEader
            resultTable.AppendLine("<tr>");
            resultTable.AppendLine("<th>StructureName</th>");
            resultTable.AppendLine("<th>DVHObjective</th>");
            resultTable.AppendLine("<th>Evaluator</th>");
            resultTable.AppendLine("<th>Variation</th>");
            resultTable.AppendLine("<th>Priority</th>");
            foreach (PlanResult planResult in Results)
            {
                resultTable.AppendLine(string.Format("<th colspan='3'>{0}:<br>{1}</th>", planResult.CourseName, planResult.PlanName));
            }

            resultTable.AppendLine("</tr>");
            resultTable.AppendLine("</thead>");

            // Print data.
            foreach (DataRow myRow in dt.Rows)
            {
                resultTable.AppendLine("<tr>");
                foreach (DataColumn myColumn in dt.Columns)
                {
                    string starttag = "<td>";
                    string str = myRow[myColumn.ColumnName].ToString().Replace("<", "&lt;").Replace(">", "&gt;");
                    string colName = myColumn.ColumnName.ToString();

                    // Set colors for Met and remove words.
                    if (colName.StartsWith("Met"))
                    {
                        starttag = str.Contains("Goal") ? "<td class='pass'>" :
                                   str.Contains("Variation") ? "<td class='variation'>" :
                                   str.Contains("Not met") ? "<td class='fail'>" : 
                                   "<td class='notEval'>";
                        str = "";
                    }

                    // Colors for warnings.
                    else if (colName.StartsWith("Warning"))
                    {
                        starttag = !string.IsNullOrEmpty(str) ? "<td class='warning'>" : "<td>";
                    }
                    resultTable.AppendLine(starttag + str + "</td>");
                }
                resultTable.AppendLine("</tr>");
            }
            strHTMLBuilder.Replace("###ResultsTable", resultTable.ToString().TrimEnd('\r', '\n'));

            // Build warning table, if applicable.
            if (WarningDictionary.Any())
            {
                StringBuilder warningTable = new StringBuilder();
                warningTable.AppendLine("<table class='warnings'>");
                warningTable.AppendLine("<tr>");
                warningTable.AppendLine("<td>Warnings:");
                foreach (KeyValuePair<string, string> w in WarningDictionary)
                {
                    warningTable.AppendLine(string.Format("<br>{0}: {1}", w.Value, w.Key));
                }

                warningTable.AppendLine("</td>");
                warningTable.AppendLine("</tr>");
                warningTable.AppendLine("</table>");
                warningTable.Replace("<table", "\t\t<table");
                warningTable.Replace("</table>", "\t\t</table>");
                warningTable.Replace("<br>", "\t\t\t\t<br>");
                warningTable.Replace("</td>", "\t\t\t\t</td>");
                strHTMLBuilder.Replace("###WarningsTable", warningTable.ToString().TrimEnd('\r','\n'));
            }
            else
            {
                strHTMLBuilder.Replace("###WarningsTable", "");
            }

            // Replace empty cells with non breaking space so empty rows don't disappear
            strHTMLBuilder.Replace("<td></td>", "<td>&nbsp;</td>"); 

            // Add tabs for formatting
            strHTMLBuilder.Replace("<tr>", "\t\t\t<tr>");
            strHTMLBuilder.Replace("</tr>", "\t\t\t</tr>");
            strHTMLBuilder.Replace("<td", "\t\t\t\t<td");
            strHTMLBuilder.Replace("<th>", "\t\t\t\t<th>");
            strHTMLBuilder.Replace("<th ", "\t\t\t\t<th ");
            strHTMLBuilder.Replace("</thead", "\t\t\t</thead");
            strHTMLBuilder.Replace("<thead>", "\t\t\t<thead>");

            return strHTMLBuilder.ToString();
        }
    }
}



