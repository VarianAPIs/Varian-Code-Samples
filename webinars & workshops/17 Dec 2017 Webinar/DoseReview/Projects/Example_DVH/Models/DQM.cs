using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example_DVH.Models
{
    class DQM
    {
        public bool dqmType { get; set; }//true= dose at volume and false = volume at dose
        public string dosValue { get; set; }
        public string volValue { get; set; }
        public bool dosType { get; set; }//true = absolute, false = relative
        public bool volType { get; set; }
        public string[] structureNames { get; set; }

        //set the dqms
        public static List<DQM> get_Vals(string template)
        {
            List<DQM> dqms = new List<Models.DQM>();
            if (template == "Prostate")
            {
                string[] ptvNames = new string[] { "PTV", "PTVprost SV marg" };
                string[] bladderNames = new string[] { "bladder", "Bladder", "BLADDER" };
                dqms.Add(new DQM
                {
                    dqmType = true,
                    volType = false,
                    volValue = "95",
                    dosType = false,
                    structureNames = ptvNames
                });
                dqms.Add(new DQM
                {
                    dqmType = true,
                    volType = false,
                    volValue = "1",
                    dosType = false,
                    structureNames = ptvNames
                });
                dqms.Add(new DQM
                {
                    dqmType = false,
                    volType = false,
                    dosValue = "3000",
                    dosType = false,
                    structureNames = bladderNames
                });
            }
            return dqms;
        }
    }
}
