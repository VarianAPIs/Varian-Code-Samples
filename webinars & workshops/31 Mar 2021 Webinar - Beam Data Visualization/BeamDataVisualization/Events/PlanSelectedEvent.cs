using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace BeamDataVisualization.Events
{
    public class PlanSelectedEvent:PubSubEvent<PlanSetup>
    {
    }
}
