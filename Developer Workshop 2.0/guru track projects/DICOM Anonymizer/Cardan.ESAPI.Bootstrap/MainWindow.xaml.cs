using Cardan.ESAPI.Bootstrap.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using V = VMS.TPS.Common.Model.API;

namespace Cardan.ESAPI.Bootstrap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow() 
        {
            InitializeComponent();
            var esapi = ESAPIApplication.Instance;
            this.ContentRendered+=MainWindow_ContentRendered;
            esapi.ScriptContext = new V.ScriptContext(null, null);
            FillScript(esapi.ScriptContext, ESAPIApplication.Instance.Context);
            new Script().Execute(esapi.ScriptContext, this);
        }

        private void FillScript(V.ScriptContext context, V.Application app)
        {
            typeof(V.ScriptContext)
             .GetField("m_patient", BindingFlags.Instance | BindingFlags.NonPublic)
             .SetValue(ESAPIApplication.Instance.ScriptContext, null);
        }


        void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            if (this.Content is Frame)
            {
                var frame = this.Content as Frame;
                var stackPanel = new StackPanel();
                this.Content = stackPanel;
                var selectPat = new SelectPatient();
                var selectPatContent = (FrameworkElement)selectPat.Content;
                selectPatContent.DataContext = selectPat;
                selectPat.Content = null;
                stackPanel.Children.Add(selectPatContent);
                stackPanel.Children.Add(frame);
                this.Content = stackPanel;
            }   
        }
    }
}
