using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using VMS.TPS.VisualScripting.ElementInterface;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

namespace PlanChecker
{
    // TODO: Replace the existing class name with your own class name.
    public class PlanCheckElement : VisualScriptElement
    {
        public PlanCheckElement() { }
        public PlanCheckElement(IVisualScriptElementRuntimeHost host) { }

        public override bool RequiresRuntimeConsole { get { return false; } }
        public override bool RequiresDatabaseModifications { get { return false; } }


        [ActionPackExecuteMethod]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<check> Execute(PlanSetup ps)
        {
            // TODO: Add your code here.
            List<check> check_list = new List<check>();
            check c1 = new check() { Name = "Target" };
            bool t_exists = !String.IsNullOrEmpty(ps.TargetVolumeID);
            c1.Evaluation = t_exists ? "Target Volume: " + ps.TargetVolumeID : "No Target Volume";
            c1.Result = t_exists ? "Pass" : "Fail";
            check_list.Add(c1);
            //is max dose inside target volume. 
            check c2 = new check() { Name = "Max Dose in Target" };
            Structure target = ps.StructureSet.Structures.First(x => x.Id == ps.TargetVolumeID);
            c2.Evaluation = target.IsPointInsideSegment(ps.Dose.DoseMax3DLocation) ?
                $"Max Dose {ps.Dose.DoseMax3D} is inside {target.Id}" : $"Max Dose {ps.Dose.DoseMax3D} not in target";
            c2.Result = target.IsPointInsideSegment(ps.Dose.DoseMax3DLocation) ? "Pass" : "Fail";
            check_list.Add(c2);
            if(m_options["Modality"] == "IMRT")
            {
                //check if the LMC MU = Beam's MU
                double dmax = 0;
                double fmu = 0;
                Beam b = ps.Beams.First();
                foreach(string line in b.CalculationLogs.First(x=>x.Category == "LMC").MessageLines)
                {
                    if(line.Contains("Maximum MU")) { dmax = Convert.ToDouble(line.Split('=').Last()); }
                    if(line.Contains("Lost MU factor")) { fmu = Convert.ToDouble(line.Split('=').Last()); }
                }
                double lmc_mu = dmax * fmu;
                double b_mu = b.Meterset.Value;
                bool compare = Math.Abs(lmc_mu - b_mu) < 0.05 * b_mu;
                check c3 = new check() { Name = "MU renormalization" };
                c3.Evaluation = $"LMC MU = {lmc_mu}; Beam MU = {b_mu}";
                c3.Result = compare ? "Pass" : "Fail";
                check_list.Add(c3);
            }
            return check_list;
        }

        public override string DisplayName
        {
            get
            {
                // TODO: Replace "Element Name" with the name that you want to be displayed in the Visual Scripting UI.
                return "Plan Checker 9000";
            }
        }

        IDictionary<string, string> m_options = new Dictionary<string, string>();
        public override void SetOption(string key, string value)
        {
            m_options.Add(key, value);
        }

        public override IEnumerable<KeyValuePair<string, IEnumerable<string>>> AllowedOptions
        {
            get
            {
                return new KeyValuePair<string, IEnumerable<string>>[] {
            new KeyValuePair<string, IEnumerable<string>>("Site", new string[] { "Prostate","Head and Neck","Lung" }),
            new KeyValuePair<string, IEnumerable<string>>("Modality", new string[]{"IMRT","VMAT","SRS"})
          };
            }
        }
        public class check
        {
            public string Name { get; set; }
            public string Evaluation { get; set; }
            public string Result { get; set; }
        }
    }
}
