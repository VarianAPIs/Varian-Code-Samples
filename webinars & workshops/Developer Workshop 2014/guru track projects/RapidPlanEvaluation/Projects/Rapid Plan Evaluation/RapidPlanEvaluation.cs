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
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using RapidPlanEvaluation;

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
            if (context.PlanSumsInScope.Count() > 0)
            {
                MessageBox.Show("This script currently doesn't support Plan Sums. \nPlease close them and run the script again", "Input Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<PlanSetup> plansInScope = new List<PlanSetup>();
            foreach (var pitem in context.PlansInScope)
                plansInScope.Add(pitem);

            PlanSetup plan = context.PlanSetup;
            if (plan == null)
                throw new ApplicationException("Please open a plan");

            Start(context.Patient, context.Course, plansInScope, plan, context.CurrentUser, window);
        }

        /// <summary>
        /// Starts execution of script. This method can be called directly from PluginTester or indirectly from Eclipse
        /// through the Execute method.
        /// </summary>
        /// <param name="patient">Opened patient</param>
        /// <param name="plansInScope">Planning Items in scope</param>
        /// <param name="pItem">Opened Planning Item</param>
        /// <param name="currentUser">Current user</param>
        /// <param name="window">WPF window</param>
        public static void Start(Patient patient, Course course, List<PlanSetup> plansInScope, PlanSetup openedPlan, User currentUser, Window window)
        {
            try
            {
                if (plansInScope.Count != 2)
                    throw new ApplicationException("You need to have two plans opened");

                PlanSetup stdPlan = null;
                PlanSetup rapidPlan = null;
                // check if all planItems have dose
                foreach (PlanSetup plan in plansInScope)
                {
                    if (plan.Dose == null)
                    {
                        MessageBox.Show("Plan '" + plan.Id + "' does not contain a valid dose.\n Please close this plan and restart the script",
                        "Invalud Plan", MessageBoxButton.OK, MessageBoxImage.Error);
                        window.Close();
                        return;
                    }

                    if (plan.DVHEstimates.Count() == 0)
                    {
                        //assume this is the standard plan
                        stdPlan = plan;
                    }
                    else
                    {
                        //the rapidPlan
                        rapidPlan = plan;
                    }
                }

                window.Title = "Rapid Plan Evaluation";

                // get reference to selected plan
                MainControl main = new MainControl();
                main.patient = patient;
                main.user = currentUser;
                main.Course = course;
                main.StructureSet = openedPlan.StructureSet;
                main.StdPlan = stdPlan;
                main.RapidPlan = rapidPlan;
                //main.PItemsInScope = PItemsInScope;

                var dockPanel = new System.Windows.Controls.DockPanel();
                dockPanel.Children.Add(main);
                window.Width = 1100;
                window.Content = dockPanel;
                window.Closing += WindowClosingHandler;


            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public static void WindowClosingHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
           // DVHDataModel.Instance.Clear();
        }
    }
}
