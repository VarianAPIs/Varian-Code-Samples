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

using System.Xml;

namespace PluginTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {
        /// <summary>
        /// This method is where the plugin gets started. Please modify it for your especific.
        /// This example assumes your scripts needs Patient, opened Course, list of PlanningItems in scope, opened
        /// PlanSetup, CurrentUser and the WPF window.
        public void StartPlugin(Patient patient, List<Course> courses, List<PlanningItem> plansInScope, PlanningItem pItem, User currentUser, Window window)
        {
            PluginScriptExample.Main.Start(patient, courses[0], plansInScope, pItem as PlanSetup, currentUser, window);
        }

        //The implementation of MainWindow follows. Feel free to take a look if you want some examples of WPF code behind.
        const string recentFileName = "PluginTesterRecent.xml";

        private ViewModel _viewModel;
        private bool _firstTime;

        private VMS.TPS.Common.Model.API.Application _application;

        public class MyScript : BindableBase
        {
            public string Name { get; set; }
            public string Description { get; set; }
            private bool _isChecked;
            public bool IsChecked
            {
                get { return _isChecked; }
                set
                {
                    _isChecked = value;
                    NotifyPropertyChanged("IsChecked");
                }
            }
        }

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

        public class RecentPatient : BindableBase
        {
            public string PatientId { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string OpenPlanId { get; set; }
            public string AllCourses
            {
                get
                {
                    return String.Join(", ", Courses.ToArray());
                }
            }

            public string AllPlansInScope
            {
                get
                {
                    return String.Join(", ", PlansInScope.ToArray());
                }
            }

            public ObservableCollection<string> PlansInScope { get; private set; }
            public ObservableCollection<string> Courses { get; private set; }
            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }

            public RecentPatient()
            {
                PlansInScope = new ObservableCollection<string>();
                Courses = new ObservableCollection<string>();
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
            private ObservableCollection<MyScript> _scripts = new ObservableCollection<MyScript>();

            public ObservableCollection<MyScript> Scripts
            {
                get
                {
                    return _scripts;
                }
            }

            private ObservableCollection<RecentPatient> _recentPatients = new ObservableCollection<RecentPatient>();
            public ObservableCollection<RecentPatient> RecentPatients
            {
                get
                {
                    return _recentPatients;
                }
            }

            private ObservableCollection<MyPatient> _patients = new ObservableCollection<MyPatient>();

            public ObservableCollection<MyPatient> Patients
            {
                get
                {
                    return _patients;
                }

            }

            private string _filter;
            public string Filter
            {
                get { return _filter; }
                set
                {
                    if (value != _filter)
                    {
                        _filter = value;
                        FilteredPatients.Refresh();
                        NotifyPropertyChanged("Filter");
                    }
                }
            }

            public ICollectionView FilteredPatients;

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

            private RecentPatient _selectedRecentPatient = null;
            public RecentPatient SelectedRecentPatient
            {
                get { return _selectedRecentPatient; }
                set
                {
                    _selectedRecentPatient = value;
                    NotifyPropertyChanged("SelectedRecentPatient");
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
                    ObservableCollection<MyPlanItem> plans = new ObservableCollection<MyPlanItem>();
                    foreach (var course in SelectedPatient.Courses.Where(s => s.IsSelected))
                    {
                        foreach (var plan in course.PlanItems)
                            plans.Add(plan);
                    }

                    return plans;
                }
            }

            private ObservableCollection<MyPlanItem> _plansInScope = new ObservableCollection<MyPlanItem>();
            public ObservableCollection<MyPlanItem> PlansInScope { get { return _plansInScope; } }

            public ViewModel()
            {
                FilteredPatients = CollectionViewSource.GetDefaultView(Patients);
                FilteredPatients.Filter = o => String.IsNullOrEmpty(Filter) ? true : ((MyPatient)o).LastName.StartsWith(Filter, true, null);
            }
        }


