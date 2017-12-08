using System;
using Microsoft.Practices.Unity;
using Web.Controllers;
using Data;
using Infrastructure.IO;
using Service;
using Components.ExceptionHandlers;
using AdminSetup.Models.Models;
using Components.Logging;
using AutoMapper;
using Models.Company.Models;
using AdminSetup.Models.ViewModels;
using Models.Company.ViewModels;
using Models.Company.DatabaseViews;
using Services.BasicSetup;

namespace AdminSetup.App_Start
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
            //container.RegisterType<UsersAdminController>(new InjectionConstructor());
            container.RegisterType<RolesAdminController>(new InjectionConstructor());
            container.RegisterType<ApplicationDbContext, ApplicationDbContext>("New");

            container.RegisterType<IDataContext, ApplicationDbContext>(new PerRequestLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new PerRequestLifetimeManager());
            container.RegisterType<ILogger, LogActivity>();

            container.RegisterType<IComboHelpListService, ComboHelpListService>(new PerRequestLifetimeManager());
            container.RegisterType<IExceptionHandler, ExceptionHandler>(new PerRequestLifetimeManager());
            container.RegisterType<IModuleService, ModuleService>(new PerRequestLifetimeManager());
            container.RegisterType<ISubModuleService, SubModuleService>(new PerRequestLifetimeManager());
            container.RegisterType<IAdminSetupService, AdminSetupService>(new PerRequestLifetimeManager());
            container.RegisterType<IControllerActionService, ControllerActionService>(new PerRequestLifetimeManager());
            container.RegisterType<IMenuService, MenuService>(new PerRequestLifetimeManager());
            container.RegisterType<IMvcControllerService, MvcControllerService>(new PerRequestLifetimeManager());
            container.RegisterType<INotificationService, NotificationService>(new PerRequestLifetimeManager());
            container.RegisterType<IRedirectService, RedirectService>(new PerRequestLifetimeManager());
            container.RegisterType<IRolesControllerActionService, RolesControllerActionService>(new PerRequestLifetimeManager());
            container.RegisterType<IRolesDivisionService, RolesDivisionService>(new PerRequestLifetimeManager());
            container.RegisterType<IRolesMenuService, RolesMenuService>(new PerRequestLifetimeManager());
            container.RegisterType<IRolesSiteService, RolesSiteService>(new PerRequestLifetimeManager());
            container.RegisterType<IUserBookMarkService, UserBookMarkService>(new PerRequestLifetimeManager());
            container.RegisterType<IUserPermissionService, UserPermissionService>(new PerRequestLifetimeManager());
            container.RegisterType<IUserRolesService, UserRolesService>(new PerRequestLifetimeManager());
            container.RegisterType<IUsersService, UsersService>(new PerRequestLifetimeManager());
            container.RegisterType<ISiteService, SiteService>(new PerRequestLifetimeManager());
            container.RegisterType<IDivisionService, DivisionService>(new PerRequestLifetimeManager());
            container.RegisterType<IGodownService, GodownService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<ActivityLog>, Repository<ActivityLog>>();
            container.RegisterType<IRepository<ControllerAction>, Repository<ControllerAction>>();
            container.RegisterType<IRepository<MvcController>, Repository<MvcController>>();
            container.RegisterType<IRepository<Menu>, Repository<Menu>>();
            container.RegisterType<IRepository<MenuSubModule>, Repository<MenuSubModule>>();
            container.RegisterType<IRepository<MenuModule>, Repository<MenuModule>>();
            container.RegisterType<IRepository<RolesControllerAction>, Repository<RolesControllerAction>>();
            container.RegisterType<IRepository<RolesDivision>, Repository<RolesDivision>>();
            container.RegisterType<IRepository<RolesMenu>, Repository<RolesMenu>>();
            container.RegisterType<IRepository<RolesSite>, Repository<RolesSite>>();
            container.RegisterType<IRepository<UserBookMark>, Repository<UserBookMark>>();
            container.RegisterType<IRepository<RolesSite>, Repository<RolesSite>>();
            container.RegisterType<IRepository<_Users>, Repository<_Users>>();
            container.RegisterType<IRepository<_Roles>, Repository<_Roles>>();
            container.RegisterType<IRepository<_Employee>, Repository<_Employee>>();
            container.RegisterType<IRepository<Site>, Repository<Site>>();
            container.RegisterType<IRepository<Division>, Repository<Division>>();
            container.RegisterType<IRepository<Godown>, Repository<Godown>>();


            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Site, SiteViewModel>();
                cfg.CreateMap<MvcController, MvcControllerViewModel>();
                cfg.CreateMap<MvcControllerViewModel, MvcController>();
                cfg.CreateMap<DocumentType, DocumentTypeViewModel>();
                cfg.CreateMap<RolesDivisionViewModel, RolesDivision>();
                cfg.CreateMap<RolesDivision, RolesDivisionViewModel>();
                cfg.CreateMap<RolesSiteViewModel, RolesSite>();
                cfg.CreateMap<RolesSite, RolesSiteViewModel>();
            });
            //Mapper.Initialize(cfg => cfg.CreateMap<MvcController, MvcControllerViewModel>());
            //Mapper.Initialize(cfg => cfg.CreateMap<MvcControllerViewModel, MvcController>());
            //Mapper.Initialize(cfg => cfg.CreateMap<DocumentType, DocumentTypeViewModel>());
            //Mapper.Initialize(cfg => cfg.CreateMap<RolesDivisionViewModel, RolesDivision>());
            //Mapper.Initialize(cfg => cfg.CreateMap<RolesDivision, RolesDivisionViewModel>());
            //Mapper.Initialize(cfg => cfg.CreateMap<RolesSiteViewModel, RolesSite>());
            //Mapper.Initialize(cfg => cfg.CreateMap<RolesSite, RolesSiteViewModel>());

        }
    }
}
