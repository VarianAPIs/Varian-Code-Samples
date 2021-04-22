using BeamDataVisualization.Events;
using BeamDataVisualization.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace BeamDataVisualization.ViewModels
{
    public class ScanPlotViewModel:BindableBase
    {
            private bool _bScanType;

            public bool bScanType
            {
                get { return _bScanType; }
                set { SetProperty(ref _bScanType, value); SetPlotModels(); }
            }

            private IEventAggregator _eventAggregator;
            private bool _bFieldSize;

            public bool bFieldSize
            {
                get { return _bFieldSize; }
                set { SetProperty(ref _bFieldSize, value); SetPlotModels(); }
            }
            public ObservableCollection<PlotModel> ScanPlotModels { get; private set; }
            public List<BeamScanModel> BeamScans { get; private set; }

            public ScanPlotViewModel(IEventAggregator eventAggregator)
            {
                ScanPlotModels = new ObservableCollection<PlotModel>();
                BeamScans = new List<BeamScanModel>();
                bScanType = true;
                _eventAggregator = eventAggregator;
                _eventAggregator.GetEvent<PlanSelectedEvent>().Subscribe(PlanSelected);
            }
            private void PlanSelected(PlanSetup plan)
            {
                BeamScans.Clear();
                var shift = Convert.ToDouble(ConfigurationManager.AppSettings["PhantomThickness"]) / 2.0;
                foreach (var beam in plan.Beams)
                {
                    var fieldX = (beam.ControlPoints.First().JawPositions.X2 - beam.ControlPoints.First().JawPositions.X1) / 10.0;
                    var fieldY = (beam.ControlPoints.First().JawPositions.Y2 - beam.ControlPoints.First().JawPositions.Y1) / 10.0;
                    var depths = new double[] { Convert.ToDouble(ConfigurationManager.AppSettings[beam.EnergyModeDisplayName]), 5, 10, 20, 30 };
                    foreach (var depth in depths)
                    {
                        // crossline profiles

                        VVector start = new VVector((beam.ControlPoints.First().JawPositions.X1 - 50) * (1000.0 + depth * 10.0) / 1000.0, (depth - shift) * 10.0, 0.0);
                        VVector end = new VVector((beam.ControlPoints.First().JawPositions.X2 + 50) * (1000.0 + depth * 10.0) / 1000.0, (depth - shift) * 10.0, 0.0);
                        var profile = beam.Dose.GetDoseProfile(start, end, new double[Convert.ToInt32(end.x - start.x) + 1]);
                        var beamscan = new BeamScanModel
                        {
                            Energy = beam.EnergyModeDisplayName,
                            BeamScanType = BeamScanTypeEnum.CrosslineProfile,
                            Depth = depth,
                            FieldX = fieldX,
                            FieldY = fieldY,
                            DisplayTxt = $"Crossline {fieldX:F0}*{fieldY:0} - {depth:F1}"
                        };
                        foreach (var point in profile)
                        {
                            beamscan.BeamDataPoints.Add(new BeamDataPointModel
                            {
                                Position = point.Position.x / 10.0,
                                DoseValue = point.Value
                            });
                        }
                        BeamScans.Add(beamscan);
                        // diagonal profiles
                        var diag_pos = Math.Sqrt(Math.Pow(beam.ControlPoints.First().JawPositions.X1 - 50, 2) + Math.Pow(beam.ControlPoints.First().JawPositions.Y1 - 50, 2)) * (1000.0 + depth * 10.0) / 1000.0;
                        VVector start_diag = new VVector(-1.0 * diag_pos, (depth - shift) * 10.0, -1.0 * diag_pos);
                        VVector end_diag = new VVector(diag_pos, (depth - shift) * 10.0, diag_pos);
                        var diagonal = beam.Dose.GetDoseProfile(start_diag, end_diag, new double[Convert.ToInt32(end_diag.x - start_diag.x) + 1]);
                        var diagScan = new BeamScanModel
                        {
                            Energy = beam.EnergyModeDisplayName,
                            BeamScanType = BeamScanTypeEnum.DiagonalProfile,
                            Depth = depth,
                            FieldX = fieldX,
                            FieldY = fieldY,
                            DisplayTxt = $"Diagonal {fieldX:F0}*{fieldY:F0} - {depth:F1}"
                        };
                        foreach (var point in diagonal)
                        {
                            diagScan.BeamDataPoints.Add(new BeamDataPointModel
                            {
                                Position = point.Position.x < 0 ? -1.0 * Math.Sqrt(Math.Pow(point.Position.x, 2) + Math.Pow(point.Position.z, 2)) / 10.0 :
                                Math.Sqrt(Math.Pow(point.Position.x, 2) + Math.Pow(point.Position.z, 2)) / 10.0,
                                DoseValue = point.Value
                            });
                        }
                        BeamScans.Add(diagScan);

                    }
                    // pdds
                    VVector start_dd = new VVector(0.0, -10.0 * shift, 0.0);//surface
                    VVector end_dd = new VVector(0.0, -10 * shift + 300.0, 0.0);
                    var pdd = beam.Dose.GetDoseProfile(start_dd, end_dd, new double[Convert.ToInt32(end_dd.y - start_dd.y) + 1]);
                    var pdd_scan = new BeamScanModel
                    {
                        Energy = beam.EnergyModeDisplayName,
                        BeamScanType = BeamScanTypeEnum.DepthDose,
                        FieldX = fieldX,
                        FieldY = fieldY,
                        DisplayTxt = $"PDD {fieldX:F0}*{fieldY:F0}"
                    };
                    foreach (var point in pdd)
                    {
                        pdd_scan.BeamDataPoints.Add(new BeamDataPointModel
                        {
                            Position = point.Position.y / 10.0 + shift,
                            DoseValue = point.Value
                        });
                    }
                    BeamScans.Add(pdd_scan);
                }
                SetPlotModels();
            }

            private void SetPlotModels()
            {
                if (BeamScans.Count() > 0)
                {
                    ScanPlotModels.Clear();

                    if (bFieldSize && bScanType)
                    {
                        foreach (var plot in BeamScans.GroupBy(x => x.BeamScanType))
                        {
                            foreach (var grouped_plot in plot.GroupBy(x => x.FieldX))
                            {
                                PlotModel plotModel = new PlotModel
                                {
                                    Title = $"{plot.Key.ToString()} at {grouped_plot.Key:F1}cm",
                                    LegendPosition = LegendPosition.RightTop,
                                    LegendPlacement = LegendPlacement.Outside
                                };
                                plotModel.Axes.Add(new LinearAxis
                                {
                                    Title = GetXAxisTitleFromScan(plot.Key),
                                    Position = AxisPosition.Bottom
                                });
                                plotModel.Axes.Add(new LinearAxis
                                {
                                    Title = "Dose [%]",
                                    Position = AxisPosition.Left
                                });
                                foreach (var scan in grouped_plot)
                                {
                                    double norm_factor = GetNormFromScan(scan);
                                    var series = new LineSeries();
                                    foreach (var point in scan.BeamDataPoints)
                                    {
                                        series.Points.Add(new DataPoint(point.Position, point.DoseValue / norm_factor * 100.0));
                                    }
                                    plotModel.Series.Add(series);

                                }
                                ScanPlotModels.Add(plotModel);
                                //plotModel.InvalidatePlot(true);
                            }
                        }
                    }
                    else if (bScanType)
                    {
                        foreach (var plot in BeamScans.GroupBy(x => x.BeamScanType))
                        {
                            PlotModel plotModel = new PlotModel
                            {
                                Title = $"{plot.Key.ToString()}s",
                                LegendPosition = LegendPosition.RightTop,
                                LegendPlacement = LegendPlacement.Outside
                            };
                            plotModel.Axes.Add(new LinearAxis
                            {
                                Title = GetXAxisTitleFromScan(plot.Key),
                                Position = AxisPosition.Bottom
                            });
                            plotModel.Axes.Add(new LinearAxis
                            {
                                Title = "Dose [%]",
                                Position = AxisPosition.Left
                            });
                            foreach (var scan in plot)
                            {
                                double norm_factor = GetNormFromScan(scan);
                                var series = new LineSeries();
                                foreach (var point in scan.BeamDataPoints)
                                {
                                    series.Points.Add(new DataPoint(point.Position, point.DoseValue / norm_factor * 100.0));
                                }
                                plotModel.Series.Add(series);

                            }
                            ScanPlotModels.Add(plotModel);
                            //plotModel.InvalidatePlot(true);
                        }
                    }
                    else if (bFieldSize)
                    {
                        foreach (var plot in BeamScans.GroupBy(x => x.FieldX))
                        {
                            PlotModel plotModel = new PlotModel
                            {
                                Title = $"Scans at {plot.Key:F1}cm",
                                LegendPosition = LegendPosition.RightTop,
                                LegendPlacement = LegendPlacement.Outside
                            };
                            plotModel.Axes.Add(new LinearAxis
                            {
                                Title = "Position [cm]",
                                Position = AxisPosition.Bottom
                            });
                            plotModel.Axes.Add(new LinearAxis
                            {
                                Title = "Dose [%]",
                                Position = AxisPosition.Left
                            });
                            foreach (var scan in plot)
                            {
                                double norm_factor = GetNormFromScan(scan);
                                var series = new LineSeries();
                                foreach (var point in scan.BeamDataPoints)
                                {
                                    series.Points.Add(new DataPoint(point.Position, point.DoseValue / norm_factor * 100.0));
                                }
                                plotModel.Series.Add(series);

                            }
                            ScanPlotModels.Add(plotModel);
                            //plotModel.InvalidatePlot(true);
                        }
                    }
                    else
                    {
                        //all scans get their own plot.
                        foreach (var scan in BeamScans.OrderBy(x => x.BeamScanType).ThenBy(x => x.FieldX))
                        {
                            PlotModel plotModel = new PlotModel
                            {
                                Title = scan.DisplayTxt,
                                LegendPosition = LegendPosition.RightTop,
                                LegendPlacement = LegendPlacement.Outside
                            };
                            plotModel.Axes.Add(new LinearAxis
                            {
                                Title = GetXAxisTitleFromScan(scan.BeamScanType),
                                Position = AxisPosition.Bottom
                            });
                            plotModel.Axes.Add(new LinearAxis
                            {
                                Title = "Dose [%]",
                                Position = AxisPosition.Left
                            });
                            double norm_factor = GetNormFromScan(scan);
                            var series = new LineSeries();
                            foreach (var point in scan.BeamDataPoints)
                            {
                                series.Points.Add(new DataPoint(point.Position, point.DoseValue / norm_factor * 100.0));
                            }
                            plotModel.Series.Add(series);
                            ScanPlotModels.Add(plotModel);
                            //plotModel.InvalidatePlot(true);
                        }

                    }
                }
            }

            private double GetNormFromScan(BeamScanModel scan)
            {
                if (scan.BeamScanType == BeamScanTypeEnum.DepthDose)
                {
                    return scan.BeamDataPoints.Max(x => x.DoseValue);
                }
                else
                {
                    return scan.BeamDataPoints.FirstOrDefault(x => x.Position >= 0).DoseValue;
                }
            }

            private string GetXAxisTitleFromScan(BeamScanTypeEnum beamScanType)
            {
                switch (beamScanType)
                {
                    case BeamScanTypeEnum.CrosslineProfile:
                        return $"Position X [cm]";
                    case BeamScanTypeEnum.DepthDose:
                        return "Postion Z [cm]";
                    case BeamScanTypeEnum.DiagonalProfile:
                        return "Position [cm]";
                    case BeamScanTypeEnum.InlineProfile:
                        return "Position Y [cm]";
                    default:
                        return "Unknown [cm]";
                }
            }
        }
}
