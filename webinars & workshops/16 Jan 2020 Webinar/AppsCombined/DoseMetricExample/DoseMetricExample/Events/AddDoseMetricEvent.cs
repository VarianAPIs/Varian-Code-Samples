using DoseMetricExample.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoseMetricExample.Events
{
    public class AddDoseMetricEvent : PubSubEvent<DoseMetricModel>
    {
    }
}
