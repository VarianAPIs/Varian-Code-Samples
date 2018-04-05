using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Uab.VMS.Console.Views
{
    public class MyOverloadInsightWindow : OverloadInsightWindow
    {
        int openPar = 0;
        int variableNum = 0;
        TextArea _area;

        public MyOverloadInsightWindow(TextArea a)
            : base(a)
        {
            _area = a;
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            //"("
            if (e.Key == Key.D9 && (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || (e.KeyboardDevice.IsKeyDown(Key.RightShift))))
            {
                openPar++;
            }
            //")"
            if (e.Key == Key.D0 && (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || (e.KeyboardDevice.IsKeyDown(Key.RightShift))))
            {
                if (openPar == 0) { this.Close(); }
                else
                {
                    openPar--;
                }
            }
            if (e.Key == Key.OemComma)
            {
                variableNum++;
            }
            SetPosition(_area.Caret.Position);
        }
    }
}
