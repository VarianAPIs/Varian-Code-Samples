////////////////////////////////////////////////////////////////////////////////
// GenerateWebDVH.cs
//
//  A ESAPI v11/v13 Script that generates a DVH in javascript using
//  Google's LineChart.  Works only in Chrome, works only when an
//  internet connection is present.  Loads the base javascript
//  that does the heavy lifting from https://www.google.com/jsapi.
//
//  Change the structure names on Line 64 to those you want displayed
//  on the DVH chart.
//  
//  To test whether this Script will work in your environment, load the included
//  generated HTML file "sample_dvh.html" in Chrome.  If you see a DVH
//  chart with two graphs for 'Parotid LT' and 'Parotid RT' then this 
//  script will work for you.
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
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Xml;
using System.Xml.Linq;

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window*/)
    {
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Indent = true;
      settings.IndentChars = ("\t");
      System.IO.MemoryStream mStream = new System.IO.MemoryStream();
      using (XmlWriter writer = XmlWriter.Create(mStream, settings))
      {
        // generate DVHs in an HTML report for selected structures
        string[] selectedStructures = { "Parotid LT", "Parotid RT" }; // TODO: change these to the structures you want to generate DVHs for
        ExportDVHs(context.PlanSetup, context.StructureSet, selectedStructures, writer);

        // done writing
        writer.Flush();
        mStream.Flush();

        // write the XML file.
        string temp = System.Environment.GetEnvironmentVariable("TEMP");
        string htmlPath = string.Format("{0}\\{1}({2})-plan-{3}.html", temp, 
          context.Patient.LastName, context.Patient.Id, context.PlanSetup.Id);
        using (System.IO.FileStream file = new System.IO.FileStream
          (htmlPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
        {
          // Have to rewind the MemoryStream in order to read its contents. 
          mStream.Position = 0;
          mStream.CopyTo(file);
          file.Flush();
          file.Close();
        }

        // 'Start' generated HTML file to launch browser window
        System.Diagnostics.Process.Start(htmlPath);
        // Sleep for a few seconds to let internet browser window to start
        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
      }
    }

    void ExportDVHs(PlanSetup plan, StructureSet ss, string[] structureIds, XmlWriter writer)
    {
      writer.WriteStartElement("html");

      writer.WriteStartElement("head");
      writer.WriteElementString("title", "web dvh");
      XElement script = new XElement("script", 
                    new XAttribute("type", "text/javascript"),
                    new XAttribute("src", "https://www.google.com/jsapi"),
                    "// need this due to bug in google LineChart javascript"
                    );
      script.WriteTo(writer);
      script = new XElement("script",
                    new XAttribute("type", "text/javascript"),
                    @"
        google.load('visualization', '1', { packages: ['corechart'] });
");
      script.WriteTo(writer);
      Tuple<string, DVHData>[] listDVHs = new Tuple<string, DVHData>[structureIds.Length];
      int index = 0, maxPtsIndex=0;
      int maxDVHPts = int.MinValue;
      // search through the list of structure ids until we find one
      Structure structure = null;
      foreach (string volumeId in structureIds)
      {
        structure = (from s in ss.Structures
                     where s.Id.ToUpper().CompareTo(volumeId.ToUpper()) == 0
                     select s).FirstOrDefault();

        if (structure == null)
          continue; // structure not found
        DVHData dvh = plan.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1);
        listDVHs[index] = new Tuple<string, DVHData>(volumeId, dvh);
        if (maxDVHPts < dvh.CurveData.Count())
        {
          maxPtsIndex = index;
        }
        maxDVHPts = Math.Max(maxDVHPts, dvh.CurveData.Count());
        index++;
      }
      // collect the dose / volume data into a single matrix
      double [,] dvhData = new double[maxDVHPts, listDVHs.Count()];
      for(int j = 0; j < listDVHs.Count(); j++)
        for (int i = 0; i < maxDVHPts; i++)
        {
          dvhData[i, j] = 0.0;
          DVHData dvh = listDVHs[j].Item2;
          if (dvh.CurveData.Count() > i)
            dvhData[i, j] = dvh.CurveData[i].Volume; 
        }
      string javascript = @"
        function drawVisualization() {
          var data = google.visualization.arrayToDataTable([
            ['Dose'";
      foreach (Tuple<string, DVHData> dvhItemData1 in listDVHs)
      {
        javascript += ", '" + dvhItemData1.Item1 + "'";
      }
         javascript += @"],
";
      int x = 0;   
      foreach (DVHPoint pt in listDVHs[maxPtsIndex].Item2.CurveData)
        {
           string doseVolumes = string.Format("[{0:0.0}", pt.DoseValue.Dose);
          for(int j = 0; j < listDVHs.Count(); j++)
          {
            doseVolumes += string.Format(",{0:0.00000}", dvhData[x,j]);
          }
          doseVolumes += @"],";
          javascript += doseVolumes;
          x++;
        }
        javascript += @"
        ]);

            // Create and draw the visualization.
            new google.visualization.LineChart(document.getElementById('xxxxx')).
            draw(data, {
                width: 1000, height: 800,
                vAxis: { maxValue: 100 }
            }
                );
        }


        google.setOnLoadCallback(drawVisualization);
";
        XElement script2 = new XElement("script", 
                    new XAttribute("type", "text/javascript"),
                    javascript
                    );
        script2.WriteTo(writer);
     
      writer.WriteEndElement();// head

      XElement body = new XElement("body", 
                        new XElement("div",
                          new XAttribute("id", "xxxxx"),
                          new XAttribute("style", "width: 900px; height: 500px;")
                          ));
      body.WriteTo(writer);
      writer.WriteEndElement();// html
    }

  }
}
