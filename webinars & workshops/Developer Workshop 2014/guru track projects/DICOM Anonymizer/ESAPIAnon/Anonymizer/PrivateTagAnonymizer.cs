using EvilDICOM.Core;
using EvilDICOM.Core.Helpers;
using EvilDICOM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESAPIAnon.Anonymizers
{
    public class PrivateTagAnonymizer : IAnonymizer
    {
        public void Anonymize(DICOMObject d)
        {
            foreach (IDICOMElement el in d.AllElements.Where(e => ByteHelper.HexStringToByteArray(e.Tag.Group)[1] % 2 != 0))
            {
                d.Remove(el.Tag.CompleteID);
            }
        }

        public string Name
        {
            get { return "Private Tag Anonymizer"; }
        }
    }
}