        public MainWindow(VMS.TPS.Common.Model.API.Application Application)
        {
            _application = Application;

            //  _application.CurrentUser = _application.CurrentUser;

            _viewModel = new ViewModel();

            InitializeComponent();

            this.DataContext = _viewModel;
            //lstPatients.SelectedIndex = 1;
            //lstPlans.SelectedIndex=0;

            LoadRecentPatients();

            //Show list of scripts if running in ALLSCRIPTS mode
            itmScripts.Visibility = Visibility.Collapsed;
#if ALLSCRIPTS
            itmScripts.Visibility = Visibility.Visible;
            ScriptsConfiguration config = MyConfig.GetScriptsSection();
            foreach (ScriptElement script in config.Scripts)
            {
                _viewModel.Scripts.Add(new MyScript
                {
                    Name = script.Name,
                    Description = script.Description,
                    IsChecked = false
                });
            }
            _viewModel.Scripts[0].IsChecked = true;
#endif
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            MyPatient myPatient = _viewModel.SelectedPatient;
            string patientId = myPatient.PatientId;

            PatientSummary patientSummary = _application.PatientSummaries.FirstOrDefault(s => s.Id == patientId);
            if (patientSummary == null) return;

            Patient patient = _application.OpenPatient(patientSummary);
            List<Course> courses = new List<Course>();
            foreach (var vmCourse in _viewModel.SelectedPatient.Courses.Where(s => s.IsSelected))
            {
                Course course = patient.Courses.FirstOrDefault(s => s.Id == vmCourse.CourseId);
                if (course != null)
                    courses.Add(course);
            }
            if (courses.Count == 0)
            {
                MessageBox.Show("Can't find selected courses in Aria");
                return;
            }

            //Plan items in scope
            List<PlanningItem> PlansInScope = new List<PlanningItem>();
            PlanningItem pItem = null;
            foreach (var pitem in _viewModel.CoursePlanItems.Where(s => s.IsInScope))
            {
                //check if planSetup
                foreach (var course in courses)
                {
                    pItem = course.PlanSetups.FirstOrDefault(s => s.Id == pitem.pItemId);
                    if (pItem == null)
                        pItem = course.PlanSums.FirstOrDefault(s => s.Id == pitem.pItemId);

                    if (pItem != null)
                    {
                        PlansInScope.Add(pItem);
                        break;
                    }
                }
                if (pItem == null)
                {
                    MessageBox.Show("No Planning Item found with Id = " + pitem.pItemId, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
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
            foreach (var course in courses)
            {
                pItem = course.PlanSetups.FirstOrDefault(s => s.Id == openPlanId);
                if (pItem == null)
                    pItem = course.PlanSums.FirstOrDefault(s => s.Id == openPlanId);
                if (pItem != null)
                    break;
            }
            if (pItem == null)
            {
                MessageBox.Show("No Planning Item found with Id = " + openPlanId, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            Window window = new Window();
            window.Closing += WindowClosingHandler;
            window.Show();

            //Save selections for use in next run
            SaveToRecent();
            try
            {
                StartPlugin(patient, courses, PlansInScope, pItem, _application.CurrentUser, window);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Caught exception from Script\n" + ex.Message + "\n" + ex.StackTrace);
                return;
            }
        }


        public void WindowClosingHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _application.ClosePatient();
            Window.GetWindow(this).Close();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            RadioButton button = (RadioButton)sender;

            string courseId = button.Content.ToString();

            _viewModel.SelectedCourse = _viewModel.SelectedPatient.Courses.FirstOrDefault(s => s.CourseId == courseId);
            LoadPlans(_viewModel.SelectedCourse);
            Cursor = Cursors.Arrow;
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
            Cursor = Cursors.Wait;
            if (_viewModel.SelectedPatient == null)
                return;

            if (_viewModel.SelectedPatient.Courses.Count == 0)
            {
                //Load courses and plans into viewmodel
                LoadCourses();
            }

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
            Cursor = Cursors.Arrow;
        }

        private void LoadCourses()
        {
            Patient patient = _application.OpenPatientById(_viewModel.SelectedPatient.PatientId);
            foreach (Course course in patient.Courses)
            {
                MyCourse vmCourse = new MyCourse();
                vmCourse.CourseId = course.Id;
                _viewModel.SelectedPatient.Courses.Add(vmCourse);
            }
            _application.ClosePatient();
        }

        private void LoadPlans(MyCourse vmCourse)
        {
            Patient patient = _application.OpenPatientById(_viewModel.SelectedPatient.PatientId);
            Course course = patient.Courses.SingleOrDefault(s => s.Id == vmCourse.CourseId);
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

            _application.ClosePatient();
        }



        private void OpenPatient_Checked(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = true;
        }


        private void SaveToRecent()
        {
            string filename = "PluginTesterRecent.xml";

            XmlDocument xDoc = new XmlDocument();
            XmlElement xRoot;
            if (File.Exists(filename))
            {
                xDoc.Load(filename);
                xRoot = xDoc.DocumentElement;
            }
            else
            {
                xRoot = xDoc.CreateElement("RecentCases");
                xDoc.AppendChild(xRoot);
            }

            bool addNew = true;
            foreach (XmlElement node in xRoot.ChildNodes)
            {
                string patId = null;
                List<string> courseIds = new List<string>();
                List<string> planScopeIds = new List<string>();
                string openPlan = null;
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    if (node2.Name == "Id")
                        patId = node2.InnerText;
                    else if (node2.Name == "Courses")
                    {
                        foreach (XmlNode node3 in node2.ChildNodes)
                            courseIds.Add(node3.InnerText);
                    }
                    else if (node2.Name == "PlansInScope")
                    {
                        foreach (XmlNode node3 in node2.ChildNodes)
                            planScopeIds.Add(node3.InnerText);
                    }
                    else if (node2.Name == "OpenedPlan")
                        openPlan = node2.InnerText;

                }
                if (patId == _viewModel.SelectedPatient.PatientId)
                {
                    //compare the courses
                    List<string> openedCourses = new List<string>();
                    foreach (var openCourse in _viewModel.SelectedPatient.Courses.Where(s => s.IsSelected))
                        openedCourses.Add(openCourse.CourseId);
                    List<string> FirstNotSecond = courseIds.Except(openedCourses).ToList();
                    List<string> SecondNotFirst = openedCourses.Except(courseIds).ToList();
                    if (FirstNotSecond.Count == 0 && SecondNotFirst.Count == 0)
                    {
                        //compare the plans in scope
                        List<string> pInScope = new List<string>();
                        foreach (var pscope in _viewModel.CoursePlanItems.Where(s => s.IsInScope))
                            pInScope.Add(pscope.pItemId);
                        FirstNotSecond = planScopeIds.Except(pInScope).ToList();
                        SecondNotFirst = pInScope.Except(planScopeIds).ToList();
                        if (FirstNotSecond.Count == 0 && SecondNotFirst.Count == 0)
                        {
                            if (openPlan == _viewModel.CoursePlanItems.SingleOrDefault(s => s.IsOpened).pItemId)
                            {
                                addNew = false;
                                break;
                            }
                        }
                    }
                }
            }

            if (addNew)
            {
                XmlElement xNew = xDoc.CreateElement("Case");
                //Patient Id
                XmlElement xNodeId = xDoc.CreateElement("Id");
                XmlText xTxtId = xDoc.CreateTextNode(_viewModel.SelectedPatient.PatientId);
                xNodeId.AppendChild(xTxtId);
                xNew.AppendChild(xNodeId);
                //Last Name
                XmlElement xNodeLN = xDoc.CreateElement("LastName");
                XmlText xTxtLN = xDoc.CreateTextNode(_viewModel.SelectedPatient.LastName);
                xNodeLN.AppendChild(xTxtLN);
                xNew.AppendChild(xNodeLN);
                //First Name
                XmlElement xNodeFN = xDoc.CreateElement("FirstName");
                XmlText xTxtFN = xDoc.CreateTextNode(_viewModel.SelectedPatient.FirstName);
                xNodeFN.AppendChild(xTxtFN);
                xNew.AppendChild(xNodeFN);
                //Courses
                XmlElement xNodeCourses = xDoc.CreateElement("Courses");
                foreach (var course in _viewModel.SelectedPatient.Courses.Where(s => s.IsSelected))
                {
                    XmlElement xNodeCourse = xDoc.CreateElement("Course");
                    XmlText xTxtCourse = xDoc.CreateTextNode(course.CourseId);
                    xNodeCourse.AppendChild(xTxtCourse);
                    xNodeCourses.AppendChild(xNodeCourse);
                }
                xNew.AppendChild(xNodeCourses);
                //Plans in scope
                string openedPlanId = null;
                XmlElement xNodePlansScope = xDoc.CreateElement("PlansInScope");
                xNew.AppendChild(xNodePlansScope);
                foreach (var plan in _viewModel.CoursePlanItems.Where(s => s.IsInScope))
                {
                    XmlElement xNodeScope = xDoc.CreateElement("PlanId");
                    XmlText xTxtScope = xDoc.CreateTextNode(plan.pItemId);
                    xNodeScope.AppendChild(xTxtScope);
                    xNodePlansScope.AppendChild(xNodeScope);
                    if (plan.IsOpened)
                        openedPlanId = plan.pItemId;
                }

                //Opened plan
                if (openedPlanId != null)
                {
                    XmlElement xNodePlan = xDoc.CreateElement("OpenedPlan");
                    XmlText xTxtPlan = xDoc.CreateTextNode(openedPlanId);
                    xNodePlan.AppendChild(xTxtPlan);
                    xNew.AppendChild(xNodePlan);
                }

                xRoot.AppendChild(xNew);
            }
            xDoc.Save(filename);





            //using (StreamWriter sw = new StreamWriter(recentFileName, false))
            //{
            //    sw.WriteLine(_viewModel.SelectedPatient.PatientId);
            //    sw.WriteLine(_viewModel.SelectedCourse.CourseId);
            //    sw.WriteLine(_viewModel.PlansInScope.Count);
            //    string openedPlanId = string.Empty;
            //    foreach (var plan in _viewModel.PlansInScope)
            //    {
            //        sw.WriteLine(plan.pItemId);
            //        if (plan.IsOpened)
            //            openedPlanId = plan.pItemId;
            //    }
            //    sw.WriteLine(openedPlanId);
            //}

        }

        private void LoadRecentPatients()
        {

            if (!File.Exists(recentFileName))
                return;

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(recentFileName);
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot == null) return;

            foreach (XmlNode node in xRoot.ChildNodes)
            {
                RecentPatient patient = new RecentPatient();
                patient.IsSelected = false;
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    if (node2.Name == "Id")
                        patient.PatientId = node2.InnerText;
                    else if (node2.Name == "LastName")
                        patient.LastName = node2.InnerText;
                    else if (node2.Name == "FirstName")
                        patient.FirstName = node2.InnerText;
                    else if (node2.Name == "Courses")
                    {
                        foreach (XmlNode node3 in node2.ChildNodes)
                            patient.Courses.Add(node3.InnerText);
                    }
                    else if (node2.Name == "PlansInScope")
                        foreach (XmlNode node3 in node2.ChildNodes)
                            patient.PlansInScope.Add(node3.InnerText);
                    else if (node2.Name == "OpenedPlan")
                        patient.OpenPlanId = node2.InnerText;
                }
                _viewModel.RecentPatients.Add(patient);
            }

        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            _viewModel.Patients.Clear();

            if (_viewModel.Filter == null || _viewModel.Filter == "*")
                foreach (PatientSummary patientSummary in _application.PatientSummaries)
                {
                    MyPatient myPatient = new MyPatient();
                    myPatient.PatientId = patientSummary.Id;
                    myPatient.LastName = patientSummary.LastName;
                    myPatient.FirstName = patientSummary.FirstName;
                    _viewModel.Patients.Add(myPatient);

                }
            else
                foreach (PatientSummary patientSummary in _application.PatientSummaries.Where(s => s.LastName.StartsWith(_viewModel.Filter, true, null)))
                {
                    MyPatient myPatient = new MyPatient();
                    myPatient.PatientId = patientSummary.Id;
                    myPatient.LastName = patientSummary.LastName;
                    myPatient.FirstName = patientSummary.FirstName;
                    _viewModel.Patients.Add(myPatient);

                }

            Cursor = Cursors.Arrow;
        }


        private void chkCourses_Checked(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            CheckBox check = (CheckBox)sender;

            string courseId = check.Content.ToString();

            MyCourse course = _viewModel.SelectedPatient.Courses.FirstOrDefault(s => s.CourseId == courseId);
            if (course != null && course.PlanItems.Count == 0)
                LoadPlans(course);

            _viewModel.NotifyPropertyChanged("CoursePlanItems");
            Cursor = Cursors.Arrow;
        }

        private void chkCourses_Unchecked(object sender, RoutedEventArgs e)
        {
            _viewModel.NotifyPropertyChanged("CoursePlanItems");
        }

        private void btnAllPlansScoped_Click(object sender, RoutedEventArgs e)
        {
            foreach (var plan in _viewModel.CoursePlanItems)
                plan.IsInScope = true;
        }

        private void btnDelFromHistory_Click(object sender, RoutedEventArgs e)
        {
            RecentPatient recPatient = ((FrameworkElement)sender).DataContext as RecentPatient;

            if (!File.Exists(recentFileName))
                return;

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(recentFileName);
            XmlElement xRoot = xDoc.DocumentElement;
            if (xRoot == null) return;

            foreach (XmlNode node in xRoot.ChildNodes)
            {
                string patientId = null;
                string lastName = null;
                string firstName = null;
                List<string> courses = new List<string>();
                List<string> plansInScope = new List<string>();
                string openPlanId = null;
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    if (node2.Name == "Id")
                        patientId = node2.InnerText;
                    else if (node2.Name == "LastName")
                        lastName = node2.InnerText;
                    else if (node2.Name == "FirstName")
                        firstName = node2.InnerText;
                    else if (node2.Name == "Courses")
                    {
                        foreach (XmlNode node3 in node2.ChildNodes)
                            courses.Add(node3.InnerText);
                    }
                    else if (node2.Name == "PlansInScope")
                        foreach (XmlNode node3 in node2.ChildNodes)
                            plansInScope.Add(node3.InnerText);
                    else if (node2.Name == "OpenedPlan")
                        openPlanId = node2.InnerText;
                }

                if (patientId != recPatient.PatientId ||
                    lastName != recPatient.LastName ||
                    firstName != recPatient.FirstName ||
                    !courses.SequenceEqual(recPatient.Courses) ||
                    !plansInScope.SequenceEqual(recPatient.PlansInScope) ||
                    openPlanId != recPatient.OpenPlanId)
                {
                    continue;
                }

                xRoot.RemoveChild(node);
                break;
            }
            xDoc.Save(recentFileName);
            _viewModel.RecentPatients.Clear();
            LoadRecentPatients();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            RecentPatient recPatient = ((FrameworkElement)sender).DataContext as RecentPatient;
            string patientId = recPatient.PatientId;

            Patient patient = _application.OpenPatientById(patientId);
            if (patient == null)
            {
                MessageBox.Show("Can't find the selected patient in Aria");
                Cursor = Cursors.Arrow;
                return;
            }

            List<Course> courses = new List<Course>();
            foreach (string courseId in recPatient.Courses)
            {
                Course course = patient.Courses.FirstOrDefault(s => s.Id == courseId);
                if (course != null)
                    courses.Add(course);
            }
            if (courses.Count == 0)
            {
                MessageBox.Show("Can't find selected courses in Aria");
                Cursor = Cursors.Arrow;
                return;
            }

            //Plan items in scope
            List<PlanningItem> PlansInScope = new List<PlanningItem>();
            PlanningItem pItem = null;
            foreach (string pitemId in recPatient.PlansInScope)
            {
                //check if planSetup
                foreach (var course in courses)
                {
                    pItem = course.PlanSetups.FirstOrDefault(s => s.Id == pitemId);
                    if (pItem == null)
                        pItem = course.PlanSums.FirstOrDefault(s => s.Id == pitemId);

                    if (pItem != null)
                    {
                        PlansInScope.Add(pItem);
                        break;
                    }
                }
                if (pItem == null)
                {
                    MessageBox.Show("No Planning Item found with Id = " + pitemId, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Cursor = Cursors.Arrow;
                    return;
                }
            }

            //Opened plan Item
            string openPlanId = recPatient.OpenPlanId;

            //check if planSetup 
            foreach (var course in courses)
            {
                pItem = course.PlanSetups.FirstOrDefault(s => s.Id == openPlanId);
                if (pItem == null)
                    pItem = course.PlanSums.FirstOrDefault(s => s.Id == openPlanId);
                if (pItem != null)
                    break;
            }
            if (pItem == null)
            {
                MessageBox.Show("No Planning Item found with Id = " + openPlanId, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Cursor = Cursors.Arrow;
                return;
            }


            Window window = new Window();
            window.Closing += WindowClosingHandler;
            window.Show();

            Cursor = Cursors.Arrow;
            try
            {
                StartPlugin(patient, courses, PlansInScope, pItem, _application.CurrentUser, window);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Caught exception from Script\n" + ex.Message + "\n" + ex.StackTrace);
                return;
            }
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
