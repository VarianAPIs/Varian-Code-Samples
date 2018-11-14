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
            //Standard Functions
            StandardFunctionsDemo.CumulativeDVHExample();
            StandardFunctionsDemo.DoseAtVolumeExample();
            StandardFunctionsDemo.VolumeAtDoseExample();

            //ESAPIX
            ESAPIXFunctionsDemo.DifferentialDVHExample();
            ESAPIXFunctionsDemo.DoseAtVolumeExample();
            ESAPIXFunctionsDemo.VolumeAtDoseExample();

            //Plan Constraints
            PlanConstraintsDemo.GetAndRunPlanConstraints();
            PlanConstraintsDemo.CreateESAPIXConstraints();

            //Mining
            DVHMiningDemo.RunExample();

            //Plotting
            PlottingExample.PlotDVH();

            Console.Read();
        }
    }
}
