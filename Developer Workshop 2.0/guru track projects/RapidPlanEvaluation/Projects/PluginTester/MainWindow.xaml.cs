#region copyright
////////////////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Regents of the University of Michigan
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;

namespace PluginTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {
        private ViewModel _viewModel;
        private bool _firstTime;

        private VMS.TPS.Common.Model.API.Application _application;

        public class MyPatient : BindableBase
        {
            public string PatientId { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public ObservableCollection<MyCourse> Courses { get; private set; }

            public MyPatient()
            {
                Courses = new ObservableCollection<MyCourse>();
            }
        }

        public class MyCourse : BindableBase
        {
            public string CourseId { get; set; }
            public ObservableCollection<MyPlanItem> PlanItems { get; private set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    _isSelected = value;
                    NotifyPropertyChanged("CoursePlanItems");
                }
            }

            public MyCourse()
            {
                PlanItems = new ObservableCollection<MyPlanItem>();
            }
        }

        public class MyPlanItem : BindableBase
        {
            public string pItemId { get; set; }

            private bool _isInScope;
            public bool IsInScope
            {
                get { return _isInScope; }
                set
                {
                    _isInScope = value;
                    NotifyPropertyChanged();
                }
            }

            private bool _isSum;
            public bool IsPlanSum
            {
                get { return _isSum; }
                set
                {
                    _isSum = value;
                    NotifyPropertyChanged();
                }
            }

            private bool _isOpened;
            public bool IsOpened
            {
                get { return _isOpened; }
                set
                {
                    _isOpened = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public class ViewModel : BindableBase
        {


            private ObservableCollection<MyPatient> _patients = new ObservableCollection<MyPatient>();

            public ObservableCollection<MyPatient> Patients
            {
                get
                {
                    return _patients;
                }

            }

            private MyPatient _selectedPatient = null;
            public MyPatient SelectedPatient
            {
                get { return _selectedPatient; }
                set
                {
                    _selectedPatient = value;
                    NotifyPropertyChanged("PatientCourses");
                }
            }

            private MyCourse _selectedCourse = null;
            public MyCourse SelectedCourse
            {
                get { return _selectedCourse; }
                set
                {
                    _selectedCourse = value;
                    NotifyPropertyChanged("CoursePlanItems");
                    PlansInScope.Clear();
                }
            }

            public ObservableCollection<MyCourse> PatientCourses
            {
                get
                {
                    if (SelectedPatient == null)
                        return null;

                    return SelectedPatient.Courses;
                }
            }

            public ObservableCollection<MyPlanItem> CoursePlanItems
            {
                get
                {
                    if (SelectedPatient == null)
                        return null;

                    if (SelectedCourse == null)
                        return null;

                    return SelectedCourse.PlanItems;
                }
            }

            private ObservableCollection<MyPlanItem> _plansInScope = new ObservableCollection<MyPlanItem>();
            public ObservableCollection<MyPlanItem> PlansInScope { get { return _plansInScope; } }
        }

        public MainWindow(VMS.TPS.Common.Model.API.Application Application)
        {
            _application = Application;

            _viewModel = new ViewModel();

            InitializeComponent();

            this.DataContext = _viewModel;
            //lstPatients.SelectedIndex = 1;
            //lstPlans.SelectedIndex=0;

            foreach (PatientSummary patientSummary in _application.PatientSummaries)
            {
                MyPatient myPatient = new MyPatient();
                myPatient.PatientId = patientSummary.Id;
                myPatient.LastName = patientSummary.LastName;
                myPatient.FirstName = patientSummary.FirstName;

                Patient patient = _application.OpenPatient(patientSummary);
                foreach (Course course in patient.Courses)
                {
                    MyCourse vmCourse = new MyCourse();
                    vmCourse.CourseId = course.Id;
                    foreach (PlanSetup planSetup in course.PlanSetups)
                        vmCourse.PlanItems.Add(new MyPlanItem
                        {
                            pItemId = planSetup.Id,
                            IsInScope = false,
                            IsPlanSum = false
                        });
                    foreach (PlanSum planSum in course.PlanSums)
                        vmCourse.PlanItems.Add(new MyPlanItem
                        {
                            pItemId = planSum.Id,
                            IsInScope = false,
                            IsPlanSum = true
                        });

                    myPatient.Courses.Add(vmCourse);

                }
                _application.ClosePatient();

                _viewModel.Patients.Add(myPatient);

            }

            OpenSelections();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            MyPatient myPatient = _viewModel.SelectedPatient;
            string patientId = myPatient.PatientId;

            PatientSummary patientSummary = _application.PatientSummaries.FirstOrDefault(s => s.Id == patientId);
            if (patientSummary == null) return;

            Patient patient = _application.OpenPatient(patientSummary);
            Course course = patient.Courses.FirstOrDefault(s => s.Id == _viewModel.SelectedCourse.CourseId);
            if (course == null)
            {
                MessageBox.Show("Can't find selected course in Aria");
                return;
            }

            //planId items in scope
            List<PlanSetup> PlansInScope = new List<PlanSetup>();
            foreach (var pitem in _viewModel.PlansInScope)
            {
                //check if planSetup
                PlanSetup plan = (PlanSetup)course.PlanSetups.FirstOrDefault(s => s.Id == pitem.pItemId);
                if (plan != null)
                    PlansInScope.Add(plan);
            }

            //Opened plan Item
            string openPlanId = string.Empty;
            foreach (var pitem in _viewModel.PlansInScope)
            {
                if (pitem.IsOpened)
                {
                    openPlanId = pitem.pItemId;
                    break;
                }
            }

            //check if planSetup 
            PlanSetup openedPlan = course.PlanSetups.FirstOrDefault(s => s.Id == openPlanId);
            if (openedPlan == null)
            {
                MessageBox.Show("No PlanSetup found with Id = " + openPlanId, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }



            Window window = new Window();
            window.Closing += WindowClosingHandler;
            window.Show();

            //Save selections for use in next run
            SaveSelections();
            try
            {

                VMS.TPS.Script.Start(patient, course, PlansInScope, openedPlan, _application.CurrentUser, window);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caught exception from Script\n" + ex.Message + "\n" + ex.StackTrace);
                return;
            }
        }

        private void WindowClosingHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Window.GetWindow(this).Close();
        }


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton button = (RadioButton)sender;

            string courseId = button.Content.ToString();

            _viewModel.SelectedCourse = _viewModel.SelectedPatient.Courses.FirstOrDefault(s => s.CourseId == courseId);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;

            StackPanel panel = (StackPanel)check.Content;

            Label label = (Label)panel.Children[0];

            string planid = label.Content.ToString();

            _viewModel.PlansInScope.Add(_viewModel.CoursePlanItems.FirstOrDefault(s => s.pItemId == planid));
        }


        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox check = (CheckBox)sender;

            StackPanel panel = (StackPanel)check.Content;

            Label label = (Label)panel.Children[0];

            string planid = label.Content.ToString();

            _viewModel.PlansInScope.Remove(_viewModel.CoursePlanItems.FirstOrDefault(s => s.pItemId == planid));
        }

