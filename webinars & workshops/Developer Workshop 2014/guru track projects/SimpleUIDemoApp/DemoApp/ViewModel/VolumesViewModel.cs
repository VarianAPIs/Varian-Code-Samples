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
    /// Represents a container of structure volumes
    /// </summary>
    public class VolumesViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly SiteRepository _siteRepository;
        readonly VolumeRepository _volumeRepository;
        readonly PatientRepository _patientRepository;
        readonly VMS.TPS.Common.Model.API.Application _vapp;
        readonly Dictionary<VolumeViewModel, List<MPatient>> _dictpatients; // patients indexed by volume
        readonly List<StructureTempViewModel> _avialstructures;
        readonly int _filterlevel;
        StructureTempViewModel _selectedStructure;
        VolumeViewModel _selectedVolume;
        #endregion // Fields

        #region Constructor

        public VolumesViewModel(VMS.TPS.Common.Model.API.Application vapp, PatientRepository patientRepository, SiteRepository siteRepository, VolumeRepository volumeRepository, int filterlevel)
        {
            if (patientRepository == null)
                throw new ArgumentNullException("paientRepository");
            if (siteRepository == null)
                throw new ArgumentNullException("siteRepository");
            if (volumeRepository == null)
                throw new ArgumentNullException("volumeRepository");
            base.DisplayName = Strings.VolumesViewModel_DisplayName;

            _patientRepository = patientRepository;
            _siteRepository = siteRepository;
            _volumeRepository = volumeRepository;            
            _vapp = vapp;
            _filterlevel = filterlevel;
            _avialstructures = new List<StructureTempViewModel>();
            _dictpatients = new Dictionary<VolumeViewModel, List<MPatient>>();

            
            // Populate the AllVolumes collection.
            this.CreateVolumes();
        }

        void CreateVolumes()
        {
           List<VolumeViewModel> all =
                (from cust in _volumeRepository.GetAllVolumes()
                 select new VolumeViewModel(cust, _volumeRepository)).ToList();

            this.AllVolumes = new ObservableCollection<VolumeViewModel>(all);

            _dictpatients.Clear();
            foreach (VolumeViewModel vol in AllVolumes)
            {
                _dictpatients.Add(vol, new List<MPatient>());
            }

            this.VolumeChartSeries = new ObservableCollection<KeyValuePair<string, float>>();
            ChartTitle = "Volume distributions";

           
            // this should update the char
            PopulateStructures();

             
            SelectedVolume = AllVolumes[0];
            

            _patientRepository.SelectedPatientAdded += this.OnSelectedPatientAddedToRepository;

            _siteRepository.SiteIsSelected += this.OnSiteIsSelected;
            
        }

        #endregion // Constructor

        #region Helpers

        List<StructureTempViewModel> CreateStructures(List<string> structures)
        {
            List<StructureTempViewModel> lstTmp = new List<StructureTempViewModel>();
            foreach (string str in structures)
                lstTmp.Add(new StructureTempViewModel(str));
            return lstTmp;
        }
        void PopulateStructures()
        {
            //List<Site> selectedSites = _siteRepository.GetSelectedSite();
            Site selectedSite = _siteRepository.SelectedSite;
            _avialstructures.Clear();
            if (selectedSite != null)
                _avialstructures.AddRange(CreateStructures(selectedSite.Structures));
            else
                _avialstructures.AddRange(CreateStructures(_siteRepository.GetSites()[0].Structures));
            AvailableStructures = new ObservableCollection<StructureTempViewModel>(_avialstructures);
            SelectedStructure = _avialstructures[0];
        }
        void UpdateChartSeries()
        {
            foreach (KeyValuePair<VolumeViewModel, List<MPatient>> kvp in _dictpatients)
            {
                kvp.Value.Clear();

            }
            VolumeChartSeries.Clear();

            int patcnt = 0;
            foreach (MPatient pat in (_filterlevel > 0) ? _patientRepository.GetSelectedPatients(_filterlevel - 1) : _patientRepository.GetAllPatients())
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
                                if (str.Name.ToUpper().Contains(_selectedStructure.Name.ToUpper()))
                                {
                                    bDone = true;
                                    this.VolumeChartSeries.Add(new KeyValuePair<string, float>((++patcnt).ToString(),
                                        (float)str.Volume));
                                    foreach (VolumeViewModel vol in AllVolumes)
                                    {
                                        if (str.Volume > vol.Range[0] && str.Volume < vol.Range[1])
                                        {
                                            _dictpatients[vol].Add(pat);
                                            //break;
                                        }
                                    }
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
                _vapp.ClosePatient();
            }
        }
       
        #endregion
        #region Public Interface

        /// <summary>
        /// Returns a collection of all the VolumeViewModel objects.
        /// </summary>
        public ObservableCollection<VolumeViewModel> AllVolumes { get; private set; }

        /// <summary>
        /// Returns a collection of all the structure names of the selected site.
        /// </summary>
        public ObservableCollection<StructureTempViewModel> AvailableStructures { get; private set; }

        /// <summary>
        /// Returns a collection of volumes for volume ChartSeries
        /// </summary>
        public ObservableCollection<KeyValuePair<string, float>> VolumeChartSeries { get; private set; }

        public object SelectedStructure
        {
            get
            {
                return _selectedStructure;
            }
            set
            {
                _selectedStructure = value as StructureTempViewModel;
                List<Site> selectedSites = _siteRepository.GetSelectedSite();
                if (selectedSites.Count > 0)
                {
                    foreach (StructureTempViewModel tmp in AvailableStructures)
                    {
                        selectedSites[0].SelectStructure(tmp.Name,  (tmp == _selectedStructure));
                    }
                }
                UpdateChartSeries();
            }
        }
        public string ChartTitle { get; set; }
        public object SelectedVolume
        {
            get
            {
                return _selectedVolume;
            }
            set
            {
                _selectedVolume = value as VolumeViewModel;

                _selectedVolume.IsSelected = true;
                _patientRepository.ClearSelectedPatients(_filterlevel);
                _patientRepository.SuspendEvent = true;
                foreach (MPatient pat in _dictpatients[_selectedVolume]) // patients of the site
                {
                    if (pat == _dictpatients[_selectedVolume][_dictpatients[_selectedVolume].Count - 1])
                        _patientRepository.SuspendEvent = false;
                    _patientRepository.AddSelectedPatient(_filterlevel, pat);
                }
                _patientRepository.SuspendEvent = false;
            }
        }
        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (VolumeViewModel custVM in this.AllVolumes)
                custVM.Dispose();

            this.AllVolumes.Clear();

            _patientRepository.SelectedPatientAdded -= this.OnSelectedPatientAddedToRepository;
        }

        #endregion // Base Class Overrides

        #region Event Handling Methods

        void OnSelectedPatientAddedToRepository(object sender, PatientAddedEventArgs e)
        {
            if (e.Level < _filterlevel) // updated from previous level
                UpdateChartSeries();
        }

        void OnSiteIsSelected(object sender, SiteSelectedEventArgs e)
        {
            PopulateStructures();
        }
        #endregion // Event Handling Methods
    }
}
