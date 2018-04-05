using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DICOMUI
{
	/// <summary>
	/// Interaction logic for Splash.xaml
	/// </summary>
	public partial class Splash : Window
	{
        Storyboard Showboard2;
        Thread loadingThread;
        private delegate void ShowDelegate();
        ShowDelegate loadme;

        public Splash()
        {
            this.InitializeComponent();
            Showboard2 = this.Resources["Storyboard2"] as Storyboard;
            Showboard2.Seek(new TimeSpan(0));

            Loaded += Window_Loaded;
            loadme = new ShowDelegate(Animate);
        }

        public void Animate()
        {
            BeginStoryboard(Showboard2);
            Timer t = new Timer((ti) => { this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { Close(); }); }, null, 2000, Timeout.Infinite);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingThread = new Thread(Load);
            loadingThread.Start();
        }

        private void Load()
        {
            this.Dispatcher.Invoke(loadme);
        }

        public void ShowError(Exception e)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate() { Close(); });
        }
	}
}