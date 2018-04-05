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
    /// A UI-friendly wrapper for a ESAPI Structure object.
    /// </summary>
    public class ESAPIStructureViewModel : ViewModelBase
    {
        #region Fields

        #endregion // Fields

        #region Constructor

        public ESAPIStructureViewModel(v.Structure structure)
        {
            if (structure == null)
                throw new ArgumentNullException("structure");
            
            _apidataobject = structure;

            Properties.Add(new Property("Id", structure.Id));
            Properties.Add(new Property("Volume", String.Format("{0:.0}",structure.Volume)));
       
        }

        #endregion // Constructor

        #region structure Properties


        #endregion // Patient Properties

        #region Presentation Properties


        public override string DisplayName
        {
            get
            {
                v.Structure _structure = _apidataobject as v.Structure;
                return String.Format("{0, -20},  volume {1:.0} cc",
                    _structure.Id, _structure.Volume);
            }
        }

        #endregion // Presentation Properties

        #region Public Methods


        #endregion // Public Methods

        #region Private Helpers

        #endregion // Private Helpers

       
    }
}