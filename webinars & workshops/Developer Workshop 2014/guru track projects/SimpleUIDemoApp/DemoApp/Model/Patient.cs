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
using System.Diagnostics;
using System.Text.RegularExpressions;
using DemoApp.Properties;

namespace DemoApp.Model
{
    public class MPatient : IDataErrorInfo
    {
        #region Creation

        public static MPatient CreateNewPatient()
        {
            return new MPatient();
        }

        public static MPatient CreatePatient(
           VMS.TPS.Common.Model.API.PatientSummary vpat)
        {
            return new MPatient
            {
                
                FirstName = vpat.FirstName,
                LastName = vpat.LastName,
                Sex = vpat.Sex,
                MRN = vpat.Id
            };
        }

        protected MPatient()
        {
        }

        #endregion // Creation

        #region State Properties

        /// <summary>
        /// Gets/sets the MRN of the patient.
        /// </summary>
        public string MRN { get; private set; }

        /// <summary>
        /// Gets/sets the patient's first name.
        /// </summary>
        public string FirstName { get;  private set; }

        /// <summary>
        /// Gets/sets patient sex
        /// The default value is false.
        /// </summary>
        public string Sex { get; private set; }
       

        /// <summary>
        /// Gets/sets the patient's last name.
        /// </summary>
        public string LastName { get; private set; }

        #endregion // State Properties

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error { get { return null; } }

        string IDataErrorInfo.this[string propertyName]
        {
            get { return this.GetValidationError(propertyName); }
        }

        #endregion // IDataErrorInfo Members

        #region Validation

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                    if (GetValidationError(property) != null)
                        return false;

                return true;
            }
        }

        static readonly string[] ValidatedProperties = 
        { 
            "MRN", 
            "FirstName", 
            "LastName",
        };

        string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "MRN":
                    error = this.ValidateMRN();
                    break;

                case "FirstName":
                    error = this.ValidateFirstName();
                    break;

                case "LastName":
                    error = this.ValidateLastName();
                    break;

                default:
                    Debug.Fail("Unexpected property being validated on Patient: " + propertyName);
                    break;
            }

            return error;
        }

        string ValidateMRN()
        {
            if (IsStringMissing(this.MRN))
            {
                return Strings.Patient_Error_MissingMRN;
            }
            return null;
        }

        string ValidateFirstName()
        {
            if (IsStringMissing(this.FirstName))
            {
                return Strings.Patient_Error_MissingFirstName;
            }
            return null;
        }

        string ValidateLastName()
        {
           if (IsStringMissing(this.LastName))
              return Strings.Patient_Error_MissingLastName;
           
           return null;
        }

        static bool IsStringMissing(string value)
        {
            return
                String.IsNullOrEmpty(value) ||
                value.Trim() == String.Empty;
        }

        static bool IsValidEmailAddress(string email)
        {
            if (IsStringMissing(email))
                return false;

            // This regex pattern came from: http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        #endregion // Validation
    }
}
