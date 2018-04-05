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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using DemoApp.DataAccess;
using DemoApp.Model;
using DemoApp.Properties;

namespace DemoApp.ViewModel
{
    /// <summary>
    /// A UI-friendly wrapper for a MPatient object.
    /// </summary>
    public class VolumeViewModel : ViewModelBase
    {
        #region Fields

        readonly Volume _volume;
        readonly VolumeRepository _volumeRepository;

        #endregion // Fields

        #region Constructor

        public VolumeViewModel(Volume volume, VolumeRepository volumeRepository)
        {
           if (volumeRepository == null)
                throw new ArgumentNullException("volumeRepository");

            _volume = volume;
            _volumeRepository = volumeRepository;
        }

        #endregion // Constructor

        #region Properties
        public string Key
        {
            get { return _volume.Key; }
        }

        public float[] Range
        {
            get { return _volume.Value; }
        }

        public override string DisplayName
        {
            get
            {
                return _volume.Key;
            }
        }
        public bool IsSelected
        {
            get
            {
                return _volume.IsSelected;
            }
            set
            {
                if (value != _volume.IsSelected)
                    _volume.IsSelected = value;
            }
        }
        #endregion
    }
}