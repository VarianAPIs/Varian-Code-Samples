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

//Sample license text.
using System;
using System.Collections.Generic;
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
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PluginScriptExample
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        private ViewModel _viewModel;

        //Public properties used to receive data from main program
        private StructureSet _structureSet;
        public StructureSet StructureSet
        {
            get { return _structureSet; }

            set
            {
                _structureSet = value;
            }
        }

        private PlanSetup _plan;
        public PlanSetup Plan
        {
            get { return _plan; }
            set
            {
                _plan = value;
                lblPlan.Content = _plan.Id;
            }
        }

        private List<PlanningItem> _pItemsInScope;
        public List<PlanningItem> PItemsInScope
        {
            get { return _pItemsInScope; }
            set
            {
                _pItemsInScope = value;
                foreach (var pitem in _pItemsInScope)
                    _viewModel.PlanningItemIds.Add(pitem.Id);
            }
        }

        private Patient _patient;
        public Patient patient
        {
            get { return _patient; }
            set
            {
                _patient = value;
                lblPatient.Content = _patient.Id;
            }
        }

        private User _user;
        public User user
        {
            get { return _user; }
            set
            {
                _user = value;
                lblUser.Content = _user.Id;
            }
        }

        public class ViewModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public ObservableCollection<string> _planningItemIds;
            public ObservableCollection<string> PlanningItemIds
            {
                get { return _planningItemIds; }
                set { _planningItemIds = value; }
            }

            public ViewModel()
            {
                _planningItemIds = new ObservableCollection<string>();
            }

            private void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        
        public MainControl()
        {
            InitializeComponent();

            _viewModel = new ViewModel();
            this.DataContext = _viewModel;
        }
    }
}
