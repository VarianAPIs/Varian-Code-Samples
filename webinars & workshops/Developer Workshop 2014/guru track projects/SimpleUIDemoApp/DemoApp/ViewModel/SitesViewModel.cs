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
using DemoApp.Model;
namespace DemoApp.ViewModel
{
    /// <summary>
    /// Represents a container of SitesViewModel objects
    /// that has support for staying synchronized with the
    /// SiteRepository.
    /// </summary>
    public class SitesViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly SiteRepository _siteRepository;
        readonly PatientRepository _patientRepository;
        readonly VMS.TPS.Common.Model.API.Application _vapp;
        readonly Dictionary<string, List<MPatient>> _dictpatients; // iddexed by sites
        readonly int _filterlevel;
        SiteViewModel _selectedSite;
        #endregion // Fields

        #region Constructor

        public SitesViewModel(VMS.TPS.Common.Model.API.Application vapp, PatientRepository patientRepository, SiteRepository siteRepository, int filterlevel)
        {
            if (patientRepository == null)
                throw new ArgumentNullException("paientRepository");
            if (siteRepository == null)
                throw new ArgumentNullException("siteRepository");
            base.DisplayName = Strings.SitesViewModel_DisplayName;

            _patientRepository = patientRepository;
            _siteRepository = siteRepository;
            _vapp = vapp;
            _filterlevel = filterlevel;

            _dictpatients = new Dictionary<string, List<MPatient>>();

            // Populate the AllSites collection with SiteViewModels.
            this.CreateSites();
        }

        void CreateSites()
        {
            List<SiteViewModel> all =
                (from cust in _siteRepository.GetSites()
                 select new SiteViewModel(cust, _siteRepository)).ToList();

            //foreach (MPatientViewModel cvm in all)
            //    cvm.PropertyChanged += this.OnMPatientViewModelPropertyChanged;

            this.AllSites = new ObservableCollection<SiteViewModel>(all);

            _dictpatients.Clear();
            foreach (SiteViewModel site in AllSites)
            {
                _dictpatients.Add(site.Name, new List<MPatient>());
            }
            UpdateChartSeries();

            ChartTitle = "Available Sites";

            SelectedSite = (from site in AllSites where _dictpatients[site.Name].Count > 0 select site).First(); // AllSites[0];

            _patientRepository.SelectedPatientAdded += this.OnSelectedPatientAddedToRepository;
        }

        #endregion // Constructor

        #region Helpers

        void UpdateSelectedPatient()
        {
            _patientRepository.ClearSelectedPatients(_filterlevel);
            _patientRepository.SuspendEvent = true;
            foreach (MPatient pat in _dictpatients[_selectedSite.Name]) // patients of the site
            {
                if (pat == _dictpatients[_selectedSite.Name][_dictpatients[_selectedSite.Name].Count - 1])
                    _patientRepository.SuspendEvent = false;
                _patientRepository.AddSelectedPatient(_filterlevel, pat);
            }
            _patientRepository.SuspendEvent = false;
        }
        void UpdateChartSeries()
        {
            foreach (KeyValuePair<string, List<MPatient>> kvp in _dictpatients)
            {
                kvp.Value.Clear();
            }
            foreach (MPatient pat in (_filterlevel > 0)?_patientRepository.GetSelectedPatients(_filterlevel-1):_patientRepository.GetAllPatients())
            {
                bool bDone = false;
                VMS.TPS.Common.Model.API.Patient vpat = _vapp.OpenPatientById(pat.MRN);
                foreach (VMS.TPS.Common.Model.API.Course course in vpat.Courses)
                {
                    foreach (VMS.TPS.Common.Model.API.PlanSetup setup in course.PlanSetups)
                    {
                        if (setup.StructureSet != null)
                        {
                            foreach (VMS.TPS.Common.Model.API.Structure str in setup.StructureSet.Structures)
                            {
                                foreach (SiteViewModel site in AllSites)
                                {
                                    foreach (string sz in site.Structures)
                                    {
                                        if (str.Id.ToUpper().Contains(sz.ToUpper()))
                                        //if (site.Structures.Contains(str.Id.ToUpper()))
                                        {
                                            bDone = true;
                                            _dictpatients[site.Name].Add(pat);
                                            break;
                                        }
                                    }
                                    if (bDone)
                                        break;
                                }
                                if (bDone)
                                    break;
                            }
                        }
                        if (bDone)
                            break;
                    }
                    if (bDone)
                        break;
                }
                if (!bDone)
                    _dictpatients[AllSites[AllSites.Count-1].Name].Add(pat);
                _vapp.ClosePatient();
            }
            this.SiteChartSeries = new ObservableCollection<KeyValuePair<string, int>>();
            foreach (KeyValuePair<string, List<MPatient>> kvp in _dictpatients)
            {
                this.SiteChartSeries.Add(new KeyValuePair<string, int>(kvp.Key, kvp.Value.Count));
            }

        }
        #endregion
        #region Public Interface

        /// <summary>
        /// Returns a collection of all the SiteViewModel objects.
        /// </summary>
        public ObservableCollection<SiteViewModel> AllSites { get; private set; }

        /// <summary>
        /// Returns a collection of CharSeriess of month histogram
        /// </summary>
        public ObservableCollection<KeyValuePair<string, int>> SiteChartSeries { get; private set; }

        public object SelectedSite
        {
            get
            {
                return _selectedSite;
            }
            set
            {
                _selectedSite = value as SiteViewModel;
                foreach (SiteViewModel svm in AllSites)
                {
                    svm.IsSelected = false;
                }
                _selectedSite.IsSelected = true;
                
                UpdateSelectedPatient();
            }
        }
        public string ChartTitle { get; set; }
        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (SiteViewModel custVM in this.AllSites)
                custVM.Dispose();

            this.AllSites.Clear();

            _patientRepository.SelectedPatientAdded -= this.OnSelectedPatientAddedToRepository;
        }

        #endregion // Base Class Overrides

        #region Event Handling Methods

        void OnSelectedPatientAddedToRepository(object sender, PatientAddedEventArgs e)
        {
            if (e.Level < _filterlevel) // updated from previous level
                UpdateChartSeries();
        }
        #endregion // Event Handling Methods
    }
}
