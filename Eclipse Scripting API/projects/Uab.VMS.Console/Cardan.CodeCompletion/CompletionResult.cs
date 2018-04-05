using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeCompleteTests;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace Cardan.CodeCompletion
{
    public class CompletionResult
    {
        private IList<ISelectedCompletionData> _completionData;
        private IOverloadProvider _overloadProvider;

        public CompletionResult(IList<ISelectedCompletionData> completionData, IOverloadProvider overloadProvider)
        {
            this._completionData = completionData;
            this._overloadProvider = overloadProvider;
        }

        public IOverloadProvider OverloadProvider
        {
            get { return _overloadProvider; }
        }

        public IList<ISelectedCompletionData> CompletionData
        {
            get { return _completionData; }
        }

        public void AddItem(string text)
        {
           CompletionData.Add(new CompletionData(text));
        }
    }
}
