[assembly: WebActivator.PreApplicationStartMethod(typeof(RussellGroup.Pims.Website.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(RussellGroup.Pims.Website.App_Start.NinjectWebCommon), "Stop")]

namespace RussellGroup.Pims.Website.App_Start
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Web.Mvc.FilterBindingSyntax;
    using RussellGroup.Pims.Website.Helpers;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IActiveDirectoryHelper>().To<ActiveDirectoryHelper>();

            kernel.BindFilter<AuthorizationFilter>(FilterScope.Action, 0)
                .WhenActionMethodHas<PimsAuthorizeAttribute>()
                .WithPropertyValueFromActionAttribute<PimsAuthorizeAttribute>("Roles", f => f.Roles);

            kernel.BindFilter<AuthorizationFilter>(FilterScope.Controller, 0)
                .WhenControllerHas<PimsAuthorizeAttribute>()
                .WithPropertyValueFromControllerAttribute<PimsAuthorizeAttribute>("Roles", f => f.Roles);
        }        
    }
}
