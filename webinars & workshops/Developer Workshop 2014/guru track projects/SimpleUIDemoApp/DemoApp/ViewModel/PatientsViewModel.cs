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

namespace DemoApp.ViewModel
{
    /// <summary>
    /// Represents a container of MPatientViewModel objects
    /// that has support for staying synchronized with the
    /// PatientRepository.
    /// </summary>
    public class PatientsViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly PatientRepository _patientRepository;

        readonly int _filterlevel;

        string _selecedmonth;

        #endregion // Fields

        #region Constructor

        public PatientsViewModel(VMS.TPS.Common.Model.API.Application vapp, PatientRepository patientRepository, int filterlevel)
        {
            if (patientRepository == null)
                throw new ArgumentNullException("paientRepository");

            base.DisplayName = Strings.PatientsViewModel_DisplayName;

            _patientRepository = patientRepository;

            _filterlevel = filterlevel;

            // Populate the AllPatients collection with MPatientViewModel.
            this.CreatePatients();
        }

        void CreatePatients()
        {
            List<MPatientViewModel> all =
                (from cust in _patientRepository.GetAllPatients()
                 select new MPatientViewModel(cust, _patientRepository)).ToList();

            foreach (MPatientViewModel cvm in all)
                cvm.PropertyChanged += this.OnMPatientViewModelPropertyChanged;

            this.AllPatients = new ObservableCollection<MPatientViewModel>(all);

            this.MonthChartSeries = new ObservableCollection<KeyValuePair<string, int>>(_patientRepository.GetChartSeries());

            this.Months= new ObservableCollection<string> (_patientRepository.GetMonths());

            ChartTitle = "Patients in last " + Months[Months.Count - 1] + " months";

            SelectedMonth = Months[0];

            this.AllPatients.CollectionChanged += this.OnCollectionChanged;
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the MPatientViewModel objects.
        /// </summary>
        public ObservableCollection<MPatientViewModel> AllPatients { get; private set; }

        /// <summary>
        /// Returns a collection of CharSeriess of month histogram
        /// </summary>
        public ObservableCollection<KeyValuePair<string, int>> MonthChartSeries { get; private set; }
        public ObservableCollection<string> Months { get; set; }
        public string SelectedMonth
        {
            get
            {
                return _selecedmonth;
            }
            set
            {
                _selecedmonth = value;
                _patientRepository.ClearSelectedPatients(_filterlevel);
                _patientRepository.SuspendEvent = true;
                List<Model.MPatient> lstPats = _patientRepository.GetPatientsByMonth(_selecedmonth);
                foreach (Model.MPatient pat in lstPats)
                {
                    if (pat == lstPats[lstPats.Count - 1])
                        _patientRepository.SuspendEvent = false;
                    _patientRepository.AddSelectedPatient(_filterlevel, pat);
                }
                _patientRepository.SuspendEvent = false;
            }
        }
        public string ChartTitle { get; set; }
        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (MPatientViewModel custVM in this.AllPatients)
                custVM.Dispose();

            this.AllPatients.Clear();
            this.AllPatients.CollectionChanged -= this.OnCollectionChanged;

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

            // When a Patient is selected or unselected, we must let the
            // world know that the TotalSelectedSales property has changed,
            // so that it will be queried again for a new value.
            if (e.PropertyName == IsSelected)
                this.OnPropertyChanged("TotalSelectedSales");
        }


        #endregion // Event Handling Methods
    }
}