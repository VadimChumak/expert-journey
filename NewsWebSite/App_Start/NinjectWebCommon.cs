[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NewsWebSite.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NewsWebSite.App_Start.NinjectWebCommon), "Stop")]

namespace NewsWebSite.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Models;
    using NHibernate;
    using NHibernate.Tool.hbm2ddl;
    using NHibernate.Cfg;
    using Models.Repository;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using Microsoft.AspNet.SignalR;
    using NinjectDependencyResolvers;
    using Models.Services;

    public static class NinjectWebCommon
    {
        public static readonly Bootstrapper bootstrapper = new Bootstrapper();

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
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IArticleRepository>().To<ArticleRepository>();
            kernel.Bind<IUserRepository>().To<UserRepository>();
            kernel.Bind<ICommentsRepository>().To<CommentsRepository>();
            kernel.Bind<INotifiactionsRepository>().To<NotifiactionsRepository>();
            kernel.Bind<ITagRepository>().To<TagRepository>();
            kernel.Bind<UserManager<AppUser, int>>().To<UserManager<AppUser, int>>();
            kernel.Bind<SignInManager<AppUser, int>>().To<SignInManager<AppUser, int>>();
            kernel.Bind<IUserStore<AppUser, int>>().To<CustomUserStore>();
            kernel.Bind<NotificationsCountService>().To<NotificationsCountService>();
            kernel.Bind<ArticleService>().To<ArticleService>();

            kernel.Bind<IAuthenticationManager>().ToMethod(_ => HttpContext.Current.GetOwinContext().Authentication);

            kernel.Bind<ISessionFactory>().ToMethod(context =>
            {
                var configuration = new Configuration();

                configuration.Configure();
                configuration.AddAssembly(typeof(Article).Assembly);

                ISessionFactory sessionFactory = configuration.BuildSessionFactory();

                //Позволяет Nhibernate самому создавать в БД таблицу и поля к ним. 
                new SchemaUpdate(configuration).Execute(true, true);

                return sessionFactory;
            }).InSingletonScope();

            GlobalHost.DependencyResolver = new SignalRNinjectDependencyResolver(kernel);
        }
    }
}
