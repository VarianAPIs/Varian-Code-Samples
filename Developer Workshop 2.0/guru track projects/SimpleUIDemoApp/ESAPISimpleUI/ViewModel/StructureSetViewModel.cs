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
    /// A UI-friendly wrapper for a ESAPI StructureSet object.
    /// </summary>
    public class ESAPIStructureSetViewModel : ViewModelBase
    {
        #region Fields

    
        bool _isSelected;


        #endregion // Fields

        #region Constructor

        public ESAPIStructureSetViewModel(v.StructureSet structureset)
        {
            if (structureset == null)
                throw new ArgumentNullException("structureset");

            _apidataobject = structureset;

            Properties.Add(new Property("Id", structureset.Id));
            Properties.Add(new Property("# of Structures", new List<v.Structure>(structureset.Structures).Count));
        }

        #endregion // Constructor

        #region Structure Set Properties


        #endregion // Patient Properties

        #region Presentation Properties


        public override string DisplayName
        {
            get
            {
                v.StructureSet _structureset = _apidataobject as v.StructureSet;
                int structcount = 0;
                foreach(v.Structure s in _structureset.Structures) ++structcount;
                return String.Format("{0,-8}, Containts {1} structures", _structureset.Id, structcount );
            }
        }

        #endregion // Presentation Properties

        #region Public Methods


        #endregion // Public Methods

        #region Private Helpers

        #endregion // Private Helpers

       
    }
}