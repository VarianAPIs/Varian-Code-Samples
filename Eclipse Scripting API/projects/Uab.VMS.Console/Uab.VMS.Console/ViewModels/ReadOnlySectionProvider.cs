using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uab.VMS.Console.ViewModels
{
    public class ReadOnlySectionProvider : IReadOnlySectionProvider
    {
        private TextArea _area;

        public ReadOnlySectionProvider(TextArea area)
        {
            _area = area;
        }

        public bool CanInsert(int offset)
        {
            var firstOffset = _area.Document.Lines.Last().Offset - 1;
            return offset >= firstOffset;
        }

        public IEnumerable<ICSharpCode.AvalonEdit.Document.ISegment> GetDeletableSegments(ICSharpCode.AvalonEdit.Document.ISegment segment)
        {
            var firstOffset = _area.Document.Lines
                .Reverse()
                .First(l => _area.Document.GetText(l)
                .StartsWith(">")).Offset;
            var lastOffset = _area.Document.Lines.Last().EndOffset;

            if (segment.Offset < firstOffset + 1)
            {
                return new List<Segment>();
            }

            var startingOffset = segment.Offset < firstOffset + 1 ? firstOffset + 1 : segment.Offset;
            var seg = new Segment()
            {
                EndOffset = segment.EndOffset,
                Length = segment.Length,
                Offset = segment.Offset
            };

            return new List<Segment>() { seg };
        }
    }
}
