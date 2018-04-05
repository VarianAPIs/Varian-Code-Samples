using ICSharpCode.AvalonEdit.CodeCompletion;

namespace Cardan.CodeCompletion
{
    public interface ISelectedCompletionData : ICompletionData
    {
        bool IsSelected { get; }

        string SortText { get; }
    }
}
