using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Uab.VMS.Console.ViewModels;
using UAB;

namespace Uab.VMS.Console.Views 
{
    /// <summary>
    /// Interaction logic for Console.xaml
    /// </summary>
    public partial class Console : Window, IView
    {
        public Console()
        {
            InitializeComponent();
            this.DataContext =  new ConsoleViewModel();
            new Splash().ShowDialog();
        }
    }
}
