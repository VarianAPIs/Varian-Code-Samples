using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;
using System.Text.RegularExpressions;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// ViewModel for PlanChooser.
    /// Displays all Courses and Plans / PlanSums in the current patient, allowing user to choose any number of them.
    /// By default, all PlanningItems in scope are selected.
    /// Any courses without plans, or plans without calculated dose are not selectable.
    /// 
    /// This implements INotifyPropertyChanged in order to alert the PlanChooserView that data has been updated.
    /// </summary>
    public class PlanChooserViewModel : INotifyPropertyChanged
    {
        // GUI properties
        public ReadOnlyCollection<CourseForTreeViewModel> _courses;
        public string PatientInfo { get; set; }
        public string CSVFileTextBox { get; set; }
        public Window GUIWindow;
        bool _selectAllCheckBox;

        DataModel CallingDataModel { get; set; }

        // Return object
        public List<PlanningItem> ChosenPlans = new List<PlanningItem>();

        // Constructor.
        public PlanChooserViewModel(DataModel dataModel)
        {
            // Set local variables.
            CallingDataModel = dataModel;
            Patient patient = CallingDataModel.ContextPatient;
            IEnumerable<PlanSetup> planSetupsInScope = CallingDataModel.ContextPlanSetupsInScope;
            IEnumerable<PlanSum> planSumsInScope = CallingDataModel.ContextPlanSumsInScope;

            // Info to display at the top.
            PatientInfo = "Name (ID):  " + patient.ToString();
            CSVFileTextBox = Path.GetFileName(CallingDataModel.CSVFileName);

            // Add courses and plans to the Plan Tree. Plans get added within the CourseForTree class.
            // Reorders courses for clinical courses first (e.g.: C1 Brain) and then other course (e.g.: QA, EVAL, etc.)
            Regex c_numberRegex = new Regex(@"^(?i)(C)\d+");
            List<Course> orderedCourseList = patient.Courses.
                                             OrderBy(x => !c_numberRegex.IsMatch(x.Id)).
                                             ThenBy(x => x.Id.ToUpper()).ToList();
            _courses = new ReadOnlyCollection<CourseForTreeViewModel>(
                            (from course in orderedCourseList
                            select new CourseForTreeViewModel(new CourseForTree(course, planSetupsInScope, planSumsInScope), planSetupsInScope, planSumsInScope))
                            .ToList());

            // Create a new modal GUI window for user to choose plans to analyze.
            // Modal windows allow the program execution to pause until the window is closed.
            PlanChooserView GUI = new PlanChooserView(this);
            GUIWindow = new Window
            {
                Title = "Plan Comparison Tool",
                Content = GUI,
                MinHeight = 200,
                Height = 500,
                MinWidth = 200,
                Width = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            GUIWindow.ShowDialog();
        }

        // When apply button is clicked, get chosen plans and exit GUI.
        public void RunReport()
        {
            ChosenPlans.AddRange(from CourseForTreeViewModel course in CoursesList
                                 from PlanForTreeViewModel plan in course.Children
                                 where plan.IsChecked == true
                                 select plan.PlanningItemObject);
            if (ChosenPlans.Any())
            {
                GUIWindow.Close();
            }
            else
            {
                MessageBox.Show("No plans chosen. Please choose a plan."); // Don't close GUI and allow user to choose again.
            }
        }

        // When Save as CSV button is pressed, save results.
        public void SaveAsCSV()
        {
            // Launch file selection box
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = CallingDataModel.WORKBOOK_RESULT_DIR,
                Title = "Save Results as CSV",
                DefaultExt = "csv",
                Filter = "CSV files (*.csv)|*.csv",
                FilterIndex = 0
            };

            // Set property if user didn't cancel.
            if (saveFileDialog.ShowDialog() != false)
            {
                CallingDataModel.CSVSaveLocation = saveFileDialog.FileName;
            }

            RunReport();
        }

        // When Change EBRT file is pressed, allow user to select new file.
        public void ChangeEBRT_btn_Click()
        {
            // Find the csv file.
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = "csv",
                //InitialDirectory = Path.GetDirectoryName(CallingViewModel.CSVFileName),
                Multiselect = false,
                Title = "Please choose new EBRT file",
                ShowReadOnly = true,
                Filter = "CSV files (*.csv)|*.csv",
                FilterIndex = 0,
                RestoreDirectory = true,
                CheckFileExists = true
            };
            if (fileDialog.ShowDialog() == false)
            {
                return;    // user canceled
            }

            CallingDataModel.CSVFileName = fileDialog.FileName;
            CallingDataModel.CSVFileIsNotDefault = true;
            CSVFileTextBox = Path.GetFileName(CallingDataModel.CSVFileName);
            CallingDataModel.CreateObjectives();
            this.OnPropertyChanged("CSVFileTextBox");
        }

        // Interactions for select/unselect all checkbox.
        public bool SelectAllCheckBox
        {
            get { return _selectAllCheckBox; }
            set
            {
                if (value == _selectAllCheckBox)
                {
                    return;
                }

                _selectAllCheckBox = value;
                if (_selectAllCheckBox)
                {
                    _courses.ToList().ForEach(x => x.IsChecked = true);
                }
                else
                {
                    _courses.ToList().ForEach(x => x.IsChecked = false);
                }

                this.OnPropertyChanged("IsChecked");
            }
        }

        public ReadOnlyCollection<CourseForTreeViewModel> CoursesList
        {
            get { return _courses; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        #endregion // INotifyPropertyChanged Members
    }
}
