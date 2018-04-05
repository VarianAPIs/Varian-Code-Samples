//---------------------------------------------------------------------------------------------
/// <summary>
/// Eclipse v11/v13 ESAPI script that exports in-memory structures to VTK formatted files.
/// </summary>
/// <license>
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
/// </license>
//---------------------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    //---------------------------------------------------------------------------------------------
    public void Execute(ScriptContext context /*, System.Windows.Window window*/)
    {
      Patient patient = context.Patient;
      if (context.StructureSet == null)
      {
        MessageBox.Show("Please load a patient with a 3D structure set.", "Varian Developer");
        return;
      }

      string temp = System.Environment.GetEnvironmentVariable("TEMP");
      string folder = string.Format(@"{0}\Export3D\{1}", temp, MakeFilenameValid(patient.Id));
      if (!Directory.Exists(folder))
      {
        Directory.CreateDirectory(folder);
      }

      ExportStructures(context.StructureSet, folder);
      if (context.PlanSetup != null && context.PlanSetup.Dose != null)
      {
        context.PlanSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
        ExportDose(context.PlanSetup.Dose, folder);
      }
      ExportBolus(context.StructureSet, folder);
      MessageBox.Show("Exported 3D data to " + folder + ".\n\n" +
                      "You can now use a 3rd party product such as MeshLab (meshlab.sourceforge.net)\n" +
                      "or ParaView (www.paraview.org) to view the exported data.",
                      "Varian Developer");
    }

    //---------------------------------------------------------------------------------------------
    void ExportStructures(StructureSet ss, string folder)
    {
      foreach (Structure structure in ss.Structures)
      {
        if (!structure.HasSegment)
          continue;
        string id = structure.Id;
        string filename = MakeFilenameValid(id);
        SaveTriangleMeshToPlyFile(structure.MeshGeometry, structure.Color, folder + "\\" + filename + ".ply");
      }
    }

    //---------------------------------------------------------------------------------------------
    void ExportDose(Dose dose, string folder)
    {
      string filename = folder + "\\dose.vtk";
      SaveDoseOrImageToVTKStructurePoints(dose, null, filename);

      foreach (Isodose isodose in dose.Isodoses)
      {
        string id = isodose.Level.ToString();
        filename = MakeFilenameValid(id.Replace(" ", "_").Replace("%", "p"));
        SaveTriangleMeshToPlyFile(isodose.MeshGeometry, isodose.Color, folder + "\\" + filename + ".ply");
      }
    }

    //---------------------------------------------------------------------------------------------
    void ExportBolus(StructureSet ss, string folder)
    {
      foreach (Structure structure in ss.Structures)
      {
        if (!structure.HasSegment || structure.DicomType != "BOLUS")
          continue;
        string id = structure.Id;
        string filename = MakeFilenameValid(id);

        //comment out the file output you do not want
        SaveTriangleMeshToPlyFile(structure.MeshGeometry, folder + "\\" + filename + ".ply");
        //SaveTriangleMeshtoStlFile(structure.MeshGeometry, folder + "\\" + filename + ".stl");
      }
    }

    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method saves the given MeshGeometry3D to the given file in the PLY format
    /// also known as Polygon File Format or Stanfor Triangle Formatn
    /// </summary>
    /// <param name="mesh">Trianglemesh to export</param>
    /// <param name="col">Color of the mesh</param>
    /// <param name="outputFileName">Name of the file to write.</param>
    //---------------------------------------------------------------------------------------------
    void SaveTriangleMeshToPlyFile(MeshGeometry3D mesh, Color color, string outputFileName)
    {
      if (mesh == null)
        return;

      if (File.Exists(outputFileName))
      {
        File.SetAttributes(outputFileName, FileAttributes.Normal);
        File.Delete(outputFileName);
      }

      Point3DCollection vertexes = mesh.Positions;
      Int32Collection indexes = mesh.TriangleIndices;

      byte alpha = (byte)(0.6*255);

      using (TextWriter writer = new StreamWriter(outputFileName))
      {
        writer.WriteLine("ply");
        writer.WriteLine("format ascii 1.0");
        writer.WriteLine("element vertex " + vertexes.Count);

        writer.WriteLine("property float x");
        writer.WriteLine("property float y");
        writer.WriteLine("property float z");

        writer.WriteLine("property uchar red");
        writer.WriteLine("property uchar green");
        writer.WriteLine("property uchar blue");
        writer.WriteLine("property uchar alpha");

        writer.WriteLine("element face " + indexes.Count / 3);

        writer.WriteLine("property list uchar int vertex_indices");

        writer.WriteLine("end_header");

        foreach (Point3D v in vertexes)
        {
          writer.Write(v.X.ToString("e") + " ");
          writer.Write(v.Y.ToString("e") + " ");
          writer.Write(v.Z.ToString("e") + " ");

          writer.Write(color.R + " ");
          writer.Write(color.G + " ");
          writer.Write(color.B + " ");
          writer.Write(alpha);
          writer.WriteLine();
        }

        int i = 0;
        while (i < indexes.Count)
        {
          writer.Write("3 ");
          writer.Write(indexes[i++] + " ");
          writer.Write(indexes[i++] + " ");
          writer.Write(indexes[i++] + " ");
          writer.WriteLine();
        }
      }
    }

    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method saves the given MeshGeometry3D to the given file in the PLY format
    /// Calculates vertex normals required for poisson reconstruction
    /// </summary>
    /// <param name="mesh">Trianglemesh to export</param>
    /// <param name="outputFileName">Name of the file to write.</param>
    //---------------------------------------------------------------------------------------------
    void SaveTriangleMeshToPlyFile(MeshGeometry3D mesh, string outputFileName)
    {
      if (mesh == null)
        return;

      if (File.Exists(outputFileName))
      {
        File.SetAttributes(outputFileName, FileAttributes.Normal);
        File.Delete(outputFileName);
      }

      Point3DCollection vertexes = mesh.Positions;
      Int32Collection indexes = mesh.TriangleIndices;

      using (TextWriter writer = new StreamWriter(outputFileName))
      {
        writer.WriteLine("ply");
        writer.WriteLine("format ascii 1.0");
        writer.WriteLine("element vertex " + vertexes.Count);

        writer.WriteLine("property float x");
        writer.WriteLine("property float y");
        writer.WriteLine("property float z");
        writer.WriteLine("property float nx");
        writer.WriteLine("property float ny");
        writer.WriteLine("property float nz");

        writer.WriteLine("element face " + indexes.Count / 3);

        writer.WriteLine("property list uchar int vertex_indices");

        writer.WriteLine("end_header");

        for (int v = 0; v < vertexes.Count(); v++)
        {
          Vector3D normal = CalculateVertexNormal(mesh, v);

          writer.Write(vertexes[v].X.ToString("e") + " ");
          writer.Write(vertexes[v].Y.ToString("e") + " ");
          writer.Write(vertexes[v].Z.ToString("e") + " ");
          writer.Write(normal.X.ToString("e") + " ");
          writer.Write(normal.Y.ToString("e") + " ");
          writer.Write(normal.Z.ToString("e"));

          writer.WriteLine();
        }

        int i = 0;
        while (i < indexes.Count)
        {
          writer.Write("3 ");
          writer.Write(indexes[i++] + " ");
          writer.Write(indexes[i++] + " ");
          writer.Write(indexes[i++] + " ");
          writer.WriteLine();
        }
      }
    }

    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method saves the given MeshGeometry3D to the STL file format, including surface normals
    /// The file should be ready for 3D printing
    /// </summary>
    /// <param name="mesh">Trianglemesh to export</param>
    /// <param name="outputFileName">File path and name of the file to write.</param>
    //---------------------------------------------------------------------------------------------
    void SaveTriangleMeshtoStlFile(MeshGeometry3D mesh, string outputFileName)
    {
      if (mesh == null)
        return;

      if (File.Exists(outputFileName))
      {
        File.SetAttributes(outputFileName, FileAttributes.Normal);
        File.Delete(outputFileName);
      }

      Point3DCollection vertexes = mesh.Positions;
      Int32Collection indexes = mesh.TriangleIndices;

      Point3D p1, p2, p3;
      Vector3D n;

      string text;

      using (TextWriter writer = new StreamWriter(outputFileName))
      {
        writer.WriteLine("solid Bolus");

        for (int v = 0; v < mesh.TriangleIndices.Count(); v += 3)
        {
          //gather the 3 points for the face and the normal
          p1 = vertexes[indexes[v]];
          p2 = vertexes[indexes[v + 1]];
          p3 = vertexes[indexes[v + 2]];
          n = CalculateSurfaceNormal(p1, p2, p3);

          text = string.Format("facet normal {0} {1} {2}", n.X, n.Y, n.Z);
          writer.WriteLine(text);
          writer.WriteLine("outer loop");
          text = String.Format("vertex {0} {1} {2}", p1.X, p1.Y, p1.Z);
          writer.WriteLine(text);
          text = String.Format("vertex {0} {1} {2}", p2.X, p2.Y, p2.Z);
          writer.WriteLine(text);
          text = String.Format("vertex {0} {1} {2}", p3.X, p3.Y, p3.Z);
          writer.WriteLine(text);
          writer.WriteLine("endloop");
          writer.WriteLine("endfacet");

        }


      }
    }



    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method saves the given dose or image voxels to a file in the vtk structure points format.
    /// Note that either Dose or Image is given as parameter and the other must be null.
    /// </summary>
    /// <param name="dose">Dose to output or null</param>
    /// <param name="image">Image to output or null</param>
    /// <param name="outputFileName">>Name of the file to write</param>
    //---------------------------------------------------------------------------------------------
    public static void SaveDoseOrImageToVTKStructurePoints(Dose dose, Image image, string outputFileName)
    {
      if (File.Exists(outputFileName))
      {
        File.SetAttributes(outputFileName, FileAttributes.Normal);
        File.Delete(outputFileName);
      }

      int W, H, D;
      double sx, sy, sz;
      VVector origin, rowDirection, columnDirection;
      if (dose != null)
      {
        W = dose.XSize;
        H = dose.YSize;
        D = dose.ZSize;
        sx = dose.XRes;
        sy = dose.YRes;
        sz = dose.ZRes;
        origin = dose.Origin;
        rowDirection = dose.XDirection;
        columnDirection = dose.YDirection;
      }
      else
      {
        W = image.XSize;
        H = image.YSize;
        D = image.ZSize;
        sx = image.XRes;
        sy = image.YRes;
        sz = image.ZRes;
        origin = image.Origin;
        rowDirection = image.XDirection;
        columnDirection = image.YDirection;
      }

      using (TextWriter writer = new StreamWriter(outputFileName))
      {
        writer.WriteLine("# vtk DataFile Version 3.0");
        writer.WriteLine("vtk output");
        writer.WriteLine("ASCII");
        writer.WriteLine("DATASET STRUCTURED_POINTS");
        writer.WriteLine("DIMENSIONS " + W + " " + H + " " + D);

        int[,] buffer = new int[W, H];

        double xsign = rowDirection.x > 0 ? 1.0 : -1.0;
        double ysign = columnDirection.y > 0 ? 1.0 : -1.0;
        double zsign = GetZDirection(rowDirection, columnDirection).z > 0 ? 1.0 : -1.0;

        writer.WriteLine("ORIGIN " + origin.x.ToString() + " " + origin.y.ToString() + " " + origin.z.ToString());
        writer.WriteLine("SPACING " + sx * xsign + " " + sy * ysign + " " + sz * zsign);
        writer.WriteLine("POINT_DATA " + W * H * D);
        writer.WriteLine("SCALARS image_data unsigned_short 1");
        writer.WriteLine("LOOKUP_TABLE default");

        int maxValueForScaling = dose != null ? FindMaxValue(dose) : 0;

        for (int z = 0; z < D; z++)
        {
          if (dose != null) dose.GetVoxels(z, buffer);
          else image.GetVoxels(z, buffer);
          for (int y = 0; y < H; y++)
          {
            for (int x = 0; x < W; x++)
            {
              int value = buffer[x, y];
              UInt16 curvalue = 0;
              if (image != null)
                curvalue = (UInt16)value;
              else
                curvalue = (UInt16)((double)value * 100 / (double)maxValueForScaling);
              writer.Write(curvalue + " ");
            }
            writer.WriteLine();
          }
        }
      }
    }

    //---------------------------------------------------------------------------------------------
    /// <summary>
    /// This method finds the maximum vocel value of the dose matrix
    /// </summary>
    /// <param name="dose">Dose</param>
    /// <returns>Maximum value</returns>
    //---------------------------------------------------------------------------------------------
    static int FindMaxValue(Dose dose)
    {
      int maxValue = 0;
      int[,] buffer = new int[dose.XSize, dose.YSize];
      if (dose != null)
      {
        for (int z = 0; z < dose.ZSize; z++)
        {
          dose.GetVoxels(z, buffer);
          for (int y = 0; y < dose.YSize; y++)
          {
            for (int x = 0; x < dose.XSize; x++)
            {
              int value = buffer[x, y];
              if (value > maxValue)
                maxValue = value;
            }
          }
        }
      }
      return maxValue;
    }

    private static VVector GetZDirection(VVector a, VVector b)
    {
      // return cross product
      return new VVector(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }


    string MakeFilenameValid(string s)
    {
      char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
      foreach (char ch in invalidChars)
      {
        s = s.Replace(ch, '_');
      }
      return s;
    }

    //creates a face’s surface normal from the face’s three points
    Vector3D CalculateSurfaceNormal(Point3D p1, Point3D p2, Point3D p3)
    {
      Vector3D v1 = new Vector3D(0, 0, 0);             // Vector 1 (x,y,z) & Vector 2 (x,y,z)
      Vector3D v2 = new Vector3D(0, 0, 0);
      Vector3D normal = new Vector3D(0, 0, 0);

      // Finds The Vector Between 2 Points By Subtracting
      // The x,y,z Coordinates From One Point To Another.

      // Calculate The Vector From Point 2 To Point 1
      v1.X = p1.X - p2.X;                  // Vector 1.x=Vertex[0].x-Vertex[1].x
      v1.Y = p1.Y - p2.Y;                  // Vector 1.y=Vertex[0].y-Vertex[1].y
      v1.Z = p1.Z - p2.Z;                  // Vector 1.z=Vertex[0].y-Vertex[1].z
      // Calculate The Vector From Point 3 To Point 2
      v2.X = p2.X - p3.X;                  // Vector 1.x=Vertex[0].x-Vertex[1].x
      v2.Y = p2.Y - p3.Y;                  // Vector 1.y=Vertex[0].y-Vertex[1].y
      v2.Z = p2.Z - p3.Z;                  // Vector 1.z=Vertex[0].y-Vertex[1].z

      // Compute The Cross Product To Give Us A Surface Normal
      normal.X = v1.Y * v2.Z - v1.Z * v2.Y;   // Cross Product For Y - Z
      normal.Y = v1.Z * v2.X - v1.X * v2.Z;   // Cross Product For X - Z
      normal.Z = v1.X * v2.Y - v1.Y * v2.X;   // Cross Product For X - Y

      normal.Normalize();

      return normal;
    }

    //creates a normal for a single vertex by searching all faces it is connected with
    //then averaging the surface normal for those faces
    Vector3D CalculateVertexNormal(MeshGeometry3D mesh, int vertex)
    {
      Vector3D normal = new Vector3D(0, 0, 0);
      List<Vector3D> normals = new List<Vector3D>();

      //foreach triangle
      for (int i = 0; i < mesh.TriangleIndices.Count(); i += 3)
      {
        //foreach vertex
        for (int ii = 0; ii < 3; ii++)
        {
          //calculates and add the surface normal if the face uses that vertex
          if (mesh.TriangleIndices[i + ii] == vertex)
          {
            Vector3D surfaceNormal = CalculateSurfaceNormal(mesh.Positions[mesh.TriangleIndices[i]], mesh.Positions[mesh.TriangleIndices[i + 1]], mesh.Positions[mesh.TriangleIndices[i + 2]]);
            normals.Add(surfaceNormal);
          }
        }
      }

      //average the normals and normalize
      foreach (Vector3D v in normals)
      {
        normal += v;
      }

      normal = normal / normals.Count();

      normal.Normalize();

      return normal;
    }

  }
}
