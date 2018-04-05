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
using System.Linq;
using System.Text;
using ESAPISimpleUI.ViewModel;
using v = VMS.TPS.Common.Model.API;
namespace ESAPISimpleUI.Factory
{
    /// <summary>
    /// Factory class that creates an instance of view model depending on the input object type
    /// </summary>
    public class ViewModelFactory
    {
        public static ViewModelBase CreateViewModel(v.ApiDataObject vobj)
        {
            if (vobj is v.PlanSetup)
            {
                return new ESAPIPlanSetupViewModel(vobj as v.PlanSetup);
            }
            else if (vobj is v.Beam)
            {
                return new ESAPIBeamViewModel(vobj as v.Beam);
            }
            else if (vobj is v.Structure)
            {
                return new ESAPIStructureViewModel(vobj as v.Structure);
            }
            else if (vobj is v.StructureSet)
            {
                return new ESAPIStructureSetViewModel(vobj as v.StructureSet);
            } 
            else if (vobj is v.Course)
            {
                return new ESAPICourseViewModel(vobj as v.Course);
            }
            else {
                throw new NotSupportedException(String.Format("The type {0} is not supported yet.", vobj.GetType().ToString()));
            }
        }
    }
}
