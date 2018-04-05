////////////////////////////////////////////////////////////////////////////////
// EUDScript.cs
//
//  A ESAPI v11+ script that demonstrates use of EUD for calculating NTCP and TCP.
//  This is a port of the Matlab script from Gay & Niemierko.
//
//  Reference: Phys Med. 2007 Dec; 23(3-4):115-25. Epub 2007 Sep 7. 
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
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;
using System.Windows;
using System.Windows.Controls;
using System.IO;


namespace VMS.TPS
{

  public class Script
  {

    #region Resources

    // Parameters for calculating DVH.
    public double BinWidth { private set; get; }
    public ExternalPlanSetup Plan { private set; get; }

    // Interface to Eclipse.
    public ScriptContext Context { private set; get; } 

    // EUD and TCP/NTCP parameters for known cases.
    private Dictionary<string, EUDparameters> m_pameterLibrary = new Dictionary<string, EUDparameters>();
    public Dictionary<string, EUDparameters> ParameterLibrary
    { private set { m_pameterLibrary = value; }
      get { return m_pameterLibrary; }
    }

    // Constructor
    public Script()
    {
      // Set bin width.
      BinWidth = 0.01;

      // Define parameters for known cases. Some values are obtained from Phys Med. 2007 Dec; 23(3-4):115-25. Epub 2007 Sep 7.
      // For the alpha/beta ratio, we have used 10Gy for all organ and tumor types.
      const double alphaToBeta = 10.0;
      ParameterLibrary.Add("Breast", new EUDparameters(-7.2, 2.0, 35.0, alphaToBeta));
      ParameterLibrary.Add("Melanoma", new EUDparameters(-10.0, 2.0, 35.0, alphaToBeta));
      ParameterLibrary.Add("Squamous cc", new EUDparameters(-13.0, 2.0, 35.0, alphaToBeta));
      ParameterLibrary.Add("Brain", new EUDparameters(5.0, 3.0, 35.0, alphaToBeta));
      ParameterLibrary.Add("Brainstem", new EUDparameters(7.0, 3.0, 65.0, alphaToBeta));
      ParameterLibrary.Add("Optic chiasm", new EUDparameters(25.0, 3.0, 65.0, alphaToBeta));
      ParameterLibrary.Add("Colon", new EUDparameters(6.0, 4.0, 55.0, alphaToBeta));
      ParameterLibrary.Add("Ear (acute otitis)", new EUDparameters(31.0, 3.0, 40.0, alphaToBeta));
      ParameterLibrary.Add("Ear (chronic otitis)", new EUDparameters(31.0, 4.0, 65.0, alphaToBeta));
      ParameterLibrary.Add("Esophagus", new EUDparameters(19.0, 4.0, 68.0, alphaToBeta));
      ParameterLibrary.Add("Heart", new EUDparameters(3.0, 3.0, 50.0, alphaToBeta));
      ParameterLibrary.Add("Kidney", new EUDparameters(1.0, 4.0, 28.0, alphaToBeta));
      ParameterLibrary.Add("Lens", new EUDparameters(3.0, 1.0, 18.0, alphaToBeta));
      ParameterLibrary.Add("Liver", new EUDparameters(3.0, 3.0, 40.0, alphaToBeta));
      ParameterLibrary.Add("Lung", new EUDparameters(1.0, 2.0, 24.5, alphaToBeta));
      ParameterLibrary.Add("Optic nerve", new EUDparameters(25.0, 3.0, 65.0, alphaToBeta));
      ParameterLibrary.Add("Parotids", new EUDparameters(0.5, 3.0, 20.0, alphaToBeta));
      ParameterLibrary.Add("Retina", new EUDparameters(15.0, 2.0, 65.0, alphaToBeta));
      ParameterLibrary.Add("Spinal cord", new EUDparameters(13.0, 4.0, 20.0, alphaToBeta));
    }

    #endregion

