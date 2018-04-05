using System.Windows;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
    class Script
    {
        public void Execute(ScriptContext context)
        {
            MessageBox.Show("Hello world " + context.CurrentUser.Id);
        }
    }
}