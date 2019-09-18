using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClasicConsole.Injection;
using ClasicConsole.Reports;
using Ninject;
using Ninject.Modules;
using IQueryProvider = ClasicConsole.Reports.IQueryProvider;

namespace ClasicConsole
{
    public sealed class ServicesNInjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IReportService>().To<ReportService>();
        }
    }

    public static class NinjectBulder
    {
        private static StandardKernel _appInjectKernel;
        public static StandardKernel Container => _appInjectKernel ?? (_appInjectKernel = CreateInjectionContainer());

       
        private static StandardKernel CreateInjectionContainer()
        {
            var kernel = new StandardKernel(new DalNInjectModule(),
                new QueriesNInjectModule(),
                new ServicesNInjectModule()
            );

            // bind the injection kernel to self with a singleton value
            kernel.Bind<StandardKernel>()
                .ToConstant<StandardKernel>(kernel)
                .InSingletonScope();

            kernel.Bind<IKernel>()
                .ToConstant<StandardKernel>(kernel)
                .InSingletonScope();
            QueryProvider appQueryProvider = new QueryProvider(kernel);
            kernel.Bind<IQueryProvider>()
                .ToConstant<QueryProvider>(appQueryProvider);
            // kernel.Bind<IReportService>().To<ReportService>();
            return kernel;
        }
    }
}
