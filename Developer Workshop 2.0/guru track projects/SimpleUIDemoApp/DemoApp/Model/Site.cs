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
using DemoApp.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DemoApp.Model
{
    public class Site : IDataErrorInfo
    {
        #region Creation

        public static Site CreateNewSite()
        {
            return new Site();
        }

        public static Site CreateSite( 
            string name, List<string> structures)
        {
            return new Site
            {
                Name = name,
                Structures = structures,
                IsSelected = false,
                _dictStructureSelected = InitDict(structures),
            };
        }

        static Dictionary<string, bool> InitDict(List<string> structures)
        {
            Dictionary<string, bool> dict = new Dictionary<string, bool>();
            foreach (string str in structures)
            {
                dict.Add(str, false);
            }
            return dict;
        }
        Dictionary<string, bool> _dictStructureSelected;

        protected Site()
        {
        }

        #endregion // Creation

        #region State Properties

        /// <summary>
        /// Gets/sets the name of the site.
        /// </summary>
        public string Name { get; set; }

        public List<string> Structures { get; set; }

        public bool IsSelected { get; set; }

        public void SelectStructure(string structure, bool b)
        {
            if (_dictStructureSelected.ContainsKey(structure))
                _dictStructureSelected[structure] = b;
        }

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
            "Name", 
        };

        string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "Name":
                    error = this.ValidateName();
                    break;
                default:
                    Debug.Fail("Unexpected property being validated on Site: " + propertyName);
                    break;
            }

            return error;
        }

        string ValidateName()
        {
            if (IsStringMissing(this.Name))
            {
                return Strings.Site_Error_MissingName;
            }

            return null;
        }

        static bool IsStringMissing(string value)
        {
            return
                String.IsNullOrEmpty(value) ||
                value.Trim() == String.Empty;
        }
        #endregion // Validation
    }
}
