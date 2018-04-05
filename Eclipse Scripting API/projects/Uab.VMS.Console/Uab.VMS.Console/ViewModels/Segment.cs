using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uab.VMS.Console.ViewModels
{
    public class Segment : ISegment
    {
        public int EndOffset { get; set; }

        public int Length { get; set; }

        public int Offset { get; set; }
    }
}
