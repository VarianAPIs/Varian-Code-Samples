////////////////////////////////////////////////////////////////////////////////
// SVGFromDVH.cs
//
// Create an SVG file for given set of structures. The figure includes also the 
// DVH estimates.
//  
// Applies to: ESAPI v11, v13, v13.5, v13.6.
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
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Windows.Media;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
  class SVGFromDVH
  {
    public static readonly XNamespace SvgNS = "http://www.w3.org/2000/svg";

    static XAttribute AttributeFromColor(string attributeName, Color color)
    {
      return new XAttribute(attributeName, "rgb(" + color.R + "," + color.G + "," + color.B + ")");
    }

    static XElement PathFromGrid(Color stroke, double pixelOffsetX, double pixelOffsetY, double maxX, double maxY, double scaleX, double scaleY, double intervalX, double intervalY)
    {
      double epsilonX = 0.0001;
      double epsilonY = 0.0001;

      double width = maxX * scaleX;
      double height = maxY * scaleY;

      int nX = Math.Abs((int)(width / (scaleX * intervalX) + epsilonX)) + 1;
      int nY = Math.Abs((int)(height / (scaleY * intervalY) + epsilonY)) + 1;

      var strokeAttr = AttributeFromColor("stroke", stroke);

      var builder = new StringBuilder();

      for (int i = 0; i < nX; i++)
      {
        double x0 = i * intervalX * scaleX + pixelOffsetX;
        double y0 = pixelOffsetY;
        double x1 = x0;
        double y1 = pixelOffsetY + height;
        builder.AppendFormat("M {0} {1} L {2} {3} ", x0, y0, x1, y1);
      }

      // Add extra vertical line to the right right to close the box.
      {
        var x0 = width + pixelOffsetX;
        var x1 = x0;
        var y0 = pixelOffsetY;
        var y1 = pixelOffsetY + height;
        builder.AppendFormat("M {0} {1} L {2} {3} ", x0, y0, x1, y1);
      }


      for (int i = 0; i < nY; i++)
      {
        double x0 = pixelOffsetX;
        double y0 = i * intervalY * scaleY + pixelOffsetY;
        double x1 = pixelOffsetX + width;
        double y1 = y0;
        builder.AppendFormat("M {0} {1} L {2} {3} ", x0, y0, x1, y1);
      }

      return new XElement(SvgNS + "path", new XAttribute("d", builder.ToString()), strokeAttr, new XAttribute("fill", "transparent"));
    }

    static IEnumerable<XElement> LabelsFromDVH(Color textColor, double pixelOffsetX, double pixelOffsetY, double maxX, double maxY, double intervalX, double intervalY, double scaleX, double scaleY, string unitX, string unitY)
    {
      double epsilon = 0.00001;

      double width = maxX * scaleX;
      double height = maxY * scaleY;

      int nX = Math.Abs((int)(width / (scaleX * intervalX) + epsilon)) + 1;
      int nY = Math.Abs((int)(height / (scaleY * intervalY) + epsilon)) + 1;

      var result = new List<XElement>(nX);

      for (int i = 1; i < nX; i++)
      {
        double x = pixelOffsetX + i * intervalX * scaleX;
        double y = pixelOffsetY;
        string text = String.Format("{0:0.00}", i * intervalX);

        var textEl = new XElement(SvgNS + "text",
                                  AttributeFromColor("fill", textColor),
                                  new XAttribute("text-anchor", "middle"),
                                  new XAttribute("dy", "1em"),
                                  new XAttribute("x", x),
                                  new XAttribute("y", y),
                                  text);
        result.Add(textEl);
      }

      // unit label
      {
        double centerX = pixelOffsetX + 0.5 * width;
        double y = 1.025*pixelOffsetY;
        string text = String.Format("Relative dose [{0}]", unitX);
        var textEl = new XElement(SvgNS + "text",
                                  AttributeFromColor("fill", textColor),
                                  new XAttribute("text-anchor", "middle"),
                                  new XAttribute("dy", "2em"),
                                  new XAttribute("x", centerX),
                                  new XAttribute("y", y),
                                  text);
        result.Add(textEl);
      }

      for (int i = 1; i < nY; i++)
      {
        double x = pixelOffsetX;
        double y = pixelOffsetY + i * intervalY * scaleY;
        string text = String.Format("{0:0.00}", i * intervalY);

        var textEl = new XElement(SvgNS + "text",
                                  AttributeFromColor("fill", textColor),
                                  new XAttribute("dx", "-1ex"),
                                  new XAttribute("text-anchor", "end"),
                                  new XAttribute("dominant-baseline", "central"),
                                  new XAttribute("x", x),
                                  new XAttribute("y", y),
                                  text);
        result.Add(textEl);
      }

      // unit label
      {
        double x = 0;
        double centerY = pixelOffsetY + 0.5 * height;
        string text = String.Format("Volume [{0}]", unitY);
        var textEl = new XElement(SvgNS + "text",
                                  AttributeFromColor("fill", textColor),
                                  new XAttribute("dx", "2ex"),
                                  new XAttribute("text-anchor", "start"),
                                  new XAttribute("dominant-baseline", "central"),
                                  new XAttribute("x", x),
                                  new XAttribute("y", centerY),
                                  new XAttribute("transform", "rotate(-90 30, 130)"),
                                  text);
        result.Add(textEl);
      }

      var zeroEl = new XElement(SvgNS + "text",
                                AttributeFromColor("fill", textColor),
                                new XAttribute("dx", "-1ex"),
                                new XAttribute("dy", "1em"),
                                new XAttribute("text-anchor", "end"),
                                new XAttribute("x", pixelOffsetX),
                                new XAttribute("y", pixelOffsetY),
                                "0");
      result.Add(zeroEl);

      return result;
    }

    static XElement PathFromDVH(DVHPoint[] points, Color stroke, double pixelOffsetX, double pixelOffsetY, double scaleX, double scaleY)
    {
      var y = points[0].Volume;
      var x = points[0].DoseValue.Dose;
      var builder = new StringBuilder();

      builder.Append("M ").Append(pixelOffsetX + x * scaleX).Append(' ').Append(y * scaleY + pixelOffsetY);

      for (int i = 1; i < points.Length || points[i - 1].Volume == 0.0; i++)
      {
        y = points[i].Volume;
        x = points[i].DoseValue.Dose;
        builder.Append(" L").Append(pixelOffsetX + x * scaleX).Append(' ').Append(y * scaleY + pixelOffsetY);
      }

      var rgbStr = "rgb(" + stroke.R + "," + stroke.G + "," + stroke.B + ")";

      return new XElement(SvgNS + "path", new XAttribute("d", builder.ToString()), new XAttribute("stroke", rgbStr), new XAttribute("fill", "transparent"));
    }

    static XElement PolygonFromDVHestimates(DVHPoint[] lowerEst, DVHPoint[] upperEst, Color stroke, double pixelOffsetX, double pixelOffsetY, double scaleX, double scaleY)
    {
      var y = lowerEst[0].Volume;
      var x = lowerEst[0].DoseValue.Dose;
      var builder = new StringBuilder();

      builder.Append(pixelOffsetX + x * scaleX).Append(' ').Append(y * scaleY + pixelOffsetY);

      for (int i = 1; i < lowerEst.Length; i++)
      {
        y = lowerEst[i].Volume;
        x = lowerEst[i].DoseValue.Dose;
        builder.Append(' ').Append(pixelOffsetX + x * scaleX).Append(' ').Append(y * scaleY + pixelOffsetY);
      }

      for (int i = upperEst.Length-1; i >= 0; i--)
      {
        y = upperEst[i].Volume;
        x = upperEst[i].DoseValue.Dose;
        builder.Append(' ').Append(pixelOffsetX + x * scaleX).Append(' ').Append(y * scaleY + pixelOffsetY);
      }

      var rgbStr = "rgb(" + stroke.R + "," + stroke.G + "," + stroke.B + ")";
      const string opacity = "0.25"; // Make the polygon filling transparent.
      var polygonClr = string.Format("fill:{0}; stroke:{1}; opacity:{2}", rgbStr, rgbStr, opacity);
      return new XElement(SvgNS + "polygon", new XAttribute("points", builder.ToString()), new XAttribute("style", polygonClr));
    }

    static float Clamp(float value, float min, float max)
    {
      return Math.Max(min, Math.Min(max, value));
    }

    static Color DisplayToPrintColor(Color color)
    {
      // darken to 75 %
      float multiplier = 0.75f;

      float inR = (1 / 255.0f) * color.R;
      float inG = (1 / 255.0f) * color.G;
      float inB = (1 / 255.0f) * color.B;

      float maxColor = Math.Max(inR, Math.Max(inG, inB));
      float minColor = Math.Min(inR, Math.Min(inG, inB));
      float inL = 0.5f * maxColor + 0.5f * minColor;
      float outL = inL * multiplier;
      float diff = outL - inL;

      float outR = Clamp(inR + diff, 0, 1);
      float outG = Clamp(inG + diff, 0, 1);
      float outB = Clamp(inB + diff, 0, 1);

      return Color.FromRgb((byte)(outR * 255.0f), (byte)(outG * 255.0f), (byte)(outB * 255.0f));
    }

    public static void SaveSVGFromStructures(string outPath, PlanSetup plan, IEnumerable<Structure> structures, int pixelWidth, int pixelHeight)
    {
      if (!structures.Any())
        return;

      int pixelOffsetX = 64, pixelOffsetY = pixelHeight - 48;

      var outElement = new XElement(SvgNS + "svg",
                                    new XAttribute("width", pixelWidth*1.25),
                                    new XAttribute("height", pixelHeight),
                                    new XAttribute("font-size", "9pt"),
                                    new XAttribute("font-family", "\"Times New Roman\", Cambria, times, serif"));

      int contentWidth = pixelWidth - pixelOffsetX - 48;
      int contentHeight = pixelOffsetY - 32;

      double binWidth = 1.0;
      double dvhMaxX = structures.Select(s => plan.GetDVHCumulativeData(s, plan.DoseValuePresentation, VolumePresentation.Relative, binWidth).CurveData.Max(x => x.DoseValue.Dose)).Max();
      double dvhMaxY = structures.Select(s => plan.GetDVHCumulativeData(s, plan.DoseValuePresentation, VolumePresentation.Relative, binWidth).CurveData.Max(y => y.Volume)).Max();

      var estimates = GetUpperAndLowerEstimates(plan);
      var maxEstimate = estimates.Max(est => Math.Max(est.Value.Item1.Max(l => l.DoseValue.Dose), est.Value.Item2.Max(u => u.DoseValue.Dose)));
      if (maxEstimate > dvhMaxX)
      {
        dvhMaxX = maxEstimate;
      }

      double scaleX = contentWidth / dvhMaxX;
      double scaleY = -contentHeight / dvhMaxY;
      const double intervalX = 20.0;
      double intervalY = dvhMaxY / 5.0;

      string unitX = null, unitY = null;

      // Add DVH estimates
      foreach (var estimate in estimates)
      {
        if (estimate.Value.Item1.Length > 1 && estimate.Value.Item2.Length > 1)
        {
          var polygon = PolygonFromDVHestimates(estimate.Value.Item1, estimate.Value.Item2, DisplayToPrintColor(estimate.Key.Color), pixelOffsetX, pixelOffsetY, scaleX, scaleY);
          outElement.Add(polygon);
        }
      }

      outElement.Add(PathFromGrid(Color.FromRgb(192, 192, 192), pixelOffsetX, pixelOffsetY, dvhMaxX, dvhMaxY, scaleX, scaleY, intervalX, intervalY));

      foreach (var structure in structures)
      {
        var dvhData = plan.GetDVHCumulativeData(structure, plan.DoseValuePresentation, VolumePresentation.Relative, 1.0f);
        var pathEl = PathFromDVH(dvhData.CurveData, DisplayToPrintColor(structure.Color), pixelOffsetX, pixelOffsetY, scaleX, scaleY);
        unitX = dvhData.CurveData[0].DoseValue.UnitAsString;
        unitY = dvhData.CurveData[0].VolumeUnit;
        outElement.Add(pathEl);
      }

      outElement.Add(LabelsFromDVH(Color.FromRgb(64, 64, 64), pixelOffsetX, pixelOffsetY, dvhMaxX, dvhMaxY, intervalX, intervalY, scaleX, scaleY, unitX, unitY));
      outElement.Add(CurveIdentificationsFromDVH(plan, structures, pixelOffsetX + 1.025 * dvhMaxX * scaleX, pixelOffsetY + 0.9 * dvhMaxY * scaleY, 0.1 * dvhMaxY * scaleY));

      outElement.Save(outPath);
    }

    private static IEnumerable<XElement> CurveIdentificationsFromDVH(PlanSetup plan, IEnumerable<Structure> structures, double x0, double y0, double step)
    {
      var textElements = new List<XElement>();
      var text = new XElement(SvgNS + "tspan", new XAttribute("style", "font-weight:bold"), new XAttribute("text-decoration", "underline"), "Structures");
      textElements.Add(new XElement(SvgNS + "text", new XAttribute("x", x0), new XAttribute("y", y0), new XAttribute("fill", "black"), text));
      for (int i=0; i < structures.Count(); i++)
      {
        var y = y0 - (i + 1)*step;
        var color = DisplayToPrintColor(structures.ElementAt(i).Color);
        var id = structures.ElementAt(i).Id;
        var rgbStr = "rgb(" + color.R + "," + color.G + "," + color.B + ")";
        textElements.Add(new XElement(SvgNS + "text", new XAttribute("x", x0), new XAttribute("y", y), new XAttribute("fill", rgbStr), id));
      }
      return textElements;
    }

    private static Dictionary<Structure, Tuple<DVHPoint[], DVHPoint[]>> GetUpperAndLowerEstimates(PlanSetup plan)
    {
      var estimates = plan.DVHEstimates.ToList();
      var res = new Dictionary<Structure, Tuple<DVHPoint[], DVHPoint[]>>();
      foreach (var structure in plan.StructureSet.Structures)
      {
        if (estimates.Any(est => est.Structure.Id == structure.Id))
        {
          var lower = estimates.Single(est => est.Structure.Id == structure.Id && est.Type == DVHEstimateType.Lower).CurveData;
          if (estimates.Any(est => est.Structure.Id == structure.Id && est.Type == DVHEstimateType.Upper))
          {
            var upper = estimates.Single(est => est.Structure.Id == structure.Id && est.Type == DVHEstimateType.Upper).CurveData;
            if (lower.Length > 1 && upper.Length > 1)
            {
              lower = FixUnits(lower, plan);
              upper = FixUnits(upper, plan);
              res.Add(structure, new Tuple<DVHPoint[], DVHPoint[]>(lower, upper));
            }  
          }
        }
      }
      return res;
    }

    private static DVHPoint[] FixUnits(DVHPoint[] points, PlanSetup plan)
    {
      if (points.First().DoseValue.Unit != DoseValue.DoseUnit.Percent)
      {
        return points.Select(p => new DVHPoint(new DoseValue(100.0 * p.DoseValue.Dose / plan.TotalPrescribedDose.Dose, DoseValue.DoseUnit.Percent),
                                               p.Volume, p.VolumeUnit)).ToArray();
      }
      return points;
    }
  }
}
