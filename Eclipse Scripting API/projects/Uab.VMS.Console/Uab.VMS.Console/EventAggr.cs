using Microsoft.Practices.Prism.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Uab.VMS.Console
{
    public sealed class EventAggr
    {
        private static EventAggregator instance = null;
        private EventAggr() { }

        public static EventAggregator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventAggregator();
                }
                return instance;
            }
        }
    }
}

