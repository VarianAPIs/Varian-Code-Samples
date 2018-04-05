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

//Sample license text.
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using PluginScriptExample;

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        /// <summary>
        /// This is the method called by Eclipse when script starts
        /// </summary>
        /// <param name="context"></param>
        /// <param name="window"></param>
        public void Execute(ScriptContext context, System.Windows.Window window)
        {
            List<PlanningItem> pItemsScope = new List<PlanningItem>();
            foreach (PlanSetup plan in context.PlansInScope)
                pItemsScope.Add(plan);
            foreach (PlanSum psum in context.PlanSumsInScope)
                pItemsScope.Add(psum);

            PlanSetup openPlan = context.PlanSetup;
            if (openPlan == null)
                throw new ApplicationException("No plan opened");

           PluginScriptExample.Main.Start(context.Patient, context.Course, pItemsScope, openPlan, context.CurrentUser, window);
        }
    }
}

namespace PluginScriptExample
{
    public class Main
    {
        /// <summary>
        /// Starts execution of script. This method can be called directly from PluginTester or indirectly from Eclipse
        /// through the Execute method.
        /// </summary>
        /// <param name="patient">Opened patient</param>
        /// <param name="PItemsInScope">Planning Items in scope</param>
        /// <param name="pItem">Opened Planning Item</param>
        /// <param name="currentUser">Current user</param>
        /// <param name="window">WPF window</param>
        public static void Start(Patient patient, Course course,List<PlanningItem> PItemsInScope, PlanSetup plan, User currentUser, Window window)
        {
            try
            {
                // get reference to selected plan

                MainControl main = new MainControl();
                main.Plan = plan;
                main.patient = patient;
                main.user = currentUser;
                main.PItemsInScope = PItemsInScope;
                main.StructureSet = plan.StructureSet;

                var dockPanel = new System.Windows.Controls.DockPanel();
                dockPanel.Children.Add(main);
                window.Width = 800;
                window.Height = 350;
                window.Content = dockPanel;

            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
