////////////////////////////////////////////////////////////////////////////////
// CreateOptStructures.cs
//
//  A ESAPI v15.1+ script that demonstrates optimization structure creation.
//
// Applies to:
//      Eclipse Scripting API
//          15.1.1
//          15.5
//
// Copyright (c) 2017-2018 Varian Medical Systems, Inc.
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
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        // Change these IDs to match your clinical conventions
        const string PTV_ID = "PTV";
        const string RECTUM_ID = "Rectum";
        const string EXPANDED_PTV_ID = "PTV+5mm";
        const string RECTUM_OPT_ID = "RectumOpt5mm";
        const string SCRIPT_NAME = "Opt Structures Script";

        public Script()
        {
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            if (context.Patient == null || context.StructureSet == null)
            {
                MessageBox.Show("Please load a patient, 3D image, and structure set before running this script.", SCRIPT_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            StructureSet ss = context.StructureSet;

            // find Rectum
            Structure rectum = ss.Structures.FirstOrDefault(x => x.Id == RECTUM_ID);
            if (rectum == null)
            {
                MessageBox.Show(string.Format("'{0}' not found!", RECTUM_ID), SCRIPT_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // find PTV
            Structure ptv = ss.Structures.FirstOrDefault(x => x.Id == PTV_ID);
            if (ptv == null)
            {
                MessageBox.Show(string.Format("'{0}' not found!", PTV_ID), SCRIPT_NAME, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            context.Patient.BeginModifications();   // enable writing with this script.

            //============================
            // GENERATE 5mm expansion of PTV
            //============================

            // create the empty "ptv+5mm" structure
            Structure ptv_5mm = ss.AddStructure("PTV", EXPANDED_PTV_ID);

            // expand PTV
            ptv_5mm.SegmentVolume = ptv.Margin(5.0);

            //============================
            // subtract rectum from expansion to create 5mm buffer
            //============================
            Structure buffered_rectum = ss.AddStructure("AVOIDANCE", RECTUM_OPT_ID);

            // calculate overlap structures using Boolean operators.
            buffered_rectum.SegmentVolume = rectum.Sub(ptv_5mm); //'Sub' subtracts overlapping volume of expanded PTV from rectum

            string message = string.Format("{0} volume = {4}\n{1} volume = {5}\n{2} volume = {6}\n{3} volume = {7}",
                    ptv.Id, rectum.Id, ptv_5mm.Id, buffered_rectum.Id,
                    ptv.Volume, rectum.Volume, ptv_5mm.Volume, buffered_rectum.Volume);
            MessageBox.Show(message);

            ss.RemoveStructure(ptv_5mm);
        }
    }
}

