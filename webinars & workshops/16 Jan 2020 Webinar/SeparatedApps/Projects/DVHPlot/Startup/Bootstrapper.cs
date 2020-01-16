using Autofac;
using DVHPlot.ViewModels;
using DVHPlot.Views;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace DVHPlot.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap(PlanSetup plan)
        {
            var builder = new ContainerBuilder();
            //initial view
            builder.RegisterType<MainView>().AsSelf();
            //viewmodels
            builder.RegisterType<MainViewModel>().AsSelf();
            builder.RegisterType<DVHViewModel>().AsSelf();
            builder.RegisterType<DVHSelectionViewModel>().AsSelf();
            
            //events 
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            //esapi data
            builder.RegisterInstance(plan);

            return builder.Build();
        }
    }
}
