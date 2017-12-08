using System;
using Microsoft.Practices.Unity;
using Infrastructure.IO;
using AutoMapper;
using Data;
using Components.Logging;
using Services.BasicSetup;
using Service;
using Components.ExceptionHandlers;
using Models.Reports.Models;
using Models.Company.Models;
using Models.Reports.ViewModels;
using Services.Reports;

namespace Reports.App_Start
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

            //container.RegisterType<ApplicationDbContext, ApplicationDbContext>("New");

            container.RegisterType<IDataContext, ApplicationDbContext>(new PerRequestLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new PerRequestLifetimeManager());
            container.RegisterType<IModificationCheck, ModificationCheck>(new PerRequestLifetimeManager());
            container.RegisterType<ILogger, LogActivity>(new PerRequestLifetimeManager());
            container.RegisterType<IExceptionHandler, ExceptionHandler>(new PerRequestLifetimeManager());
            container.RegisterType<IDocumentTypeService, DocumentTypeService>(new PerRequestLifetimeManager());
            container.RegisterType<IReportLineService, ReportLineService>(new PerRequestLifetimeManager());
            container.RegisterType<IReportUIDValuesService, ReportUIDValuesService>(new PerRequestLifetimeManager());
            container.RegisterType<IReportHeaderService, ReportHeaderService>(new PerRequestLifetimeManager());
            container.RegisterType<IReportHelpListService, ReportHelpListService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<ReportHeader>, Repository<ReportHeader>>();
            container.RegisterType<IRepository<ReportLine>, Repository<ReportLine>>();
            container.RegisterType<IRepository<ReportUIDValues>, Repository<ReportUIDValues>>();
            container.RegisterType<IRepository<ActivityLog>, Repository<ActivityLog>>();
            container.RegisterType<IRepository<DocumentType>, Repository<DocumentType>>();


            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<ReportHeader, ReportHeaderViewModel>();
                cfg.CreateMap<ReportHeaderViewModel, ReportHeader>();
                cfg.CreateMap<ReportLine, ReportLineViewModel>();
                cfg.CreateMap<ReportLineViewModel, ReportLine>();
            });
        }
    }
}

