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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using System.Xml.Linq;
using DemoApp.Model;

namespace DemoApp.DataAccess
{
    /// <summary>
    /// Represents a source of volumes in the application.
    /// </summary>
    public class VolumeRepository
    {
        #region Fields

        readonly List<Volume> _volumes;
        readonly List<Volume> _lowerboundvolumes;
        readonly List<Volume> _upperboundvolumes;
        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Creates a new repository of patients.
        /// </summary>
        /// <param name="volumeDataFile">DO NOT USE now -- The relative path to an XML resource file that contains volume data.</param>
        public VolumeRepository(string volumeDataFile)
        {
            _lowerboundvolumes = LoadLowerBoundVolumes(volumeDataFile);
            _upperboundvolumes = LoadUpperBoundVolumes(volumeDataFile);
            _volumes = MergeVolumes(_lowerboundvolumes,_upperboundvolumes);
        }

        #endregion // Constructor

        #region Public Interface

      
        /// <summary>
        /// Returns a shallow-copied list of all volumes in the repository.
        /// </summary>
        public List<Volume> GetAllVolumes()
        {
            return new List<Volume>(_volumes);
        }

        #endregion // Public Interface

        #region Private Helpers
        static List<Volume> LoadLowerBoundVolumes(string volumeDataFile)
        {
            List<Volume> volumes = new List<Volume>();
            if (string.IsNullOrEmpty(volumeDataFile))
            {
                for (int i = 0; i <= 100; i += 10)
                {
                    volumes.Add(Volume.CreateVolume("> " + i.ToString(), new float[] { i, float.MaxValue}));

                };               
            } 
            return volumes;
        }
        static List<Volume> LoadUpperBoundVolumes(string volumeDataFile)
        {
            List<Volume> volumes = new List<Volume>();
            if (string.IsNullOrEmpty(volumeDataFile))
            {
                for (int i = 10; i <= 100; i += 10)
                {
                    volumes.Add(Volume.CreateVolume("< " + i.ToString(), new float[] { 0, i}));
                };
            }
            return volumes;
        }
        static List<Volume> MergeVolumes(List<Volume> v1, List<Volume> v2)
        {
            List<Volume> volumes = new List<Volume>();
            volumes.AddRange(v1);
            volumes.AddRange(v2);
            return volumes;
        }
        static List<Volume> LoadVolumes(string volumeDataFile)
        {
            if (string.IsNullOrEmpty(volumeDataFile))
            {
                List<Volume> volumes = new List<Volume>();
                for (int i = 0; i <100; i+=10)
                {
                    volumes.Add(Volume.CreateVolume(">"+(i + 10).ToString(), new float[] { i, float.MaxValue }));
                    
                };
                return volumes;
            }
            // In a real application, the data would come from an external source,
            // but for this demo let's keep things simple and use a resource file.
            using (Stream stream = GetResourceStream(volumeDataFile))
            using (XmlReader xmlRdr = new XmlTextReader(stream))
                return
                    (from volumeElem in XDocument.Load(xmlRdr).Element("volumes").Elements("volume")
                     select Volume.CreateVolume(
                        (string)volumeElem.Attribute("Name"), new float[]{0,0}
                         )).ToList();
        }

        static Stream GetResourceStream(string resourceFile)
        {
            Uri uri = new Uri(resourceFile, UriKind.RelativeOrAbsolute);

            StreamResourceInfo info = Application.GetResourceStream(uri);
            if (info == null || info.Stream == null)
                throw new ApplicationException("Missing resource file: " + resourceFile);

            return info.Stream;
        }

        #endregion // Private Helpers
    }
}