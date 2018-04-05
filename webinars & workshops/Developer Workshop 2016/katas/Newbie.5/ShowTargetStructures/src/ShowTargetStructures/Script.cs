////////////////////////////////////////////////////////////////////////////////
// ShowTargetStructures
//
//  A ESAPI v11+ script
//
// Kata newbie.5)	
//  Newbie.5 - Write a plug-in script that shows the target structures for 
//   each opened plan or plan sum.
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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    public class Script
    {
        public void Execute(ScriptContext scriptContext)
        {
            MessageBox.Show(GetAndFormatTargets(GetPlanningItems(scriptContext)));
        }

        private IEnumerable<PlanningItem> GetPlanningItems(ScriptContext scriptContext)
        {
            var plans = scriptContext.PlansInScope;
            var planSums = scriptContext.PlanSumsInScope;

            if (plans != null)
            {
                if (planSums != null)
                {
                    return plans.Concat<PlanningItem>(planSums);
                }
                else
                {
                    return plans;
                }
            }
            else
            {
                if (planSums != null)
                {
                    return planSums;
                }
                else
                {
                    return Enumerable.Empty<PlanningItem>();
                }
            }
        }

        private string GetAndFormatTargets(IEnumerable<PlanningItem> planningItems)
        {
            return string.Join("\n", planningItems.Select(GetAndFormatTargets));
        }

        private string GetAndFormatTargets(PlanningItem planningItem)
        {
            return planningItem.Id + ": " + GetAndFormatTargetsOnly(planningItem);
        }

        private string GetAndFormatTargetsOnly(PlanningItem planningItem)
        {
            return FormatTargets(GetTargets(GetStructureSet(planningItem)));
        }

        private StructureSet GetStructureSet(PlanningItem planningItem)
        {
            if (planningItem is PlanSetup)
            {
                return ((PlanSetup)planningItem).StructureSet;
            }
            else if (planningItem is PlanSum)
            {
                return ((PlanSum)planningItem).StructureSet;
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<Structure> GetTargets(StructureSet structureSet)
        {
            if (structureSet != null && structureSet.Structures != null)
            {
                return structureSet.Structures.Where(IsTarget);
            }
            else
            {
                return Enumerable.Empty<Structure>();
            }
        }

        private bool IsTarget(Structure structure)
        {
            return structure.DicomType == "PTV" ||
                   structure.DicomType == "CTV" ||
                   structure.DicomType == "GTV";
        }

        private string FormatTargets(IEnumerable<Structure> targets)
        {
            return targets.Any() ? string.Join(", ", targets.Select(t => t.Id)) : "No targets.";
        }
    }
}
