using DoseMetrics.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoseMetrics.Events
{
    public class AddDoseMetricEvent:PubSubEvent<DoseMetricModel>
    {
    }
}
