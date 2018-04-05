using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;

namespace Cardan.CodeCompletion
{
    public class InteractiveContainerHolder
    {
        private InteractiveTextContainer _tc;

        public InteractiveContainerHolder(TextEditor te)
        {
            _tc = new InteractiveTextContainer(te);
        }

        internal InteractiveTextContainer GetContainer()
        {
            return _tc;
        }
    }
}
