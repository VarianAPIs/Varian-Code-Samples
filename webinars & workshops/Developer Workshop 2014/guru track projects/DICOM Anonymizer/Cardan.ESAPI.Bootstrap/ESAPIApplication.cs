using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace Cardan.ESAPI
{

    public class ESAPIApplication
    {

        private static readonly Lazy<ESAPIApplication> _instance = new Lazy<ESAPIApplication>(() => new ESAPIApplication());

        // private to prevent direct instantiation.

        private ESAPIApplication()
        {
            Context = Application.CreateApplication(null, null);
        }

        public Application Context { get; private set; }

        public static bool IsLoaded { get; set; }

        //Lightweight not from Varian
        public ScriptContext ScriptContext { get; set; }


        // accessor for instance

        public static ESAPIApplication Instance
        {

            get
            {
                IsLoaded = true;

                return _instance.Value;

            }

        }



        public static void Dispose()
        {

            if (IsLoaded) { Instance.Context.Dispose(); }

        }

    }

}

