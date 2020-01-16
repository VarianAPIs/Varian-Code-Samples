using DVHPlot.Events;
using DVHPlot.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace DVHPlot.ViewModels
{
    public class DVHViewModel:BindableBase
    {
        private PlanSetup _plan;
        private IEventAggregator _eventAggregator;
        public PlotModel DVHPlotModel { get; set; }

        public DVHViewModel(PlanSetup plan,
            IEventAggregator eventAggregator)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            DVHPlotModel = new PlotModel();
            SetPlotModelProeprties();
            //GetDefaultDVH();
            _eventAggregator.GetEvent<StructureSelectionEvent>().Subscribe(OnStructureSelectionChanged);
        }

        private void OnStructureSelectionChanged(StructureSelectionModel selStructure)
        {
            if (selStructure.bIsChecked)
            {
                //add to the plot model
                Structure s = _plan.StructureSet.Structures.FirstOrDefault(x => x.Id == selStructure.Id);
                DVHData dvh = _plan.GetDVHCumulativeData(s,
                    DoseValuePresentation.Absolute,
                    VolumePresentation.Relative,
                    1);
                if (dvh != null)
                {
                    GeneratePlotSeries(s, dvh);
                }
            }
            else
            {
                //remove from teh plot model
                if (DVHPlotModel.Series.FirstOrDefault(x => x.Title == selStructure.Id) != null)
                {
                    DVHPlotModel.Series.Remove(DVHPlotModel.Series.FirstOrDefault(x => x.Title == selStructure.Id));
                }
            }
            DVHPlotModel.InvalidatePlot(true);
        }

        private void SetPlotModelProeprties()
        {
            DVHPlotModel.Title = $"DVH for {_plan.Id}";
            DVHPlotModel.LegendPlacement = LegendPlacement.Outside;
            DVHPlotModel.LegendPosition = LegendPosition.RightTop;
            DVHPlotModel.Axes.Add(new LinearAxis
            {
                Title = $"Dose [{_plan.DosePerFraction.UnitAsString}]",
                Position = AxisPosition.Bottom
            });
            DVHPlotModel.Axes.Add(new LinearAxis
            {
                Title = $"Volume [%]",
                Position = AxisPosition.Left
            });
        }
        //dvh generation is controlled only by events from the StructureSelectionModel.
        //private void GetDefaultDVH()
        //{
        //    foreach(var structure in _plan.StructuresSelectedForDvh)
        //    {
        //        var dvh = _plan.GetDVHCumulativeData(structure,
        //            DoseValuePresentation.Absolute,
        //            VolumePresentation.Relative,
        //            1);
        //        if (dvh != null)
        //        {
        //            GeneratePlotSeries(structure, dvh);
        //        }
        //    }
        //}
        private void GeneratePlotSeries(Structure structure, DVHData dvh)
        {
            var dvhSeries = new LineSeries
            {
                Title = structure.Id,
                Color = OxyColor.FromArgb(structure.Color.A, structure.Color.R, structure.Color.G, structure.Color.B)
            };
            foreach(var dvhPoint in dvh.CurveData)
            {
                dvhSeries.Points.Add(new DataPoint(dvhPoint.DoseValue.Dose, dvhPoint.Volume));
            }
            DVHPlotModel.Series.Add(dvhSeries);
        }
    }
}
