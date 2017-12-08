using System;
using Microsoft.Practices.Unity;
using Data.Customize;
using Infrastructure.IO;
using Models.Customize.Models;
using Services.Customize;
using Models.BasicSetup.Models;
using Services.BasicSetup;
using Models.Company.Models;
using Components.Logging;
using AutoMapper;
using Models.Customize.ViewModels;
using TimePlanValidator.ViewModels;
using Models.Company.ViewModels;
using Models.BasicSetup.ViewModels;
using Components.ExceptionHandlers;
using TimePlanValidator;
using TimePlanValidator.Models;
using Service;
using Models.Company.DatabaseViews;

namespace Customize.App_Start
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

            container.RegisterType<ApplicationDbContext, ApplicationDbContext>("New");

            container.RegisterType<IDataContext, ApplicationDbContext>(new PerRequestLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderHeaderCharge>, Repository<JobOrderHeaderCharge>>();
            container.RegisterType<IJobOrderHeaderChargeService, JobOrderHeaderChargeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderLineCharge>, Repository<JobOrderLineCharge>>();
            container.RegisterType<IJobOrderLineChargeService, JobOrderLineChargeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderHeader>, Repository<JobOrderHeader>>();
            container.RegisterType<IJobOrderHeaderService, JobOrderHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveHeader>, Repository<JobReceiveHeader>>();
            container.RegisterType<IJobReceiveHeaderService, JobReceiveHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveHeaderExtended>, Repository<JobReceiveHeaderExtended>>();
            container.RegisterType<IJobReceiveHeaderExtendedService, JobReceiveHeaderExtendedService>(new PerRequestLifetimeManager());

            container.RegisterType<IRecipeHeaderService, RecipeHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<ISubRecipeService, SubRecipeService>(new PerRequestLifetimeManager());

            container.RegisterType<IDyeingService, DyeingService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockLine>, Repository<StockLine>>();
            container.RegisterType<IRecipeLineService, RecipeLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderSettings>, Repository<JobOrderSettings>>();
            container.RegisterType<IJobOrderSettingsService, JobOrderSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveSettings>, Repository<JobReceiveSettings>>();
            container.RegisterType<IJobReceiveSettingsService, JobReceiveSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<CostCenter>, Repository<CostCenter>>();
            container.RegisterType<ICostCenterService, CostCenterService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<CostCenterStatus>, Repository<CostCenterStatus>>();
            container.RegisterType<ICostCenterStatusService, CostCenterStatusService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<LedgerAccount>, Repository<LedgerAccount>>();
            container.RegisterType<ILedgerAccountService, LedgerAccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<DocumentType>, Repository<DocumentType>>();
            container.RegisterType<IDocumentTypeService, DocumentTypeService>(new PerRequestLifetimeManager());

            container.RegisterType<ILogger, LogActivity>();
            container.RegisterType<IRepository<ActivityLog>, Repository<ActivityLog>>();

            container.RegisterType<IModificationCheck, ModificationCheck>();

            container.RegisterType<IRepository<Stock>, Repository<Stock>>();
            container.RegisterType<IStockService, StockService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockAdj>, Repository<StockAdj>>();
            container.RegisterType<IStockAdjService, StockAdjService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockHeader>, Repository<StockHeader>>();
            container.RegisterType<IStockHeaderService, StockHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockBalance>, Repository<StockBalance>>();
            container.RegisterType<IStockBalanceService, StockBalanceService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockProcess>, Repository<StockProcess>>();
            container.RegisterType<IStockProcessService, StockProcessService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Process>, Repository<Process>>();
            container.RegisterType<IProcessService, ProcessService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockProcessBalance>, Repository<StockProcessBalance>>();
            container.RegisterType<IStockProcessBalanceService, StockProcessBalanceService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderHeaderStatus>, Repository<JobOrderHeaderStatus>>();
            container.RegisterType<IJobOrderHeaderStatusService, JobOrderHeaderStatusService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderLineStatus>, Repository<JobOrderLineStatus>>();
            container.RegisterType<IJobOrderLineStatusService, JobOrderLineStatusService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveLineStatus>, Repository<JobReceiveLineStatus>>();
            container.RegisterType<IJobReceiveLineStatusService, JobReceiveLineStatusService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderLineExtended>, Repository<JobOrderLineExtended>>();
            container.RegisterType<IJobOrderLineExtendedService, JobOrderLineExtendedService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockLineExtended>, Repository<StockLineExtended>>();
            container.RegisterType<IStockLineExtendedService, StockLineExtendedService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PerkDocumentType>, Repository<PerkDocumentType>>();
            container.RegisterType<IPerkDocumentTypeService, PerkDocumentTypeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Perk>, Repository<Perk>>();
            container.RegisterType<IPerkService, PerkService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderLine>, Repository<JobOrderLine>>();
            container.RegisterType<IJobOrderLineService, JobOrderLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveLine>, Repository<JobReceiveLine>>();
            container.RegisterType<IJobReceiveLineService, JobReceiveLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IChargesCalculationService, ChargesCalculationService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<CalculationProduct>, Repository<CalculationProduct>>();
            container.RegisterType<ICalculationProductService, CalculationProductService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<CalculationFooter>, Repository<CalculationFooter>>();
            container.RegisterType<ICalculationFooterService, CalculationFooterService>(new PerRequestLifetimeManager());

            container.RegisterType<INotificationService, NotificationService>(new PerRequestLifetimeManager());

            container.RegisterType<ICustomizeComboHelpListService, CustomizeComboHelpListService>(new PerRequestLifetimeManager());

            container.RegisterType<IExceptionHandler, ExceptionHandler>();

            container.RegisterType<IDocumentValidation, DocumentValidation>();
            container.RegisterType<IRepository<DocumentTypeTimeExtension>, Repository<DocumentTypeTimeExtension>>();
            container.RegisterType<IRepository<DocumentTypeTimePlan>, Repository<DocumentTypeTimePlan>>();

            container.RegisterType<IRepository<Product>, Repository<Product>>();
            container.RegisterType<IRepository<Unit>, Repository<Unit>>();

            container.RegisterType<IRepository<Product>, Repository<Product>>();

            Mapper.Initialize(m => { m.CreateMap<JobOrderSettings, JobOrderSettingsViewModel>();
            m.CreateMap<JobOrderSettingsViewModel, JobOrderSettings>();
            m.CreateMap<Perk, PerkViewModel>();
            m.CreateMap<PerkViewModel, Perk>();
            m.CreateMap<JobOrderHeaderViewModel, DocumentUniqueId>();
            m.CreateMap<RecipeHeaderViewModel, DocumentUniqueId>();
            m.CreateMap<DyeingViewModel, DocumentUniqueId>();
            m.CreateMap<JobOrderHeader, DocumentUniqueId>();
            m.CreateMap<JobOrderHeader, JobOrderHeaderViewModel>();
            m.CreateMap<JobOrderHeaderViewModel, JobOrderHeader>();
            m.CreateMap<JobOrderHeader, JobOrderHeader>();
            m.CreateMap<JobReceiveHeader, DocumentUniqueId>();
            m.CreateMap<PerkViewModel, JobOrderPerk>();
            m.CreateMap<JobOrderPerk, JobOrderPerk>();
            m.CreateMap<JobOrderLine, JobOrderLineViewModel>();
            m.CreateMap<JobOrderLineViewModel, JobOrderLine>();
            m.CreateMap<HeaderChargeViewModel, JobOrderHeaderCharge>().ForMember("Id", x => x.Ignore());
            m.CreateMap<JobOrderHeaderCharge, HeaderChargeViewModel>().ForMember("Id", x => x.Ignore());
            m.CreateMap<LineChargeViewModel, JobOrderLineCharge>().ForMember("Id", x => x.Ignore());
            m.CreateMap<JobOrderLineCharge, LineChargeViewModel>().ForMember("Id", x => x.Ignore());
            m.CreateMap<JobOrderLineCharge, JobOrderLineCharge>();
            m.CreateMap<JobOrderHeaderCharge, JobOrderHeaderCharge>();
            m.CreateMap<HeaderChargeViewModel, HeaderChargeViewModel>();
            m.CreateMap<LineChargeViewModel, LineChargeViewModel>();
            m.CreateMap<CalculationFooterViewModel, HeaderChargeViewModel>();
            m.CreateMap<CalculationProductViewModel, LineChargeViewModel>();
            m.CreateMap<JobOrderHeader, RecipeHeaderViewModel>();
            m.CreateMap<RecipeHeaderViewModel, JobOrderHeader>();
            m.CreateMap<JobReceiveHeader, DyeingViewModel>();
            m.CreateMap<DyeingViewModel, JobReceiveHeader>();
            m.CreateMap<StockLine, RecipeLineViewModel>();
            m.CreateMap<RecipeLineViewModel, StockLine>();
            });
        }
    }
}
