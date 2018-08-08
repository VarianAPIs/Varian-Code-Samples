using ESAPIX.Common;
using ESAPIX.Extensions;
using ESAPIX.Helpers;
using ESAPIX.Helpers.DVH;
using ESAPIX.Helpers.Filters;
using ESAPIX.Helpers.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V = VMS.TPS.Common.Model.API;
using SQ = ESAPIX.Helpers.DVH.StructureQuery;

namespace ESAPIX_Demos
{
    public class DVHDemo
    {
        public static void MayoQueryDemo()
        {
            //Wrap VMS in Facades (subtle ESAPIX injection)
            var sac = new StandAloneContext(() => V.Application.CreateApplication());
            sac.Thread.Invoke(() =>
            {
                var app = sac.Application;
                var pat = app.OpenPatientById(Globals.PatientId);
                var allPlans = pat.Courses.SelectMany(c => c.PlanSetups).ToList();
                //ESAPIX.Facade.API.PlanSetup
                var firstPlan = allPlans.First(pl => pl.Dose != null);
                var ptv = firstPlan.StructureSet.Structures.First(s => s.DicomType == MagicStrings.DICOMType.PTV);
                var mean = firstPlan.ExecuteQuery("Mean[Gy]", ptv);
                Console.WriteLine($"PTV Mean[Gy] = {mean}");
                var d99 = firstPlan.ExecuteQuery("D99%[Gy]", ptv);
                Console.WriteLine($"PTV D99%[Gy] = {d99}");
                var v95 = firstPlan.ExecuteQuery("V95%[%]", ptv);
                Console.WriteLine($"PTV V95%[%] = {v95}");
            });
        }

        /// <summary>
        /// Demonstrates the DVH mining capabilities of ESAPIX. Writes DVH data to file.
        /// </summary>
        public static void DVHMinerDemo()
        {
            var sac = new StandAloneContext(() => V.Application.CreateApplication());

            var miner = new Miner();
            miner.Logger.LogRequested += (msg) => { Console.WriteLine(msg); };
            miner.IncludePlanSums = true;
            miner.IncludeDVH(new DVHParams());
            //Find the structure set with the closest string match to the input (doesn't have to be exact)
            miner.SetStructureSetFilter("RSCH_Brain Dos", MatchType.CLOSEST_MATCH);

            //Add ids to miner
            CsvFile.Read(@"..\Desktop\patientIds.csv")
                .Column<string>("PatientId")
                .ToList()
                .ForEach(id => miner.AddPatientId(id));

            //Creates a CSV file containing the metrics requested
            var csv = miner.GetMetrics(sac.Application,
                new SQ("BRAIN", "Mean[Gy]", "D99%[Gy]"),
                new SQ("Cerebellum_ANT", "Mean[Gy]"),
                new SQ("Cerebellum_POST", "Mean[Gy]"),
                new SQ("Temporal_Inf Gy", "Mean[Gy]"),
                new SQ("Temporal_Mid Gy", "Mean[Gy]"),
                new SQ("Temporal_Sup Gy", "Mean[Gy]"),
                new SQ("TemporalLobe_MED", "Mean[Gy]"),
                new SQ("Vermis", "Mean[Gy]"));

            sac.Dispose();
            //Write CSV to file
            csv.Write(@"\\MyDesktop\patientData.csv");
        }
    }
}
