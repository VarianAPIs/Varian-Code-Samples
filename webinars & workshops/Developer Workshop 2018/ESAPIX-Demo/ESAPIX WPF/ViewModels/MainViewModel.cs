using ESAPIX.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESAPIX.Facade.API;

namespace ESAPIX_WPF.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private IScriptContext _ctx;

        public MainViewModel(IScriptContext ctx)
        {
            _ctx = ctx;

            //Example data bind
            FillDetails(ctx.PlanSetup);
            //Handle plan changes (in standalone mode)
            ctx.PlanSetupChanged += FillDetails;
        }

        public void FillDetails(PlanSetup ps)
        {
            Id = ps?.Id;
            UID = ps?.UID;
            IsDoseCalculated = ps?.Dose != null;
            NBeams = ps?.Beams.Count();
        }

        private string id;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private string uid;

        public string UID
        {
            get { return uid; }
            set { SetProperty(ref uid, value); }
        }

        private int? nBeams;

        public int? NBeams
        {
            get { return nBeams; }
            set { SetProperty(ref nBeams, value); }
        }

        private bool isDoseCalculated;

        public bool IsDoseCalculated
        {
            get { return isDoseCalculated; }
            set { SetProperty(ref isDoseCalculated, value); }
        }
    }
}
