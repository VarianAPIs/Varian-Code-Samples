//using DICOMTest;

using Microsoft.Practices.Prism.Mvvm;
using Uab.VMS.Console.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
//using Uab.RO.ESAPIX.Proxies;
using i = System.Windows.Interactivity;
using x = Microsoft.Expression.Interactivity;
using p = Microsoft.Practices.Prism.Interactivity;
using Uab.VMS.Console;
using System.Reflection;
using UAB;

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        public void Execute(VMS.TPS.Common.Model.API.ScriptContext context, System.Windows.Window window)
        {
           // XamlAssemblyLoader.LoadAssemblies();
            //var vm = new ConsoleViewModel();

            //new Splash().ShowDialog();

            ScriptContextX.Instance = context;

            var con = new Uab.VMS.Console.Views.Console();
            //con.DataContext = vm;
            con.ShowDialog();

            if (window != null)
            {
                window.Loaded += (sender, e) =>
                {
                    window.Close();
                };
            }
        }

        //Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //{
        //    var name = new AssemblyName(args.Name).Name;
        //    var res = "Uab.VMS.Console.dlls." + name + ".dll";

        //    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res))
        //    {
        //        if (stream != null)
        //        {
        //            using (stream)
        //            {
        //                byte[] data = new byte[stream.Length];
        //                stream.Read(data, 0, data.Length);
        //                return Assembly.Load(data);
        //            }
        //        }
        //    }
        //    if (res.Contains("CodeAnalysis"))
        //        MessageBox.Show("Couldn't find " + res);
        //    return null;
        //}


        public class XamlAssemblyLoader
        {
            //This method exists as a hack to get the XAML assebmlies loaded in the plugin
            public static void LoadAssemblies()
            {
                var av = new ICSharpCode.AvalonEdit.TextEditor();
                var ac = new p.InvokeCommandAction();
                i.InvokeCommandAction action = new i.InvokeCommandAction();
                action.CommandName = "Loaded";
                var window = new Window();
                x.VisualStateUtilities.GetVisualStateGroups(window);
                ViewModelLocator.SetAutoWireViewModel(window, false);
            }
        }
    }
}


