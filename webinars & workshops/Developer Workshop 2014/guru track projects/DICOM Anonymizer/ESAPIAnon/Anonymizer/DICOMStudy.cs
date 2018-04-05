using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESAPIAnon
{
    public class DICOMStudy
    {
        public string ID { get; set; }
        public DateTime? Date { get; set; }
        public DICOMFileType Type { get; set; }
    }
}
