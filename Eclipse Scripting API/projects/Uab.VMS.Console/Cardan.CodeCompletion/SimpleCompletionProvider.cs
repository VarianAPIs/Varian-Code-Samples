using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using VMS.TPS.Common.Model.Types;

namespace Cardan.CodeCompletion
{
    public class SimpleCompletionProvider
    {
        private ScriptCs.ScriptServices _service;
        private ICSharpCode.AvalonEdit.Document.TextDocument text;
        private EnumProvider eProvider;
        private StaticProvider sProvider;

        public SimpleCompletionProvider(ScriptCs.ScriptServices service, ICSharpCode.AvalonEdit.Document.TextDocument text)
        {
            this._service = service;
            this.text = text;
            eProvider = new EnumProvider();
            sProvider = new StaticProvider();
        }
        public static bool IsCompletionTrigger(System.Windows.Input.KeyEventArgs e)
        {
            return e.Key == Key.OemPeriod;
        }

        public IEnumerable<ISelectedCompletionData> GetCompletionData()
        {
            var lastLine = text.Lines.Last();
            var context = text.GetText(lastLine).TrimStart('>');
            var words = context.Split(' ');
            var last = words.Last().TrimEnd('.');

            List<CompletionData> data = new List<CompletionData>();
            if (eProvider.IsEnum(last, ref data))
            {
                return data;
            }

            else if (char.IsUpper(last[0]) && sProvider.IsClassName(last, ref data))
            {
                return data;
            }
            else
            {
                var result = _service.Executor.ExecuteScript(last);
                MethodInfo[] lMethods = new MethodInfo[0];
                if (result.ReturnValue != null)
                {
                    var type = result.ReturnValue.GetType();

                    Type[] genType;
                    if (LinqExtensionProvider.IsLinqType(type, out genType))
                    {
                        lMethods = LinqExtensionProvider.GetExtensions(type);
                    }
                    var extensions = type.GetExtensionMethods();
                    var props = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => char.IsUpper(p.Name[0]))
                        .Concat(extensions)
                        .Concat(lMethods)
                        .Select(p => p.Name)
                        .OrderBy(p => p)
                        .Distinct()
                        .Select(p => new CompletionData(p));

                    return props;
                }
            }
            return new CompletionData[0];
        }

    }
}
