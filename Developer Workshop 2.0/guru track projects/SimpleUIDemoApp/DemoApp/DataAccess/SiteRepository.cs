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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using System.Xml.Linq;
using DemoApp.Model;

namespace DemoApp.DataAccess
{
    /// <summary>
    /// Represents a source of sites in the application.
    /// </summary>
    public class SiteRepository
    {
        #region Fields

        readonly List<Site> _sites;

        Site _selectedsite;
       
        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new repository of patients.
        /// </summary>
        /// <param name="siteDataFile">DO NOT USE now --The relative path to an XML resource file that contains site data.</param>
        public SiteRepository(string siteDataFile)
        {
            _sites = LoadSites(siteDataFile);
            _selectedsite = null;
        }

        #endregion // Constructor

        #region Public Interface

        
        /// <summary>
        /// Returns a shallow-copied list of all sites in the repository.
        /// </summary>
        public List<Site> GetSites()
        {
            return new List<Site>(_sites);
        }
        /// <summary>
        /// Returns a shallow-copied list of all sites in the repository.
        /// </summary>
        public List<Site> GetSelectedSite()
        {
            List<Site> ret = new List<Site>();
            foreach (Site s in _sites)
            {
                if (s.IsSelected)
                    ret.Add(s);
            }
            return ret;
        }
        public Site SelectedSite
        {
            get
            {
                return _selectedsite;
            }
            set 
            {
                _selectedsite = value;
                if (this.SiteIsSelected != null)
                    this.SiteIsSelected(this, new SiteSelectedEventArgs(_selectedsite));
            }
        }

        /// <summary>
        /// Raised when a site is selected
        /// </summary>
        public event EventHandler<SiteSelectedEventArgs> SiteIsSelected;

        #endregion // Public Interface

        #region Private Helpers

        static List<Site> LoadSites(string siteDataFile)
        {
            if (string.IsNullOrEmpty(siteDataFile))
            {
                // for demo purpose only
                List<Site> sites = new List<Site>{
                    Site.CreateSite("H&N", new List<string>{"PAROTID","BRAINSTEM"}),
                    Site.CreateSite("Prostate", new List<string>{"BLADDER","RECTUM"}),
                    Site.CreateSite("Others", new List<string>()),
                };
                return sites;
            }
            // In a real application, the data would come from an external source,
            // but for this demo let's keep things simple and use a resource file.
            using (Stream stream = GetResourceStream(siteDataFile))
            using (XmlReader xmlRdr = new XmlTextReader(stream))
                return
                    (from siteElem in XDocument.Load(xmlRdr).Element("sites").Elements("site")
                     select Site.CreateSite(
                        (string)siteElem.Attribute("Name"), new List<string>()
                         )).ToList();
        }

        static Stream GetResourceStream(string resourceFile)
        {
            Uri uri = new Uri(resourceFile, UriKind.RelativeOrAbsolute);

            StreamResourceInfo info = Application.GetResourceStream(uri);
            if (info == null || info.Stream == null)
                throw new ApplicationException("Missing resource file: " + resourceFile);

            return info.Stream;
        }

        #endregion // Private Helpers
    }
}