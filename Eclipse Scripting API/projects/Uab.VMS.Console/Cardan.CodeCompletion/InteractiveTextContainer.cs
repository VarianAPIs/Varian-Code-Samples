using System;
using System.Linq;
using CodeCompleteTests;
using CodeCompleteTests.Extensions;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.CodeAnalysis.Text;
using TextChangeEventArgs = Microsoft.CodeAnalysis.Text.TextChangeEventArgs;

namespace Cardan.CodeCompletion
{
    public class InteractiveTextContainer : SourceTextContainer
    {
        private TextEditor _te;
        private InteractiveText _oldText;
        private InteractiveText _newText;

        public InteractiveTextContainer(TextEditor te)
        {
            _te = te;
            _te.Document.Changed += DocumentOnChanged;
            _te.Document.Changing += DocumentOnChanging;
            SetCurrent();
        }

        private void SetCurrent()
        {
            var codeLines = _te.Document.GetCodeLines();
            var codeText = codeLines.SelectMany(l => _te.Document.GetText(l).TrimStart('>') + Environment.NewLine);
            codeText = codeText.Take(codeText.Count() - Environment.NewLine.Length); //Remove last line break
            var text = new string(codeText.ToArray());
            _newText = new InteractiveText(text);
        }

        private void DocumentOnChanging(object sender, DocumentChangeEventArgs e)
        {
            _oldText = (InteractiveText)CurrentText;
        }

        private void DocumentOnChanged(object sender, DocumentChangeEventArgs e)
        {
            SetCurrent();
            var textChangeRange = new TextChangeRange(
                new TextSpan(e.Offset, e.RemovalLength),
                e.RemovalLength == 0 ? e.InsertionLength : e.RemovalLength);
            OnTextChanged(new TextChangeEventArgs(_oldText, CurrentText, textChangeRange));
        }

        private void OnTextChanged(TextChangeEventArgs textChangeEventArgs)
        {
            if (TextChanged != null)
            {
                if (textChangeEventArgs != null)
                    if (CurrentText != null)
                        TextChanged(this, new TextChangeEventArgs(_oldText, newText: CurrentText, changes: textChangeEventArgs.Changes));
            }
        }

        public override SourceText CurrentText
        {
            get { return _newText; }
        }

        public override event EventHandler<TextChangeEventArgs> TextChanged;

    }
}
