using ESAPIX.Bootstrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESAPIX.AppKit;
using ESAPIX.Common;
using System.Windows.Threading;
using ESAPIX_WPF.Views;

namespace VMS.TPS
{
    public class Script : XScriptBase
    {
        public override void XExecute(PluginContext ctx, DispatcherFrame frame)
        {
            var bs = new ScriptBootstrapper<MainView>(ctx, frame);
            bs.Run();
        }
    }
}
