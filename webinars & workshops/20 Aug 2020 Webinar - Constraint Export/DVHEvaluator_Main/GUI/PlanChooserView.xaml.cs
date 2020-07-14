using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DVHEvaluator_Main
{
    /// <summary>
    /// Interaction logic for View.xaml
    /// </summary>
    public partial class PlanChooserView : UserControl
    {
        public PlanChooserViewModel ViewModel { get; set; }

        public PlanChooserView(PlanChooserViewModel viewModel)
        {
            InitializeMaterialDesign();
            InitializeComponent();
            ViewModel = viewModel;
            base.DataContext = ViewModel;
        }

        private void InitializeMaterialDesign()
        {
            // Create dummy objects to force the MaterialDesign assemblies to be loaded
            // from this assembly, which causes the MaterialDesign assemblies to be searched
            // relative to this assembly's path. Otherwise, the MaterialDesign assemblies
            // are searched relative to Eclipse's path, so they're not found.
            // 
            // This will copy the relevent MaterialDesign dlls to the output directory
            // However, if you want to use this as a plugin, you'll have to copy the MaterialDesign files
            // into the Eclipse directory OR install Costura.Fody from Nuget
            //   - Install Costura.Fody
            //   - Downgrade Fody (not Costura.Fody) to 4.2.1 to work with pre-2019 Visual Studio
            //   - This package will merge all resources into the one output DLL on compile.
            Chip chip = new Chip();
            Card card = new Card();
            Hue hue = new Hue("Dummy", Colors.Black, Colors.White);
        }

        private void SelectAll_chkbox_Clic(object sender, RoutedEventArgs e) { }

        private void RunReport_btn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RunReport();
        }

        private void SaveAsCSV_btn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveAsCSV();
        }

        private void ChangeEBRT_btn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ChangeEBRT_btn_Click();
        }
    }
}