        private void dtgPatients_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (!_firstTime)
            {
                if (_viewModel.SelectedCourse != null)
                {
                    _viewModel.SelectedCourse.IsSelected = false;
                    foreach (var plan in _viewModel.SelectedCourse.PlanItems)
                    {
                        plan.IsInScope = false;
                        plan.IsOpened = false;
                    }
                    _viewModel.SelectedCourse = null;
                }

                _viewModel.PlansInScope.Clear();
                btnStart.IsEnabled = false;
            }
            _firstTime = false;
        }

        private void OpenPatient_Checked(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = true;
        }


        private void SaveSelections()
        {
            string filename = "PluginTester.start";

            using (StreamWriter sw = new StreamWriter(filename, false))
            {
                sw.WriteLine(_viewModel.SelectedPatient.PatientId);
                sw.WriteLine(_viewModel.SelectedCourse.CourseId);
                sw.WriteLine(_viewModel.PlansInScope.Count);
                string openedPlanId = string.Empty;
                foreach (var plan in _viewModel.PlansInScope)
                {
                    sw.WriteLine(plan.pItemId);
                    if (plan.IsOpened)
                        openedPlanId = plan.pItemId;
                }
                sw.WriteLine(openedPlanId);
            }

        }

        private void OpenSelections()
        {
            string filename = "PluginTester.start";

            if (!File.Exists(filename))
                return;


            using (StreamReader sr = new StreamReader(filename))
            {
                string selPatiId = sr.ReadLine();
                string selCourseID = sr.ReadLine();
                int numPlans = Convert.ToInt32(sr.ReadLine());
                _viewModel.SelectedPatient = _viewModel.Patients.FirstOrDefault(s => s.PatientId == selPatiId);
                if (_viewModel.SelectedPatient == null)
                    return;


                MyCourse selCourse = _viewModel.SelectedPatient.Courses.FirstOrDefault(s => s.CourseId == selCourseID);
                if (selCourse == null)
                    return;

                selCourse.IsSelected = true;
                _viewModel.SelectedCourse = selCourse;

                for (int i = 0; i < numPlans; i++)
                {
                    string planId = sr.ReadLine();
                    MyPlanItem pItem = _viewModel.SelectedCourse.PlanItems.FirstOrDefault(s => s.pItemId == planId);
                    if (pItem != null)
                    {
                        pItem.IsInScope = true;
                        _viewModel.PlansInScope.Add(pItem);
                    }
                }

                string openPlanId = sr.ReadLine();
                foreach (var plan in _viewModel.PlansInScope)
                    if (plan.pItemId == openPlanId)
                    {
                        plan.IsOpened = true;
                        break;
                    }

            }

            _firstTime = true;
            btnStart.IsEnabled = true;

        }

    }


    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
