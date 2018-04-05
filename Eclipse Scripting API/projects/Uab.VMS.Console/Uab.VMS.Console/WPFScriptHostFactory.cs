using ScriptCs;
using ScriptCs.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uab.VMS.Console
{
    public class WPFScriptHostFactory : IScriptHostFactory
    {
        public IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            return new WPFScriptHost(scriptPackManager, new ScriptEnvironment(scriptArgs));
        }
    }
}
