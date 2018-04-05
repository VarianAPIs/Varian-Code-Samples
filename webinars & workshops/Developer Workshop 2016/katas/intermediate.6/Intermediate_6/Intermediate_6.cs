////////////////////////////////////////////////////////////////////////////////
// Intermediate_6.cs
//
//  A ESAPI v11+ script that demonstrates creation of control structures.
//
// Kata Intermediate.6)    
//  Find all PTVs in a structure set and create control (“ring”) structures for each PTV
//
// Applies to:
//      Eclipse Scripting API
//            15.0
//     Eclipse Scripting API for Research Users
//            13, 13.5, 13.6, 13.7, 15.0
//
// Copyright (c) 2016 Richard Popple, The University of Alabama at Birmingham
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
////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
    {

        const int maxStructureIdLength = 16;
        int ringThicknessMm = 5;
        string ringDicomType = "CONTROL";

        string ringIdSubstring = String.Format("_{0}", ringThicknessMm);

        var structureSet = context.StructureSet;
        var allPtvs = structureSet.Structures.Where(s => s.DicomType.Equals("PTV")).ToList();

        context.Patient.BeginModifications();

        foreach (var ptv in allPtvs)
        {
            int maxLengthOfRingIdPrefix = maxStructureIdLength - ringIdSubstring.Length;
            string ringId = ptv.Id.Length <= maxLengthOfRingIdPrefix ? ptv.Id : ptv.Id.Remove(maxLengthOfRingIdPrefix);
            ringId += ringIdSubstring;

            if (structureSet.CanAddStructure(ringDicomType, ringId))
            {
                var ring = structureSet.AddStructure(ringDicomType, ringId);
                var ptvMarginSegmentVolume = ptv.Margin((double)ringThicknessMm);
                ring.SegmentVolume = ptvMarginSegmentVolume.And(ptv.SegmentVolume.Not());
            }

        }

    }
  }
}