    /// <summary>
    /// Business logic for the script.
    /// </summary>
    public void Execute(ScriptContext context)
    {

      Context = context;

      Plan = GetPlan(Context);
      if (Plan == null)
      {
        return;
      }

      // Define EUD and TCP/NTCP parameters for each structure in the plan. Here we use the corresponding value from the resources (except for 'body').
      // This is a patient and plan specific part.
      Dictionary<string, EUDparameters> parameters = new Dictionary<string, EUDparameters>();
      parameters.Add("body", new EUDparameters(2.0, 1.0, 25.0, 10.0));
      parameters.Add("cord", ParameterLibrary["Spinal cord"]);
      parameters.Add("heart", ParameterLibrary["Heart"]);
      parameters.Add("left lung", ParameterLibrary["Lung"]);
      parameters.Add("right lung", ParameterLibrary["Lung"]);
      parameters.Add("PTV (breast tis)", ParameterLibrary["Breast"]);

      // Get structures defined in the plan.
      var structures = Plan.StructureSet.Structures;

      // Container for calculation results.
      Dictionary<string, CalculationResult> dvhData = new Dictionary<string, CalculationResult>();

      // Calculate NTCP for normal tissue structures and TCP for target structures.
      foreach (var st in structures)
      {
        DVHData dataItem = Plan.GetDVHCumulativeData(st, Common.Model.Types.DoseValuePresentation.Absolute, Common.Model.Types.VolumePresentation.Relative, BinWidth);
        EUDparameters pr = parameters[st.Id];
        CalculationResult res = CalculateEUDandProbability(dataItem, pr.gamma50, pr.a, pr.D50, pr.AlphaToBeta);
        dvhData.Add(st.Id, res);

      }

      ReportResults(dvhData);

      return;

    }
   
    /// <summary>
    /// Implementation of the Gay-Niemierko method for calculating EUD-based TCP/NTCP. Reference: Phys Med. 2007 Dec; 23(3-4):115-25. Epub 2007 Sep 7. 
    /// </summary>
    /// <param name="dataItem">DVH data</param>
    /// <param name="gamma50">Parameter controlling the steepness of the TCP/NTCP curve.</param>
    /// <param name="aParameter">Tolerance parameter for EUD calculation.</param>
    /// <param name="d50">Dose corresponding to 50% probability for normal tissue complication/tumor control.</param>
    /// <param name="alphaToBetaRatio">Alpha/beta ratio for tumor or critical organ.</param>
    /// <returns></returns>
    private CalculationResult CalculateEUDandProbability(DVHData dataItem, double gamma50, double aParameter, double d50, double alphaToBetaRatio)
    {

      double standardFractionation = Plan.UniqueFractionation.PrescribedDosePerFraction.Dose;
      int? numberOfFractions = Plan.UniqueFractionation.NumberOfFractions;

      if (!numberOfFractions.HasValue || numberOfFractions <= 0)
      {
        string info = "This plan has no valid fractions specified.";
        string caption = "Error";
        MessageBox.Show(info, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        return new CalculationResult(null, null);
      }

      int nb = dataItem.CurveData.Length;
      double[,] dvh = new double[nb, 2];
      for (int i = 0; i < nb; i++)
      {
        dvh[i, 0] = dataItem.CurveData[i].DoseValue.Dose;
        dvh[i, 1] = dataItem.CurveData[i].Volume;
      }

      double[,] differentialDVH = new double[nb-1, 2];
      for (int i = 1; i < nb; i++)
      {
        differentialDVH[i - 1, 0] = 0.5*(dvh[i,0] + dvh[i-1,0]);
        differentialDVH[i - 1, 1] = dvh[i - 1, 1] - dvh[i, 1];
      }
      nb--;

      // DVH corresponding to biologically equivalent dose.
      double[,] bedDvh = new double[nb, 2];
      double totalVolume = 0.0;

      // Calculating the biologically equivalent dose and the total volume.
      for (int i = 0; i < nb; i++)
      {
        bedDvh[i,0] = differentialDVH[i,0] * (alphaToBetaRatio + differentialDVH[i,0]/numberOfFractions.Value)/(alphaToBetaRatio + standardFractionation);
        totalVolume += differentialDVH[i,1];
      }

      // Normalizing volume data to 1 (therefore, total volume corresponds to 1).
      for (int i = 0; i < nb; i++)
      {
        bedDvh[i, 1] = differentialDVH[i, 1] / totalVolume;
      }

      // Calculate the EUD.
      double eud = 0.0;
      for (int i = 0; i < nb; i++)
      {
        eud += bedDvh[i,1] * Math.Pow(bedDvh[i,0], aParameter);
      }
      eud = Math.Pow(eud, 1.0/aParameter);

      // Calculate the probability (in percents).
      double prob = 100.0 / ( 1.0 + Math.Pow(d50/eud,4.0*gamma50) );

      return new CalculationResult(eud, prob);
    }

    #region Helper methods

    /// <summary>
    /// Report calculation results in a MessageBox.
    /// </summary>
    private void ReportResults(Dictionary<string, CalculationResult> dvhData)
    {
      // Sort the data such the elements with highest probability are at the top.
      var data = dvhData.OrderBy(x => -x.Value.Probality);

      var window = new Window();
      var scrollView = new ScrollViewer();
      scrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
      var panel = new StackPanel();
      panel.Orientation = Orientation.Vertical;

      var title = new Label();
      title.Content = "Results:";
      title.FontSize = 1.5 * title.FontSize;
      panel.Children.Add(title);

      var grid = new Grid();
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());
      grid.ColumnDefinitions.Add(new ColumnDefinition());

      // Add header
      AddRow("Structure", "EUD", "TCP/NTCP", 0, grid, isHeader: true);

      // Add results
      var counter = 1;
      foreach (var item in data)
      {
        AddRow(item.Key, string.Format("{0:0.0}Gy", item.Value.EUD), string.Format("{0:0.0}%", item.Value.Probality), counter, grid);
        counter++;
      }

      panel.Children.Add(grid);
      scrollView.Content = panel;
      window.Content = scrollView;
      window.Width = 350;
      window.Height = 400;
      window.ShowDialog();
    }

