using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM = VMS.TPS.Common.Model.API;
using D = VMS.TPS.Common.Model.Types.DoseValuePresentation;
using V = VMS.TPS.Common.Model.Types.VolumePresentation;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ESAPIX.Extensions;

namespace DiggingIntoDVH
{
    class PlottingExample
    {
        public static void PlotDVH()
        {
            using (var app = VM.Application.CreateApplication())
            {
                var patient = app.OpenPatientById("DA00005");
                var allPlans = patient.Courses.SelectMany(c => c.PlanSetups);
                var plan = allPlans.FirstOrDefault(p => p.Id == "ABD ARC2");
                var plot = CreatePlot(plan);
                var view = new PlotView();
                view.DataContext = plot;
                view.ShowDialog();
            }
        }

        private static PlotModel CreatePlot(VM.PlanSetup plan)
        {
            var model = new PlotModel();
            //Add Dose Axis
            model.Axes.Add(new LinearAxis()
            {
                Title = "Dose [cGy]",
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MinorGridlineThickness = 1,
                MinorGridlineColor = OxyColor.FromRgb(15, 15, 15),
            });
            //Add Volume Axis
            model.Axes.Add(new LinearAxis()
            {
                Title = "Volume [%]",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MinorGridlineColor = OxyColor.FromRgb(15, 15, 15)
            });

            foreach (var str in plan.StructureSet.Structures)
            {
                var ser = new LineSeries();
                ser.Color = OxyColor.FromRgb(str.Color.R, str.Color.G, str.Color.B);
                foreach (var pt in plan.GetDVHCumulativeData(str, D.Absolute, V.Relative, 0.1).CurveData)
                {
                    ser.Points.Add(new DataPoint(pt.DoseValue.GetDoseCGy(), pt.Volume));
                }
                model.Series.Add(ser);
            }

            return model;
        }
    }
}
