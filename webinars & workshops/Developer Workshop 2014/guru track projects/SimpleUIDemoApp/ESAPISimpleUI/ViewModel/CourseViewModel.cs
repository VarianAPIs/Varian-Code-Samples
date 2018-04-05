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
using System.Collections.Generic;
using v = VMS.TPS.Common.Model.API;

namespace ESAPISimpleUI.ViewModel
{
    /// <summary>
    /// A UI-friendly wrapper for a ESAPI Course object.
    /// </summary>
    public class ESAPICourseViewModel : ViewModelBase
    {
        #region Fields

        bool _isSelected;


        #endregion // Fields

        #region Constructor

        public ESAPICourseViewModel(v.Course course)
        {
            if (course == null)
                throw new ArgumentNullException("course");

            _apidataobject = course;

            Properties.Add(new Property("Id", course.Id));
            Properties.Add(new Property("Starting Date", course.StartDateTime));
            Properties.Add(new Property("# of Plans", new List<v.PlanSetup>(course.PlanSetups).Count));
            Properties.Add(new Property("# of PlanSums", new List<v.PlanSum>(course.PlanSums).Count));
  
        }

        #endregion // Constructor

        #region Course Properties


        #endregion // Patient Properties

        #region Presentation Properties


        public override string DisplayName
        {
            get
            {
                v.Course _course = _apidataobject as v.Course;
                int pcount = 0;
                foreach (v.PlanSetup p in _course.PlanSetups) ++pcount;
                int pscount = 0;
                foreach (v.PlanSum ps in _course.PlanSums) ++pscount;
                return String.Format("{0,-6}, contains {1} plans, contains {2} plan-sums", _course.Id, pcount, pscount);
            }
        }

        #endregion // Presentation Properties

        #region Public Methods


        #endregion // Public Methods

        #region Private Helpers

        #endregion // Private Helpers


    }
}