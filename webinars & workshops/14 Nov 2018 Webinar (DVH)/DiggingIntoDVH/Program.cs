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
            StandardFunctionsDemo.VolumeAtDoseExample();
            StandardFunctionsDemo.DoseAtVolumeExample();
            //StandardFunctionsDemo.CumulativeDVHExample();
        }
    }
}
