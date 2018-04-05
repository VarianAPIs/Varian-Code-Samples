
//////////////////////////////////////////////////////////////////////////////////////
// PrescriptionDialog.xaml.cs
//
// Code behind for the prescription dialog. 
//  
// Copyright (c) 2015 Varian Medical Systems, Inc.
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
//////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
  /// <summary>
  /// Interaction logic for PrescriptionDialog.xaml
  /// </summary>
  public partial class PrescriptionDialog : Window
  {
    private int m_numberOfFractions;
    public int? NumberOfFractions 
    {
      get { return m_numberOfFractions; }
      set
      {
        if (value.HasValue)
        {
          m_numberOfFractions = value.Value;
        }
      } 
    }

    private double m_dosePerFraction;
    public double? DosePerFraction
    {
      get { return m_dosePerFraction; }
      set
      {
        if (value.HasValue)
        {
          m_dosePerFraction = value.Value;
        }
      }
    }

    private double m_ptvMargin;
    public double? PTVMargin
    {
      get { return m_ptvMargin; }
      set
      {
        if (value.HasValue)
        {
          m_ptvMargin = value.Value;
        }
      }
    }

    public string PatientId { get; private set; }
    public List<string> Structures { get; private set; }
    public string SelectedStructure { get; private set; }

    public PrescriptionDialog(string patientId, double dosePerFraction, int numberOfFractions, double? marginForPTVInMM = null, IEnumerable<Structure> structures = null)
    {
      PatientId = patientId;
      m_dosePerFraction = dosePerFraction;
      m_numberOfFractions = numberOfFractions;
      m_ptvMargin = marginForPTVInMM.HasValue ? marginForPTVInMM.Value : 0;
      Structures = structures != null ? structures.Select(st => st.Id).ToList() : new List<string>(); 
      Structures.Sort();
      InitializeComponent();
    }

    private void OnItemSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
      SelectedStructure = m_structureSelection.SelectedItem.ToString();
      if (SelectedStructure != string.Empty)
      {
        m_okButton.IsEnabled = true;
      }
    }

    private void OnOkButtonClick(object sender, RoutedEventArgs e)
    {
      Close();
    }
  }
}
