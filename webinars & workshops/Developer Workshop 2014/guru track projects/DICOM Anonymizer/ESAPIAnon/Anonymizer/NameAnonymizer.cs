using EvilDICOM.Core;
using EvilDICOM.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESAPIAnon.Anonymizers
{
    /// <summary>
    /// Removes all names from the DICOM File
    /// </summary>
    class NameAnonymizer : IAnonymizer
    {
        public string Name
        {
            get { return "Name Anonymizer"; }
        }

        public void Anonymize(DICOMObject d)
        {
            foreach (var name in d.FindAll(VR.PersonName))
            {
                d.Remove(name.Tag);
            }
        }
    }
}
