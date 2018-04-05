////////////////////////////////////////////////////////////////////////////////
// Patient summary
//
//  A ESAPI v11+ script
//
// Kata Intermediate.3 - Write a plug-in script that will go through each of 
//  the courses and display the plans, prescriptions, and status of the plans 
//  (like a “treatment history” report).
//
//
// Applies to:
//      Eclipse Scripting API
//          11, 13.6, 13.7, 15.0,15.1
//
// Copyright (c) 2016 Varian Medical Systems, Inc.
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
using System.Windows;
using PatientSummary;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class Script
    {
        public void Execute(ScriptContext scriptContext, Window mainWindow)
        {
            var mainViewModel = new MainViewModel(scriptContext.Patient);
            var mainView = new MainView(mainViewModel);
            mainWindow.Content = mainView;
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mainWindow.Width = 600;
            mainWindow.Height = 800;
        }
    }
}
