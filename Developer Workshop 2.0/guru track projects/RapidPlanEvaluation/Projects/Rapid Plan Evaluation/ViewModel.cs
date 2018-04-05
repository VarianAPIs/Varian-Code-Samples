#region copyright
////////////////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
//////////////////////////////////////////////////////////////////////////////////
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Collections.ObjectModel;

namespace RapidPlanEvaluation
{
        //The Viewmodel

        public class VMStructure: BindableBase
        {
            public string Id { get; set; }

            private ObservableCollection<VMMetric> _doseMetrics;
            public ObservableCollection<VMMetric>  DoseMetrics
            {
                get { return _doseMetrics; }
                set
                {
                    _doseMetrics = value;
                }
            }

            private ObservableCollection<double> _volume;
            public ObservableCollection<double> Volume
            {
                get { return _volume; }
                set { _volume = value; }
            }

            private ObservableCollection<string> _fieldNames;
            public ObservableCollection<string> FieldNames
            {
                get { return _fieldNames; }
                set { _fieldNames = value; }
            }

            public bool UpdatingNow { get; set; }

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }

            public bool IsTarget { get; set; }

            public Color Color { get; set; }

            private bool _useBioDose;
            public bool UseBioDose
            {
                get { return _useBioDose; }
                set
                {
                    _useBioDose = value;
                    NotifyPropertyChanged("UseBioDose");
                }
            }


            public VMStructure()
            {
                _doseMetrics = new ObservableCollection<VMMetric>();
                _volume = new ObservableCollection<double>();
                _fieldNames = new ObservableCollection<string>();
            }
        }

        public class VMMetric : BindableBase
        {
            private string _name;
            public string Name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }

            private bool _usebiodose;
            public bool UseBioDose
            {
                get { return _usebiodose; }
                set
                {
                    _usebiodose = value;
                    NotifyPropertyChanged("UseBioDose");
                }
            }

            private bool _canUseBiodose;
            public bool CanUseBiodose
            {
                get { return _canUseBiodose; }
                set
                {
                    _canUseBiodose = value;
                    NotifyPropertyChanged("CanUseBiodose");
                }
            }

            private bool _showNTCPParams;
            public bool ShowNTCPParams
            {
                get { return _showNTCPParams; }
                set
                {
                    _showNTCPParams = value;
                    NotifyPropertyChanged("ShowNTCPParams");
                }
            }

            public NTCPParams NTCPParameters { get; set; }

            private double _stdPlanMetric;
            public double StdPlanMetric
            {
                get { return _stdPlanMetric; }
                set
                {
                    _stdPlanMetric = value;
                    NotifyPropertyChanged("StdPlanMetric");
                    NotifyPropertyChanged("StdMinusEst");
                    NotifyPropertyChanged("RpMinusEst");
                    NotifyPropertyChanged("StdMinusRp");
                }
            }

            private double _rapidPlanMetric;
            public double RapidPlanMetric
            {
                get { return _rapidPlanMetric; }
                set
                {
                    _rapidPlanMetric = value;
                    NotifyPropertyChanged("RapidPlanMetric");
                    NotifyPropertyChanged("StdMinusEst");
                    NotifyPropertyChanged("RpMinusEst");
                    NotifyPropertyChanged("StdMinusRp");
                }
            }

            private double _rapidEstimateMetric;
            public double RapidPlanEstimateMetric
            {
                get { return _rapidEstimateMetric; }
                set
                {
                    _rapidEstimateMetric = value;
                    NotifyPropertyChanged("RapidPlanEstimateMetric");
                    NotifyPropertyChanged("StdMinusEst");
                    NotifyPropertyChanged("RpMinusEst");
                    NotifyPropertyChanged("StdMinusRp");
                }
            }

            public double StdMinusEst
            {
                get { return StdPlanMetric - RapidPlanEstimateMetric; }
            }

            public double RpMinusEst
            {
                get { return RapidPlanMetric - RapidPlanEstimateMetric; }
            }

            public double StdMinusRp
            {
                get { return StdPlanMetric - RapidPlanMetric; }
            }

            public void ClearMetrics()
            {
                StdPlanMetric = 0;
                RapidPlanMetric = 0;
                RapidPlanEstimateMetric = 0;
            }

            public VMMetric()
            {
                UseBioDose = false;
                NTCPParameters = new NTCPParams();
                ShowNTCPParams = false;
            }
        }

        public class NTCPParams : BindableBase
        {
            //alphabeta="2.5" lkbn="0.99" lkbm="0.37" lkbd50="30.8"
            public double _alphabeta;
            public double AlphaBeta
            {
                get { return _alphabeta; }
                set
                {
                    _alphabeta = value;
                    NotifyPropertyChanged("AlphaBeta");
                }
            }

            public double _LKBn;
            public double LKBn
            {
                get { return _LKBn; }
                set
                {
                    _LKBn = value;
                    NotifyPropertyChanged("LKBn");
                }
            }

            public double _LKBm;
            public double LKBm
            {
                get { return _LKBm; }
                set
                {
                    _LKBm = value;
                    NotifyPropertyChanged("LKBm");
                }
            }

            public double _LKBD50;
            public double LKBD50
            {
                get { return _LKBD50; }
                set
                {
                    _LKBD50 = value;
                    NotifyPropertyChanged("LKBD50");
                }
            }

        }


        public class ViewModel : BindableBase
        {
            private ObservableCollection<VMStructure> _structures = new ObservableCollection<VMStructure>();
            public ObservableCollection<VMStructure> Structures
            { get { return _structures; } }

        }

}
