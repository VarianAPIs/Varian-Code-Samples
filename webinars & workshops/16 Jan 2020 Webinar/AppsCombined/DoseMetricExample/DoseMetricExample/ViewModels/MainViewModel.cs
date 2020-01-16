using DoseMetricExample.Events;
using DoseMetricExample.Models;
using DoseMetricExample.Views;
using DoseParameters;
using DVHPlot.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using OxyPlot.Wpf;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace DoseMetricExample.ViewModels
{
    public class MainViewModel
    {
        public DelegateCommand SaveTemplateCommand{ get; set; }
        public DelegateCommand LoadMetricsCommand { get; set; }
        public DelegateCommand PrintCommand { get; private set; }
        public MainViewModel(DoseMetricSelectionViewModel doseMetricSelectionViewModel,
            DoseMetricViewModel doseMetricViewModel,
            DVHSelectionViewModel dVHSelectionViewModel,
            DVHViewModel dVHViewModel,
            DoseParametersViewModel doseParametersViewModel,
            IEventAggregator eventAggregator)
        {
            DoseMetricViewModel = doseMetricViewModel;
            DoseMetricSelectionViewModel = doseMetricSelectionViewModel;
            DVHSelectionViewModel = dVHSelectionViewModel;
            DVHViewModel = dVHViewModel;
            DoseParametersViewModel = doseParametersViewModel;
            _eventAggregator = eventAggregator;
            SaveTemplateCommand = new DelegateCommand(OnSaveTemplate);
            LoadMetricsCommand = new DelegateCommand(OnLoadMetrics);
            PrintCommand = new DelegateCommand(OnPrint);
        }

        private void OnPrint()
        {
            FlowDocument fd = new FlowDocument()
            {
                FontSize = 10
            };
            fd.Blocks.Add(new BlockUIContainer(new DoseParametersView { DataContext = DoseParametersViewModel }));
            fd.Blocks.Add(new BlockUIContainer(new DoseMetricView { DataContext = DoseMetricViewModel }));
            Section dvhPage = new Section();//
            //dvhPage.BreakPageBefore = true;
            BitmapSource bmp = new PngExporter().ExportToBitmap(DVHViewModel.DVHPlotModel);
            dvhPage.Blocks.Add(new BlockUIContainer(new System.Windows.Controls.Image { Source = bmp, Height = 950, Width = 700 }));
            fd.Blocks.Add(dvhPage);

            PrintDialog printer = new PrintDialog();
            fd.PageHeight = 1056;
            fd.PageWidth = 816;
            fd.PagePadding = new Thickness(50);
            fd.ColumnGap = 0;
            fd.ColumnWidth = 816;
            IDocumentPaginatorSource dps = fd;
            if (printer.ShowDialog() == true)
            {
                printer.PrintDocument(dps.DocumentPaginator, "Sample Report");
            }
        }

        private void OnLoadMetrics()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON (*.json)|*.json";
            if (ofd.ShowDialog() == true)
            {
                foreach (var dm in JsonConvert.DeserializeObject<List<DoseMetricModel>>(File.ReadAllText(ofd.FileName)))
                {
                    _eventAggregator.GetEvent<AddDoseMetricEvent>().Publish(dm);
                }
            }
        }

        private void OnSaveTemplate()
        {
            if (DoseMetricViewModel.DoseMetrics.Count() > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "JSON (*.json)|*.json";
                if (sfd.ShowDialog() == true)
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        sw.Write(JsonConvert.SerializeObject(DoseMetricViewModel.DoseMetrics));
                    }
                }
            }
        }

        public DoseMetricViewModel DoseMetricViewModel { get; }
        public DoseMetricSelectionViewModel DoseMetricSelectionViewModel { get; }
        public DVHSelectionViewModel DVHSelectionViewModel { get; }
        public DVHViewModel DVHViewModel { get; }
        public DoseParametersViewModel DoseParametersViewModel { get; }

        private IEventAggregator _eventAggregator;

    }
}
