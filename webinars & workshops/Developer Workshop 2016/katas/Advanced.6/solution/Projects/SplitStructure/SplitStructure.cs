////////////////////////////////////////////////////////////////////////////////
// SplitStructure.cs
//
// Applies to:
//      Eclipse Scripting API
//          15.1
//
//      Eclipse Scripting API for Research Users
//          13, 13.6, 13.7, 15.0, 15.1
//
// Copyright (c) 2016 Varian Medical Systems, Inc.
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
using System.Windows.Controls;

[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
    {
      var ss = context.StructureSet;
      var roi = ss.Structures.Single(st => st.Id == "LIVER");
      var target = ss.Structures.Single(st => st.Id == "PTV_4680");
      //var s = SelectStructureWindow.SelectStructure(ss); // A user interface for selecting structure
      //if (s == null) return;
      
      context.Patient.BeginModifications();
      //var newStr = EnlargeStructure(s, ss, 30, 30);
      //if (newStr != null)
      //{
      //  MessageBox.Show("New structure was created: " + newStr.Id);
      //}
      //else
      //{
      //  MessageBox.Show("New structure could not be created");
      //}
      if (SplitStructure(ss, target, roi))
      {
        MessageBox.Show("New structures were created");
      }
      else
      {
        MessageBox.Show("New structures could not be created");
      }
    }

    /// <summary>
    /// This splits a structure into two. It creates a margin around the target,
    /// so that it approximately crosses the mass center of the structure, and then
    /// uses boolean operators to create two new structures. Note: due to the nature
    /// of the segment model in Eclipse, the new structures do not always perfectly cover
    /// the whole volume of the original structure.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="ss"></param>
    /// <returns></returns>
    bool SplitStructure(StructureSet ss, Structure target, Structure roi)
    {
      if (ss.CanAddStructure(roi.DicomType, roi.Id + "_spl1"))
      {
        Structure newStr1 = ss.AddStructure(roi.DicomType, roi.Id + "_spl1");
        Structure newStr2 = ss.AddStructure(roi.DicomType, roi.Id + "_spl2");

        VVector targetCenter = target.CenterPoint;
        VVector roiCenter = roi.CenterPoint;
        double dist = (targetCenter - roiCenter).Length;

        //figure out distance from target center to target surface
        System.Collections.BitArray buffer = new System.Collections.BitArray(100);
        SegmentProfile profile = target.GetSegmentProfile(targetCenter, roiCenter, buffer);
        double distToTargetSurface = 0;
        foreach (SegmentProfilePoint point in profile)
        {
          if (point.Value == false)
          {
            //first point outside structure
            distToTargetSurface = (point.Position - targetCenter).Length;
            break;
          }
        }
        //SegmentVolume seg = target.Margin(dist - distToTargetSurface);
        SegmentVolume seg = target.LargeMargin(dist - distToTargetSurface);
        newStr1.SegmentVolume = seg.And(roi);
        newStr2.SegmentVolume = roi.Sub(newStr1);
        return true;
      }
      return false;
    }

  }

  static class StructureExtension
  {
    /// <summary>
    /// Because Structure.Margin() has upper limit of 50mm for the margin, this
    /// extension allows larger values.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="ss"></param>
    /// <param name="mm"></param>
    /// <returns></returns>
    public static SegmentVolume LargeMargin(this Structure target, double mm)
    {
      double mmLeft = mm;
      SegmentVolume targetLeft = target.SegmentVolume;
      while (mmLeft > 50)
      {
        mmLeft -= 50;
        targetLeft = targetLeft.Margin(50);
      }
      SegmentVolume result = targetLeft.Margin(mmLeft);
      return result;
    }
  }

  #region notused
  class SelectStructureWindow : Window
  {
    public static Structure SelectStructure(StructureSet ss)
    {
      m_w = new Window();
      m_w.Width = 400;
      m_w.Height = 400;
      m_w.Title = "Select structure:";
      var grid = new Grid();
      m_w.Content = grid;
      var list = new ListBox();
      foreach (var s in ss.Structures)
      {
        list.Items.Add(s);
      }
      list.VerticalAlignment = VerticalAlignment.Top;
      list.Margin = new Thickness(10, 10, 10, 10);
      grid.Children.Add(list);
      var button = new Button();
      button.Content = "OK";
      button.Height = 40;
      button.VerticalAlignment = VerticalAlignment.Bottom;
      button.Margin = new Thickness(10, 10, 10, 10);
      button.Click += button_Click;
      grid.Children.Add(button);
      if (m_w.ShowDialog() == true)
      {
        return (Structure)list.SelectedItem;
      }
      return null;
    }

    static Window m_w = null;

    static void button_Click(object sender, RoutedEventArgs e)
    {
      m_w.DialogResult = true;
      m_w.Close();
    }

    /// <summary>
    /// Select a section of body structure that is X mm up and down from an existing structure's
    /// top and bottom slices. Make it into a new structure.
    /// </summary>
    /// <param name="s">The structure to enlarge</param>
    /// <param name="ss">The structure set where to add</param>
    /// <param name="upMm">The margin up in mm</param>
    /// <param name="downMm">The margin down in mm</param>
    /// <returns>The new structure. Null if new structure cannot be added.</returns>
    Structure EnlargeStructure(Structure s, StructureSet ss, double upMm, double downMm)
    {
      if (ss.CanAddStructure(s.DicomType, s.Id + "_enl"))
      {
        var newStr = ss.AddStructure(s.DicomType, s.Id + "_enl");
        var image = ss.Image;
        double res = image.ZRes;
        int startSlice = 0;
        for (int z = 0; z < image.ZSize; z++)
        {
          if (s.GetContoursOnImagePlane(z).Count() > 0)
          {
            startSlice = (int)(z - downMm / res);
            break;
          }
        }
        int stopSlice = image.ZSize - 1;
        for (int z = image.ZSize - 1; z >= 0; z--)
        {
          if (s.GetContoursOnImagePlane(z).Count() > 0)
          {
            stopSlice = (int)(z + upMm / res);
            break;
          }
        }
        startSlice = Math.Max(startSlice, 0);
        stopSlice = Math.Min(stopSlice, image.ZSize - 1);
        var contour = new VVector[4]
        {
          // these are real coordinates, not voxel coordinates
          new VVector(-10000, -10000, 0),
          new VVector(10000, -10000, 0),
          new VVector(10000, 10000, 0),
          new VVector(-10000, 10000, 0)
        };

        for (int z = startSlice; z <= stopSlice; z++)
        {
          newStr.AddContourOnImagePlane(contour, z);
        }
        var body = ss.Structures.First(st => st.DicomType == "EXTERNAL");
        newStr.SegmentVolume = newStr.And(body);
        return newStr;
      }
      return null;
    }
    #endregion
  }
}