    private void AddRow(string col1, string col2, string col3, int rowIndex, Grid grid, bool isHeader = false)
    {
      grid.RowDefinitions.Add(new RowDefinition());

      var margin = isHeader ? new Thickness(10, 10, 10, 10) : new Thickness(10, 0, 0, 10);

      var label1 = new Label();
      label1.Content = col1;
      label1.Margin = margin;
      grid.Children.Add(label1);
      label1.SetValue(Grid.RowProperty, rowIndex);
      label1.SetValue(Grid.ColumnProperty, 0);

      var label2 = new Label();
      label2.Content = col2;
      label2.Margin = margin;
      grid.Children.Add(label2);
      label2.SetValue(Grid.RowProperty, rowIndex);
      label2.SetValue(Grid.ColumnProperty, 1);

      var label3 = new Label();
      label3.Content = col3;
      label3.Margin = margin;
      grid.Children.Add(label3);
      label3.SetValue(Grid.RowProperty, rowIndex);
      label3.SetValue(Grid.ColumnProperty, 2);

      if (isHeader)
      {
        label1.FontWeight = FontWeights.Bold;
        label2.FontWeight = FontWeights.Bold;
        label3.FontWeight = FontWeights.Bold;
      }
      
    }

    /// <summary>
    /// Get the currently active plan from the script context. 
    /// </summary>
    private ExternalPlanSetup GetPlan(ScriptContext ctx)
    {
      Patient pt = ctx.Patient;
      if (pt == null)
      {
        string info = "No patient is currently open. Open a patient before executing this script.";
        string caption = "No patient available";
        MessageBox.Show(info, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        return null;
      }

      Course cs = ctx.Course;
      if (cs == null)
      {
        string info = "No course is currently open. Open a course before executing this script.";
        string caption = "No course available";
        MessageBox.Show(info, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        return null;
      }

      ExternalPlanSetup plan = ctx.ExternalPlanSetup;
      if (plan.Dose == null)
      {
        string info = string.Format("Plan '{0}' does not have a valid dose. Perform dose calculation before executing this script.", plan.Id);
        string caption = "No dose available";
        MessageBox.Show(info, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        return null;
      }

      return plan;
    }

    #endregion

  }

  #region Data types

  public struct CalculationResult
  {
    private double? m_eud;
    public double? EUD { get { return m_eud; } }

    private double? m_probability;
    public double? Probality { get { return m_probability; } }

    public CalculationResult(double? eud, double? prob)
    {
      m_eud = eud;
      m_probability = prob;
    }
  }

  public struct EUDparameters
  {
    private double m_a;
    public double a { get { return m_a; } }

    private double m_gamma50;
    public double gamma50 { get { return m_gamma50; } }

    private double m_d50;
    public double D50 { get { return m_d50; } }

    private double m_alphaToBeta;
    public double AlphaToBeta { get { return m_alphaToBeta; } }

    public EUDparameters(double a, double g50, double d50, double alphaToBeta)
    {
      m_a = a;
      m_gamma50 = g50;
      m_d50 = d50;
      m_alphaToBeta = alphaToBeta;
    }
  }

  #endregion

}
