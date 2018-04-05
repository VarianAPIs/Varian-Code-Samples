using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESAPIAnon
{
    public enum DICOMFileType
    {
        RT_IMAGE,
        CT_IMAGE,
        MRI_IMAGE,
        PET_IMAGE,
        RT_DOSE,
        RT_PLAN,
        RT_STRUCT,
        REGISTRATION,
        OTHER
    }
}
