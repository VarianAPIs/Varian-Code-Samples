using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using V = VMS.TPS.Common.Model.API;

namespace Uab.VMS.Console
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static V.Application VApp
        {
            get;
            set;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            if (VApp != null)
            {
                VApp.Dispose();
            }
        }
    }
}
