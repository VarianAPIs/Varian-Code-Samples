using System;
using System.Windows.Media;
using CodeCompleteTests;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;

namespace Cardan.CodeCompletion
{
    public class CompletionData : ISelectedCompletionData
    {
        private readonly CompletionItem _item;
        private object _description;

        public CompletionData(CompletionItem item)
        {
            _item = item;
            Text = item.DisplayText;
            Content = item.DisplayText;
        }

        public CompletionData(string text)
        {
            Text = text;
            Content = text;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public ImageSource Image { get; private set; }

        public string Text { get; private set; }

        public object Content { get; private set; }

        public object Description
        {
            get
            {
                if (_description == null && _item!=null)
                {
                    _description = _item.GetDescriptionAsync().Result.ToDisplayString();
                }
                return _description;
            }
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public double Priority { get; private set; }

        public bool IsSelected
        {
            get;
            set;
        }

        public string SortText
        {
            get { return Text; }
        }

        public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ICSharpCode.AvalonEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var change = Content;
            textArea.Document.Replace(completionSegment, (string)change);
        }
    }
}
