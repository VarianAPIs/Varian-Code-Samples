#region copyright
////////////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Varian Medical Systems, Inc.
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
//////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DemoApp.DataAccess;
using DemoApp.Properties;
using System.Diagnostics;
using System.Windows.Input;


namespace DemoApp.ViewModel
{
    /// <summary>
    /// Represents a container of MPatientViewModel objects
    /// that has support for staying synchronized with the
    /// PatientRepository.  This class also provides information
    /// related to multiple selected patients.
    /// </summary>
    public class SelectedPatientsViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly PatientRepository _patientRepository;
        readonly RelayCommand _showplanscommand;
        MPatientViewModel _selectedpatient;
        readonly VMS.TPS.Common.Model.API.Application _vapp;
        #endregion // Fields

        #region Constructor

        public SelectedPatientsViewModel(VMS.TPS.Common.Model.API.Application vapp, PatientRepository patientRepository)
        {
            if (patientRepository == null)
                throw new ArgumentNullException("patientRepository");

            base.DisplayName = Strings.SelectedPatientsViewModel_DisplayName;

            _vapp = vapp;

            _patientRepository = patientRepository;

            // Subscribe for notifications of when a new patient is added from the filter.
            _patientRepository.SelectedPatientAdded += this.OnSelectedPatientAddedToRepository;

            // Populate theSelectedPatients collection with MPatientViewModels.
            this.CreateSelectedPatients();
            _showplanscommand = new RelayCommand(param => this.ShowPlans());

        }

        void CreateSelectedPatients()
        {
            List<MPatientViewModel> all =
                (from cust in _patientRepository.GetSelectedPatients(_patientRepository.GetLastestFilterLevel()) 
                 select new MPatientViewModel(cust, _patientRepository)).ToList();

            foreach (MPatientViewModel cvm in all)
                cvm.PropertyChanged += this.OnMPatientViewModelPropertyChanged;

            this.SelectedPatients = new ObservableCollection<MPatientViewModel>(all);
            this.SelectedPatients.CollectionChanged += this.OnCollectionChanged;
        }

        #endregion // Constructor
        #region Commmand
        void ShowPlans()
        {
            if (_selectedpatient != null)
            {
                VMS.TPS.Common.Model.API.Patient pat = _vapp.OpenPatientById(_selectedpatient.MRN);
                List<VMS.TPS.Common.Model.API.PlanSetup> lstPlans = new List<VMS.TPS.Common.Model.API.PlanSetup>();
                foreach (VMS.TPS.Common.Model.API.Course c in pat.Courses)
                {
                    lstPlans.AddRange(c.PlanSetups);
                }
                List<VMS.TPS.Common.Model.API.Beam> lstBeams = new List<VMS.TPS.Common.Model.API.Beam>();
                lstBeams.AddRange(lstPlans[0].Beams);

                ESAPISimpleUI.View.ListBoxWindow win = new ESAPISimpleUI.View.ListBoxWindow("Test selection box", "Select a plan", System.Windows.Controls.SelectionMode.Single, ESAPISimpleUI.View.ViewType.DataGrid,
                    lstPlans);
                bool bOK = win.ShowDialog().Value;
                if (bOK)
                {
                   List < VMS.TPS.Common.Model.API.PlanSetup> lstSelectedPlans = win.SelectedItems.Cast<VMS.TPS.Common.Model.API.PlanSetup>().ToList();
                   if (lstSelectedPlans != null && lstSelectedPlans.Count > 0)
                   {
                       ESAPISimpleUI.View.ListBoxWindow win2 = new ESAPISimpleUI.View.ListBoxWindow("Test selection box", "Select beams",
                          System.Windows.Controls.SelectionMode.Multiple, ESAPISimpleUI.View.ViewType.DataGrid,
                          lstSelectedPlans[0].Beams);
                       bOK = win2.ShowDialog().Value;
                       if (bOK)
                       {
                           List<VMS.TPS.Common.Model.API.Beam> lstSelectedBeams = win2.SelectedItems.Cast<VMS.TPS.Common.Model.API.Beam>().ToList();
                       }
                   }
                }
               
               
                _vapp.ClosePatient();
            }
        }
        #endregion
        #region Public Interface

        /// <summary>
        /// Returns a collection of all the MPatientViewModel objects.
        /// </summary>
        public ObservableCollection<MPatientViewModel> SelectedPatients { get; private set; }


        public ICommand ShowPlansCommand
        {
            get
            {
                return _showplanscommand;
            }
        }
        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (MPatientViewModel custVM in this.SelectedPatients)
                custVM.Dispose();

            this.SelectedPatients.Clear();
            this.SelectedPatients.CollectionChanged -= this.OnCollectionChanged;

            _patientRepository.SelectedPatientAdded -= this.OnSelectedPatientAddedToRepository;
        }

        #endregion // Base Class Overrides

        #region Event Handling Methods

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (MPatientViewModel custVM in e.NewItems)
                    custVM.PropertyChanged += this.OnMPatientViewModelPropertyChanged;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (MPatientViewModel custVM in e.OldItems)
                    custVM.PropertyChanged -= this.OnMPatientViewModelPropertyChanged;
        }

        void OnMPatientViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string IsSelected = "IsSelected";

            // Make sure that the property name we're referencing is valid.
            // This is a debugging technique, and does not execute in a Release build.
            (sender as MPatientViewModel).VerifyPropertyName(IsSelected);
            if ((sender as MPatientViewModel).IsSelected)
                _selectedpatient = (sender as MPatientViewModel);
        }


        void OnSelectedPatientAddedToRepository(object sender, PatientAddedEventArgs e)
        {
            if (e.Level == _patientRepository.GetLastestFilterLevel())
            {
                this.SelectedPatients.Clear();
                this.SelectedPatients.CollectionChanged -= this.OnCollectionChanged;

                CreateSelectedPatients();
                //var viewModel = new MPatientViewModel(e.NewPatient, _patientRepository);
                //this.SelectedPatients.Add(viewModel);
            }
        }

        #endregion // Event Handling Methods
    }
}