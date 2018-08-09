////////////////////////////////////////////////////////////////////////////////
// Helpers.cs
//
// Helper methods to manipulate courses etc.
//  
// Applies to: ESAPI v13, v13.5, v13.6 and v15.6
//
// Copyright (c) 2018 Varian Medical Systems, Inc.
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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public static class Helpers
    {

        public static bool CheckStructures(Patient patient)
        {
            if (patient.StructureSets.Any()) return true;
            const string message = "Patient does not have any structures.";
            const string title = "Invalid patient";
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        public static Course GetCourse(Patient patient, string courseId)
        {
            var res = patient.Courses.Where(c => c.Id == courseId);
            if (res.Any())
            {
                var oldCourse = res.Single();
                patient.RemoveCourse(oldCourse);
            }
            var course = patient.AddCourse();
            course.Id = courseId;
            return course;
        }

        public static void RemoveOldPlan(Course course, string planId)
        {
            var oldPlans = course.PlanSetups.Where(plan => plan.Id == planId);
            if (oldPlans.Any())
            {
                var plansToBeRemoved = oldPlans.ToArray();
                foreach (var plan in plansToBeRemoved)
                {
                    course.RemovePlanSetup(plan);
                }
            }
        }

        public static ExternalPlanSetup FindPlanSetup(Patient patient, string courseId, string planSetupId)
        {
            var plans = new List<PlanSetup>();
            foreach (var c in patient.Courses)
            {
                if (c.Id == courseId)
                {
                    var temp = c.PlanSetups.Where(p => p.Id == planSetupId);
                    plans.AddRange(temp);
                }
            }
            //return plans.Single() as ExternalPlanSetup;
            return plans.First() as ExternalPlanSetup;
        }

        public static void RemoveStructures(StructureSet structureSet, List<string> structureIDs)
        {
            foreach (var id in structureIDs)
            {
                if (structureSet.Structures.Any(st => st.Id == id))
                {
                    var st = structureSet.Structures.Single(x => x.Id == id);
                    structureSet.RemoveStructure(st);
                }
            }
        }
    }
}
