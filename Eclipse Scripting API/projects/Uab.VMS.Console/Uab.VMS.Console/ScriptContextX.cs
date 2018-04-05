using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace Uab.VMS.Console
{
    public class ScriptContextX
    {
        private static ScriptContext instance;

        private ScriptContextX() { }

        public static ScriptContext Instance
        {
            get
            {
                return instance;
            }
            internal set
            {
                instance = value;
            }
        }
    }
}
