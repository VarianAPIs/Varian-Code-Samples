using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Cardan.CodeCompletion
{
    public class InteractiveText : SourceText
    {
        private string _text = string.Empty;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
            }
        }

        public InteractiveText(string text)
        {
            _text = text;
        }

        public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            Text.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }

        public override int Length
        {
            get { return Text.Length; }
        }

        public override char this[int position]
        {
            get { return Text[position]; }
        }
    }
}
