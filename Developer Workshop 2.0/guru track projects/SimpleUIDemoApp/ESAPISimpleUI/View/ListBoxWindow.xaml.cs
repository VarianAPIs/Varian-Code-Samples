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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using v=VMS.TPS.Common.Model.API;
namespace ESAPISimpleUI.View
{
    public enum ViewType
    {
        ListBox,
        DataGrid
    }
    /// <summary>
    /// Interaction logic for ListBoxWindow.xaml
    /// </summary>
    public partial class ListBoxWindow : Window
    {
        ESAPISimpleUI.ViewModel.ListViewModel _vm;

        /// <summary>
        /// Constructor for a generic listbox winodow for selection of Eclispe data objects
        /// </summary>
        /// <param name="title"></param>
        /// <param name="label"></param>
        /// <param name="selectionmode"></param>
        /// <param name="vobjs"></param>
        public ListBoxWindow(string title, string label, 
            System.Windows.Controls.SelectionMode selectionmode,
            ViewType type,
            IEnumerable<v.ApiDataObject> vobjs )
        {
            InitializeComponent();
            this.Title = title;
            _vm = new ESAPISimpleUI.ViewModel.ListViewModel();
            
            _vm.CreateList(vobjs);
            _vm.Label = label;
            _vm.ListBoxSelectionMode = selectionmode;

            UserControl view;
            if (type == ViewType.ListBox)
                view = new ESAPISimpleUI.View.GenericListView();
            else
                view = new ESAPISimpleUI.View.GenericDataGridView();
            view.DataContext = _vm;
            this.gridRow0.Children.Add(view);
        }

        public ListBoxWindow(IEnumerable<v.ApiDataObject> vobjs) :
            this("Generic selection window for Eclispe data objects", "select:", 
                System.Windows.Controls.SelectionMode.Single, ViewType.ListBox, vobjs)
        {
        }

        public List<v.ApiDataObject> SelectedItems
        {
            get {
                return _vm.SelectedApiDataObjects;
            }
        }
        public string ListBoxLabel {
            get { return _vm.Label; }
            set { _vm.Label = value; }
        }
        public System.Windows.Controls.SelectionMode ListBoxSelectionMode
        {
            get { return _vm.ListBoxSelectionMode; }
            set { _vm.ListBoxSelectionMode = value; }
        }
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
