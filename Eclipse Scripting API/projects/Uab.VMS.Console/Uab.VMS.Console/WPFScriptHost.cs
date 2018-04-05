using ScriptCs;
using ScriptCs.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Uab.RO.ESAPIX.Proxies;
using V = VMS.TPS.Common.Model.API;

namespace Uab.VMS.Console
{
    public class WPFScriptHost : ScriptHost
    {
        public WPFConsoleRelay Console { get; set; }
        public WPFScriptHost(IScriptPackManager manager, ScriptEnvironment evn)
            : base(manager, evn)
        {
            Console = new WPFConsoleRelay();
            this.Application = new Application();
        }

        public V.ScriptContext Context
        {
            get
            {
                return ScriptContextX.Instance;
            }
        }

        public string Password { get { return ""; } }

        public Application Application { get; private set; }
    }


    public class Application
    {
        public V.Application CreateApplication(string username, string password)
        {
            var vapp = V.Application.CreateApplication(username, password);
            App.VApp = vapp;
            return vapp;
        }
    }
}
