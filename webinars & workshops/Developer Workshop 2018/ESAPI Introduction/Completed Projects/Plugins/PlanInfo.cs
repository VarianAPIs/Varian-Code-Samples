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
            string name = context.Patient.Name;
            MessageBox.Show(name);
            //pull informatino from the plansetup.
            string msg = "";
            PlanSetup ps = context.PlanSetup;

            //prescription information from the plan.
            msg += String.Format("Number of Fractions: {0}\n", ps.NumberOfFractions);
            msg += String.Format("Dose per Fraction: {0}\n", ps.DosePerFraction);
            msg += String.Format("Total Dose: {0}\n", ps.TotalDose);

            //get approval history of the plan.
            msg += "Approval History\n";
            foreach(var approve in ps.ApprovalHistory)
            {
                msg += String.Format("\tStatus: {0} by {1} {2}\n"
                    , approve.ApprovalStatus, approve.UserId, approve.ApprovalDateTime);
            }
            //calculation model used
            msg += String.Format("Dose calculation Model: {0}\n", ps.PhotonCalculationModel);

            //iterate through the beams and get some information about these fields.
            msg += String.Format("Plan has {0} fields\n", ps.Beams.Count());
            foreach(Beam b in ps.Beams.OrderBy(o=>o.BeamNumber))
            {
                msg += String.Format("\t{0}: {1:F1}MU\n", b.Id, b.Meterset.Value);
                msg += String.Format("\t\tGantry: {0}; Collimator: {1}\n",
                    b.ControlPoints.First().GantryAngle, b.ControlPoints.First().CollimatorAngle);
            }

            //iterate information through the structures.
            msg += String.Format("Structure set {0} has {1} structures\n",
                ps.StructureSet.Id, ps.StructureSet.Structures.Count());
            foreach(Structure s in ps.StructureSet.Structures)
            {
                msg += String.Format("\t{0}: {1:F1}cc Type: {2}\n",
                    s.Id, s.Volume, s.DicomType);
            }

            //find the target volume of the plan.
            Structure target = ps.StructureSet.Structures.Single(o => o.Id == ps.TargetVolumeID);
            //D95 of the target volume
            msg += "Custom Dose Metrics\n";
            DoseValue d95 = ps.GetDoseAtVolume(target, 
                95, 
                VolumePresentation.Relative, 
                DoseValuePresentation.Absolute);
            msg += String.Format("\t{0}: D95 = {1}\n", target.Id, d95);
            //D2 of the target volume
            DoseValue d2 = ps.GetDoseAtVolume(target,
                2,
                VolumePresentation.Relative,
                DoseValuePresentation.Absolute);
            msg += String.Format("\t{0}: D2 = {1}\n", target.Id, d2);
            //maximum dose to the cord.
            Structure Cord = ps.StructureSet.Structures.Single(o => o.Id == "Cord");
            DoseValue cmax = ps.GetDoseAtVolume(
                Cord,
                0,
                VolumePresentation.Relative,
                DoseValuePresentation.Absolute);
            msg += String.Format("\t{0}: Max = {1}\n", Cord.Id, cmax);
            Structure parotid = ps.StructureSet.Structures.First(o => o.Id.Contains("Parotid"));
            double v30Gy = ps.GetVolumeAtDose(parotid,
                new DoseValue(3000, DoseValue.DoseUnit.cGy),
                VolumePresentation.Relative);
            msg += String.Format("\t{0}: D95 = {1:F2}%\n", parotid.Id, v30Gy);


            MessageBox.Show(msg);
            //export the target DVH to a CSV File.
            //declare file save path
            string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) +
                "\\dvh.csv";
            //get hte dvh of the target.
            DVHData dvh = ps.GetDVHCumulativeData(
                target,
                DoseValuePresentation.Absolute,
                VolumePresentation.Relative,
                1);
            //write the file.
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine("Dose, Volume");
                foreach(DVHPoint dvhd in dvh.CurveData)
                {
                    sw.WriteLine(String.Format("{0:F2},{1:F2}"
                        , dvhd.DoseValue.Dose, dvhd.Volume));
                }
            }

        }
    }
}

