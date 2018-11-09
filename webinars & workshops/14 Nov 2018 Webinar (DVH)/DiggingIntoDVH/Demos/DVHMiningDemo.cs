using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM = VMS.TPS.Common.Model.API;
using D = VMS.TPS.Common.Model.Types.DoseValuePresentation;
using V = VMS.TPS.Common.Model.Types.VolumePresentation;
using static ESAPIX.Helpers.Filters.MatchType;
using SQ = ESAPIX.Helpers.DVH.StructureQuery;

namespace DiggingIntoDVH
{
    class DVHMiningDemo
    {
        public static void RunExample()
        {
            using (var app = VM.Application.CreateApplication())
            {
                var miner = new ESAPIX.Helpers.DVH.Miner();
                miner.AddPatientId("DA00005");
                miner.SetStructureSetFilter("ABDOM", CLOSEST_MATCH);
                var csv = miner.GetMetrics(app, new SQ("PTV_3000", "D95%[Gy]", "D99%[Gy]"));
                csv.Write(@"D:\Examples\minerExample.csv");
            }
        }
    }
}
