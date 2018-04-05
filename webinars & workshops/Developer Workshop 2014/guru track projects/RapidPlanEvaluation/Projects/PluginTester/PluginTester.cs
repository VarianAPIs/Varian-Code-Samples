#region copyright
////////////////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Regents of the University of Michigan
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using System.Xml;

//-------------------------------------------------------------------------------------------
// PluginTester
//
// This application is designed to test the EclipsePlanCheck script from outside of Eclipse to allow for debugging.
// It starts a simple UI where you can choose a patient/plan.
//
// It also can run the script in batch mode for testing and developing purposes. The application accepts these parameters:
//     /test-file       loads patient list from testpatients.xml file
//     /test-db         loads active patients from Aria database
//     /version         return EclipsePlanCheck version
//
//     if no parameters are present the UI is started 
//
//-------------------------------------------------------------------------------------------
namespace PluginTester
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                using (VMS.TPS.Common.Model.API.Application app = VMS.TPS.Common.Model.API.Application.CreateApplication("allrights", "allrights"))
                {
                    Execute(app, args);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                //  MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        static void Execute(VMS.TPS.Common.Model.API.Application app, string[] args)
        {
            //check if file with patient info exists
            if (args.Count() == 1 && args[0] == "/test-file")
            {
                if (System.IO.File.Exists("testpatients.xml"))
                {
                    try
                    {
                        //--load patient list to DOM
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.Load("testpatients.xml");

                        //--run batch test
                        //runBatchTest(app, xDoc);

                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                    }
                }
            }
            else
            {
                //start UI
                RunFromUI(app);
            }
        }

   
        static void RunFromUI(VMS.TPS.Common.Model.API.Application app)
        {
            Window window = new Window();
            MainWindow mainWindow = new MainWindow(app);
            window.Title = "UM Plugin Tester";
            window.Content = mainWindow;
            window.Width = 1200;
            window.Height = 600;
            window.ShowDialog();
        }
    }

}
