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
using v = VMS.TPS.Common.Model.API;

namespace ESAPISimpleUI.ViewModel
{
    /// <summary>
    /// A UI-friendly wrapper for a ESAPI PlanSeup object.
    /// </summary>
    public class ESAPIPlanSetupViewModel : ViewModelBase
    {
        #region Fields

        #endregion // Fields

        #region Constructor

        public ESAPIPlanSetupViewModel(v.PlanSetup plansetup)
        {
            if (plansetup == null)
                throw new ArgumentNullException("plansetup");

             _apidataobject = plansetup;

             Properties.Add(new Property("Course Id", plansetup.Course.Id));
             Properties.Add(new Property("Id", plansetup.Id));
             Properties.Add(new Property("Approval Status", plansetup.ApprovalStatus));
             Properties.Add(new Property("Dose valid", plansetup.IsDoseValid));
         }

        #endregion // Constructor

        #region PlanSetup Properties

       
        #endregion // Patient Properties

        #region Presentation Properties
       

        public override string DisplayName
        {
            get
            {
                v.PlanSetup _plansetup = _apidataobject as v.PlanSetup;
                if (_plansetup == null)
                    return "";
               return String.Format("{3,-6}-{0,-16}, {1,-10}, {2,-8}", 
                   _plansetup.Id, _plansetup.ApprovalStatus.ToString(),
                   string.Format("Dose valid:{0}",_plansetup.IsDoseValid.ToString()),
                   _plansetup.Course.Id);
            }
        }

       

        #endregion // Presentation Properties

        #region Public Methods


        #endregion // Public Methods

        #region Private Helpers

        #endregion // Private Helpers

    }
}