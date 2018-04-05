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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Collections.ObjectModel;

namespace RapidPlanEvaluation
{
    /// <summary>
    /// Interaction logic for SiteChooser.xaml
    /// </summary>
    public partial class SiteChooser : Window
    {
        public static readonly DependencyProperty SitesListProperty =
            DependencyProperty.Register("SitesList", typeof(List<BodySite>), typeof(SiteChooser));
        public List<BodySite> SitesList
        {
            get { return (List<BodySite>)GetValue(SitesListProperty); }
            set { SetValue(SitesListProperty, value); }
        }

        //The NTCP parameters
        public double AlphaBeta { get; private set; }
        public double LKBn { get; private set; }
        public double LKBm { get; private set; }
        public double LKBD50 { get; private set; }

        public class BodySite : BindableBase
        {
            public string Name { get; set; }
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
        }
        public SiteChooser()
        {
            InitializeComponent();

            //Read body sites from configuration file
            SitesList = new List<BodySite>();
            SitesConfiguration sitesConfig = RapidPlanEvaluation.Myconfig.GetSitesConfiguration();
            foreach (Site site in sitesConfig.Sites)
            {
                SitesList.Add(new BodySite
                {
                    Name = site.Name,
                    IsSelected = false
                });
            }
            SitesList[0].IsSelected = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            BodySite selSite = SitesList.SingleOrDefault(s => s.IsSelected);
            SitesConfiguration sitesConfig = RapidPlanEvaluation.Myconfig.GetSitesConfiguration();
            foreach (Site site in sitesConfig.Sites)
            {
                if (site.Name == selSite.Name)
                {
                    AlphaBeta = site.AlphaBeta;
                    LKBn = site.LKBn;
                    LKBm = site.LKBm;
                    LKBD50 = site.LKBd50;
                    break;
                }
            }

            DialogResult = true;
        }
    }
}
