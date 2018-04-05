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
    public class MPatientViewModel : ViewModelBase, IDataErrorInfo
    {
        #region Fields

        readonly MPatient _patient;
        readonly PatientRepository _patientRepository;
        string _patientType;
        string[] _patientTypeOptions;
        bool _isSelected;
      
        #endregion // Fields

        #region Constructor

        public MPatientViewModel(MPatient pat, PatientRepository patientRepository)
        {
            if (pat == null)
                throw new ArgumentNullException("pat");

            if (patientRepository == null)
                throw new ArgumentNullException("patientRepository");

            _patient = pat;
            _patientRepository = patientRepository;
        }

        #endregion // Constructor

        #region Patient Properties

        public string MRN
        {
            get { return _patient.MRN; }
          
        }

        public string FirstName
        {
            get { return _patient.FirstName; }
          
        }


        public string LastName
        {
            get { return _patient.LastName; }
            
        }

        public string Sex
        {
            get { return _patient.Sex; }

        }

        #endregion // Patient Properties

        #region Presentation Properties

        /// <summary>
        /// Gets/sets a value that indicates what type of patient this is.
        /// </summary>
        public string PatientType
        {
            get { return _patientType; }
        }

       

        public override string DisplayName
        {
            get
            {
               return String.Format("{0}, {1}", _patient.LastName, _patient.FirstName);
            }
        }

        /// <summary>
        /// Gets/sets whether this patient is selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;

                base.OnPropertyChanged("IsSelected");
            }
        }

        #endregion // Presentation Properties

        #region Public Methods

        #endregion // Public Methods

        #region Private Helpers

        #endregion // Private Helpers

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_patient as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                error = (_patient as IDataErrorInfo)[propertyName];

                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }

        #endregion // IDataErrorInfo Members
    }
}