using ScriptCs.Contracts;
using Uab.VMS.Console.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Uab.VMS.Console
{
    public class WPFConsoleRelay : IConsole
    {
        public void Write(object value)
        {
            EventAggr.Instance.GetEvent<WriteEvent>().Publish(value.ToString());
        }

        public void Write(string value){
            EventAggr.Instance.GetEvent<WriteEvent>().Publish(value);
        }

        public void WriteLine()
        {
            EventAggr.Instance.GetEvent<WriteLineEvent>().Publish(string.Empty);
        }

        public void WriteLine(object value)
        {
            EventAggr.Instance.GetEvent<WriteLineEvent>().Publish(value.ToString());
        }

        public void WriteLine(string value)
        {
            EventAggr.Instance.GetEvent<WriteLineEvent>().Publish(value);
        }

        public string ReadLine()
        {
            AutoResetEvent ar = new AutoResetEvent(false);
            var line = string.Empty;
            EventAggr.Instance.GetEvent<LineReadyEvent>().Subscribe((readyLine) =>
            {
                ar.Set();
                line = readyLine.Replace(">", "");
            });
            ar.WaitOne();
            return line;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public void ResetColor()
        {
            EventAggr.Instance.GetEvent<ForegroundColorSetEvent>().Publish(ConsoleColor.White);
        }

        private ConsoleColor _color = ConsoleColor.White;
        public ConsoleColor ForegroundColor
        {
            get { return _color; }
            set
            {
                _color = value;
                EventAggr.Instance.GetEvent<ForegroundColorSetEvent>().Publish(value);
            }
        }

        public event EventHandler WriteLineHandler;
    }
}
