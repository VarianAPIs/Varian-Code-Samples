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

using System.Windows.Threading;
using System.Runtime.ExceptionServices;


namespace PluginTester
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            //AppDomain.CurrentDomain.FirstChanceException +=
            //    (object source, FirstChanceExceptionEventArgs e) =>
            //    {
            //        string sMsg = string.Format("Exception raised in {0}: {1}\n\nDetails:\n{2}", AppDomain.CurrentDomain.FriendlyName, e.Exception.Message,e.Exception.ToString());
            //        //ignore the varian osp exception we are getting
            //        if (!sMsg.Contains("OSP"))
            //            MessageBox.Show(sMsg);
            //    };

            AppDomain.CurrentDomain.UnhandledException +=
                (object sender, UnhandledExceptionEventArgs e) =>
                {
                    string msg = string.Format("Unhandled Exception raised in {0}: {1}\n\nDetails:\n{2}", AppDomain.CurrentDomain.FriendlyName, (e.ExceptionObject as Exception).Message, (e.ExceptionObject as Exception).ToString());
                    //ignore the varian osp exception we are getting
                    if (!msg.Contains("OSP"))
                        MessageBox.Show(msg);
                };

            try
            {
                using (VMS.TPS.Common.Model.API.Application app = VMS.TPS.Common.Model.API.Application.CreateApplication(null, null)) 
                {
                    Execute(app);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                //  MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        static void Execute(VMS.TPS.Common.Model.API.Application app)
        {
            Window window = new Window();
            MainWindow mainWindow = new MainWindow(app);
            window.Title = "UM Plugin Tester";
            window.Content = mainWindow;
            window.Width = 1200;
            window.Height = 700;
            window.ShowDialog();
        }

    }

}
