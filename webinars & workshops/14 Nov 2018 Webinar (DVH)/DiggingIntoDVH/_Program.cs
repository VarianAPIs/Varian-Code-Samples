using ESAPIX.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiggingIntoDVH
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            DVHMiningDemo.RunExample();
            PlanConstraintsDemo.CreateESAPIXConstraints();
            PlottingExample.PlotDVH();
            PlanConstraintsDemo.GetAndRunPlanConstraints();
            StandardFunctionsDemo.VolumeAtDoseExample();
            StandardFunctionsDemo.DoseAtVolumeExample();
            //StandardFunctionsDemo.CumulativeDVHExample();
            Console.Read();
        }
    }
}
