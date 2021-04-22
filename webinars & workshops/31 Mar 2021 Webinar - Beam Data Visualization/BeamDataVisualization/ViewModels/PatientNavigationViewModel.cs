using BeamDataVisualization.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace BeamDataVisualization.ViewModels
{
    public class PatientNavigationViewModel:BindableBase
    {
        private string _patientId;

        public string PatientId
        {
            get { return _patientId; }
            set { SetProperty(ref _patientId, value);OpenPatientCommand.RaiseCanExecuteChanged(); }
        }


        private Application _app;
        private IEventAggregator _eventAggregator;
        public ObservableCollection<Course> Courses { get; private set; }
        public ObservableCollection<PlanSetup> Plans { get; private set; }
        private PlanSetup _selectedPlan;

        public PlanSetup SelectedPlan
        {
            get { return _selectedPlan; }
            set
            {
                SetProperty(ref _selectedPlan, value);
                if (SelectedPlan != null)
                {
                    //draw scans.
                    _eventAggregator.GetEvent<PlanSelectedEvent>().Publish(SelectedPlan);
                }
            }
        }

        private Course _selectedCourse;
        private Patient _patient;

        public Course SelectedCourse
        {
            get { return _selectedCourse; }
            set
            {
                SetProperty(ref _selectedCourse, value);
                if (SelectedCourse != null)
                {
                    SetPlans();
                }
            }
        }

  
        public DelegateCommand OpenPatientCommand { get; private set; }
        public PatientNavigationViewModel(Application app, string patientId, string courseId, string planId,IEventAggregator eventAggregator)
        {
            _app = app;
            _eventAggregator = eventAggregator;
            Courses = new ObservableCollection<Course>();
            Plans = new ObservableCollection<PlanSetup>();
            if (!String.IsNullOrEmpty(patientId))
            {
                PatientId = patientId;
                OnOpenPatient();
            }
            if (!String.IsNullOrEmpty(courseId) && _patient != null)
            {
                SelectedCourse = _patient.Courses.FirstOrDefault(x => x.Id == courseId);
            }
            if (!String.IsNullOrEmpty(planId) && _patient != null && SelectedCourse != null)
            {
                SelectedPlan = SelectedCourse.PlanSetups.FirstOrDefault(x => x.Id == planId);
            }
            OpenPatientCommand = new DelegateCommand(OnOpenPatient, CanOpenPatient);
        }

        private void OnOpenPatient()
        {
            _app.ClosePatient();
            _patient = _app.OpenPatientById(PatientId);
            if (_patient != null)
            {
                SetCourses();
            }
        }

        private void SetCourses()
        {
            SelectedCourse = null;
            Courses.Clear();
            foreach (var course in _patient.Courses)
            {
                Courses.Add(course);
            }
        }

        private bool CanOpenPatient()
        {
            return !String.IsNullOrEmpty(PatientId);
        }

        private void SetPlans()
        {
            SelectedPlan = null;
            Plans.Clear();
            foreach (var plan in SelectedCourse.PlanSetups)
            {
                Plans.Add(plan);
            }
        }
    }
}
