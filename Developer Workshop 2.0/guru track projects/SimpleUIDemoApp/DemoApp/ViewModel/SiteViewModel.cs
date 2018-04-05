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
using System.ComponentModel;
using System.Windows.Input;
using DemoApp.DataAccess;
using DemoApp.Model;
using DemoApp.Properties;

namespace DemoApp.ViewModel
{
    /// <summary>
    /// A UI-friendly wrapper for a MPatient object.
    /// </summary>
    public class SiteViewModel : ViewModelBase
    {
        #region Fields

        readonly Site _site;
        readonly SiteRepository _siteRepository;

        #endregion // Fields

        #region Constructor

        public SiteViewModel(Site site, SiteRepository siteRepository)
        {
            if (site == null)
                throw new ArgumentNullException("site");

            if (siteRepository == null)
                throw new ArgumentNullException("siteRepository");

            _site = site;
            _siteRepository = siteRepository;
        }

        #endregion // Constructor

        #region Properties
        public string Name
        {
            get { return _site.Name; }
            set
            {
                if (value == _site.Name)
                    return;

                _site.Name = value;

                base.OnPropertyChanged("Name");
            }
        }

        public System.Collections.Generic.List<string> Structures
        {
            get { return _site.Structures; }
            set
            {
                _site.Structures = value;

                base.OnPropertyChanged("Structures");
            }
        }

        public bool IsSelected
        {
            get { return _site.IsSelected; }
            set
            {
                if (value == _site.IsSelected)
                    return;

                _site.IsSelected = value;

                if (value == true)
                    _siteRepository.SelectedSite = _site;

                base.OnPropertyChanged("IsSelected");
            }
        }

        public override string DisplayName
        {
            get
            {
                return String.Format("{0}", _site.Name);
            }
        }
        
        #endregion

       
    }
}