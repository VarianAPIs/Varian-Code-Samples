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
    /// Represents a source of patients in the application.
    /// </summary>
    public class PatientRepository
    {
        #region Fields

        readonly List<MPatient> _origpatients;

        readonly Dictionary<int,List<MPatient>> _dictfilterpatients; // filtertred patients index by level

        //readonly Dictionary<int, string> _dictfilter; // filterrs index by level

        readonly Dictionary<string, List<MPatient>> _dictPatientByMonth;

        readonly int _months;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new repository of patients.
        /// </summary>
        /// <param name="pats"> list of Eclipse API PatientSummary</param>
        /// <param name="months">number of months of patients</param>
        public PatientRepository(List<VMS.TPS.Common.Model.API.PatientSummary> pats, int months)
        {
            _months = months;
            //_patients = LoadPatients(pats);
            //_origpatients = new List<MPatient>(_patients);
            _dictPatientByMonth = BuildPatientDictionary(pats, months);
            _origpatients = CreatePatients();
            _dictfilterpatients = new Dictionary<int, List<MPatient>>();
            
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Suspend update event
        /// </summary>
        public bool SuspendEvent { get; set; }
       
        /// <summary>
        /// Returns true if the specified patient exists in the
        /// repository, or false if it is not.
        /// </summary>
        public bool ContainsPatient(MPatient pat)
        {
            if (pat == null)
                throw new ArgumentNullException("pat");

            return _origpatients.Contains(pat);
        }

        /// <summary>
        /// Raised when a patient is placed into the repository.
        /// </summary>
        public event EventHandler<PatientAddedEventArgs> SelectedPatientAdded;

        /// <summary>
        /// Places the specified patient into the repository.
        /// If the patient is already in the repository, an
        /// exception is not thrown.
        /// </summary>
        public void AddSelectedPatient(int level, MPatient pat)
        {
            if (pat == null)
                throw new ArgumentNullException("pat");
            if (!_dictfilterpatients.ContainsKey(level))
                _dictfilterpatients.Add(level, new List<MPatient>());

            if (!_dictfilterpatients[level].Contains(pat))
            {
                _dictfilterpatients[level].Add(pat);

                if (this.SelectedPatientAdded != null && !SuspendEvent)
                    this.SelectedPatientAdded(this, new PatientAddedEventArgs(pat,level));
            }
        }
        /// <summary>
        ///clear selected patients in the repository.
        /// </summary>
        public void ClearSelectedPatients(int level)
        {
            if (level < 0 || level > _dictfilterpatients.Keys.Count - 1)
                return;
            _dictfilterpatients[level].Clear();
        }
        /// <summary>
        /// Returns a shallow-copied list of all patients in the repository.
        /// </summary>
        public List<MPatient> GetAllPatients()
        {
            return new List<MPatient>(_origpatients);
        }
        /// <summary>
        /// Returns a shallow-copied list of all patients in the repository.
        /// </summary>
        public List<MPatient> GetPatientsByMonth(string month)
        {
            if (_dictPatientByMonth.ContainsKey(month))
            {
                return new List<MPatient>(_dictPatientByMonth[month]);
            }
            return new List<MPatient>();
        }
        /// <summary>
        /// Returns a shallow-copied list of selected patients in the repository.
        /// </summary>
        public List<MPatient> GetSelectedPatients(int level)
        {
            if (level > _dictfilterpatients.Keys.Count - 1)
                throw new ArgumentOutOfRangeException("level");
            if (level < 0) // latest one
                return new List<MPatient>(_dictfilterpatients[_dictfilterpatients.Count - 1]);
            return new List<MPatient>(_dictfilterpatients[level]);
        }

        /// <summary>
        /// Return lastest filter level
        /// </summary>
        /// <returns></returns>
        public int GetLastestFilterLevel()
        {
            int rk = 0;
            foreach (int k in _dictfilterpatients.Keys)
            {
                if (k > rk)
                    rk = k;
            }
            return rk;
        }
        /// <summary>
        /// Returns a shallow-copied list of month-indexed patient diectionary
        /// </summary>
        public List<KeyValuePair<string,int>> GetChartSeries()
        {
            List<KeyValuePair<string, int>> lstSeries = new List<KeyValuePair<string, int>>();
            foreach (KeyValuePair<string, List<MPatient>> kvp in _dictPatientByMonth)
            {
                lstSeries.Add(new KeyValuePair<string,int>(kvp.Key, kvp.Value.Count));
            }
            return lstSeries;
        }
        /// <summary>
        /// Returns a shallow-copied list of months in the patient repository
        /// </summary>
        public List<string> GetMonths()
        {
            List<string> lstMonths = new List<string>();
            for (int i = 1; i <= _months; i++)
                lstMonths.Add(i.ToString());
            return lstMonths;
        }
        #endregion // Public Interface

        #region Private Helpers

        static Dictionary<string, List<MPatient>> BuildPatientDictionary(List<VMS.TPS.Common.Model.API.PatientSummary> pats, int months)
        {
             // For the test purpose we are interesting in patient records created during the last 12 months
            DateTime now = DateTime.Now;
            Dictionary<string, List<MPatient>> dictPatientByMonth = new Dictionary<string, List<MPatient>>();
            for (int i = 1; i <= months; i++)
                dictPatientByMonth.Add(i.ToString(), new List<MPatient>());
            foreach (VMS.TPS.Common.Model.API.PatientSummary pat in pats)
            {
                if (pat.CreationDateTime.HasValue)
                {
                    int monthspan = (now.Month - pat.CreationDateTime.Value.Month + 12 * (now.Year - pat.CreationDateTime.Value.Year));
                    if (monthspan < months)
                    {
                        monthspan++;
                        for (int j = monthspan; j <=12; j++)
                           dictPatientByMonth[j.ToString()].Add(MPatient.CreatePatient(pat));
           
                    }
                }
            }
            return dictPatientByMonth;
        }
        List<MPatient> CreatePatients()
        {
            List<MPatient> mpats = new List<MPatient>();

            foreach(KeyValuePair<string,List<MPatient>> kvp in _dictPatientByMonth)
            {
                mpats.AddRange(kvp.Value);
            }
            return mpats;
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