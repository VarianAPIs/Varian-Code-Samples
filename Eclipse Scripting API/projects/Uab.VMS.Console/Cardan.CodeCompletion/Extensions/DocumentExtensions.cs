using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Document;

namespace CodeCompleteTests.Extensions
{
    public static class DocumentHelper
    {
        public static IEnumerable<DocumentLine> GetCodeLines(this TextDocument doc)
        {
            return doc.Lines.Where(l =>
            {
                var txt = doc.GetText(l);
                var codeStart = txt.StartsWith(">");
                if (codeStart)
                {
                    var codeEnd = txt.TrimEnd().EndsWith(";") || txt.EndsWith("}");
                    return codeEnd || l.NextLine == null;
                }
                return false;
            });
        }

        public static IEnumerable<DocumentLine> GetCodeLinesBefore(this TextDocument doc)
        {
            return doc.Lines.Where(l =>
            {
                var txt = doc.GetText(l);
                var codeStart = txt.StartsWith(">");
                if (codeStart)
                {
                    var codeEnd = txt.TrimEnd().EndsWith(";") || txt.EndsWith("}");
                    return codeEnd && l.NextLine != null;
                }
                return false;
            });
        }

        public static IEnumerable<DocumentLine> GetReplLines(this TextDocument doc)
        {
            return doc.Lines.Where(l =>
            {
                var txt = doc.GetText(l);
                var codeStart = txt.StartsWith(">");
                if (codeStart)
                {
                    var codeEnd = txt.TrimEnd().EndsWith(";") || txt.EndsWith("}");
                    return !codeEnd && l.NextLine != null;
                }
                return true;
            });
        }

        public static int GetReplTextLength(this TextDocument doc)
        {
            int count = 0;
            doc.GetReplLines().ToList().ForEach(l =>
            {
                count += l.TotalLength;
            });
            return count;
        }

        public static int GetCharactersBeforeCurrentLine(this TextDocument doc)
        {
            var count = 0;
            doc.Lines.ToList().ForEach(l =>
            {
                if (l.NextLine != null)
                {
                    count += l.TotalLength;
                }
            });
            return count;
        }
    }
}
