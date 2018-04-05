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
using System.Windows.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ESAPISimpleUI.Factory;
using v = VMS.TPS.Common.Model.API;

namespace ESAPISimpleUI.ViewModel
{
    /// <summary>
    /// A generic ListViewModel for list of ESAPI data objects, such as PlanSetup, Beam, Structure,...etc.
    /// </summary>
    public class ListViewModel : ViewModelBase
    {
        #region Fields

        ViewModelBase _selectedvm;
        //Dictionary<ViewModelBase, T> _dictVMtoPlanObject;
        //readonly IEnumerable<v.PlanSetup> _plansetups;
        #endregion // Fields

        #region Constructor
        public ListViewModel()
        {
            // default
            this.AllItems = new ObservableCollection<ViewModelBase>();
            ListBoxSelectionMode = System.Windows.Controls.SelectionMode.Multiple;
        }


        public void CreateList(IEnumerable<v.ApiDataObject> vobjs)
        {
            if (vobjs == null)
                throw new ArgumentNullException("Varian object list");

            // creating list of item's viewmodel based on the type of the input object
            List<ViewModelBase> all =
                (from vobj in vobjs
                 select ViewModelFactory.CreateViewModel(vobj)).ToList();
            
            this.AllItems = new ObservableCollection<ViewModelBase>(all);

            ColumnCollection = new ObservableCollection<DataGridColumn>();

            // set up binding for columns
            var columns = all.First()
                    .Properties
                    .Select((x, i) => new { Name = x.Name, Index = i })
                    .ToArray();

            foreach (var column in columns)
            {
                var binding = new Binding(string.Format("Properties[{0}].Value", column.Index));

                ColumnCollection.Add(new DataGridTextColumn() { Header = column.Name, Binding = binding });
            } 
            Label = "Select";
        }

        #endregion // Constructor

        #region Helpers


        #endregion

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the ESAPI[type]ViewModel objects.
        /// </summary>
        public ObservableCollection<ViewModelBase> AllItems { get; private set; }


        /// <summary>
        /// Returns a collection of DataGridColumns.
        /// </summary>
        public ObservableCollection<DataGridColumn> ColumnCollection { get; private set; }


        /// <summary>
        /// The label on top of the list box
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The item the user selected
        /// </summary>
        public object SelectedItem
        {
            get
            {
                return _selectedvm;
            }
            set
            {
                _selectedvm = value as ViewModelBase;
                _selectedvm.IsSelected = true;

            }
        }
       
        public SelectionMode ListBoxSelectionMode { get; set; }

        public List<v.ApiDataObject> SelectedApiDataObjects
        {
            get
            {
                List<v.ApiDataObject> listObjs = new List<v.ApiDataObject>();
                foreach (ViewModelBase vm in AllItems)
                {
                    if (vm.IsSelected)
                    {
                        listObjs.Add(vm.ApiDataObject);
                    }
                }
                return listObjs;
            }
        }
        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (ViewModelBase vm in this.AllItems)
                vm.Dispose();

            this.AllItems.Clear();

        }

        #endregion // Base Class Overrides
    }
}
