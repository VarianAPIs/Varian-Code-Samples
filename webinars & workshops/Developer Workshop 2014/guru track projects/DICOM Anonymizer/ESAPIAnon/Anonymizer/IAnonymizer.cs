using EvilDICOM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESAPIAnon.Anonymizers
{
    public interface IAnonymizer
    {
        string Name { get; }
        void Anonymize(DICOMObject d);
    }
}
