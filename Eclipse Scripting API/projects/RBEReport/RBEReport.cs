////////////////////////////////////////////////////////////////////////////////
// RBEReport.cs
//
//  A ESAPI v13 Script that generates a Radiobiological Effect report
//  from a plan setup or a plan sum.
//
//  See the article "Radiobiological Effect report and Aria Document post"
//  for details.
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
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context, System.Windows.Window window)
    {
      if (context.PlanSetup == null && context.PlanSumsInScope == null)
      {
        throw new ApplicationException("No active plan");
      }
      if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
      {
        throw new ApplicationException("Please close other plan sums");
      }
      List<PlanningItem> PItemsInScope = new List<PlanningItem>();
      foreach (var pitem in context.PlansInScope)
        PItemsInScope.Add(pitem);
      foreach (var pitem in context.PlanSumsInScope)
        PItemsInScope.Add(pitem);

      PlanningItem openedPItem = null;
      if (context.PlanSetup != null)
        openedPItem = context.PlanSetup;
      else
        openedPItem = context.PlanSumsInScope.First();

      Start(context.Patient, PItemsInScope, openedPItem, context.Course, context.StructureSet, context.CurrentUser, window);
    }

    /// <summary>
    /// Starts execution of script. This method can be called directly from PluginTester or indirectly from Eclipse
    /// through the Execute method.
    /// </summary>
    /// <param name="patient">Opened patient</param>
    /// <param name="PItemsInScope">Planning Items in scope</param>
    /// <param name="pItem">Opened Planning Item</param>
    /// <param name="currentUser">Current user</param>
    /// <param name="window">WPF window</param>
    public static void Start(Patient pat, List<PlanningItem> PItemsInScope, PlanningItem pItem, Course course, StructureSet ss, User currentUser, Window window)
    {
      if (pat == null || pItem == null)
      {
        throw new ApplicationException("Please open a plan or a plan sum before running the script");
      }

      window.Title = "Radiobiological Effect Window";
      var uc = new RBEReport.UserControl1();
      window.Content = uc;

      uc.VM.PatientId = pat.Id;
      uc.VM.PatientName = pat.FirstName + " " + pat.LastName;
      if (pat.DateOfBirth != null)
      {
        uc.VM.DOB = pat.DateOfBirth.GetValueOrDefault().Date.ToShortDateString();
      }
      if (course != null)
      {
        uc.VM.CourseId = course.Id;
      }
      else
      {
        uc.VM.CourseId = GetPlanSumCourse(pat, pItem as PlanSum);
      }
      uc.VM.PlanId = pItem.Id;
      if (pItem is PlanSetup)
      {
        uc.VM.Approval = (pItem as PlanSetup).ApprovalStatus.ToString();
      }
      else
      {
        uc.VM.Approval = string.Empty; //if nothing is set (Approval == null), MigraDoc crashes.
      }
      uc.VM.Modification = pItem.HistoryUserName;
      uc.VM.Date = pItem.HistoryDateTime.ToString();

      if (pItem is PlanSetup)
      {
        uc.VM.AddPlanningItem(pItem as PlanSetup);
      }
      else
      {
        PlanSum sum = pItem as PlanSum;
        foreach (PlanSetup ps in sum.PlanSetups)
        {
          uc.VM.AddPlanningItem(ps);
        }
      }
    }

    static string GetPlanSumCourse(Patient pat, PlanningItem item)
    {
      PlanSum sum = item as PlanSum;
      foreach (Course cour in pat.Courses)
      {
        foreach (PlanSum ps in cour.PlanSums)
        {
          if (ps == sum)
          {
            return cour.Id;
          }
        }
      }
      return "";
    }
  }
}

