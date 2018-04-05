using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;
using V = VMS.TPS.Common.Model.API;

namespace Cardan.ESAPI.Bootstrap.Views
{
    /// <summary>
    /// Interaction logic for SelectPatient.xaml
    /// </summary>
    public partial class SelectPatient : Page, INotifyPropertyChanged
    {
        public SelectPatient()
        {
            InitializeComponent();
            this.DataContext = this;
            this.patientContextMenu.Visibility = System.Windows.Visibility.Visible;
            this.hideContextButton.Visibility = System.Windows.Visibility.Visible;
            this.showContextButton.Visibility = System.Windows.Visibility.Collapsed;
            //Button_Click(null, null);
            Courses = new ObservableCollection<Course>();
        }

        public string PatientId { get; set; }
        public string Status { get; set; }
        public ObservableCollection<Course> Courses { get; set; }
        private Course _selCourse;
        public Course SelectedCourse
        {
            get { return _selCourse; }
            set
            {
                _selCourse = value;
                var ctx = ESAPIApplication.Instance.Context;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var scheduler = new StaTaskScheduler(1);
            UpdateStatus("Searching...");
            Task.Factory.StartNew(() =>
            {
                Dispatcher.BeginInvoke(new Action(UpdatePatient));
            }, CancellationToken.None, TaskCreationOptions.None, scheduler);
        }

        public void UpdatePatient()
        {
            var esapi = ESAPIApplication.Instance.Context;
            var sc = ESAPIApplication.Instance.ScriptContext;
          
            var patients = esapi.PatientSummaries.ToList();
            var found = esapi.PatientSummaries.FirstOrDefault(p => p.Id == PatientId);
            if (found != null)
            {
                var patient = esapi.OpenPatientById(PatientId);
                typeof(V.ScriptContext)
                    .GetField("m_patient", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(sc, patient);

                UpdateStatus(string.Format("Current Context is {0}, {1} | {2}", found.LastName, found.FirstName, found.Id));
                Courses.Clear();
                
                var courses = patient.Courses;
                courses.ToList().ForEach(c => Courses.Add(c));
                SelectedCourse = Courses.FirstOrDefault();
                OnPropertyChanged("Courses");
                OnPropertyChanged("SelectedCourse");
                OnPropertyChanged("Status");
            }
            else
            {
                MessageBox.Show(string.Format("Can't find patient {0}.", PatientId), "ERROR");
                UpdateStatus("");
            }
        }

        private void UpdateStatus(string p)
        {
            Status = p;
            OnPropertyChanged("Status");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ShowPatientContext_Click(object sender, RoutedEventArgs e)
        {
            this.patientContextMenu.Visibility = System.Windows.Visibility.Visible;
            this.hideContextButton.Visibility = System.Windows.Visibility.Visible;
            this.showContextButton.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void HidePatientContext_Click(object sender, RoutedEventArgs e)
        {
            this.patientContextMenu.Visibility = System.Windows.Visibility.Collapsed;
            this.hideContextButton.Visibility = System.Windows.Visibility.Collapsed;
            this.showContextButton.Visibility = System.Windows.Visibility.Visible;
        }
    }
}
