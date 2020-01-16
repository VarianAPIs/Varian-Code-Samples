using Autofac;
using DoseMetricExample.ViewModels;
using DoseMetricExample.Views;
using DoseParameters;
using DVHPlot.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace DoseMetricExample.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap(PlanSetup plan)
        {
            var container = new ContainerBuilder();
            container.RegisterType<MainView>();
            //viewmodels
            container.RegisterType<MainViewModel>();
            container.RegisterType<DoseMetricSelectionViewModel>();
            container.RegisterType<DoseMetricViewModel>();
            container.RegisterType<DoseParametersViewModel>();
            container.RegisterType<DVHViewModel>();
            container.RegisterType<DVHSelectionViewModel>();
            //event
            container.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            //esapi data
            container.RegisterInstance<PlanSetup>(plan);
            return container.Build();
        }
    }
}
