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

    public void Execute(ScriptContext context /*, System.Windows.Window window*/)
    {
      // TODO : Add here your code that is called when the script is launched from Eclipse
        PlanSetup plan = context.PlanSetup;
        MessageBox.Show("plan id = " + plan.Id);
    }
  }
}
