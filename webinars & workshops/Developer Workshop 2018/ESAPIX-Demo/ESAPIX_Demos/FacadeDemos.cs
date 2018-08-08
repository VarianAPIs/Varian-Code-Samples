using ESAPIX.Common;
using ESAPIX.Facade.API;
using ESAPIX.Facade.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V = VMS.TPS.Common.Model.API;

namespace ESAPIX_Demos
{
    public class FacadeDemos
    {
        /// <summary>
        /// This demo shows that a task can be run on a different thread on an ESAPIX facade. Try doing this with 
        /// ESAPI to see a failure (see MultithreadFailDemo() below)
        /// </summary>
        /// <returns></returns>
        public async static Task MultithreadDemo()
        {
            //Wrap VMS in Facades (subtle ESAPIX injection)
            var sac = new StandAloneContext(() => V.Application.CreateApplication());
            var app = sac.Application;

            var patAsync = await Task.Run(() =>
            {
                return app.OpenPatientById(Globals.PatientId);
            });

            Console.WriteLine($"Opened {patAsync.FirstName} {patAsync.LastName} asynchronously");

            sac.Dispose();
        }

        /// <summary>
        /// This demo shows that a task CAN'T be run on a different thread on an ESAPI. 
        /// </summary>
        /// <returns></returns>
        public async static Task MultithreadFailDemo()
        {
            //Normal ESAPI (not ESAPIX)
            using (var app = V.Application.CreateApplication())
            {
                //GET READY TO CRASH ESAPI!
                var patAsync = await Task.Run(() =>
                {
                    return app.OpenPatientById(Globals.PatientId);
                });

                Console.WriteLine($"Opened {patAsync.FirstName} {patAsync.LastName} asynchronously");
            }
        }

        /// <summary>
        /// This demo shows how to serialize an ESAPIX facade object for unit testing or offline development
        /// </summary>
        public static void SerializeFacade()
        {
            //Wrap VMS in Facades (subtle ESAPIX injection)
            var sac = new StandAloneContext(() => V.Application.CreateApplication());

            var app = sac.Application;
            var pat = app.OpenPatientById(Globals.PatientId);
            var allPlans = pat.Courses.SelectMany(c => c.PlanSetups).ToList();
            //ESAPIX.Facade.API.PlanSetup
            var firstPlan = allPlans.First();
            Console.WriteLine($"Serializing {firstPlan.Id} plan with {firstPlan.Beams.Count()} beams");
            //To JSON string - Could write to a file or use for unit testing
            var json = FacadeSerializer.Serialize(firstPlan);

            //Bring it back (detatched)
            var deserializedPlan = FacadeSerializer.Deserialize<PlanSetup>(json);
            Console.WriteLine($"Deserialized {deserializedPlan.Id} plan with {deserializedPlan.Beams.Count()} beams");
            sac.Dispose();
        }

        /// <summary>
        /// Demonstrates how not to debug locals on ESAPIX thread
        /// </summary>
        public static void FacadeDebuggingLocalsFail()
        {
            //Wrap VMS in Facades (subtle ESAPIX injection)
            var sac = new StandAloneContext(() => V.Application.CreateApplication());

            var app = sac.Application;
            var pat = app.OpenPatientById(Globals.PatientId);
            var allPlans = pat.Courses.SelectMany(c => c.PlanSetups).ToList();
            //ESAPIX.Facade.API.PlanSetup
            var firstPlan = allPlans.First();
            //Put a breakpoint here and inspect firstPlan
            sac.Dispose();
        }

        /// <summary>
        /// Demonstrates how to debug locals on ESAPIX thread
        /// </summary>
        public static void FacadeDebuggingLocals()
        {
            //Wrap VMS in Facades (subtle ESAPIX injection)
            var sac = new StandAloneContext(() => V.Application.CreateApplication());
            //Invoke on ESAPIX thread
            sac.Thread.Invoke(() =>
            {
                var app = sac.Application;
                var pat = app.OpenPatientById(Globals.PatientId);
                var allPlans = pat.Courses.SelectMany(c => c.PlanSetups).ToList();
                //ESAPIX.Facade.API.PlanSetup
                var firstPlan = allPlans.First();
                //Put a breakpoint here and inspect firstPlan
            });
            sac.Dispose();
        }
    }
}
