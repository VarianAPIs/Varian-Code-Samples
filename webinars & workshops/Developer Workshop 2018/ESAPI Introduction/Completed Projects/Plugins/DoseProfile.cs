using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.IO;

[assembly: AssemblyVersion("1.0.0.1")]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            // TODO : Add here the code that is called when the script is launched from Eclipse.
            //get access to the plan.
            PlanSetup ps = context.PlanSetup;
            Dose d = ps.Dose;
            //create a vector where the dose profile will start.
            VVector start = new VVector();
            start.x = -100;//mm
            start.y = -100;//location from the dicom origin.
            start.z = 0;
            VVector end = new VVector();
            end.x = 100;
            end.y = start.y;
            end.z = start.z;
            //size of the doseprofile.
            double[] size = new double[201];
            DoseProfile dp = d.GetDoseProfile(start,
                end,
                size);

            //write this doseprofile to a csv file.
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) +
                "\\profile.csv";
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("Position,Dose");
                foreach(ProfilePoint pp in dp)
                {
                    sw.WriteLine(String.Format("{0},{1}", pp.Position.x, pp.Value));
                }
            }
            
        }
    }
}
