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
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using DemoApp.DataAccess;
using DemoApp.Model;
using DemoApp.Properties;

namespace DemoApp.ViewModel
{
    /// <summary>
    /// The ViewModel for the application's main window.
    /// </summary>
    public class MainWindowViewModel : WorkspaceViewModel
    {
        #region Fields
                
        ReadOnlyCollection<CommandViewModel> _commands;

        readonly PatientRepository _patientRepository;
        readonly SiteRepository _siteRepository;
        readonly VolumeRepository _volumeRepository;
        ObservableCollection<WorkspaceViewModel> _workspaces;
        VMS.TPS.Common.Model.API.Application _vapp;
        #endregion // Fields

        #region Constructor

        
        public MainWindowViewModel(string szDataFile, VMS.TPS.Common.Model.API.Application vapp)
        {
            base.DisplayName = Strings.MainWindowViewModel_DisplayName;

            _siteRepository = new SiteRepository(null);

            _volumeRepository = new VolumeRepository(null);

            _vapp = vapp;
            //LoadEclipseApp();

            _patientRepository = new PatientRepository(_vapp.PatientSummaries.ToList(), 12);
        }

        #endregion // Constructor

        #region Commands

        /// <summary>
        /// Returns a read-only list of commands 
        /// that the UI can display and execute.
        /// </summary>
        public ReadOnlyCollection<CommandViewModel> Commands
        {
            get
            {
                if (_commands == null)
                {
                    List<CommandViewModel> cmds = this.CreateCommands();
                    _commands = new ReadOnlyCollection<CommandViewModel>(cmds);
                }
                return _commands;
            }
        }

        List<CommandViewModel> CreateCommands()
        {
            // commands on the control panel
            return new List<CommandViewModel>
            {
                
                new CommandViewModel(
                    Strings.MainWindowViewModel_Command_SelectPatients,
                    new RelayCommand(param => this.SelectPatients())),

                 new CommandViewModel(
                    Strings.MainWindowViewModel_Command_SelectSites,
                    new RelayCommand(param => this.SelectSites())),

                new CommandViewModel(Strings.MainWindowViewModel_Command_SelectVolume,
                    new RelayCommand(param => this.SelectVolumes())),

                 new CommandViewModel(Strings.MainWindowViewModel_Command_ViewSelectedPatients,
                    new RelayCommand(param => this.ShowSelectedPatients()))
            };
        }

        #endregion // Commands

        #region Workspaces

        /// <summary>
        /// Returns the collection of available workspaces to display.
        /// A 'workspace' is a ViewModel that can request to be closed.
        /// </summary>
        public ObservableCollection<WorkspaceViewModel> Workspaces
        {
            get
            {
                if (_workspaces == null)
                {
                    _workspaces = new ObservableCollection<WorkspaceViewModel>();
                    _workspaces.CollectionChanged += this.OnWorkspacesChanged;
                }
                return _workspaces;
            }
        }

        void OnWorkspacesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.NewItems)
                    workspace.RequestClose += this.OnWorkspaceRequestClose;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (WorkspaceViewModel workspace in e.OldItems)
                    workspace.RequestClose -= this.OnWorkspaceRequestClose;
        }

        void OnWorkspaceRequestClose(object sender, EventArgs e)
        {
            WorkspaceViewModel workspace = sender as WorkspaceViewModel;
            workspace.Dispose();
            this.Workspaces.Remove(workspace);
        }

        #endregion // Workspaces

        #region Private Helpers

        /// <summary>
        /// Create an Eclipse Application instance
        /// </summary>
        void LoadEclipseApp()
        {
            _vapp = VMS.TPS.Common.Model.API.Application.CreateApplication(null, null);
        }
        
        // comand callbacks
        void SelectPatients()
        {
            PatientsViewModel workspace =
                 this.Workspaces.FirstOrDefault(vm => vm is PatientsViewModel)
                 as PatientsViewModel;

            if (workspace == null)
            {
                workspace = new PatientsViewModel(_vapp, _patientRepository, 0);
                this.Workspaces.Add(workspace);
            }

            this.SetActiveWorkspace(workspace);
        }

        void SelectSites()
        {
            PatientsViewModel patworkspace =
                 this.Workspaces.FirstOrDefault(vm => vm is PatientsViewModel)
                 as PatientsViewModel;

            if (patworkspace == null)
            {
                System.Windows.MessageBox.Show("You must select time filter first.");
                return;
            }
            SitesViewModel workspace =
                 this.Workspaces.FirstOrDefault(vm => vm is SitesViewModel)
                 as SitesViewModel;

            if (workspace == null)
            {
                workspace = new SitesViewModel(_vapp, _patientRepository, _siteRepository, 1);
                this.Workspaces.Add(workspace);
            }

            this.SetActiveWorkspace(workspace);
        }
        void SelectVolumes()
        {
       
            PatientsViewModel patworkspace =
                 this.Workspaces.FirstOrDefault(vm => vm is PatientsViewModel)
                 as PatientsViewModel;

            if (patworkspace == null)
            {
                System.Windows.MessageBox.Show("You must select time filter first.");
                return;
            }

            SitesViewModel volworkspace =
                this.Workspaces.FirstOrDefault(vm => vm is SitesViewModel)
                as SitesViewModel;

            if (volworkspace == null)
            {
                System.Windows.MessageBox.Show("You must select site filter first.");
                return;
            }
            VolumesViewModel workspace =
                 this.Workspaces.FirstOrDefault(vm => vm is VolumesViewModel)
                 as VolumesViewModel;

            if (workspace == null)
            {
                workspace = new VolumesViewModel(_vapp, _patientRepository, _siteRepository, _volumeRepository, 2);
                this.Workspaces.Add(workspace);
            }

            this.SetActiveWorkspace(workspace);
        }
        void ShowSelectedPatients()
        {
            PatientsViewModel patworkspace =
                 this.Workspaces.FirstOrDefault(vm => vm is PatientsViewModel)
                 as PatientsViewModel;

            if (patworkspace == null)
            {
                System.Windows.MessageBox.Show("You must select time filter first.");
                return;
            }

            SelectedPatientsViewModel workspace =
                this.Workspaces.FirstOrDefault(vm => vm is SelectedPatientsViewModel)
                as SelectedPatientsViewModel;

            if (workspace == null)
            {
                workspace = new SelectedPatientsViewModel(_vapp, _patientRepository);
                this.Workspaces.Add(workspace);
            }

            this.SetActiveWorkspace(workspace);
        }
        void SetActiveWorkspace(WorkspaceViewModel workspace)
        {
            Debug.Assert(this.Workspaces.Contains(workspace));

            ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.Workspaces);
            if (collectionView != null)
                collectionView.MoveCurrentTo(workspace);
        }

        #endregion // Private Helpers
    }
}