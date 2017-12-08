using System;
using Microsoft.Practices.Unity;
using Service;
using AutoMapper;
using Model.Tasks.ViewModel;
using Model.Tasks.Models;
using Components.ExceptionHandlers;
using Infrastructure.IO;
using Data.Initial;
using Components.Logging;
using Models.Company.DatabaseViews;

namespace TaskManagement.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below. Make sure to add a Microsoft.Practices.Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your types here
            // container.RegisterType<IProductRepository, ProductRepository>();

            //container.RegisterType<AccountController>(new InjectionConstructor());
            //container.RegisterType<ApplicationDbContext, ApplicationDbContext>("New");

            container.RegisterType<IDataContext, LoginApplicationDbContext>(new PerRequestLifetimeManager());
            //container.RegisterType<IUnitOfWorkForService, UnitOfWork>(new PerRequestLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new PerRequestLifetimeManager());
            container.RegisterType<ILogger, LogActivity>();



            container.RegisterType<IComboHelpListService, ComboHelpListService>(new PerRequestLifetimeManager());
            container.RegisterType<IDARService, DARService>(new PerRequestLifetimeManager());
            container.RegisterType<IProjectService, ProjectService>(new PerRequestLifetimeManager());
            container.RegisterType<ITasksService, TasksService>(new PerRequestLifetimeManager());
            container.RegisterType<IUserTeamService, UserTeamService>(new PerRequestLifetimeManager());
            container.RegisterType<IExceptionHandler, ExceptionHandler>(new PerRequestLifetimeManager());
            container.RegisterType<INotificationService, NotificationService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ActivityLog>, Repository<ActivityLog>>();
            container.RegisterType<IRepository<Project>, Repository<Project>>(new PerRequestLifetimeManager());
            container.RegisterType<IRepository<Tasks>, Repository<Tasks>>(new PerRequestLifetimeManager());
            container.RegisterType<IRepository<DAR>, Repository<DAR>>(new PerRequestLifetimeManager());           
            container.RegisterType<IRepository<UserTeam>, Repository<UserTeam>>(new PerRequestLifetimeManager());
            container.RegisterType<IRepository<_Users>, Repository<_Users>>(new PerRequestLifetimeManager());
            container.RegisterType<IRepository<_Roles>, Repository<_Roles>>(new PerRequestLifetimeManager());
            container.RegisterType<IExceptionHandler, ExceptionHandler>(new PerRequestLifetimeManager());

            Mapper.Initialize(cfg => { cfg.CreateMap<TasksViewModel, Tasks>();
            cfg.CreateMap<Tasks, TasksViewModel>();
            cfg.CreateMap<Tasks, Tasks>();
            cfg.CreateMap<Project, Project>();
            cfg.CreateMap<ProjectViewModel, Project>();
            cfg.CreateMap<Project, ProjectViewModel>();
            cfg.CreateMap<DAR, DAR>();
            cfg.CreateMap<DAR, DARViewModel>();
            cfg.CreateMap<DARViewModel, DAR>();
            cfg.CreateMap<UserTeam, UserTeamViewModel>();
            cfg.CreateMap<UserTeamViewModel, UserTeam>();
            cfg.CreateMap<UserTeam, UserTeam>();

            });
            //Mapper.Initialize(cfg => cfg.CreateMap<Tasks, TasksViewModel>());

            //Mapper.Initialize(cfg => cfg.CreateMap<Tasks, Tasks>());
            //Mapper.Initialize(cfg => cfg.CreateMap<Project, Project>());
            //Mapper.Initialize(cfg => cfg.CreateMap<ProjectViewModel, Project>());
            //Mapper.Initialize(cfg => cfg.CreateMap<Project, ProjectViewModel>());
            //Mapper.Initialize(cfg => cfg.CreateMap<DAR, DAR>());

            //Mapper.Initialize(cfg => cfg.CreateMap<DAR, DARViewModel>());
            //Mapper.Initialize(cfg => cfg.CreateMap<DARViewModel, DAR>());

            //Mapper.Initialize(cfg => cfg.CreateMap<UserTeam, UserTeamViewModel>());
            //Mapper.Initialize(cfg => cfg.CreateMap<UserTeamViewModel, UserTeam>());
            //Mapper.Initialize(cfg => cfg.CreateMap<UserTeam, UserTeam>());
        }
    }
}
