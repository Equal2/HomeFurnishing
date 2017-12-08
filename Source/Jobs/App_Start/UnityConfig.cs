using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
//using Jobs.Controllers;
using Data.Models;
using Data.Infrastructure;
using Service;
using Model.Models;
using AutoMapper;
using Model.ViewModel;
using Model.ViewModels;
using Model.Tasks.Models;
using Components.Logging;
using Components.ExceptionHandlers;
using Model.Tasks.ViewModel;
using Model.DatabaseViews;

//using Models.Company.ViewModels;

//using Services.BasicSetup;
//using Models.Company.ViewModels;

//using Services.BasicSetup;



namespace Jobs.App_Start
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

            container.RegisterType<IRolePermissionService, RolePermissionService>(new PerRequestLifetimeManager());
            container.RegisterType<IUserRolesNewService, UserRolesNewService>(new PerRequestLifetimeManager());

            container.RegisterType<IDisplay_SaleOrderInventoryStatusService, Display_SaleOrderInventoryStatusService>(new PerRequestLifetimeManager());

            container.RegisterType<IDisplay_JobInvoiceSummaryService, Display_JobInvoiceSummaryService>(new PerRequestLifetimeManager());
            container.RegisterType<IDisplay_SaleOrderBalanceService, Display_SaleOrderBalanceService>(new PerRequestLifetimeManager());

            container.RegisterType<IMvcControllerService, MvcControllerService>(new PerRequestLifetimeManager());

            container.RegisterType<Jobs.Controllers.RolesAdminController>(new InjectionConstructor());
            container.RegisterType<ApplicationDbContext, ApplicationDbContext>("New");

            container.RegisterType<IDashBoardAutoService, DashBoardAutoService>(new PerRequestLifetimeManager());

            container.RegisterType<IDisplay_ProdOrderBalanceService, Display_ProdOrderBalanceService>(new PerRequestLifetimeManager());
            container.RegisterType<IDisplay_PaymentAdivceWithInvoiceService, Display_PaymentAdivceWithInvoiceService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Colour>, Repository<Colour>>();
            container.RegisterType<IColourService, ColourService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleOrderHeader>, Repository<SaleOrderHeader>>();
            container.RegisterType<Service.ISaleOrderHeaderService, Service.SaleOrderHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleOrderLine>, Repository<SaleOrderLine>>();
            container.RegisterType<Service.ISaleOrderLineService, Service.SaleOrderLineService>(new PerRequestLifetimeManager());




            container.RegisterType<IRepository<SaleInvoiceHeaderCharge>, Repository<SaleInvoiceHeaderCharge>>();
            container.RegisterType<ISaleInvoiceHeaderChargeService, SaleInvoiceHeaderChargeService>(new PerRequestLifetimeManager());
            
            container.RegisterType<Jobs.Controllers.AccountController>(new InjectionConstructor());
            //container.RegisterType<ApplicationDbContext, ApplicationDbContext>("New");

            container.RegisterType<IDisplay_JobOrderBalanceService, Display_JobOrderBalanceService>(new PerRequestLifetimeManager());

            container.RegisterType<IDataContext, ApplicationDbContext>(new PerRequestLifetimeManager());
            container.RegisterType<IUnitOfWorkForService, UnitOfWork>(new PerRequestLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<BusinessEntity>, Repository<BusinessEntity>>();
            container.RegisterType<IBusinessEntityService, BusinessEntityService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<LedgerAccount>, Repository<LedgerAccount>>();
            container.RegisterType<IAccountService, AccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IExceptionHandlingService, ExceptionHandlingService>(new PerRequestLifetimeManager());           

            container.RegisterType<IRepository<JobOrderHeader>, Repository<JobOrderHeader>>();
            container.RegisterType<IJobOrderHeaderService, JobOrderHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderLine>, Repository<JobOrderLine>>();
            container.RegisterType<IJobOrderLineService, JobOrderLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderSettings>, Repository<JobOrderSettings>>();
            container.RegisterType<IJobOrderSettingsService, JobOrderSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobInvoiceSettings>, Repository<JobInvoiceSettings>>();
            container.RegisterType<IJobInvoiceSettingsService, JobInvoiceSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobConsumptionSettings>, Repository<JobConsumptionSettings>>();
            container.RegisterType<IJobConsumptionSettingsService, JobConsumptionSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveSettings>, Repository<JobReceiveSettings>>();
            container.RegisterType<IJobReceiveSettingsService, JobReceiveSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveHeader>, Repository<JobReceiveHeader>>();
            container.RegisterType<IJobReceiveHeaderService, JobReceiveHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveLine>, Repository<JobReceiveLine>>();
            container.RegisterType<IJobReceiveLineService, JobReceiveLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveQAAttribute>, Repository<JobReceiveQAAttribute>>();
            container.RegisterType<IJobReceiveQAAttributeService, JobReceiveQAAttributeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveQAPenalty>, Repository<JobReceiveQAPenalty>>();
            container.RegisterType<IJobReceiveQAPenaltyService, JobReceiveQAPenaltyService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<JobReturnHeader>, Repository<JobReturnHeader>>();
            container.RegisterType<IJobReturnHeaderService, JobReturnHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReturnLine>, Repository<JobReturnLine>>();
            container.RegisterType<IJobReturnLineService, JobReturnLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveBom>, Repository<JobReceiveBom>>();
            container.RegisterType<IJobReceiveBomService, JobReceiveBomService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveByProduct>, Repository<JobReceiveByProduct>>();
            container.RegisterType<IJobReceiveByProductService, JobReceiveByProductService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderBom>, Repository<JobOrderBom>>();
            container.RegisterType<IJobOrderBomService, JobOrderBomService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobInvoiceHeader>, Repository<JobInvoiceHeader>>();
            container.RegisterType<IJobInvoiceHeaderService, JobInvoiceHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobInvoiceLine>, Repository<JobInvoiceLine>>();
            container.RegisterType<IJobInvoiceLineService, JobInvoiceLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderCancelHeader>, Repository<JobOrderCancelHeader>>();
            container.RegisterType<IJobOrderCancelHeaderService, JobOrderCancelHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderCancelLine>, Repository<JobOrderCancelLine>>();
            container.RegisterType<IJobOrderCancelLineService, JobOrderCancelLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderAmendmentHeader>, Repository<JobOrderAmendmentHeader>>();
            container.RegisterType<IJobOrderAmendmentHeaderService, JobOrderAmendmentHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobOrderRateAmendmentLine>, Repository<JobOrderRateAmendmentLine>>();
            container.RegisterType<IJobOrderRateAmendmentLineService, JobOrderRateAmendmentLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobInvoiceAmendmentHeader>, Repository<JobInvoiceAmendmentHeader>>();
            container.RegisterType<IJobInvoiceAmendmentHeaderService, JobInvoiceAmendmentHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobInvoiceRateAmendmentLine>, Repository<JobInvoiceRateAmendmentLine>>();
            container.RegisterType<IJobInvoiceRateAmendmentLineService, JobInvoiceRateAmendmentLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobWorker>, Repository<JobWorker>>();
            container.RegisterType<IJobWorkerService, JobWorkerService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonAddress>, Repository<PersonAddress>>();
            container.RegisterType<IPersonAddressService, PersonAddressService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonProcess>, Repository<PersonProcess>>();
            container.RegisterType<IPersonProcessService, PersonProcessService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonRegistration>, Repository<PersonRegistration>>();
            container.RegisterType<IPersonRegistrationService, PersonRegistrationService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Person>, Repository<Person>>();
            container.RegisterType<IPersonService, PersonService>(new PerRequestLifetimeManager()); //

            container.RegisterType<IRepository<ProductUid>, Repository<ProductUid>>();
            container.RegisterType<IProductUidService, ProductUidService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<RateConversionSettings>, Repository<RateConversionSettings>>();
            container.RegisterType<IRateConversionSettingsService, RateConversionSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockHeader>, Repository<StockHeader>>();
            container.RegisterType<IStockHeaderService, StockHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockLine>, Repository<StockLine>>();
            container.RegisterType<IStockLineService, StockLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IReportLineService, ReportLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IActivityLogService, ActivityLogService>(new PerRequestLifetimeManager());

            container.RegisterType<IJobOrderHeaderChargeService, JobOrderHeaderChargeService>(new PerRequestLifetimeManager());

            container.RegisterType<IJobOrderLineChargeService, JobOrderLineChargeService>(new PerRequestLifetimeManager());

            container.RegisterType<IChargesCalculationService, ChargesCalculationService>(new PerRequestLifetimeManager());            

            container.RegisterType<IJobInvoiceHeaderChargeService, JobInvoiceHeaderChargeService>(new PerRequestLifetimeManager());

            container.RegisterType<IJobInvoiceLineChargeService, JobInvoiceLineChargeService>(new PerRequestLifetimeManager());

            container.RegisterType<IDuplicateDocumentCheckService, DuplicateDocumentCheckService>(new PerRequestLifetimeManager());

            container.RegisterType<IPersonContactService, PersonContactService>(new PerRequestLifetimeManager());

            container.RegisterType<IPersonBankAccountService, PersonBankAccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IRateListService, RateListService>(new PerRequestLifetimeManager());

            container.RegisterType<IExcessJobReviewService, ExcessJobReviewService>(new PerRequestLifetimeManager());
            container.RegisterType<IUpdateJobExpiryService, UpdateJobExpiryService>(new PerRequestLifetimeManager());

            container.RegisterType<IJobOrderInspectionRequestHeaderService, JobOrderInspectionRequestHeaderService>(new PerRequestLifetimeManager());
            container.RegisterType<IJobOrderInspectionRequestLineService, JobOrderInspectionRequestLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IJobOrderInspectionRequestSettingsService, JobOrderInspectionRequestSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IJobOrderInspectionSettingsService, JobOrderInspectionSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IJobOrderInspectionRequestCancelHeaderService, JobOrderInspectionRequestCancelHeaderService>(new PerRequestLifetimeManager());
            container.RegisterType<IJobOrderInspectionRequestCancelLineService, JobOrderInspectionRequestCancelLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IJobOrderInspectionHeaderService, JobOrderInspectionHeaderService>(new PerRequestLifetimeManager());
            container.RegisterType<IJobOrderInspectionLineService, JobOrderInspectionLineService>(new PerRequestLifetimeManager());



            //////////////////////////////////////////Finance Services///////////////////////////////////////////////////

            container.RegisterType<IRepository<City>, Repository<City>>();
            container.RegisterType<ICityService, CityService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<CostCenter>, Repository<CostCenter>>();
            container.RegisterType<ICostCenterService, CostCenterService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Country>, Repository<Country>>();
            container.RegisterType<ICountryService, CountryService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Currency>, Repository<Currency>>();
            container.RegisterType<ICurrencyService, CurrencyService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Division>, Repository<Division>>();
            container.RegisterType<IDivisionService, DivisionService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<DocumentCategory>, Repository<DocumentCategory>>();
            container.RegisterType<IDocumentCategoryService, DocumentCategoryService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<DocumentType>, Repository<DocumentType>>();
            container.RegisterType<IDocumentTypeService, DocumentTypeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<LedgerAccount>, Repository<LedgerAccount>>();
            container.RegisterType<IAccountService, AccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<LedgerAccount>, Repository<LedgerAccount>>();
            container.RegisterType<ILedgerAccountService, LedgerAccountService>(new PerRequestLifetimeManager());

            container.RegisterType<ILedgerAccountOpeningService, LedgerAccountOpeningService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<LedgerAccountGroup>, Repository<LedgerAccountGroup>>();
            container.RegisterType<ILedgerAccountGroupService, LedgerAccountGroupService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Ledger>, Repository<Ledger>>();
            container.RegisterType<ILedgerService, LedgerService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<LedgerHeader>, Repository<LedgerHeader>>();
            container.RegisterType<ILedgerHeaderService, LedgerHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Narration>, Repository<Narration>>();
            container.RegisterType<INarrationService, NarrationService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonAddress>, Repository<PersonAddress>>();
            container.RegisterType<IPersonAddressService, PersonAddressService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonBankAccount>, Repository<PersonBankAccount>>();
            container.RegisterType<IPersonBankAccountService, PersonBankAccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Person>, Repository<Person>>();
            container.RegisterType<IPersonService, PersonService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonContact>, Repository<PersonContact>>();
            container.RegisterType<IPersonContactService, PersonContactService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonContactType>, Repository<PersonContactType>>();
            container.RegisterType<IPersonContactTypeService, PersonContactTypeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Reason>, Repository<Reason>>();
            container.RegisterType<IReasonService, ReasonService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Site>, Repository<Site>>();
            container.RegisterType<ISiteService, SiteService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<State>, Repository<State>>();
            container.RegisterType<IStateService, StateService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<TdsCategory>, Repository<TdsCategory>>();
            container.RegisterType<ITdsCategoryService, TdsCategoryService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<TdsGroup>, Repository<TdsGroup>>();
            container.RegisterType<ITdsGroupService, TdsGroupService>(new PerRequestLifetimeManager());

            container.RegisterType<IReportLineService, ReportLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IFinancialDisplayService, FinancialDisplayService>(new PerRequestLifetimeManager());

            container.RegisterType<ITrialBalanceService, TrialBalanceService>(new PerRequestLifetimeManager());

            container.RegisterType<ITrialBalanceSettingService, TrialBalanceSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IActivityLogService, ActivityLogService>(new PerRequestLifetimeManager());

            container.RegisterType<ILedgerSettingService, LedgerSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<ILedgerLineRefValueService, LedgerLineRefValueService>(new PerRequestLifetimeManager());

            //////////////////////////////////////////End Finance Services///////////////////////////////////////////////////



            //////////////////////////////////////////Sales Services///////////////////////////////////////////////////


            container.RegisterType<IRepository<SaleOrderQtyAmendmentLine>, Repository<SaleOrderQtyAmendmentLine>>();
            container.RegisterType<ISaleOrderQtyAmendmentLineService, SaleOrderQtyAmendmentLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleOrderSettings>, Repository<SaleOrderSettings>>();
            container.RegisterType<ISaleOrderSettingsService, SaleOrderSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleQuotationSettings>, Repository<SaleQuotationSettings>>();
            container.RegisterType<ISaleQuotationSettingsService, SaleQuotationSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleOrderAmendmentHeader>, Repository<SaleOrderAmendmentHeader>>();
            container.RegisterType<ISaleOrderAmendmentHeaderService, SaleOrderAmendmentHeaderService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<SaleOrderCancelHeader>, Repository<SaleOrderCancelHeader>>();
            container.RegisterType<ISaleOrderCancelHeaderService, SaleOrderCancelHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleOrderCancelLine>, Repository<SaleOrderCancelLine>>();
            container.RegisterType<ISaleOrderCancelLineService, SaleOrderCancelLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleOrderHeader>, Repository<SaleOrderHeader>>();
            container.RegisterType<Service.ISaleOrderHeaderService, Service.SaleOrderHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleOrderLine>, Repository<SaleOrderLine>>();
            container.RegisterType<Service.ISaleOrderLineService, Service.SaleOrderLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleQuotationHeader>, Repository<SaleQuotationHeader>>();
            container.RegisterType<Service.ISaleQuotationHeaderService, Service.SaleQuotationHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleQuotationHeaderDetail>, Repository<SaleQuotationHeaderDetail>>();
            container.RegisterType<Service.ISaleQuotationHeaderDetailService, Service.SaleQuotationHeaderDetailService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleQuotationLine>, Repository<SaleQuotationLine>>();
            container.RegisterType<Service.ISaleQuotationLineService, Service.SaleQuotationLineService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleQuotationHeaderChargeService, SaleQuotationHeaderChargeService>(new PerRequestLifetimeManager());
            container.RegisterType<ISaleQuotationLineChargeService, SaleQuotationLineChargeService>(new PerRequestLifetimeManager());



            container.RegisterType<IRepository<SaleEnquiryHeader>, Repository<SaleEnquiryHeader>>();
            container.RegisterType<Service.ISaleEnquiryHeaderService, Service.SaleEnquiryHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleEnquiryLine>, Repository<SaleEnquiryLine>>();
            container.RegisterType<Service.ISaleEnquiryLineService, Service.SaleEnquiryLineService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleEnquirySettingsService, SaleEnquirySettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IProductBuyerSettingsService, ProductBuyerSettingsService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<DispatchWaybillHeader>, Repository<DispatchWaybillHeader>>();
            container.RegisterType<Service.IDispatchWaybillHeaderService, Service.DispatchWaybillHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ChargeGroupPerson>, Repository<ChargeGroupPerson>>();
            container.RegisterType<IChargeGroupPersonService, ChargeGroupPersonService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<DispatchWaybillLine>, Repository<DispatchWaybillLine>>();
            container.RegisterType<Service.IDispatchWaybillLineService, Service.DispatchWaybillLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductBuyer>, Repository<ProductBuyer>>();
            container.RegisterType<IProductBuyerService, ProductBuyerService>(new PerRequestLifetimeManager());



            container.RegisterType<IRepository<ProductUid>, Repository<ProductUid>>();
            container.RegisterType<IProductUidService, ProductUidService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Person>, Repository<Person>>();
            container.RegisterType<IPersonService, PersonService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Buyer>, Repository<Buyer>>();
            container.RegisterType<IBuyerService, BuyerService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Currency>, Repository<Currency>>();
            container.RegisterType<ICurrencyService, CurrencyService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ShipMethod>, Repository<ShipMethod>>();
            container.RegisterType<IShipMethodService, ShipMethodService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<DeliveryTerms>, Repository<DeliveryTerms>>();
            container.RegisterType<IDeliveryTermsService, DeliveryTermsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Stock>, Repository<Stock>>();
            container.RegisterType<IStockService, StockService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<BusinessEntity>, Repository<BusinessEntity>>();
            container.RegisterType<IBusinessEntityService, BusinessEntityService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<LedgerAccount>, Repository<LedgerAccount>>();
            container.RegisterType<IAccountService, AccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonAddress>, Repository<PersonAddress>>();
            container.RegisterType<IPersonAddressService, PersonAddressService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonProcess>, Repository<PersonProcess>>();
            container.RegisterType<IPersonProcessService, PersonProcessService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonRegistration>, Repository<PersonRegistration>>();
            container.RegisterType<IPersonRegistrationService, PersonRegistrationService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonContact>, Repository<PersonContact>>();
            container.RegisterType<IPersonContactService, PersonContactService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonBankAccount>, Repository<PersonBankAccount>>();
            container.RegisterType<IPersonBankAccountService, PersonBankAccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IReportLineService, ReportLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IProductService, ProductService>(new PerRequestLifetimeManager());

            container.RegisterType<IPromoCodeService, PromoCodeService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDeliveryOrderHeaderService, SaleDeliveryOrderHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDeliveryOrderLineService, SaleDeliveryOrderLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IUpdateSaleExpiryService, UpdateSaleExpiryService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDeliveryOrderSettingsService, SaleDeliveryOrderSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDeliveryOrderCancelHeaderService, SaleDeliveryOrderCancelHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDeliveryOrderCancelLineService, SaleDeliveryOrderCancelLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleDispatchHeader>, Repository<SaleDispatchHeader>>();
            container.RegisterType<Service.ISaleDispatchHeaderService, Service.SaleDispatchHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleDispatchLine>, Repository<SaleDispatchLine>>();
            container.RegisterType<Service.ISaleDispatchLineService, Service.SaleDispatchLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PackingHeader>, Repository<PackingHeader>>();
            container.RegisterType<Service.IPackingHeaderService, Service.PackingHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PackingLine>, Repository<PackingLine>>();
            container.RegisterType<Service.IPackingLineService, Service.PackingLineService>(new PerRequestLifetimeManager());



            container.RegisterType<IRepository<SaleDeliveryHeader>, Repository<SaleDeliveryHeader>>();
            container.RegisterType<Service.ISaleDeliveryHeaderService, Service.SaleDeliveryHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleDeliveryLine>, Repository<SaleDeliveryLine>>();
            container.RegisterType<Service.ISaleDeliveryLineService, Service.SaleDeliveryLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleInvoiceHeader>, Repository<SaleInvoiceHeader>>();
            container.RegisterType<Service.ISaleInvoiceHeaderService, Service.SaleInvoiceHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleInvoiceLine>, Repository<SaleInvoiceLine>>();
            container.RegisterType<Service.ISaleInvoiceLineService, Service.SaleInvoiceLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleInvoiceLineDetail>, Repository<SaleInvoiceLineDetail>>();
            container.RegisterType<Service.ISaleInvoiceLineDetailService, Service.SaleInvoiceLineDetailService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PackingHeader>, Repository<PackingHeader>>();
            container.RegisterType<Service.IPackingHeaderService, Service.PackingHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PackingLine>, Repository<PackingLine>>();
            container.RegisterType<Service.IPackingLineService, Service.PackingLineService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleInvoiceSettingService, SaleInvoiceSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleInvoiceReturnHeaderService, SaleInvoiceReturnHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleInvoiceReturnLineService, SaleInvoiceReturnLineService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDispatchReturnHeaderService, SaleDispatchReturnHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDispatchReturnLineService, SaleDispatchReturnLineService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDispatchSettingService, SaleDispatchSettingService>(new PerRequestLifetimeManager());
            container.RegisterType<IPackingSettingService, PackingSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDeliverySettingService, SaleDeliverySettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Manufacturer>, Repository<Manufacturer>>();
            container.RegisterType<IManufacturerService, ManufacturerService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonDocument>, Repository<PersonDocument>>();
            container.RegisterType<IPersonDocumentService, PersonDocumentService>(new PerRequestLifetimeManager());

            container.RegisterType<IAgentService, AgentService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<DrawBackTariffHead>, Repository<DrawBackTariffHead>>();
            container.RegisterType<IDrawBackTariffHeadService, DrawBackTariffHeadService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Route>, Repository<Route>>();
            container.RegisterType<IRouteService, RouteService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<RouteLine>, Repository<RouteLine>>();
            container.RegisterType<IRouteLineService, RouteLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Courier>, Repository<Courier>>();
            container.RegisterType<ICourierService, CourierService>(new PerRequestLifetimeManager());



            //////////////////////////////////////////End Sales Services///////////////////////////////////////////////////


            //////////////////////////////////////////Human Resource Services///////////////////////////////////////////////////

            container.RegisterType<IEmployeeService, EmployeeService>(new PerRequestLifetimeManager());

            container.RegisterType<IDepartmentService, DepartmentService>(new PerRequestLifetimeManager());
            container.RegisterType<IDesignationService, DesignationService>(new PerRequestLifetimeManager());
            container.RegisterType<IBusinessEntityService, BusinessEntityService>(new PerRequestLifetimeManager());

            container.RegisterType<IPersonService, PersonService>(new PerRequestLifetimeManager());
            container.RegisterType<IPersonAddressService, PersonAddressService>(new PerRequestLifetimeManager());

            container.RegisterType<IAccountService, AccountService>(new PerRequestLifetimeManager());
            container.RegisterType<IPersonProcessService, PersonProcessService>(new PerRequestLifetimeManager());

            container.RegisterType<IPersonRegistrationService, PersonRegistrationService>(new PerRequestLifetimeManager());
            container.RegisterType<IActivityLogService, ActivityLogService>(new PerRequestLifetimeManager());

            container.RegisterType<IPersonBankAccountService, PersonBankAccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IPersonContactService, PersonContactService>(new PerRequestLifetimeManager());
            container.RegisterType<IDuplicateDocumentCheckService, DuplicateDocumentCheckService>(new PerRequestLifetimeManager());

            container.RegisterType<ILeaveTypeServices, LeaveTypeServices>(new PerRequestLifetimeManager());
            container.RegisterType<IAttendanceHeaderService, AttendanceHeaderService>(new PerRequestLifetimeManager());
            container.RegisterType<IOverTimeApplicationHeaderService, OverTimeApplicationHeaderService>(new PerRequestLifetimeManager());
            container.RegisterType<IUserReferralService, UserReferralService>(new PerRequestLifetimeManager());

            container.RegisterType<IGatePassHeaderService, GatePassHeaderService>(new PerRequestLifetimeManager());



            //////////////////////////////////////////End Human Resource Services///////////////////////////////////////////////////


            //////////////////////////////////////////Calculation Resource Services///////////////////////////////////////////////////

            container.RegisterType<ICalculationService, CalculationService>(new PerRequestLifetimeManager());
            container.RegisterType<ICalculationFooterService, CalculationFooterService>(new PerRequestLifetimeManager());
            container.RegisterType<ICalculationProductService, CalculationProductService>(new PerRequestLifetimeManager());
            container.RegisterType<IChargeService, ChargeService>(new PerRequestLifetimeManager());
            container.RegisterType<IChargeGroupPersonService, ChargeGroupPersonService>(new PerRequestLifetimeManager());
            container.RegisterType<IChargeGroupProductService, ChargeGroupProductService>(new PerRequestLifetimeManager());
            container.RegisterType<IChargeService, ChargeService>(new PerRequestLifetimeManager());

            container.RegisterType<ICalculationHeaderLedgerAccountService, CalculationHeaderLedgerAccountService>(new PerRequestLifetimeManager());
            container.RegisterType<ICalculationLineLedgerAccountService, CalculationLineLedgerAccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IDuplicateDocumentCheckService, DuplicateDocumentCheckService>(new PerRequestLifetimeManager());

            //////////////////////////////////////////End Calculation Resource Services///////////////////////////////////////////////////


            //////////////////////////////////////////Store Resource Services///////////////////////////////////////////////////

            container.RegisterType<IRepository<SalesTaxProductCode>, Repository<SalesTaxProductCode>>();
            container.RegisterType<ISalesTaxProductCodeService, SalesTaxProductCodeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ImportHeader>, Repository<ImportHeader>>();
            container.RegisterType<IImportHeaderService, ImportHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ImportLine>, Repository<ImportLine>>();
            container.RegisterType<IImportLineService, ImportLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Godown>, Repository<Godown>>();
            container.RegisterType<IGodownService, GodownService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ChargeGroupSettings>, Repository<ChargeGroupSettings>>();
            container.RegisterType<IChargeGroupSettingsService, ChargeGroupSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockHeaderSettings>, Repository<StockHeaderSettings>>();
            container.RegisterType<IStockHeaderSettingsService, StockHeaderSettingsService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<MaterialRequestSettings>, Repository<MaterialRequestSettings>>();
            container.RegisterType<IMaterialRequestSettingsService, MaterialRequestSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Person>, Repository<Person>>();
            container.RegisterType<IPersonService, PersonService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<BusinessEntity>, Repository<BusinessEntity>>();
            container.RegisterType<IBusinessEntityService, BusinessEntityService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<LedgerAccount>, Repository<LedgerAccount>>();
            container.RegisterType<IAccountService, AccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonProcess>, Repository<PersonProcess>>();
            container.RegisterType<IPersonProcessService, PersonProcessService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonRegistration>, Repository<PersonRegistration>>();
            container.RegisterType<IPersonRegistrationService, PersonRegistrationService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonAddress>, Repository<PersonAddress>>();
            container.RegisterType<IPersonAddressService, PersonAddressService>(new PerRequestLifetimeManager());

            container.RegisterType<IPersonOpeningService, PersonOpeningService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonContact>, Repository<PersonContact>>();
            container.RegisterType<IPersonContactService, PersonContactService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonRole>, Repository<PersonRole>>();
            container.RegisterType<IPersonRoleService, PersonRoleService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonBankAccount>, Repository<PersonBankAccount>>();
            container.RegisterType<IPersonBankAccountService, PersonBankAccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonSettings>, Repository<PersonSettings>>();
            container.RegisterType<IPersonSettingsService, PersonSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockHeader>, Repository<StockHeader>>();
            container.RegisterType<IStockHeaderService, StockHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockLine>, Repository<StockLine>>();
            container.RegisterType<IStockLineService, StockLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Stock>, Repository<Stock>>();
            container.RegisterType<IStockService, StockService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<StockUid>, Repository<StockUid>>();
            container.RegisterType<IStockUidService, StockUidService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<MaterialReceiveSettings>, Repository<MaterialReceiveSettings>>();
            container.RegisterType<IMaterialReceiveSettingsService, MaterialReceiveSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Dimension1>, Repository<Dimension1>>();
            container.RegisterType<IDimension1Service, Dimension1Service>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Dimension2>, Repository<Dimension2>>();
            container.RegisterType<IDimension2Service, Dimension2Service>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Dimension3>, Repository<Dimension3>>();
            container.RegisterType<IDimension3Service, Dimension3Service>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Dimension4>, Repository<Dimension4>>();
            container.RegisterType<IDimension4Service, Dimension4Service>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Godown>, Repository<Godown>>();
            container.RegisterType<IGodownService, GodownService>(new PerRequestLifetimeManager());

            container.RegisterType<IGateService, GateService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<RequisitionHeader>, Repository<RequisitionHeader>>();
            container.RegisterType<IRequisitionHeaderService, RequisitionHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<RequisitionLine>, Repository<RequisitionLine>>();
            container.RegisterType<IRequisitionLineService, RequisitionLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Process>, Repository<Process>>();
            container.RegisterType<IProcessService, ProcessService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProcessSequenceHeader>, Repository<ProcessSequenceHeader>>();
            container.RegisterType<IProcessSequenceHeaderService, ProcessSequenceHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProcessSequenceLine>, Repository<ProcessSequenceLine>>();
            container.RegisterType<IProcessSequenceLineService, ProcessSequenceLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonRateGroup>, Repository<PersonRateGroup>>();
            container.RegisterType<IPersonRateGroupService, PersonRateGroupService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Product>, Repository<Product>>();
            container.RegisterType<IProductService, ProductService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<BomDetail>, Repository<BomDetail>>();
            container.RegisterType<IBomDetailService, BomDetailService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductProcess>, Repository<ProductProcess>>();
            container.RegisterType<IProductProcessService, ProductProcessService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<FinishedProduct>, Repository<FinishedProduct>>();
            container.RegisterType<IFinishedProductService, FinishedProductService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductCustomGroupHeader>, Repository<ProductCustomGroupHeader>>();
            container.RegisterType<IProductCustomGroupHeaderService, ProductCustomGroupHeaderService>(new PerRequestLifetimeManager());





            container.RegisterType<IRepository<ProductCustomGroupLine>, Repository<ProductCustomGroupLine>>();
            container.RegisterType<IProductCustomGroupLineService, ProductCustomGroupLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductGroupSettings>, Repository<ProductGroupSettings>>();
            container.RegisterType<IProductGroupSettingsService, ProductGroupSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductGroupProcessSettings>, Repository<ProductGroupProcessSettings>>();
            container.RegisterType<IProductGroupProcessSettingsService, ProductGroupProcessSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductGroup>, Repository<ProductGroup>>();
            container.RegisterType<IProductGroupService, ProductGroupService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<ProductCategorySettings>, Repository<ProductCategorySettings>>();
            container.RegisterType<IProductCategorySettingsService, ProductCategorySettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductCategoryProcessSettings>, Repository<ProductCategoryProcessSettings>>();
            container.RegisterType<IProductCategoryProcessSettingsService, ProductCategoryProcessSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductCategory>, Repository<ProductCategory>>();
            container.RegisterType<IProductCategoryService, ProductCategoryService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<ProductTypeAttribute>, Repository<ProductTypeAttribute>>();
            container.RegisterType<IProductTypeAttributeService, ProductTypeAttributeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductType>, Repository<ProductType>>();
            container.RegisterType<IProductTypeService, ProductTypeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductTypeSettings>, Repository<ProductTypeSettings>>();
            container.RegisterType<IProductTypeSettingsService, ProductTypeSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Unit>, Repository<Unit>>();
            container.RegisterType<IUnitService, UnitService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<UnitConversion>, Repository<UnitConversion>>();
            container.RegisterType<IUnitConversionService, UnitConversionService>(new PerRequestLifetimeManager());

            container.RegisterType<IReportLineService, ReportLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IActivityLogService, ActivityLogService>(new PerRequestLifetimeManager());

            container.RegisterType<IStockInHandService, StockInHandService>(new PerRequestLifetimeManager());

            container.RegisterType<IStockProcessDisplayService, StockProcessDisplayService>(new PerRequestLifetimeManager());

            container.RegisterType<IStockInHandSettingService, StockInHandSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IRequisitionSettingService, RequisitionSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IRequisitionCancelHeaderService, RequisitionCancelHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRequisitionCancelLineService, RequisitionCancelLineService>(new PerRequestLifetimeManager());


            container.RegisterType<IDuplicateDocumentCheckService, DuplicateDocumentCheckService>(new PerRequestLifetimeManager());
            container.RegisterType<IProductUidService, ProductUidService>(new PerRequestLifetimeManager());

            container.RegisterType<IProductUidHeaderService, ProductUidHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IProductRateGroupService, ProductRateGroupService>(new PerRequestLifetimeManager());

            container.RegisterType<IRateListHeaderService, RateListHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRateListPersonRateGroupService, RateListPersonRateGroupService>(new PerRequestLifetimeManager());

            container.RegisterType<IRateListProductRateGroupService, RateListProductRateGroupService>(new PerRequestLifetimeManager());

            container.RegisterType<IRateListLineService, RateListLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRateListLineHistoryService, RateListLineHistoryService>(new PerRequestLifetimeManager());

            container.RegisterType<IExcessMaterialHeaderService, ExcessMaterialHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IExcessMaterialLineService, ExcessMaterialLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IExcessMaterialSettingsService, ExcessMaterialSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<ILedgerSettingService, LedgerSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<GatePassHeader>, Repository<GatePassHeader>>();
            container.RegisterType<IGatePassHeaderService, GatePassHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<GatePassLine>, Repository<GatePassLine>>();
            container.RegisterType<IGatePassLineService, GatePassLineService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<QAGroup>, Repository<QAGroup>>();
            container.RegisterType<IQAGroupService, QAGroupService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<QAGroupLine>, Repository<QAGroupLine>>();
            container.RegisterType<IQAGroupLineService, QAGroupLineService>(new PerRequestLifetimeManager());



            //////////////////////////////////////////End Store Resource Services///////////////////////////////////////////////////


            ////////////////////////////////////////// Planning Resource Services///////////////////////////////////////////////////

            container.RegisterType<IRepository<MaterialPlanSettings>, Repository<MaterialPlanSettings>>();
            container.RegisterType<IMaterialPlanSettingsService, MaterialPlanSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<MaterialPlanHeader>, Repository<MaterialPlanHeader>>();
            container.RegisterType<IMaterialPlanHeaderService, MaterialPlanHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<MaterialPlanForSaleOrderLine>, Repository<MaterialPlanForSaleOrderLine>>();
            container.RegisterType<IMaterialPlanForSaleOrderLineService, MaterialPlanForSaleOrderLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<MaterialPlanForProdOrder>, Repository<MaterialPlanForProdOrder>>();
            container.RegisterType<IMaterialPlanForProdOrderService, MaterialPlanForProdOrderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<MaterialPlanCancelForProdOrder>, Repository<MaterialPlanCancelForProdOrder>>();
            container.RegisterType<IMaterialPlanCancelForProdOrderService, MaterialPlanCancelForProdOrderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<MaterialPlanLine>, Repository<MaterialPlanLine>>();
            container.RegisterType<IMaterialPlanLineService, MaterialPlanLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<MaterialPlanCancelHeader>, Repository<MaterialPlanCancelHeader>>();
            container.RegisterType<IMaterialPlanCancelHeaderService, MaterialPlanCancelHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<MaterialPlanCancelForProdOrderLine>, Repository<MaterialPlanCancelForProdOrderLine>>();
            container.RegisterType<IMaterialPlanCancelForProdOrderLineService, MaterialPlanCancelForProdOrderLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<MaterialPlanCancelLine>, Repository<MaterialPlanCancelLine>>();
            container.RegisterType<IMaterialPlanCancelLineService, MaterialPlanCancelLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IModuleService, ModuleService>(new PerRequestLifetimeManager());

            container.RegisterType<ISubModuleService, SubModuleService>(new PerRequestLifetimeManager());

            container.RegisterType<IExceptionHandlingService, ExceptionHandlingService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProdOrderHeader>, Repository<ProdOrderHeader>>();
            container.RegisterType<IProdOrderHeaderService, ProdOrderHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProdOrderLine>, Repository<ProdOrderLine>>();
            container.RegisterType<IProdOrderLineService, ProdOrderLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProdOrderCancelHeader>, Repository<ProdOrderCancelHeader>>();
            container.RegisterType<IProdOrderCancelHeaderService, ProdOrderCancelHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProdOrderCancelLine>, Repository<ProdOrderCancelLine>>();
            container.RegisterType<IProdOrderCancelLineService, ProdOrderCancelLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IActivityLogService, ActivityLogService>(new PerRequestLifetimeManager());

            container.RegisterType<IDuplicateDocumentCheckService, DuplicateDocumentCheckService>(new PerRequestLifetimeManager());

            container.RegisterType<IReportLineService, ReportLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IProdOrderSettingsService, ProdOrderSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IMaterialPlanCancelForSaleOrderService, MaterialPlanCancelForSaleOrderService>(new PerRequestLifetimeManager());


            ////////////////////////////////////////// End Planning Services///////////////////////////////////////////////////


            ////////////////////////////////////////// Purchase Services///////////////////////////////////////////////////

            container.RegisterType<IPurchaseOrderHeaderService, PurchaseOrderHeaderService>(new PerRequestLifetimeManager());
            container.RegisterType<IPurchaseOrderLineService, PurchaseOrderLineService>(new PerRequestLifetimeManager());
            container.RegisterType<IExceptionHandlingService, ExceptionHandlingService>(new PerRequestLifetimeManager());
            container.RegisterType<IModuleService, ModuleService>(new PerRequestLifetimeManager());
            container.RegisterType<ISubModuleService, SubModuleService>(new PerRequestLifetimeManager());
            container.RegisterType<IPurchaseOrderSettingService, PurchaseOrderSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseIndentHeader>, Repository<PurchaseIndentHeader>>();
            container.RegisterType<IPurchaseIndentHeaderService, PurchaseIndentHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseIndentLine>, Repository<PurchaseIndentLine>>();
            container.RegisterType<IPurchaseIndentLineService, PurchaseIndentLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseOrderCancelHeader>, Repository<PurchaseOrderCancelHeader>>();
            container.RegisterType<IPurchaseOrderCancelHeaderService, PurchaseOrderCancelHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseOrderCancelLine>, Repository<PurchaseOrderCancelLine>>();
            container.RegisterType<IPurchaseOrderCancelLineService, PurchaseOrderCancelLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseIndentCancelHeader>, Repository<PurchaseIndentCancelHeader>>();
            container.RegisterType<IPurchaseIndentCancelHeaderService, PurchaseIndentCancelHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseIndentCancelLine>, Repository<PurchaseIndentCancelLine>>();
            container.RegisterType<IPurchaseIndentCancelLineService, PurchaseIndentCancelLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseInvoiceReturnHeader>, Repository<PurchaseInvoiceReturnHeader>>();
            container.RegisterType<IPurchaseInvoiceReturnHeaderService, PurchaseInvoiceReturnHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseInvoiceReturnLine>, Repository<PurchaseInvoiceReturnLine>>();
            container.RegisterType<IPurchaseInvoiceReturnLineService, PurchaseInvoiceReturnLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseGoodsReturnHeader>, Repository<PurchaseGoodsReturnHeader>>();
            container.RegisterType<IPurchaseGoodsReturnHeaderService, PurchaseGoodsReturnHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseGoodsReturnLine>, Repository<PurchaseGoodsReturnLine>>();
            container.RegisterType<IPurchaseGoodsReturnLineService, PurchaseGoodsReturnLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseGoodsReceiptHeader>, Repository<PurchaseGoodsReceiptHeader>>();
            container.RegisterType<IPurchaseGoodsReceiptHeaderService, PurchaseGoodsReceiptHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseGoodsReceiptLine>, Repository<PurchaseGoodsReceiptLine>>();
            container.RegisterType<IPurchaseGoodsReceiptLineService, PurchaseGoodsReceiptLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductUid>, Repository<ProductUid>>();
            container.RegisterType<IProductUidService, ProductUidService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseInvoiceHeader>, Repository<PurchaseInvoiceHeader>>();
            container.RegisterType<IPurchaseInvoiceHeaderService, PurchaseInvoiceHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IDuplicateDocumentCheckService, DuplicateDocumentCheckService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseInvoiceLine>, Repository<PurchaseInvoiceLine>>();
            container.RegisterType<IPurchaseInvoiceLineService, PurchaseInvoiceLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Supplier>, Repository<Supplier>>();
            container.RegisterType<ISupplierService, SupplierService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<BusinessEntity>, Repository<BusinessEntity>>();
            container.RegisterType<IBusinessEntityService, BusinessEntityService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Person>, Repository<Person>>();
            container.RegisterType<IPersonService, PersonService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonAddress>, Repository<PersonAddress>>();
            container.RegisterType<IPersonAddressService, PersonAddressService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<LedgerAccount>, Repository<LedgerAccount>>();
            container.RegisterType<IAccountService, AccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonProcess>, Repository<PersonProcess>>();
            container.RegisterType<IPersonProcessService, PersonProcessService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonRegistration>, Repository<PersonRegistration>>();
            container.RegisterType<IPersonRegistrationService, PersonRegistrationService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseWaybill>, Repository<PurchaseWaybill>>();
            container.RegisterType<IPurchaseWaybillService, PurchaseWaybillService>(new PerRequestLifetimeManager());


            container.RegisterType<IPurchaseGoodsReceiptSettingService, PurchaseGoodsReceiptSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IPurchaseInvoiceSettingService, PurchaseInvoiceSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IReportLineService, ReportLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IPurchaseIndentSettingService, PurchaseIndentSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IChargesCalculationService, ChargesCalculationService>(new PerRequestLifetimeManager());

            container.RegisterType<IPersonContactService, PersonContactService>(new PerRequestLifetimeManager());

            container.RegisterType<IPersonBankAccountService, PersonBankAccountService>(new PerRequestLifetimeManager());


            container.RegisterType<IDocEmailContentService, DocEmailContentService>(new PerRequestLifetimeManager());

            container.RegisterType<IDocNotificationContentService, DocNotificationContentService>(new PerRequestLifetimeManager());

            container.RegisterType<IUpdatePurchaseExpiryService, UpdatePurchaseExpiryService>(new PerRequestLifetimeManager());

            container.RegisterType<IPurchaseOrderRateAmendmentLineService, PurchaseOrderRateAmendmentLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IPurchaseQuotationHeaderService, PurchaseQuotationHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IPurchaseQuotationLineService, PurchaseQuotationLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IPurchaseQuotationSettingService, PurchaseQuotationSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseInvoiceHeader>, Repository<PurchaseInvoiceHeader>>();
            container.RegisterType<IPurchaseInvoiceHeaderService, PurchaseInvoiceHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PurchaseInvoiceLine>, Repository<PurchaseInvoiceLine>>();
            container.RegisterType<IPurchaseInvoiceLineService, PurchaseInvoiceLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ActivityLog>, Repository<ActivityLog>>();
            container.RegisterType<IActivityLogService, ActivityLogService>(new PerRequestLifetimeManager());



            ////////////////////////////////////////// End Purchase Services///////////////////////////////////////////////////


            ////////////////////////////////////////// Task Management Services///////////////////////////////////////////////////

            container.RegisterType<IDARService, DARService>(new PerRequestLifetimeManager());
            container.RegisterType<IProjectService, ProjectService>(new PerRequestLifetimeManager());
            container.RegisterType<ITasksService, TasksService>(new PerRequestLifetimeManager());
            container.RegisterType<IUserTeamService, UserTeamService>(new PerRequestLifetimeManager());



            container.RegisterType<IRepository<Project>, Repository<Project>>(new PerRequestLifetimeManager());
            container.RegisterType<IRepository<Tasks>, Repository<Tasks>>(new PerRequestLifetimeManager());
            container.RegisterType<IRepository<DAR>, Repository<DAR>>(new PerRequestLifetimeManager());
            container.RegisterType<ILogger, LogActivity>(new PerRequestLifetimeManager());
            container.RegisterType<IRepository<UserTeam>, Repository<UserTeam>>(new PerRequestLifetimeManager());
            container.RegisterType<IRepository<_Users>, Repository<_Users>>(new PerRequestLifetimeManager());
            container.RegisterType<IModificationCheck, ModificationCheck>(new PerRequestLifetimeManager());
            container.RegisterType<IExceptionHandler, ExceptionHandler>(new PerRequestLifetimeManager());


            ////////////////////////////////////////// End Task Management Services///////////////////////////////////////////////////


            ////////////////////////////////////////// Report Services///////////////////////////////////////////////////

            container.RegisterType<IDataContext, ApplicationDbContext>(new PerRequestLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new PerRequestLifetimeManager());
            container.RegisterType<IModificationCheck, ModificationCheck>(new PerRequestLifetimeManager());
            container.RegisterType<ILogger, LogActivity>(new PerRequestLifetimeManager());
            container.RegisterType<IExceptionHandler, ExceptionHandler>(new PerRequestLifetimeManager());
            container.RegisterType<IDocumentTypeService, DocumentTypeService>(new PerRequestLifetimeManager());
            container.RegisterType<IReportLineService, ReportLineService>(new PerRequestLifetimeManager());
            container.RegisterType<IReportHeaderService, ReportHeaderService>(new PerRequestLifetimeManager());


            container.RegisterType<IRepository<ReportHeader>, Repository<ReportHeader>>();
            container.RegisterType<IRepository<ReportLine>, Repository<ReportLine>>();
            container.RegisterType<IRepository<ReportUIDValues>, Repository<ReportUIDValues>>();
            container.RegisterType<IRepository<ActivityLog>, Repository<ActivityLog>>();
            container.RegisterType<IRepository<DocumentType>, Repository<DocumentType>>();


            ////////////////////////////////////////// End Report Services///////////////////////////////////////////////////


            ////////////////////////////////////////// Rug Services///////////////////////////////////////////////////

            container.RegisterType<IRedirectService, RedirectService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<LedgerAccount>, Repository<LedgerAccount>>();
            container.RegisterType<IAccountService, AccountService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<BomDetail>, Repository<BomDetail>>();
            container.RegisterType<IBomDetailService, BomDetailService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<BusinessEntity>, Repository<BusinessEntity>>();
            container.RegisterType<IBusinessEntityService, BusinessEntityService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Buyer>, Repository<Buyer>>();
            container.RegisterType<IBuyerService, BuyerService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Colour>, Repository<Colour>>();
            container.RegisterType<IColourService, ColourService>(new PerRequestLifetimeManager());



            container.RegisterType<IRepository<Currency>, Repository<Currency>>();
            container.RegisterType<ICurrencyService, CurrencyService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<CustomDetail>, Repository<CustomDetail>>();
            container.RegisterType<ICustomDetailService, CustomDetailService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<DeliveryTerms>, Repository<DeliveryTerms>>();
            container.RegisterType<IDeliveryTermsService, DeliveryTermsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<DescriptionOfGoods>, Repository<DescriptionOfGoods>>();
            container.RegisterType<IDescriptionOfGoodsService, DescriptionOfGoodsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Dimension2>, Repository<Dimension2>>();
            container.RegisterType<IDimension2Service, Dimension2Service>(new PerRequestLifetimeManager());

            //container.RegisterType<IRepository<Notification>, Repository<Notification>>();
            //container.RegisterType<INotificationService, NotificationService>(new PerRequestLifetimeManager());

            //container.RegisterType<IRepository<NotificationSubject>, Repository<NotificationSubject>>();
            //container.RegisterType<INotificationSubjectService, NotificationSubjectService>(new PerRequestLifetimeManager());

            container.RegisterType<IDuplicateDocumentCheckService, DuplicateDocumentCheckService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<FinishedProduct>, Repository<FinishedProduct>>();
            container.RegisterType<IFinishedProductService, FinishedProductService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PackingHeader>, Repository<PackingHeader>>();
            container.RegisterType<Service.IPackingHeaderService, Service.PackingHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PackingLine>, Repository<PackingLine>>();
            container.RegisterType<Service.IPackingLineService, Service.PackingLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IPackingSettingService, PackingSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonAddress>, Repository<PersonAddress>>();
            container.RegisterType<IPersonAddressService, PersonAddressService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<PersonProcess>, Repository<PersonProcess>>();
            container.RegisterType<IPersonProcessService, PersonProcessService>(new PerRequestLifetimeManager());



            container.RegisterType<IRepository<PersonRegistration>, Repository<PersonRegistration>>();
            container.RegisterType<IPersonRegistrationService, PersonRegistrationService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Person>, Repository<Person>>();
            container.RegisterType<IPersonService, PersonService>(new PerRequestLifetimeManager()); //

            container.RegisterType<IRepository<ProductCategory>, Repository<ProductCategory>>();
            container.RegisterType<IProductCategoryService, ProductCategoryService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductCategorySettings>, Repository<ProductCategorySettings>>();
            container.RegisterType<IProductCategorySettingsService, ProductCategorySettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductCategoryProcessSettings>, Repository<ProductCategoryProcessSettings>>();
            container.RegisterType<IProductCategoryProcessSettingsService, ProductCategoryProcessSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductCollection>, Repository<ProductCollection>>();
            container.RegisterType<IProductCollectionService, ProductCollectionService>(new PerRequestLifetimeManager());

            container.RegisterType<IProductConstructionService, ProductConstructionService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductContentHeader>, Repository<ProductContentHeader>>();
            container.RegisterType<IProductContentHeaderService, ProductContentHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductContentLine>, Repository<ProductContentLine>>();
            container.RegisterType<IProductContentLineService, ProductContentLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductDesignPattern>, Repository<ProductDesignPattern>>();
            container.RegisterType<IProductDesignPatternService, ProductDesignPatternService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductDesign>, Repository<ProductDesign>>();
            container.RegisterType<IProductDesignService, ProductDesignService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductGroup>, Repository<ProductGroup>>();
            container.RegisterType<IProductGroupService, ProductGroupService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductInvoiceGroup>, Repository<ProductInvoiceGroup>>();
            container.RegisterType<IProductInvoiceGroupService, ProductInvoiceGroupService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductStyle>, Repository<ProductStyle>>();
            container.RegisterType<IProductManufacturingStyleService, ProductManufacturingStyleService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductNature>, Repository<ProductNature>>();
            container.RegisterType<IProductNatureService, ProductNatureService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductQuality>, Repository<ProductQuality>>();
            container.RegisterType<IProductQualityService, ProductQualityService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Product>, Repository<Product>>();
            container.RegisterType<IProductService, ProductService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductShape>, Repository<ProductShape>>();
            container.RegisterType<IProductShapeService, ProductShapeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductSize>, Repository<ProductSize>>();
            container.RegisterType<IProductSizeService, ProductSizeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductSizeType>, Repository<ProductSizeType>>();
            container.RegisterType<IProductSizeTypeService, ProductSizeTypeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductStyle>, Repository<ProductStyle>>();
            container.RegisterType<IProductStyleService, ProductStyleService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductUid>, Repository<ProductUid>>();
            container.RegisterType<IProductUidService, ProductUidService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ReportHeader>, Repository<ReportHeader>>();
            container.RegisterType<IReportHeaderService, ReportHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ReportLine>, Repository<ReportLine>>();
            container.RegisterType<IReportLineService, ReportLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<RugStencil>, Repository<RugStencil>>();
            container.RegisterType<IRugStencilService, RugStencilService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleDispatchHeader>, Repository<SaleDispatchHeader>>();
            container.RegisterType<Service.ISaleDispatchHeaderService, Service.SaleDispatchHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleDispatchLine>, Repository<SaleDispatchLine>>();
            container.RegisterType<Service.ISaleDispatchLineService, Service.SaleDispatchLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleInvoiceHeader>, Repository<SaleInvoiceHeader>>();
            container.RegisterType<Service.ISaleInvoiceHeaderService, Service.SaleInvoiceHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleInvoiceLine>, Repository<SaleInvoiceLine>>();
            container.RegisterType<Service.ISaleInvoiceLineService, Service.SaleInvoiceLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleOrderHeader>, Repository<SaleOrderHeader>>();
            container.RegisterType<Service.ISaleOrderHeaderService, Service.SaleOrderHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleOrderLine>, Repository<SaleOrderLine>>();
            container.RegisterType<Service.ISaleOrderLineService, Service.SaleOrderLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleEnquiryHeader>, Repository<SaleEnquiryHeader>>();
            container.RegisterType<Service.ISaleEnquiryHeaderService, Service.SaleEnquiryHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SaleEnquiryLine>, Repository<SaleEnquiryLine>>();
            container.RegisterType<Service.ISaleEnquiryLineService, Service.SaleEnquiryLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<FeetConversionToCms>, Repository<FeetConversionToCms>>();
            container.RegisterType<IFeetConversionToCmsService, FeetConversionToCmsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SalesTaxGroupProduct>, Repository<SalesTaxGroupProduct>>();
            container.RegisterType<ISalesTaxGroupProductService, SalesTaxGroupProductService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<SalesTaxGroupParty>, Repository<SalesTaxGroupParty>>();
            container.RegisterType<ISalesTaxGroupPartyService, SalesTaxGroupPartyService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ServiceTaxCategory>, Repository<ServiceTaxCategory>>();
            container.RegisterType<IServiceTaxCategoryService, ServiceTaxCategoryService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ShipMethod>, Repository<ShipMethod>>();
            container.RegisterType<IShipMethodService, ShipMethodService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Size>, Repository<Size>>();
            container.RegisterType<ISizeService, SizeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<Stock>, Repository<Stock>>();
            container.RegisterType<IStockService, StockService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<ProductBuyer>, Repository<ProductBuyer>>();
            container.RegisterType<IProductBuyerService, ProductBuyerService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<UnitConversion>, Repository<UnitConversion>>();
            container.RegisterType<IUnitConversionService, UnitConversionService>(new PerRequestLifetimeManager());

            container.RegisterType<IReportLineService, ReportLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IJobOrderHeaderService, JobOrderHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<ITrialBalanceService, TrialBalanceService>(new PerRequestLifetimeManager());

            container.RegisterType<ICarpetSkuSettingsService, CarpetSkuSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IProductBuyerSettingsService, ProductBuyerSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleEnquirySettingsService, SaleEnquirySettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<ITrialBalanceSettingService, TrialBalanceSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleInvoiceSettingService, SaleInvoiceSettingService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleInvoiceHeaderChargeService, SaleInvoiceHeaderChargeService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleInvoiceLineChargeService, SaleInvoiceLineChargeService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobInvoiceHeader>, Repository<JobInvoiceHeader>>();
            container.RegisterType<IJobInvoiceHeaderService, JobInvoiceHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobInvoiceLine>, Repository<JobInvoiceLine>>();
            container.RegisterType<IJobInvoiceLineService, JobInvoiceLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveLine>, Repository<JobReceiveLine>>();
            container.RegisterType<IJobReceiveLineService, JobReceiveLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveSettings>, Repository<JobReceiveSettings>>();
            container.RegisterType<IJobReceiveSettingsService, JobReceiveSettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveQASettings>, Repository<JobReceiveQASettings>>();
            container.RegisterType<IJobReceiveQASettingsService, JobReceiveQASettingsService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<JobReceiveQAPenalty>, Repository<JobReceiveQAPenalty>>();
            container.RegisterType<IJobReceiveQAPenaltyService, JobReceiveQAPenaltyService>(new PerRequestLifetimeManager());



            container.RegisterType<IRepository<JobReceiveHeader>, Repository<JobReceiveHeader>>();
            container.RegisterType<IJobReceiveHeaderService, JobReceiveHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleInvoiceReturnHeaderService, SaleInvoiceReturnHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleInvoiceReturnLineService, SaleInvoiceReturnLineService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDispatchReturnHeaderService, SaleDispatchReturnHeaderService>(new PerRequestLifetimeManager());

            container.RegisterType<ISaleDispatchReturnLineService, SaleDispatchReturnLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IProcessSequenceLineService, ProcessSequenceLineService>(new PerRequestLifetimeManager());

            container.RegisterType<IDocumentTypeTimeExtensionService, DocumentTypeTimeExtensionService>(new PerRequestLifetimeManager());

            container.RegisterType<IJobOrderLineExtendedService, JobOrderLineExtendedService>(new PerRequestLifetimeManager());

            ////////////////////////////////////////// End Rug Services///////////////////////////////////////////////////


            ////////////////////////////////////////// Module Services///////////////////////////////////////////////////

            container.RegisterType<IModuleService, ModuleService>(new PerRequestLifetimeManager());
            container.RegisterType<ISubModuleService, SubModuleService>(new PerRequestLifetimeManager());
            container.RegisterType<IMenuService, MenuService>(new PerRequestLifetimeManager());
            container.RegisterType<IUserBookMarkService, UserBookMarkService>(new PerRequestLifetimeManager());
            container.RegisterType<INotificationService, NotificationService>(new PerRequestLifetimeManager());
            container.RegisterType<ISiteSelectionService, SiteSelectionService>(new PerRequestLifetimeManager());
            container.RegisterType<IUserRolesService, UserRolesService>(new PerRequestLifetimeManager());
            container.RegisterType<IControllerActionService, ControllerActionService>(new PerRequestLifetimeManager());
            container.RegisterType<IRolesControllerActionService, RolesControllerActionService>(new PerRequestLifetimeManager());
            container.RegisterType<IUsersService, UsersService>(new PerRequestLifetimeManager());
            container.RegisterType<ILogger, LogActivity>(new PerRequestLifetimeManager());
            container.RegisterType<ICompanySettingsService, CompanySettingsService>(new PerRequestLifetimeManager());
            container.RegisterType<ICompanyService, CompanyService>(new PerRequestLifetimeManager());

            container.RegisterType<IRolesDivisionService, RolesDivisionService>(new PerRequestLifetimeManager());
            container.RegisterType<IRolesMenuService, RolesMenuService>(new PerRequestLifetimeManager());
            container.RegisterType<IRolesSiteService, RolesSiteService>(new PerRequestLifetimeManager());

            container.RegisterType<IRepository<MenuModule>, Repository<MenuModule>>();
            container.RegisterType<IRepository<ActivityLog>, Repository<ActivityLog>>();
            container.RegisterType<IRepository<MenuSubModule>, Repository<MenuSubModule>>();
            container.RegisterType<IRepository<Menu>, Repository<Menu>>();
            container.RegisterType<IRepository<UserBookMark>, Repository<UserBookMark>>();
            container.RegisterType<IRepository<RolesMenu>, Repository<RolesMenu>>();
            container.RegisterType<IRepository<ControllerAction>, Repository<ControllerAction>>();
            container.RegisterType<IRepository<RolesControllerAction>, Repository<RolesControllerAction>>();
            container.RegisterType<IRepository<CompanySettings>, Repository<CompanySettings>>();
            container.RegisterType<IRepository<Company>, Repository<Company>>();

            container.RegisterType<IRepository<RolesControllerAction>, Repository<RolesControllerAction>>();
            container.RegisterType<IRepository<RolesDivision>, Repository<RolesDivision>>();
            container.RegisterType<IRepository<RolesMenu>, Repository<RolesMenu>>();
            container.RegisterType<IRepository<RolesSite>, Repository<RolesSite>>();

            container.RegisterType<IAdminSetupService, AdminSetupService>(new PerRequestLifetimeManager());

            container.RegisterType<IUserPermissionService, UserPermissionService>(new PerRequestLifetimeManager());


            ////////////////////////////////////////// End Module Services///////////////////////////////////////////////////


            //////////////////////////////////////////Finance ViewModel Mapping///////////////////////////////////////////////////

            Mapper.CreateMap<LedgerAccount, LedgerAccountViewModel>();
            Mapper.CreateMap<LedgerAccountViewModel, LedgerAccount>();

            Mapper.CreateMap<Person, PersonContactViewModel>();
            Mapper.CreateMap<PersonContactViewModel, Person>();

            Mapper.CreateMap<PersonContact, PersonContactViewModel>();
            Mapper.CreateMap<PersonContactViewModel, PersonContact>();

            Mapper.CreateMap<PersonAddress, AgentViewModel>();
            Mapper.CreateMap<AgentViewModel, PersonAddress>();

            Mapper.CreateMap<PersonAddress, EmployeeViewModel>();
            Mapper.CreateMap<EmployeeViewModel, PersonAddress>();

            Mapper.CreateMap<PersonAddress, SupplierViewModel>();
            Mapper.CreateMap<SupplierViewModel, PersonAddress>();

            Mapper.CreateMap<PersonAddress, JobWorkerViewModel>();
            Mapper.CreateMap<JobWorkerViewModel, PersonAddress>();

            Mapper.CreateMap<PersonAddress, ManufacturerViewModel>();
            Mapper.CreateMap<ManufacturerViewModel, PersonAddress>();

            Mapper.CreateMap<PersonAddress, CourierViewModel>();
            Mapper.CreateMap<CourierViewModel, PersonAddress>();

            Mapper.CreateMap<PersonAddress, TransporterViewModel>();
            Mapper.CreateMap<TransporterViewModel, PersonAddress>();

            Mapper.CreateMap<PersonAddress, BuyerViewModel>();
            Mapper.CreateMap<BuyerViewModel, PersonAddress>();

            Mapper.CreateMap<LedgerHeader, LedgerHeaderViewModel>();
            Mapper.CreateMap<LedgerHeaderViewModel, LedgerHeader>();

            Mapper.CreateMap<CityViewModel, City>();
            Mapper.CreateMap<City, CityViewModel>();

            Mapper.CreateMap<LedgerAccount, BuyerViewModel>();
            Mapper.CreateMap<BuyerViewModel, LedgerAccount>();

            Mapper.CreateMap<LedgerAccount, TransporterViewModel>();
            Mapper.CreateMap<TransporterViewModel, LedgerAccount>();

            Mapper.CreateMap<LedgerAccount, CourierViewModel>();
            Mapper.CreateMap<CourierViewModel, LedgerAccount>();

            Mapper.CreateMap<LedgerAccount, ManufacturerViewModel>();
            Mapper.CreateMap<ManufacturerViewModel, LedgerAccount>();

            Mapper.CreateMap<LedgerAccount, JobWorkerViewModel>();
            Mapper.CreateMap<JobWorkerViewModel, LedgerAccount>();

            Mapper.CreateMap<LedgerAccount, SupplierViewModel>();
            Mapper.CreateMap<SupplierViewModel, LedgerAccount>();

            Mapper.CreateMap<LedgerAccount, EmployeeViewModel>();
            Mapper.CreateMap<EmployeeViewModel, LedgerAccount>();

            Mapper.CreateMap<LedgerAccount, AgentViewModel>();
            Mapper.CreateMap<AgentViewModel, LedgerAccount>();

            Mapper.CreateMap<LedgersViewModel, Ledger>();
            Mapper.CreateMap<Ledger, LedgersViewModel>();

            Mapper.CreateMap<HeaderChargeViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<LineChargeViewModel, LineChargeViewModel>();

            Mapper.CreateMap<LedgerSetting, LedgerSettingViewModel>();
            Mapper.CreateMap<LedgerSettingViewModel, LedgerSetting>();

            Mapper.CreateMap<LedgerHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<LedgerHeader, DocumentUniqueId>();


            Mapper.CreateMap<City, City>();
            Mapper.CreateMap<CostCenter, CostCenter>();
            Mapper.CreateMap<Country, Country>();
            Mapper.CreateMap<Currency, Currency>();
            Mapper.CreateMap<Division, Division>();
            Mapper.CreateMap<DocumentCategory, DocumentCategory>();
            Mapper.CreateMap<DocumentType, DocumentType>();
            Mapper.CreateMap<LedgerAccount, LedgerAccount>();
            Mapper.CreateMap<LedgerAccountGroup, LedgerAccountGroup>();
            Mapper.CreateMap<LedgerHeader, LedgerHeader>();
            Mapper.CreateMap<LedgerLine, LedgerLine>();
            Mapper.CreateMap<Narration, Narration>();
            Mapper.CreateMap<Person, Person>();
            Mapper.CreateMap<PersonContact, PersonContact>();
            Mapper.CreateMap<PersonContactType, PersonContactType>();
            Mapper.CreateMap<PersonDocument, PersonDocument>();
            Mapper.CreateMap<Reason, Reason>();
            Mapper.CreateMap<Site, Site>();
            Mapper.CreateMap<State, State>();
            Mapper.CreateMap<TdsCategory, TdsCategory>();
            Mapper.CreateMap<TdsGroup, TdsGroup>();

            //////////////////////////////////////////End Finance ViewModel Mapping///////////////////////////////////////////////////


            //////////////////////////////////////////Sales ViewModel Mapping///////////////////////////////////////////////////

            Mapper.CreateMap<ProductBuyer, ProductBuyerViewModel>();
            Mapper.CreateMap<ProductBuyerViewModel, ProductBuyer>();

            Mapper.CreateMap<BuyerViewModel, Person>();
            Mapper.CreateMap<Person, BuyerViewModel>();

            Mapper.CreateMap<BuyerViewModel, BusinessEntity>();
            Mapper.CreateMap<BusinessEntity, BuyerViewModel>();

            Mapper.CreateMap<BuyerViewModel, Buyer>();
            Mapper.CreateMap<Buyer, BuyerViewModel>();

            Mapper.CreateMap<BuyerViewModel, PersonAddress>();
            Mapper.CreateMap<PersonAddress, BuyerViewModel>();

            Mapper.CreateMap<BuyerViewModel, LedgerAccount>();
            Mapper.CreateMap<LedgerAccount, BuyerViewModel>();

            Mapper.CreateMap<ProductBuyerSettings, ProductBuyerSettingsViewModel>();
            Mapper.CreateMap<ProductBuyerSettingsViewModel, ProductBuyerSettings>();


            Mapper.CreateMap<LineChargeViewModel, SaleInvoiceLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<SaleInvoiceLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<PersonViewModel, Person>();
            Mapper.CreateMap<Person, PersonViewModel>();

            Mapper.CreateMap<PersonViewModel, BusinessEntity>();
            Mapper.CreateMap<BusinessEntity, PersonViewModel>();

            Mapper.CreateMap<PersonViewModel, Buyer>();
            Mapper.CreateMap<Buyer, PersonViewModel>();

            Mapper.CreateMap<PersonViewModel, PersonAddress>();
            Mapper.CreateMap<PersonAddress, PersonViewModel>();

            Mapper.CreateMap<PersonViewModel, LedgerAccount>();
            Mapper.CreateMap<LedgerAccount, PersonViewModel>();




            Mapper.CreateMap<SaleOrderLine, SaleOrderLineViewModel>();
            Mapper.CreateMap<SaleOrderLineViewModel, SaleOrderLine>();

            Mapper.CreateMap<SaleOrderHeader, SaleOrderHeaderIndexViewModel>();
            Mapper.CreateMap<SaleOrderHeaderIndexViewModel, SaleOrderHeader>();

            Mapper.CreateMap<SaleOrderHeaderIndexViewModelForEdit, SaleOrderHeaderIndexViewModel>();
            Mapper.CreateMap<SaleOrderHeaderIndexViewModel, SaleOrderHeaderIndexViewModelForEdit>();


            Mapper.CreateMap<SaleOrderCancelHeaderIndexViewModel, SaleOrderCancelHeader>();
            Mapper.CreateMap<SaleOrderCancelHeader, SaleOrderCancelHeaderIndexViewModel>();

            Mapper.CreateMap<SaleOrderAmendmentHeader, SaleOrderAmendmentHeaderViewModel>();
            Mapper.CreateMap<SaleOrderAmendmentHeaderViewModel, SaleOrderAmendmentHeader>();

            Mapper.CreateMap<DispatchWaybillHeader, DispatchWaybillHeaderViewModel>();
            Mapper.CreateMap<DispatchWaybillHeaderViewModel, DispatchWaybillHeader>();

            Mapper.CreateMap<DispatchWaybillHeaderViewModelWithLog, DispatchWaybillHeaderViewModel>();
            Mapper.CreateMap<DispatchWaybillHeaderViewModel, DispatchWaybillHeaderViewModelWithLog>();

            Mapper.CreateMap<DispatchWaybillLine, DispatchWaybillLineViewModel>();
            Mapper.CreateMap<DispatchWaybillLineViewModel, DispatchWaybillLine>();


            Mapper.CreateMap<PersonContact, PersonContactViewModel>();
            Mapper.CreateMap<PersonContactViewModel, PersonContact>();

            Mapper.CreateMap<SaleDeliveryOrderHeaderViewModel, SaleDeliveryOrderHeader>();
            Mapper.CreateMap<SaleDeliveryOrderHeader, SaleDeliveryOrderHeaderViewModel>();

            Mapper.CreateMap<SaleDeliveryOrderLine, SaleDeliveryOrderLineViewModel>();
            Mapper.CreateMap<SaleDeliveryOrderLineViewModel, SaleDeliveryOrderLine>();


            Mapper.CreateMap<HeaderChargeViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<LineChargeViewModel, LineChargeViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<SaleInvoiceHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, DocumentUniqueId>();
            Mapper.CreateMap<SaleInvoiceHeaderDetail, DocumentUniqueId>();

            Mapper.CreateMap<SaleDispatchHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<SaleDispatchHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleDispatchHeaderIndexViewModel, DocumentUniqueId>();

            Mapper.CreateMap<PackingHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PackingHeader, DocumentUniqueId>();
            Mapper.CreateMap<PackingHeaderIndexViewModel, DocumentUniqueId>();



            Mapper.CreateMap<SaleDeliveryHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<SaleDeliveryHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleDeliveryHeaderIndexViewModel, DocumentUniqueId>();


            Mapper.CreateMap<DispatchWaybillHeader, DocumentUniqueId>();
            Mapper.CreateMap<DispatchWaybillHeaderViewModel, DocumentUniqueId>();

            Mapper.CreateMap<SaleDeliveryOrderCancelHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleDeliveryOrderCancelHeaderViewModel, DocumentUniqueId>();

            Mapper.CreateMap<SaleDeliveryOrderHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleDeliveryOrderHeaderViewModel, DocumentUniqueId>();

            Mapper.CreateMap<SaleOrderAmendmentHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleOrderAmendmentHeaderViewModel, DocumentUniqueId>();

            Mapper.CreateMap<SaleOrderCancelHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleOrderCancelHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<SaleOrderCancelHeaderIndexViewModel, DocumentUniqueId>();

            Mapper.CreateMap<SaleOrderHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleOrderHeaderIndexViewModel, DocumentUniqueId>();

            Mapper.CreateMap<ProductBuyer, ProductBuyer>();
            Mapper.CreateMap<SaleOrderAmendmentHeader, SaleOrderAmendmentHeader>();
            Mapper.CreateMap<SaleOrderQtyAmendmentLineViewModel, SaleOrderQtyAmendmentLine>();
            Mapper.CreateMap<SaleDeliveryOrderHeader, SaleDeliveryOrderHeader>();
            Mapper.CreateMap<SaleDeliveryOrderLineViewModel, SaleDeliveryOrderLine>();
            Mapper.CreateMap<SaleDeliveryOrderLine, SaleDeliveryOrderLine>();
            Mapper.CreateMap<SaleOrderCancelHeader, SaleOrderCancelHeader>();
            Mapper.CreateMap<SaleOrderQtyAmendmentLine, SaleOrderQtyAmendmentLine>();
            Mapper.CreateMap<SaleOrderHeader, SaleOrderHeader>();
            Mapper.CreateMap<SaleOrderLineIndexViewModel, SaleOrderLine>();
            Mapper.CreateMap<SaleOrderLine, SaleOrderLine>();
            Mapper.CreateMap<SaleOrderCancelLine, SaleOrderCancelLine>();
            Mapper.CreateMap<SaleOrderCancelLineViewModel, SaleOrderCancelLine>();
            Mapper.CreateMap<SaleDeliveryOrderSettings, SaleDeliveryOrderSettingsViewModel>();
            Mapper.CreateMap<SaleDeliveryOrderSettingsViewModel, SaleDeliveryOrderSettings>();
            Mapper.CreateMap<SaleDeliveryOrderCancelHeader, SaleDeliveryOrderCancelHeader>();
            Mapper.CreateMap<SaleDeliveryOrderCancelLine, SaleDeliveryOrderCancelLine>();
            Mapper.CreateMap<DispatchWaybillLine, DispatchWaybillLine>();
            Mapper.CreateMap<SaleDeliveryOrderSettings, SaleDeliveryOrderSettings>();

            Mapper.CreateMap<SaleDeliveryOrderCancelHeader, SaleDeliveryOrderCancelHeaderViewModel>();
            Mapper.CreateMap<SaleDeliveryOrderCancelHeaderViewModel, SaleDeliveryOrderCancelHeader>();

            Mapper.CreateMap<SaleDeliveryOrderCancelLine, SaleDeliveryOrderCancelLineViewModel>();
            Mapper.CreateMap<SaleDeliveryOrderCancelLineViewModel, SaleDeliveryOrderCancelLine>();

            Mapper.CreateMap<LineChargeViewModel, SaleInvoiceReturnLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<SaleInvoiceReturnLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, SaleInvoiceReturnHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<SaleInvoiceReturnHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, SaleInvoiceHeaderCharge>();
            Mapper.CreateMap<SaleInvoiceHeaderCharge, HeaderChargeViewModel>();


            Mapper.CreateMap<CalculationFooterViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<CalculationProductViewModel, LineChargeViewModel>();

            Mapper.CreateMap<SaleInvoiceReturnHeaderViewModel, SaleInvoiceReturnHeader>();
            Mapper.CreateMap<SaleInvoiceReturnHeader, SaleInvoiceReturnHeaderViewModel>();

            Mapper.CreateMap<SaleInvoiceReturnHeaderViewModel, SaleDispatchReturnHeader>();
            Mapper.CreateMap<SaleDispatchReturnHeader, SaleInvoiceReturnHeaderViewModel>();

            Mapper.CreateMap<SaleDispatchReturnLine, SaleInvoiceReturnLine>();
            Mapper.CreateMap<SaleInvoiceReturnLine, SaleDispatchReturnLine>();

            Mapper.CreateMap<SaleInvoiceReturnLine, SaleInvoiceReturnLineViewModel>();
            Mapper.CreateMap<SaleInvoiceReturnLineViewModel, SaleInvoiceReturnLine>();

            Mapper.CreateMap<SaleInvoiceReturnHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<SaleInvoiceReturnHeader, DocumentUniqueId>();

            Mapper.CreateMap<SaleDispatchReturnHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleDispatchReturnHeaderViewModel, DocumentUniqueId>();

            Mapper.CreateMap<SaleInvoiceReturnHeader, SaleInvoiceReturnHeader>();
            Mapper.CreateMap<SaleDispatchReturnHeader, SaleDispatchReturnHeader>();
            Mapper.CreateMap<SaleInvoiceReturnLineIndexViewModel, SaleInvoiceReturnLine>();
            Mapper.CreateMap<SaleDispatchReturnLineIndexViewModel, SaleDispatchReturnLine>();
            Mapper.CreateMap<SaleInvoiceReturnLine, SaleInvoiceReturnLine>();
            Mapper.CreateMap<SaleDispatchReturnLine, SaleDispatchReturnLine>();
            Mapper.CreateMap<SaleInvoiceReturnLineCharge, SaleInvoiceReturnLineCharge>();
            Mapper.CreateMap<SaleInvoiceReturnHeaderCharge, SaleInvoiceReturnHeaderCharge>();





            Mapper.CreateMap<SaleOrderSettings, SaleOrderSettingsViewModel>();
            Mapper.CreateMap<SaleOrderSettingsViewModel, SaleOrderSettings>();

            Mapper.CreateMap<DirectSaleInvoiceHeaderViewModel, SaleInvoiceHeader>();
            Mapper.CreateMap<SaleInvoiceHeader, DirectSaleInvoiceHeaderViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceHeaderViewModel, PackingHeader>();
            Mapper.CreateMap<PackingHeader, DirectSaleInvoiceHeaderViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceHeaderViewModel, SaleDispatchHeader>();
            Mapper.CreateMap<SaleDispatchHeader, DirectSaleInvoiceHeaderViewModel>();

            Mapper.CreateMap<DocumentTypeHeaderAttributeViewModel, DocumentTypeHeaderAttribute>();
            Mapper.CreateMap<DocumentTypeHeaderAttribute, DocumentTypeHeaderAttributeViewModel>();



            Mapper.CreateMap<SaleDispatchHeaderViewModel, PackingHeader>();
            Mapper.CreateMap<PackingHeader, SaleDispatchHeaderViewModel>();

            Mapper.CreateMap<SaleDispatchHeaderViewModel, SaleDispatchHeader>();
            Mapper.CreateMap<SaleDispatchHeader, SaleDispatchHeaderViewModel>();

            Mapper.CreateMap<PackingHeaderViewModel, PackingHeader>();
            Mapper.CreateMap<PackingHeader, PackingHeaderViewModel>();

            Mapper.CreateMap<SaleDeliveryHeaderViewModel, SaleDeliveryHeader>();
            Mapper.CreateMap<SaleDeliveryHeader, SaleDeliveryHeaderViewModel>();


            Mapper.CreateMap<DirectSaleInvoiceLineViewModel, SaleInvoiceLine>();
            Mapper.CreateMap<SaleInvoiceLine, DirectSaleInvoiceLineViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceLineViewModel, SaleInvoiceLineDetail>();
            Mapper.CreateMap<SaleInvoiceLineDetail, DirectSaleInvoiceLineViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceLineViewModel, PackingLine>();
            Mapper.CreateMap<PackingLine, DirectSaleInvoiceLineViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceLineViewModel, SaleDispatchLine>();
            Mapper.CreateMap<SaleDispatchLine, DirectSaleInvoiceLineViewModel>();

            Mapper.CreateMap<SaleEnquiryLine, SaleEnquiryLineViewModel>();
            Mapper.CreateMap<SaleEnquiryLineViewModel, SaleEnquiryLine>();

            Mapper.CreateMap<SaleEnquiryHeader, SaleEnquiryHeaderIndexViewModel>();
            Mapper.CreateMap<SaleEnquiryHeaderIndexViewModel, SaleEnquiryHeader>();

            Mapper.CreateMap<SaleEnquiryHeaderIndexViewModelForEdit, SaleEnquiryHeaderIndexViewModel>();
            Mapper.CreateMap<SaleEnquiryHeaderIndexViewModel, SaleEnquiryHeaderIndexViewModelForEdit>();

            Mapper.CreateMap<SaleEnquiryHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleEnquiryHeaderIndexViewModel, DocumentUniqueId>();

            Mapper.CreateMap<SaleEnquirySettings, SaleEnquirySettingsViewModel>();
            Mapper.CreateMap<SaleEnquirySettingsViewModel, SaleEnquirySettings>();




            Mapper.CreateMap<SaleDispatchLineViewModel, PackingLine>();
            Mapper.CreateMap<PackingLine, SaleDispatchLineViewModel>();

            Mapper.CreateMap<SaleDispatchLineViewModel, SaleDispatchLine>();
            Mapper.CreateMap<SaleDispatchLine, SaleDispatchLineViewModel>();


            Mapper.CreateMap<PackingLineViewModel, PackingLine>();
            Mapper.CreateMap<PackingLine, PackingLineViewModel>();

            Mapper.CreateMap<PackingLineViewModel, PackingLine>();
            Mapper.CreateMap<PackingLine, PackingLineViewModel>();

            Mapper.CreateMap<SaleDeliveryLineViewModel, SaleDeliveryLine>();
            Mapper.CreateMap<SaleDeliveryLine, SaleDeliveryLineViewModel>();


            Mapper.CreateMap<SaleInvoiceHeader, SaleInvoiceHeaderIndexViewModel>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, SaleInvoiceHeader>();

            Mapper.CreateMap<SaleInvoiceHeaderDetail, SaleInvoiceHeaderIndexViewModel>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, SaleInvoiceHeaderDetail>();

            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModelForEdit, SaleInvoiceHeaderIndexViewModel>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, SaleInvoiceHeaderIndexViewModelForEdit>();

            Mapper.CreateMap<SaleInvoiceLine, SaleInvoiceLineViewModel>();
            Mapper.CreateMap<SaleInvoiceLineViewModel, SaleInvoiceLine>();


            Mapper.CreateMap<SaleDispatchHeader, SaleInvoiceHeaderIndexViewModel>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, SaleDispatchHeader>();

            Mapper.CreateMap<SaleDispatchHeader, SaleInvoiceHeaderIndexViewModel>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, SaleDispatchHeader>();

            Mapper.CreateMap<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>();
            Mapper.CreateMap<SaleInvoiceSettingsViewModel, SaleInvoiceSetting>();

            Mapper.CreateMap<SaleDispatchSetting, SaleDispatchSettingsViewModel>();
            Mapper.CreateMap<SaleDispatchSettingsViewModel, SaleDispatchSetting>();

            Mapper.CreateMap<PackingSetting, PackingSettingsViewModel>();
            Mapper.CreateMap<PackingSettingsViewModel, PackingSetting>();

            Mapper.CreateMap<SaleDeliverySetting, SaleDeliverySettingsViewModel>();
            Mapper.CreateMap<SaleDeliverySettingsViewModel, SaleDeliverySetting>();

            Mapper.CreateMap<SaleDispatchReturnHeader, SaleDispatchReturnHeaderViewModel>();
            Mapper.CreateMap<SaleDispatchReturnHeaderViewModel, SaleDispatchReturnHeader>();

            Mapper.CreateMap<SaleDispatchReturnHeader, SaleDispatchReturnHeader>();

            Mapper.CreateMap<SaleDispatchReturnLine, SaleDispatchReturnLineViewModel>();
            Mapper.CreateMap<SaleDispatchReturnLineViewModel, SaleDispatchReturnLine>();

            Mapper.CreateMap<SaleDispatchReturnLine, SaleDispatchReturnLine>();


            Mapper.CreateMap<Manufacturer, ManufacturerViewModel>();
            Mapper.CreateMap<ManufacturerViewModel, Manufacturer>();

            Mapper.CreateMap<Person, ManufacturerViewModel>();
            Mapper.CreateMap<ManufacturerViewModel, Person>();

            Mapper.CreateMap<PersonAddress, ManufacturerViewModel>();
            Mapper.CreateMap<ManufacturerViewModel, PersonAddress>();

            Mapper.CreateMap<LedgerAccount, ManufacturerViewModel>();
            Mapper.CreateMap<ManufacturerViewModel, LedgerAccount>();

            Mapper.CreateMap<BusinessEntity, ManufacturerViewModel>();
            Mapper.CreateMap<ManufacturerViewModel, BusinessEntity>();

            Mapper.CreateMap<PersonDocument, PersonDocument>();

            Mapper.CreateMap<Person, AgentViewModel>();
            Mapper.CreateMap<AgentViewModel, Person>();

            Mapper.CreateMap<BusinessEntity, AgentViewModel>();
            Mapper.CreateMap<AgentViewModel, BusinessEntity>();

            Mapper.CreateMap<Agent, AgentViewModel>();
            Mapper.CreateMap<AgentViewModel, Agent>();

            Mapper.CreateMap<PersonAddress, AgentViewModel>();
            Mapper.CreateMap<AgentViewModel, PersonAddress>();

            Mapper.CreateMap<LedgerAccount, AgentViewModel>();
            Mapper.CreateMap<AgentViewModel, LedgerAccount>();

            Mapper.CreateMap<DrawBackTariffHead, DrawBackTariffHead>();

            Mapper.CreateMap<RouteLine, RouteLineViewModel>();
            Mapper.CreateMap<RouteLineViewModel, RouteLine>();

            Mapper.CreateMap<Route, Route>();
            Mapper.CreateMap<RouteLine, RouteLine>();

            Mapper.CreateMap<Courier, CourierViewModel>();
            Mapper.CreateMap<CourierViewModel, Courier>();

            Mapper.CreateMap<Person, CourierViewModel>();
            Mapper.CreateMap<CourierViewModel, Person>();

            Mapper.CreateMap<PersonAddress, CourierViewModel>();
            Mapper.CreateMap<CourierViewModel, PersonAddress>();

            Mapper.CreateMap<LedgerAccount, CourierViewModel>();
            Mapper.CreateMap<CourierViewModel, LedgerAccount>();

            Mapper.CreateMap<BusinessEntity, CourierViewModel>();
            Mapper.CreateMap<CourierViewModel, BusinessEntity>();


            Mapper.CreateMap<SaleQuotationHeader, SaleQuotationHeaderViewModel>();
            Mapper.CreateMap<SaleQuotationHeaderViewModel, SaleQuotationHeader>();

            Mapper.CreateMap<SaleQuotationHeaderDetail, SaleQuotationHeaderViewModel>();
            Mapper.CreateMap<SaleQuotationHeaderViewModel, SaleQuotationHeaderDetail>();

            Mapper.CreateMap<SaleQuotationLine, SaleQuotationLineViewModel>();
            Mapper.CreateMap<SaleQuotationLineViewModel, SaleQuotationLine>();

            Mapper.CreateMap<SaleQuotationSettings, SaleQuotationSettingsViewModel>();
            Mapper.CreateMap<SaleQuotationSettingsViewModel, SaleQuotationSettings>();

            Mapper.CreateMap<LineChargeViewModel, SaleQuotationLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<SaleQuotationLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, SaleQuotationHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<SaleQuotationHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<SaleQuotationSettings, SaleQuotationSettings>();
            Mapper.CreateMap<SaleQuotationLine, SaleQuotationLine>();

            Mapper.CreateMap<SaleQuotationHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleQuotationHeaderDetail, DocumentUniqueId>();
            Mapper.CreateMap<SaleQuotationHeaderViewModel, DocumentUniqueId>();


            //////////////////////////////////////////End Sales ViewModel Mapping///////////////////////////////////////////////////



            //Comment Start

            //////////////////////////////////////////Human Resource ViewModel Mapping///////////////////////////////////////////////////

            Mapper.CreateMap<Person, EmployeeViewModel>();
            Mapper.CreateMap<EmployeeViewModel, Person>();

            Mapper.CreateMap<BusinessEntity, EmployeeViewModel>();
            Mapper.CreateMap<EmployeeViewModel, BusinessEntity>();

            Mapper.CreateMap<Employee, EmployeeViewModel>();
            Mapper.CreateMap<EmployeeViewModel, Employee>();

            Mapper.CreateMap<PersonAddress, EmployeeViewModel>();
            Mapper.CreateMap<EmployeeViewModel, PersonAddress>();

            Mapper.CreateMap<LedgerAccount, EmployeeViewModel>();
            Mapper.CreateMap<EmployeeViewModel, LedgerAccount>();

            Mapper.CreateMap<PurchaseInvoiceHeader, PurchaseInvoiceHeaderViewModel>();
            Mapper.CreateMap<PurchaseInvoiceHeaderViewModel, PurchaseInvoiceHeader>();

            Mapper.CreateMap<PersonContactViewModel, PersonContact>();
            Mapper.CreateMap<PersonContact, PersonContactViewModel>();

            Mapper.CreateMap<HeaderChargeViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<LineChargeViewModel, LineChargeViewModel>();

            Mapper.CreateMap<Department, Department>();
            Mapper.CreateMap<Designation, Designation>();
            Mapper.CreateMap<LeaveType, LeaveType>();
            Mapper.CreateMap<Person, Person>();
            Mapper.CreateMap<PersonContact, PersonContact>();


            Mapper.CreateMap<AttendanceHeaderViewModel, AttendanceHeader>();

            Mapper.CreateMap<AttendanceHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<AttendanceHeader, DocumentUniqueId>();
            Mapper.CreateMap<AttendanceHeader, AttendanceHeader>();

            Mapper.CreateMap<OverTimeApplicationHeaderViewModel, OverTimeApplicationHeader>();
            Mapper.CreateMap<OverTimeApplicationHeader, DocumentUniqueId>();
            Mapper.CreateMap<OverTimeApplicationHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<OverTimeApplicationHeader, OverTimeApplicationHeader>();
            Mapper.CreateMap<OverTimeApplicationLine, OverTimeApplicationLine>();

            //////////////////////////////////////////End Human Resource ViewModel Mapping///////////////////////////////////////////////////


            //////////////////////////////////////////Calculation ViewModel Mapping///////////////////////////////////////////////////

            Mapper.CreateMap<CalculationFooterViewModel, CalculationFooter>();
            Mapper.CreateMap<CalculationFooter, CalculationFooterViewModel>();

            Mapper.CreateMap<CalculationProductViewModel, CalculationProduct>();
            Mapper.CreateMap<CalculationProduct, CalculationProductViewModel>();

            Mapper.CreateMap<CalculationHeaderLedgerAccount, CalculationHeaderLedgerAccountViewModel>();
            Mapper.CreateMap<CalculationHeaderLedgerAccountViewModel, CalculationHeaderLedgerAccount>();

            Mapper.CreateMap<CalculationLineLedgerAccount, CalculationLineLedgerAccountViewModel>();
            Mapper.CreateMap<CalculationLineLedgerAccountViewModel, CalculationLineLedgerAccount>();

            Mapper.CreateMap<HeaderChargeViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<LineChargeViewModel, LineChargeViewModel>();


            Mapper.CreateMap<Calculation, Calculation>();
            Mapper.CreateMap<CalculationFooter, CalculationFooter>();
            Mapper.CreateMap<CalculationProduct, CalculationProduct>();
            Mapper.CreateMap<CalculationHeaderLedgerAccount, CalculationHeaderLedgerAccount>();
            Mapper.CreateMap<CalculationLineLedgerAccount, CalculationLineLedgerAccount>();
            Mapper.CreateMap<Charge, Charge>();
            Mapper.CreateMap<ChargeGroupPerson, ChargeGroupPerson>();
            Mapper.CreateMap<ChargeGroupProduct, ChargeGroupProduct>();
            Mapper.CreateMap<ChargeType, ChargeType>();


            //////////////////////////////////////////End Calculation ViewModel Mapping///////////////////////////////////////////////////


            //////////////////////////////////////////Store ViewModel Mapping///////////////////////////////////////////////////

            Mapper.CreateMap<Person, PersonViewModel>();
            Mapper.CreateMap<PersonViewModel, Person>();

            Mapper.CreateMap<BusinessEntity, PersonViewModel>();
            Mapper.CreateMap<PersonViewModel, BusinessEntity>();

            Mapper.CreateMap<LedgerAccount, PersonViewModel>();
            Mapper.CreateMap<PersonViewModel, LedgerAccount>();

            Mapper.CreateMap<PersonAddress, PersonViewModel>();
            Mapper.CreateMap<PersonViewModel, PersonAddress>();

            Mapper.CreateMap<PersonContact, PersonContactViewModel>();
            Mapper.CreateMap<PersonContactViewModel, PersonContact>();

            Mapper.CreateMap<PersonRole, PersonRoleViewModel>();
            Mapper.CreateMap<PersonRoleViewModel, PersonRole>();

            Mapper.CreateMap<PersonSettings, PersonSettingsViewModel>();
            Mapper.CreateMap<PersonSettingsViewModel, PersonSettings>();

            Mapper.CreateMap<GatePassHeader, GatePassHeaderViewModel>();
            Mapper.CreateMap<GatePassHeaderViewModel, GatePassHeader>();

            Mapper.CreateMap<GatePassLine, GatePassLineViewModel>();
            Mapper.CreateMap<GatePassLineViewModel, GatePassLine>();

            Mapper.CreateMap<QAGroup, QAGroupViewModel>();
            Mapper.CreateMap<QAGroupViewModel, QAGroup>();

            Mapper.CreateMap<QAGroupLine, QAGroupLineViewModel>();
            Mapper.CreateMap<QAGroupLineViewModel, QAGroupLine>();

            //////
            Mapper.CreateMap<ProcessSequenceHeader, ProcessSequenceHeaderIndexViewModel>();
            Mapper.CreateMap<ProcessSequenceHeaderIndexViewModel, ProcessSequenceHeader>();

            Mapper.CreateMap<ProcessSequenceHeaderIndexViewModelForEdit, ProcessSequenceHeaderIndexViewModel>();
            Mapper.CreateMap<ProcessSequenceHeaderIndexViewModel, ProcessSequenceHeaderIndexViewModelForEdit>();


            Mapper.CreateMap<ProcessSequenceLine, ProcessSequenceLineViewModel>();
            Mapper.CreateMap<ProcessSequenceLineViewModel, ProcessSequenceLine>();

            Mapper.CreateMap<ProductGroupSettings, ProductGroupSettingsViewModel>();
            Mapper.CreateMap<ProductGroupSettingsViewModel, ProductGroupSettings>();

            Mapper.CreateMap<ProductCategorySettings, ProductCategorySettingsViewModel>();
            Mapper.CreateMap<ProductCategorySettingsViewModel, ProductCategorySettings>();

            Mapper.CreateMap<ProductTypeSettings, ProductTypeSettingsViewModel>();
            Mapper.CreateMap<ProductTypeSettingsViewModel, ProductTypeSettings>();

            Mapper.CreateMap<StockHeaderSettings, StockHeaderSettingsViewModel>();
            Mapper.CreateMap<StockHeaderSettingsViewModel, StockHeaderSettings>();

            Mapper.CreateMap<MaterialRequestSettings, MaterialRequestSettingsViewModel>();
            Mapper.CreateMap<MaterialRequestSettingsViewModel, MaterialRequestSettings>();

            Mapper.CreateMap<Stock, StockViewModel>();
            Mapper.CreateMap<StockViewModel, Stock>();

            Mapper.CreateMap<StockHeader, StockHeaderViewModel>();
            Mapper.CreateMap<StockHeaderViewModel, StockHeader>();

            Mapper.CreateMap<StockLine, StockLineViewModel>();
            Mapper.CreateMap<StockLineViewModel, StockLine>();

            Mapper.CreateMap<RequisitionHeaderViewModel, RequisitionHeader>();
            Mapper.CreateMap<RequisitionHeader, RequisitionHeaderViewModel>();

            Mapper.CreateMap<RequisitionLineViewModel, RequisitionLine>();
            Mapper.CreateMap<RequisitionLine, RequisitionLineViewModel>();

            Mapper.CreateMap<RequisitionSetting, RequisitionSettingsViewModel>();
            Mapper.CreateMap<RequisitionSettingsViewModel, RequisitionSetting>();

            Mapper.CreateMap<RequisitionCancelHeader, RequisitionCancelHeaderViewModel>();
            Mapper.CreateMap<RequisitionCancelHeaderViewModel, RequisitionCancelHeader>();

            Mapper.CreateMap<RequisitionCancelLine, RequisitionCancelLineViewModel>();
            Mapper.CreateMap<RequisitionCancelLineViewModel, RequisitionCancelLine>();

            Mapper.CreateMap<RateListHeader, RateListHeaderViewModel>();
            Mapper.CreateMap<RateListHeaderViewModel, RateListHeader>();

            Mapper.CreateMap<ExcessMaterialHeader, ExcessMaterialHeaderViewModel>();
            Mapper.CreateMap<ExcessMaterialHeaderViewModel, ExcessMaterialHeader>();

            Mapper.CreateMap<ExcessMaterialLine, ExcessMaterialLineViewModel>();
            Mapper.CreateMap<ExcessMaterialLineViewModel, ExcessMaterialLine>();

            Mapper.CreateMap<ExcessMaterialSettings, ExcessMaterialSettingsViewModel>();
            Mapper.CreateMap<ExcessMaterialSettingsViewModel, ExcessMaterialSettings>();

            Mapper.CreateMap<StockHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<StockHeader, DocumentUniqueId>();

            Mapper.CreateMap<RequisitionCancelHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<RequisitionCancelHeader, DocumentUniqueId>();

            Mapper.CreateMap<RequisitionHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<RequisitionHeader, DocumentUniqueId>();

            Mapper.CreateMap<ExcessMaterialHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<ExcessMaterialHeader, DocumentUniqueId>();

            Mapper.CreateMap<ExcessMaterialLineViewModel, DocumentUniqueId>();
            Mapper.CreateMap<ExcessMaterialLine, DocumentUniqueId>();

            Mapper.CreateMap<ProductGroupProcessSettings, ProductGroupProcessSettingsViewModel>();
            Mapper.CreateMap<ProductGroupProcessSettingsViewModel, ProductGroupProcessSettings>();

            Mapper.CreateMap<LedgerSetting, LedgerSettingViewModel>();
            Mapper.CreateMap<LedgerSettingViewModel, LedgerSetting>();


            Mapper.CreateMap<Dimension1, Dimension1ViewModel>();
            Mapper.CreateMap<Dimension1ViewModel, Dimension1>();

            Mapper.CreateMap<Dimension2, Dimension2ViewModel>();
            Mapper.CreateMap<Dimension2ViewModel, Dimension2>();

            Mapper.CreateMap<Dimension3, Dimension3ViewModel>();
            Mapper.CreateMap<Dimension3ViewModel, Dimension3>();

            Mapper.CreateMap<Dimension4, Dimension4ViewModel>();
            Mapper.CreateMap<Dimension4ViewModel, Dimension4>();


            Mapper.CreateMap<RequisitionCancelHeader, RequisitionCancelHeader>();
            Mapper.CreateMap<RequisitionCancelLineViewModel, RequisitionCancelLine>();
            Mapper.CreateMap<StockHeader, StockHeader>();
            Mapper.CreateMap<StockLine, StockLine>();
            Mapper.CreateMap<RequisitionHeader, RequisitionHeader>();
            Mapper.CreateMap<RequisitionLineViewModel, RequisitionLine>();
            Mapper.CreateMap<StockHeader, StockHeader>();
            Mapper.CreateMap<StockLine, StockLine>();
            Mapper.CreateMap<RequisitionLine, RequisitionLine>();
            Mapper.CreateMap<RequisitionCancelLine, RequisitionCancelLine>();
            Mapper.CreateMap<PersonRateGroup, PersonRateGroup>();
            Mapper.CreateMap<Dimension1, Dimension1>();
            Mapper.CreateMap<Dimension2, Dimension2>();
            Mapper.CreateMap<Godown, Godown>();
            Mapper.CreateMap<ChargeGroupSettings, ChargeGroupSettings>();
            Mapper.CreateMap<Process, Process>();
            Mapper.CreateMap<ProcessSequenceHeader, ProcessSequenceHeader>();
            Mapper.CreateMap<ProcessSequenceLine, ProcessSequenceLine>();
            Mapper.CreateMap<FinishedProduct, FinishedProduct>();
            Mapper.CreateMap<Product, Product>();
            Mapper.CreateMap<ProductCustomGroupHeader, ProductCustomGroupHeader>();
            Mapper.CreateMap<ProductCustomGroupLine, ProductCustomGroupLine>();
            Mapper.CreateMap<ProductGroup, ProductGroup>();
            Mapper.CreateMap<ProductCategory, ProductCategory>();
            Mapper.CreateMap<ProductTypeAttribute, ProductTypeAttribute>();
            Mapper.CreateMap<ProductType, ProductType>();
            Mapper.CreateMap<Unit, Unit>();
            Mapper.CreateMap<Gate, Gate>();
            Mapper.CreateMap<ProductRateGroup, ProductRateGroup>();
            Mapper.CreateMap<RateListLine, RateListLine>();
            Mapper.CreateMap<RateListHeader, RateListHeader>();
            Mapper.CreateMap<UnitConversion, UnitConversion>();
            Mapper.CreateMap<ExcessMaterialSettings, ExcessMaterialSettings>();
            Mapper.CreateMap<MaterialRequestSettings, MaterialRequestSettings>();
            Mapper.CreateMap<RequisitionSetting, RequisitionSetting>();
            Mapper.CreateMap<StockHeaderSettings, StockHeaderSettings>();

            //new
            Mapper.CreateMap<GatePassHeader, GatePassHeader>();
            Mapper.CreateMap<GatePassLineViewModel, GatePassLine>();
            Mapper.CreateMap<GatePassHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<GatePassHeader, DocumentUniqueId>();

            Mapper.CreateMap<QAGroup, QAGroup>();
            Mapper.CreateMap<QAGroupLineViewModel, QAGroupLine>();
            Mapper.CreateMap<QAGroupViewModel, DocumentUniqueId>();
            Mapper.CreateMap<QAGroup, DocumentUniqueId>();

            Mapper.CreateMap<PersonRegistration, PersonRegistration>();
            Mapper.CreateMap<LedgerAccount, LedgerAccount>();
            Mapper.CreateMap<PersonAddress, PersonAddress>();
            Mapper.CreateMap<Person, Person>();
            Mapper.CreateMap<BusinessEntity, BusinessEntity>();
            Mapper.CreateMap<ProductProcess, ProductProcess>();

            Mapper.CreateMap<ImportHeader, ImportHeaderViewModel>();
            Mapper.CreateMap<ImportHeaderViewModel, ImportHeader>();

            Mapper.CreateMap<ImportLine, ImportLineViewModel>();
            Mapper.CreateMap<ImportLineViewModel, ImportLine>();

            //////////////////////////////////////////End Store ViewModel Mapping///////////////////////////////////////////////////


            //////////////////////////////////////////Planning ViewModel Mapping///////////////////////////////////////////////////

            Mapper.CreateMap<ProdOrderSettingsViewModel, ProdOrderSettings>();
            Mapper.CreateMap<ProdOrderSettings, ProdOrderSettingsViewModel>();

            Mapper.CreateMap<MaterialPlanHeader, MaterialPlanHeaderViewModel>();
            Mapper.CreateMap<MaterialPlanHeaderViewModel, MaterialPlanHeader>();

            Mapper.CreateMap<MaterialPlanCancelHeader, MaterialPlanCancelHeaderViewModel>();
            Mapper.CreateMap<MaterialPlanCancelHeaderViewModel, MaterialPlanCancelHeader>();

            Mapper.CreateMap<MaterialPlanLineViewModel, MaterialPlanLine>();
            Mapper.CreateMap<MaterialPlanLine, MaterialPlanLineViewModel>();

            Mapper.CreateMap<MaterialPlanSettings, MaterialPlanSettingsViewModel>();
            Mapper.CreateMap<MaterialPlanSettingsViewModel, MaterialPlanSettings>();

            Mapper.CreateMap<ProdOrderLine, ProdOrderLineViewModel>();
            Mapper.CreateMap<ProdOrderLineViewModel, ProdOrderLine>();

            Mapper.CreateMap<ProdOrderHeader, ProdOrderHeaderViewModel>();
            Mapper.CreateMap<ProdOrderHeaderViewModel, ProdOrderHeader>();

            Mapper.CreateMap<ProdOrderCancelHeader, ProdOrderCancelHeaderViewModel>();
            Mapper.CreateMap<ProdOrderCancelHeaderViewModel, ProdOrderCancelHeader>();

            Mapper.CreateMap<HeaderChargeViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<LineChargeViewModel, LineChargeViewModel>();

            Mapper.CreateMap<MaterialPlanHeader, DocumentUniqueId>();
            Mapper.CreateMap<MaterialPlanHeaderViewModel, DocumentUniqueId>();

            Mapper.CreateMap<MaterialPlanCancelHeader, DocumentUniqueId>();
            Mapper.CreateMap<MaterialPlanCancelHeaderViewModel, DocumentUniqueId>();

            Mapper.CreateMap<ProdOrderCancelHeader, DocumentUniqueId>();
            Mapper.CreateMap<ProdOrderCancelHeaderViewModel, DocumentUniqueId>();

            Mapper.CreateMap<ProdOrderHeader, DocumentUniqueId>();
            Mapper.CreateMap<ProdOrderHeaderViewModel, DocumentUniqueId>();

            Mapper.CreateMap<ProdOrderHeader, ProdOrderHeader>();
            Mapper.CreateMap<ProdOrderLineViewModel, ProdOrderLine>();
            Mapper.CreateMap<ProdOrderCancelHeader, ProdOrderCancelHeader>();
            Mapper.CreateMap<ProdOrderCancelLineViewModel, ProdOrderCancelLine>();
            Mapper.CreateMap<ProdOrderCancelLine, ProdOrderCancelLine>();
            Mapper.CreateMap<MaterialPlanHeader, MaterialPlanHeader>();
            Mapper.CreateMap<MaterialPlanCancelHeader, MaterialPlanCancelHeader>();
            Mapper.CreateMap<MaterialPlanLine, MaterialPlanLine>();
            Mapper.CreateMap<ProdOrderLine, ProdOrderLine>();
            Mapper.CreateMap<PurchaseIndentLine, PurchaseIndentLine>();
            Mapper.CreateMap<MaterialPlanForSaleOrder, MaterialPlanForSaleOrder>();
            Mapper.CreateMap<MaterialPlanForProdOrderLine, MaterialPlanForProdOrderLine>();
            Mapper.CreateMap<PurchaseIndentHeader, PurchaseIndentHeader>();
            Mapper.CreateMap<MaterialPlanForProdOrder, MaterialPlanForProdOrder>();
            Mapper.CreateMap<MaterialPlanSettings, MaterialPlanSettings>();
            Mapper.CreateMap<ProdOrderSettings, ProdOrderSettings>();

            //////////////////////////////////////////End Planning ViewModel Mapping///////////////////////////////////////////////////


            //////////////////////////////////////////Purchase ViewModel Mapping///////////////////////////////////////////////////

            Mapper.CreateMap<Person, SupplierViewModel>();
            Mapper.CreateMap<SupplierViewModel, Person>();

            Mapper.CreateMap<BusinessEntity, SupplierViewModel>();
            Mapper.CreateMap<SupplierViewModel, BusinessEntity>();

            Mapper.CreateMap<Supplier, SupplierViewModel>();
            Mapper.CreateMap<SupplierViewModel, Supplier>();

            Mapper.CreateMap<PersonAddress, SupplierViewModel>();
            Mapper.CreateMap<SupplierViewModel, PersonAddress>();

            Mapper.CreateMap<LedgerAccount, SupplierViewModel>();
            Mapper.CreateMap<SupplierViewModel, LedgerAccount>();



            Mapper.CreateMap<PersonContact, PersonContactViewModel>();
            Mapper.CreateMap<PersonContactViewModel, PersonContact>();

            Mapper.CreateMap<PurchaseInvoiceHeader, PurchaseInvoiceHeaderViewModel>();
            Mapper.CreateMap<PurchaseInvoiceHeaderViewModel, PurchaseInvoiceHeader>();

            Mapper.CreateMap<PurchaseInvoiceLine, PurchaseInvoiceLineViewModel>();
            Mapper.CreateMap<PurchaseInvoiceLineViewModel, PurchaseInvoiceLine>();

            Mapper.CreateMap<PurchaseQuotationLine, PurchaseQuotationLineViewModel>();
            Mapper.CreateMap<PurchaseQuotationLineViewModel, PurchaseQuotationLine>();

            Mapper.CreateMap<PurchaseQuotationHeader, PurchaseQuotationHeaderViewModel>();
            Mapper.CreateMap<PurchaseQuotationHeaderViewModel, PurchaseQuotationHeader>();

            Mapper.CreateMap<PurchaseIndentHeader, PurchaseIndentHeaderViewModel>();
            Mapper.CreateMap<PurchaseIndentHeaderViewModel, PurchaseIndentHeader>();

            Mapper.CreateMap<PurchaseIndentLine, PurchaseIndentLineViewModel>();
            Mapper.CreateMap<PurchaseIndentLineViewModel, PurchaseIndentLine>();

            Mapper.CreateMap<PurchaseOrderHeader, PurchaseOrderHeaderViewModel>();
            Mapper.CreateMap<PurchaseOrderHeaderViewModel, PurchaseOrderHeader>();

            Mapper.CreateMap<PurchaseOrderLineViewModel, PurchaseOrderLine>();
            Mapper.CreateMap<PurchaseOrderLine, PurchaseOrderLineViewModel>();

            Mapper.CreateMap<PurchaseOrderSetting, PurchaseOrderSettingsViewModel>();
            Mapper.CreateMap<PurchaseOrderSettingsViewModel, PurchaseOrderSetting>();

            Mapper.CreateMap<LineChargeViewModel, PurchaseOrderLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseOrderLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, PurchaseOrderHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseOrderHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<PurchaseGoodsReceiptHeader, PurchaseGoodsReceiptHeaderViewModel>();
            Mapper.CreateMap<PurchaseGoodsReceiptHeaderViewModel, PurchaseGoodsReceiptHeader>();

            Mapper.CreateMap<PurchaseGoodsReceiptLine, PurchaseGoodsReceiptLineViewModel>();
            Mapper.CreateMap<PurchaseGoodsReceiptLineViewModel, PurchaseGoodsReceiptLine>();

            Mapper.CreateMap<PurchaseOrderCancelHeaderViewModel, PurchaseOrderCancelHeader>();
            Mapper.CreateMap<PurchaseOrderCancelHeader, PurchaseOrderCancelHeaderViewModel>();

            Mapper.CreateMap<PurchaseGoodsReceiptHeader, PurchaseGoodsReceiptHeaderViewModel>();
            Mapper.CreateMap<PurchaseGoodsReceiptHeaderViewModel, PurchaseGoodsReceiptHeader>();

            Mapper.CreateMap<PurchaseGoodsReceiptLine, PurchaseGoodsReceiptLineViewModel>();
            Mapper.CreateMap<PurchaseGoodsReceiptLineViewModel, PurchaseGoodsReceiptLine>();

            Mapper.CreateMap<PurchaseGoodsReceiptHeaderViewModel, PurchaseWaybill>();
            Mapper.CreateMap<PurchaseWaybill, PurchaseGoodsReceiptHeaderViewModel>();

            Mapper.CreateMap<PurchaseInvoiceHeader, PurchaseInvoiceHeaderViewModel>().ForMember(m => m.CreatedBy, x => x.Ignore())
               .ForMember(m => m.CreatedDate, x => x.Ignore())
               .ForMember(m => m.ModifiedBy, x => x.Ignore())
               .ForMember(m => m.ModifiedDate, x => x.Ignore())
               .ForMember(m => m.DivisionId, x => x.Ignore())
               .ForMember(m => m.SiteId, x => x.Ignore());
            Mapper.CreateMap<PurchaseInvoiceHeaderViewModel, PurchaseInvoiceHeader>().ForMember(m => m.CreatedBy, x => x.Ignore())
                .ForMember(m => m.CreatedDate, x => x.Ignore())
                .ForMember(m => m.ModifiedBy, x => x.Ignore())
                .ForMember(m => m.ModifiedDate, x => x.Ignore())
                .ForMember(m => m.DivisionId, x => x.Ignore())
                .ForMember(m => m.SiteId, x => x.Ignore());

            Mapper.CreateMap<PurchaseIndentCancelHeader, PurchaseIndentCancelHeaderViewModel>();
            Mapper.CreateMap<PurchaseIndentCancelHeaderViewModel, PurchaseIndentCancelHeader>();

            Mapper.CreateMap<PurchaseIndentCancelLine, PurchaseIndentCancelLineViewModel>();
            Mapper.CreateMap<PurchaseIndentCancelLineViewModel, PurchaseIndentCancelLine>();

            Mapper.CreateMap<PurchaseInvoiceReturnHeader, PurchaseInvoiceReturnHeaderViewModel>();
            Mapper.CreateMap<PurchaseInvoiceReturnHeaderViewModel, PurchaseInvoiceReturnHeader>();

            Mapper.CreateMap<PurchaseInvoiceReturnLine, PurchaseInvoiceReturnLineViewModel>();
            Mapper.CreateMap<PurchaseInvoiceReturnLineViewModel, PurchaseInvoiceReturnLine>();

            Mapper.CreateMap<PurchaseGoodsReturnHeader, PurchaseGoodsReturnHeaderViewModel>();
            Mapper.CreateMap<PurchaseGoodsReturnHeaderViewModel, PurchaseGoodsReturnHeader>();

            Mapper.CreateMap<PurchaseGoodsReturnLine, PurchaseGoodsReturnLineViewModel>();
            Mapper.CreateMap<PurchaseGoodsReturnLineViewModel, PurchaseGoodsReturnLine>();

            Mapper.CreateMap<LineChargeViewModel, PurchaseOrderLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseOrderLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<LineChargeViewModel, PurchaseOrderRateAmendmentLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseOrderRateAmendmentLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, PurchaseOrderAmendmentHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseOrderAmendmentHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<PurchaseWaybillViewModel, PurchaseWaybill>();
            Mapper.CreateMap<PurchaseWaybill, PurchaseWaybillViewModel>();

            Mapper.CreateMap<PurchaseOrderAmendmentHeader, PurchaseOrderAmendmentHeaderViewModel>();
            Mapper.CreateMap<PurchaseOrderAmendmentHeaderViewModel, PurchaseOrderAmendmentHeader>();

            Mapper.CreateMap<PurchaseOrderRateAmendmentLine, PurchaseOrderRateAmendmentLineViewModel>();
            Mapper.CreateMap<PurchaseOrderRateAmendmentLineViewModel, PurchaseOrderRateAmendmentLine>();

            Mapper.CreateMap<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSettingsViewModel>();
            Mapper.CreateMap<PurchaseGoodsReceiptSettingsViewModel, PurchaseGoodsReceiptSetting>();

            Mapper.CreateMap<PurchaseInvoiceSetting, PurchaseInvoiceSettingsViewModel>();
            Mapper.CreateMap<PurchaseInvoiceSettingsViewModel, PurchaseInvoiceSetting>();

            Mapper.CreateMap<PurchaseQuotationSetting, PurchaseQuotationSettingsViewModel>();
            Mapper.CreateMap<PurchaseQuotationSettingsViewModel, PurchaseQuotationSetting>();

            Mapper.CreateMap<CalculationFooterViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<CalculationProductViewModel, LineChargeViewModel>();

            Mapper.CreateMap<HeaderChargeViewModel, PurchaseInvoiceHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseInvoiceHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<LineChargeViewModel, PurchaseInvoiceLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseInvoiceLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, PurchaseQuotationHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseQuotationHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<LineChargeViewModel, PurchaseQuotationLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseQuotationLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<LineChargeViewModel, PurchaseInvoiceReturnLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseInvoiceReturnLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, PurchaseInvoiceReturnHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<PurchaseInvoiceReturnHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<PurchaseIndentSetting, PurchaseIndentSettingsViewModel>();
            Mapper.CreateMap<PurchaseIndentSettingsViewModel, PurchaseIndentSetting>();

            Mapper.CreateMap<PurchaseInvoiceReturnHeaderViewModel, PurchaseGoodsReturnHeader>().ForMember(m => m.PurchaseGoodsReturnHeaderId, x => x.Ignore());
            Mapper.CreateMap<PurchaseGoodsReturnHeader, PurchaseInvoiceReturnHeaderViewModel>();

            Mapper.CreateMap<PurchaseInvoiceReturnLineViewModel, PurchaseGoodsReturnLine>();
            Mapper.CreateMap<PurchaseGoodsReturnLine, PurchaseInvoiceReturnLineViewModel>();

            Mapper.CreateMap<PurchaseInvoiceReturnHeader, PurchaseGoodsReturnHeader>().ForMember(m => m.PurchaseGoodsReturnHeaderId, x => x.Ignore());
            Mapper.CreateMap<PurchaseGoodsReturnHeader, PurchaseInvoiceReturnHeader>();

            Mapper.CreateMap<PurchaseInvoiceReturnLine, PurchaseGoodsReturnLine>();
            Mapper.CreateMap<PurchaseGoodsReturnLine, PurchaseInvoiceReturnLine>();

            Mapper.CreateMap<PurchaseInvoiceHeaderViewModel, PurchaseGoodsReceiptHeader>();
            Mapper.CreateMap<PurchaseGoodsReceiptHeader, PurchaseInvoiceHeaderViewModel>();

            Mapper.CreateMap<PurchaseInvoiceLineViewModel, PurchaseGoodsReceiptLine>();
            Mapper.CreateMap<PurchaseGoodsReceiptLine, PurchaseInvoiceLineViewModel>();

            Mapper.CreateMap<HeaderChargeViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<LineChargeViewModel, LineChargeViewModel>();

            Mapper.CreateMap<PurchaseInvoiceHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PurchaseInvoiceHeader, DocumentUniqueId>();

            Mapper.CreateMap<PurchaseQuotationHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PurchaseQuotationHeader, DocumentUniqueId>();

            Mapper.CreateMap<PurchaseGoodsReceiptHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PurchaseGoodsReceiptHeader, DocumentUniqueId>();

            Mapper.CreateMap<PurchaseGoodsReturnHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PurchaseGoodsReturnHeader, DocumentUniqueId>();

            Mapper.CreateMap<PurchaseIndentCancelHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PurchaseIndentCancelHeader, DocumentUniqueId>();

            Mapper.CreateMap<PurchaseIndentHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PurchaseIndentHeader, DocumentUniqueId>();

            Mapper.CreateMap<PurchaseInvoiceReturnHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PurchaseInvoiceReturnHeader, DocumentUniqueId>();

            Mapper.CreateMap<PurchaseOrderCancelHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PurchaseOrderCancelHeader, DocumentUniqueId>();

            Mapper.CreateMap<PurchaseOrderHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PurchaseOrderHeader, DocumentUniqueId>();

            Mapper.CreateMap<PurchaseOrderAmendmentHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PurchaseOrderAmendmentHeader, DocumentUniqueId>();


            Mapper.CreateMap<PurchaseInvoiceHeader, PurchaseInvoiceHeader>();
            Mapper.CreateMap<PurchaseQuotationHeader, PurchaseQuotationHeader>();
            Mapper.CreateMap<PurchaseGoodsReceiptHeader, PurchaseGoodsReceiptHeader>();
            Mapper.CreateMap<PurchaseInvoiceLineIndexViewModel, PurchaseInvoiceLine>();
            Mapper.CreateMap<PurchaseGoodsReceiptLineViewModel, PurchaseGoodsReceiptLine>();
            Mapper.CreateMap<PurchaseIndentHeader, PurchaseIndentHeader>();
            Mapper.CreateMap<PurchaseIndentLineViewModel, PurchaseIndentLine>();
            Mapper.CreateMap<PurchaseGoodsReturnHeader, PurchaseGoodsReturnHeader>();
            Mapper.CreateMap<PurchaseGoodsReturnLineIndexViewModel, PurchaseGoodsReturnLine>();
            Mapper.CreateMap<PurchaseOrderCancelHeader, PurchaseOrderCancelHeader>();
            Mapper.CreateMap<PurchaseOrderCancelLineViewModel, PurchaseOrderCancelLine>();
            Mapper.CreateMap<PurchaseInvoiceReturnHeader, PurchaseInvoiceReturnHeader>();
            Mapper.CreateMap<PurchaseInvoiceReturnLineIndexViewModel, PurchaseInvoiceReturnLine>();
            Mapper.CreateMap<PurchaseIndentCancelHeader, PurchaseIndentCancelHeader>();
            Mapper.CreateMap<PurchaseIndentCancelLineViewModel, PurchaseIndentCancelLine>();
            Mapper.CreateMap<PurchaseOrderHeader, PurchaseOrderHeader>();
            Mapper.CreateMap<PurchaseOrderLineViewModel, PurchaseOrderLine>();
            Mapper.CreateMap<PurchaseIndentCancelLine, PurchaseIndentCancelLine>();
            Mapper.CreateMap<PurchaseIndentLine, PurchaseIndentLine>();
            Mapper.CreateMap<PurchaseOrderCancelLine, PurchaseOrderCancelLine>();
            Mapper.CreateMap<PurchaseInvoiceLine, PurchaseInvoiceLine>();
            Mapper.CreateMap<PurchaseInvoiceLineCharge, PurchaseInvoiceLineCharge>();
            Mapper.CreateMap<PurchaseInvoiceHeaderCharge, PurchaseInvoiceHeaderCharge>();
            Mapper.CreateMap<PurchaseQuotationLine, PurchaseQuotationLine>();
            Mapper.CreateMap<PurchaseQuotationLineCharge, PurchaseQuotationLineCharge>();
            Mapper.CreateMap<PurchaseQuotationHeaderCharge, PurchaseQuotationHeaderCharge>();
            Mapper.CreateMap<PurchaseGoodsReceiptLine, PurchaseGoodsReceiptLine>();
            Mapper.CreateMap<PurchaseGoodsReturnLine, PurchaseGoodsReturnLine>();
            Mapper.CreateMap<PurchaseInvoiceReturnLine, PurchaseInvoiceReturnLine>();
            Mapper.CreateMap<PurchaseInvoiceReturnLineCharge, PurchaseInvoiceReturnLineCharge>();
            Mapper.CreateMap<PurchaseInvoiceReturnHeaderCharge, PurchaseInvoiceReturnHeaderCharge>();
            Mapper.CreateMap<PurchaseOrderLine, PurchaseOrderLine>();
            Mapper.CreateMap<PurchaseOrderLineCharge, PurchaseOrderLineCharge>();
            Mapper.CreateMap<PurchaseOrderHeaderCharge, PurchaseOrderHeaderCharge>();
            Mapper.CreateMap<PurchaseOrderRateAmendmentLineCharge, PurchaseOrderRateAmendmentLineCharge>();
            Mapper.CreateMap<PurchaseOrderAmendmentHeaderCharge, PurchaseOrderAmendmentHeaderCharge>();
            Mapper.CreateMap<PurchaseGoodsReceiptSetting, PurchaseGoodsReceiptSetting>();
            Mapper.CreateMap<PurchaseIndentSetting, PurchaseIndentSetting>();
            Mapper.CreateMap<PurchaseInvoiceSetting, PurchaseInvoiceSetting>();
            Mapper.CreateMap<PurchaseQuotationSetting, PurchaseQuotationSetting>();
            Mapper.CreateMap<PurchaseOrderSetting, PurchaseOrderSetting>();

            //////////////////////////////////////////End Purchase ViewModel Mapping///////////////////////////////////////////////////


            //////////////////////////////////////////Task ViewModel Mapping///////////////////////////////////////////////////

            Mapper.CreateMap<TasksViewModel, Tasks>();
            Mapper.CreateMap<Tasks, TasksViewModel>();

            Mapper.CreateMap<Tasks, Tasks>();
            Mapper.CreateMap<Project, Project>();
            Mapper.CreateMap<ProjectViewModel, Project>();
            Mapper.CreateMap<Project, ProjectViewModel>();
            Mapper.CreateMap<DAR, DAR>();

            Mapper.CreateMap<DAR, DARViewModel>();
            Mapper.CreateMap<DARViewModel, DAR>();

            Mapper.CreateMap<UserTeam, UserTeamViewModel>();
            Mapper.CreateMap<UserTeamViewModel, UserTeam>();

            //////////////////////////////////////////End Task ViewModel Mapping///////////////////////////////////////////////////


            //////////////////////////////////////////Rug ViewModel Mapping///////////////////////////////////////////////////

            Mapper.CreateMap<UnitConversion, UnitConversionViewModel>();
            Mapper.CreateMap<UnitConversionViewModel, UnitConversion>();

            Mapper.CreateMap<ProductBuyer, ProductBuyerViewModel>();
            Mapper.CreateMap<ProductBuyerViewModel, ProductBuyer>();

            Mapper.CreateMap<ProcessSequenceLine, ProcessSequenceLineViewModel>();
            Mapper.CreateMap<ProcessSequenceLineViewModel, ProcessSequenceLine>();

            Mapper.CreateMap<ProcessSequenceLine, ProcessSequenceLine>();

            Mapper.CreateMap<Stock, StockViewModel>();
            Mapper.CreateMap<StockViewModel, Stock>();

            Mapper.CreateMap<SaleOrderLine, SaleOrderLineViewModel>();
            Mapper.CreateMap<SaleOrderLineViewModel, SaleOrderLine>();

            Mapper.CreateMap<SaleOrderHeader, SaleOrderHeaderIndexViewModel>();
            Mapper.CreateMap<SaleOrderHeaderIndexViewModel, SaleOrderHeader>();

            Mapper.CreateMap<SaleOrderHeaderIndexViewModelForEdit, SaleOrderHeaderIndexViewModel>();
            Mapper.CreateMap<SaleOrderHeaderIndexViewModel, SaleOrderHeaderIndexViewModelForEdit>();


            Mapper.CreateMap<SaleEnquiryLine, SaleEnquiryLineViewModel>();
            Mapper.CreateMap<SaleEnquiryLineViewModel, SaleEnquiryLine>();

            Mapper.CreateMap<SaleEnquiryHeader, SaleEnquiryHeaderIndexViewModel>();
            Mapper.CreateMap<SaleEnquiryHeaderIndexViewModel, SaleEnquiryHeader>();

            Mapper.CreateMap<SaleEnquiryHeaderIndexViewModelForEdit, SaleEnquiryHeaderIndexViewModel>();
            Mapper.CreateMap<SaleEnquiryHeaderIndexViewModel, SaleEnquiryHeaderIndexViewModelForEdit>();

            Mapper.CreateMap<SaleEnquiryHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleEnquiryHeaderIndexViewModel, DocumentUniqueId>();


            Mapper.CreateMap<DirectSaleInvoiceHeaderViewModel, SaleInvoiceHeader>();
            Mapper.CreateMap<SaleInvoiceHeader, DirectSaleInvoiceHeaderViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceHeaderViewModel, PackingHeader>();
            Mapper.CreateMap<PackingHeader, DirectSaleInvoiceHeaderViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceHeaderViewModel, SaleDispatchHeader>();
            Mapper.CreateMap<SaleDispatchHeader, DirectSaleInvoiceHeaderViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceLineViewModel, SaleInvoiceLine>();
            Mapper.CreateMap<SaleInvoiceLine, DirectSaleInvoiceLineViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceLineViewModel, PackingLine>();
            Mapper.CreateMap<PackingLine, DirectSaleInvoiceLineViewModel>();

            Mapper.CreateMap<DirectSaleInvoiceLineViewModel, SaleDispatchLine>();
            Mapper.CreateMap<SaleDispatchLine, DirectSaleInvoiceLineViewModel>();

            Mapper.CreateMap<SaleInvoiceHeader, SaleInvoiceHeaderIndexViewModel>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, SaleInvoiceHeader>();

            Mapper.CreateMap<SaleInvoiceHeaderDetail, SaleInvoiceHeaderIndexViewModel>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, SaleInvoiceHeaderDetail>();

            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModelForEdit, SaleInvoiceHeaderIndexViewModel>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, SaleInvoiceHeaderIndexViewModelForEdit>();

            Mapper.CreateMap<SaleInvoiceLine, SaleInvoiceLineViewModel>();
            Mapper.CreateMap<SaleInvoiceLineViewModel, SaleInvoiceLine>();


            Mapper.CreateMap<SaleDispatchHeader, SaleInvoiceHeaderIndexViewModel>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, SaleDispatchHeader>();

            Mapper.CreateMap<SaleDispatchHeader, SaleInvoiceHeaderIndexViewModel>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, SaleDispatchHeader>();

            Mapper.CreateMap<PersonAddress, AgentViewModel>();
            Mapper.CreateMap<AgentViewModel, PersonAddress>();

            Mapper.CreateMap<PersonAddress, EmployeeViewModel>();
            Mapper.CreateMap<EmployeeViewModel, PersonAddress>();

            Mapper.CreateMap<PersonAddress, SupplierViewModel>();
            Mapper.CreateMap<SupplierViewModel, PersonAddress>();

            Mapper.CreateMap<PersonAddress, TransporterViewModel>();
            Mapper.CreateMap<TransporterViewModel, PersonAddress>();

            Mapper.CreateMap<PackingHeader, PackingHeaderViewModel>();
            Mapper.CreateMap<PackingHeaderViewModel, PackingHeader>();

            Mapper.CreateMap<PackingHeaderViewModelWithLog, PackingHeaderViewModel>();
            Mapper.CreateMap<PackingHeaderViewModel, PackingHeaderViewModelWithLog>();

            Mapper.CreateMap<PackingLine, PackingLineViewModel>();
            Mapper.CreateMap<PackingLineViewModel, PackingLine>();

            Mapper.CreateMap<JobReceiveSettings, JobReceiveSettingsViewModel>();
            Mapper.CreateMap<JobReceiveSettingsViewModel, JobReceiveSettings>();

            Mapper.CreateMap<JobReceiveQASettings, JobReceiveQASettingsViewModel>();
            Mapper.CreateMap<JobReceiveQASettingsViewModel, JobReceiveQASettings>();

            Mapper.CreateMap<JobReceiveHeader, JobReceiveHeaderViewModel>();
            Mapper.CreateMap<JobReceiveHeaderViewModel, JobReceiveHeader>();

            Mapper.CreateMap<JobReceiveQAPenalty, JobReceiveQAPenaltyViewModel>();
            Mapper.CreateMap<JobReceiveQAPenaltyViewModel, JobReceiveQAPenalty>();



            Mapper.CreateMap<FinishedProductViewModel, FinishedProduct>().ForMember(m => m.CreatedDate, x => x.Ignore())
              .ForMember(m => m.CreatedBy, x => x.Ignore())
              .ForMember(m => m.ModifiedBy, x => x.Ignore())
              .ForMember(m => m.ModifiedDate, x => x.Ignore())
              .ForMember(m => m.ImageFileName, x => x.Ignore())
              .ForMember(m => m.ImageFolderName, x => x.Ignore())
              .ForMember(m => m.IsSystemDefine, x => x.Ignore())
              .ForMember(m => m.SalesTaxGroupProductId, x => x.Ignore())
              .ForMember(m => m.UnitId, x => x.Ignore());
            Mapper.CreateMap<FinishedProduct, FinishedProductViewModel>();

            Mapper.CreateMap<CustomDetail, CustomDetailViewModel>();
            Mapper.CreateMap<CustomDetailViewModel, CustomDetail>();

            Mapper.CreateMap<CustomDetailViewModelWithLog, CustomDetailViewModel>();
            Mapper.CreateMap<CustomDetailViewModel, CustomDetailViewModelWithLog>();

            Mapper.CreateMap<Buyer, BuyerViewModel>();
            Mapper.CreateMap<BuyerViewModel, Buyer>();

            Mapper.CreateMap<Person, BuyerViewModel>();
            Mapper.CreateMap<BuyerViewModel, Person>();

            Mapper.CreateMap<PersonAddress, BuyerViewModel>();
            Mapper.CreateMap<BuyerViewModel, PersonAddress>();

            Mapper.CreateMap<LedgerAccount, BuyerViewModel>();
            Mapper.CreateMap<BuyerViewModel, LedgerAccount>();

            Mapper.CreateMap<BusinessEntity, BuyerViewModel>();
            Mapper.CreateMap<BuyerViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, TransporterViewModel>();
            Mapper.CreateMap<TransporterViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, JobWorkerViewModel>();
            Mapper.CreateMap<JobWorkerViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, SupplierViewModel>();
            Mapper.CreateMap<SupplierViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, EmployeeViewModel>();
            Mapper.CreateMap<EmployeeViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, AgentViewModel>();
            Mapper.CreateMap<AgentViewModel, BusinessEntity>();

            Mapper.CreateMap<JobOrderSettings, JobOrderSettingsViewModel>();
            Mapper.CreateMap<JobOrderSettingsViewModel, JobOrderSettings>();

            Mapper.CreateMap<SaleEnquirySettings, SaleEnquirySettingsViewModel>();
            Mapper.CreateMap<SaleEnquirySettingsViewModel, SaleEnquirySettings>();

            Mapper.CreateMap<CarpetSkuSettings, CarpetSkuSettingsViewModel>();
            Mapper.CreateMap<CarpetSkuSettingsViewModel, CarpetSkuSettings>();

            Mapper.CreateMap<ProductBuyerSettings, ProductBuyerSettingsViewModel>();
            Mapper.CreateMap<ProductBuyerSettingsViewModel, ProductBuyerSettings>();

            Mapper.CreateMap<JobOrderHeader, JobOrderHeaderViewModel>();
            Mapper.CreateMap<JobOrderHeaderViewModel, JobOrderHeader>();

            Mapper.CreateMap<JobOrderPerk, PerkViewModel>();
            Mapper.CreateMap<PerkViewModel, JobOrderPerk>().ForMember(m => m.CreatedDate, x => x.Ignore()).ForMember(m => m.CreatedBy, x => x.Ignore())
                .ForMember(m => m.ModifiedDate, x => x.Ignore())
                .ForMember(m => m.ModifiedBy, x => x.Ignore());

            Mapper.CreateMap<LineChargeViewModel, JobOrderLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<JobOrderLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, JobOrderHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<JobOrderHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<CalculationFooterViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<CalculationProductViewModel, LineChargeViewModel>();

            Mapper.CreateMap<SaleInvoiceSetting, SaleInvoiceSettingsViewModel>();
            Mapper.CreateMap<SaleInvoiceSettingsViewModel, SaleInvoiceSetting>();

            Mapper.CreateMap<LineChargeViewModel, SaleInvoiceLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<SaleInvoiceLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, SaleInvoiceHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<SaleInvoiceHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<LineChargeViewModel, SaleInvoiceReturnLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<SaleInvoiceReturnLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, SaleInvoiceReturnHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<SaleInvoiceReturnHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<LineChargeViewModel, LineChargeViewModel>();

            Mapper.CreateMap<JobOrderPerk, PerkViewModel>();
            Mapper.CreateMap<PerkViewModel, JobOrderPerk>().ForMember(m => m.CreatedDate, x => x.Ignore()).ForMember(m => m.CreatedBy, x => x.Ignore())
                .ForMember(m => m.ModifiedDate, x => x.Ignore())
                .ForMember(m => m.ModifiedBy, x => x.Ignore());

            Mapper.CreateMap<Perk, PerkViewModel>();
            Mapper.CreateMap<PerkViewModel, Perk>();

            Mapper.CreateMap<SaleInvoiceReturnHeaderViewModel, SaleInvoiceReturnHeader>();
            Mapper.CreateMap<SaleInvoiceReturnHeader, SaleInvoiceReturnHeaderViewModel>();

            Mapper.CreateMap<SaleInvoiceReturnHeaderViewModel, SaleDispatchReturnHeader>();
            Mapper.CreateMap<SaleDispatchReturnHeader, SaleInvoiceReturnHeaderViewModel>();

            Mapper.CreateMap<SaleDispatchReturnLine, SaleInvoiceReturnLine>();
            Mapper.CreateMap<SaleInvoiceReturnLine, SaleDispatchReturnLine>();

            Mapper.CreateMap<SaleInvoiceReturnLine, SaleInvoiceReturnLineViewModel>();
            Mapper.CreateMap<SaleInvoiceReturnLineViewModel, SaleInvoiceReturnLine>();

            Mapper.CreateMap<DirectSaleInvoiceHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<SaleInvoiceHeader, DocumentUniqueId>();
            Mapper.CreateMap<SaleInvoiceHeaderIndexViewModel, DocumentUniqueId>();
            Mapper.CreateMap<SaleInvoiceHeaderDetail, DocumentUniqueId>();

            Mapper.CreateMap<JobOrderHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobOrderHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobReceiveHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobReceiveHeader, DocumentUniqueId>();

            Mapper.CreateMap<PackingHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<PackingHeader, DocumentUniqueId>();

            Mapper.CreateMap<SaleInvoiceReturnHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<SaleInvoiceReturnHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobReceiveLine, JobReceiveLineViewModel>();
            Mapper.CreateMap<JobReceiveLineViewModel, JobReceiveLine>();

            Mapper.CreateMap<ProductCategorySettings, ProductCategorySettingsViewModel>();
            Mapper.CreateMap<ProductCategorySettingsViewModel, ProductCategorySettings>();

            Mapper.CreateMap<ProductCategoryProcessSettings, ProductCategoryProcessSettingsViewModel>();
            Mapper.CreateMap<ProductCategoryProcessSettingsViewModel, ProductCategoryProcessSettings>();

            Mapper.CreateMap<PackingSetting, PackingSettingsViewModel>();
            Mapper.CreateMap<PackingSettingsViewModel, PackingSetting>();




            Mapper.CreateMap<SaleInvoiceReturnHeader, SaleInvoiceReturnHeader>();
            Mapper.CreateMap<SaleDispatchReturnHeader, SaleDispatchReturnHeader>();
            Mapper.CreateMap<SaleInvoiceReturnLineIndexViewModel, SaleInvoiceReturnLine>();
            Mapper.CreateMap<SaleDispatchReturnLineIndexViewModel, SaleDispatchReturnLine>();
            Mapper.CreateMap<SaleInvoiceReturnLine, SaleInvoiceReturnLine>();
            Mapper.CreateMap<SaleDispatchReturnLine, SaleDispatchReturnLine>();
            Mapper.CreateMap<SaleInvoiceReturnLineCharge, SaleInvoiceReturnLineCharge>();
            Mapper.CreateMap<SaleInvoiceReturnHeaderCharge, SaleInvoiceReturnHeaderCharge>();
            Mapper.CreateMap<PackingLine, PackingLine>();
            Mapper.CreateMap<ProductBuyer, ProductBuyer>();
            Mapper.CreateMap<SaleDispatchLine, SaleDispatchLine>();
            Mapper.CreateMap<SaleInvoiceLine, SaleInvoiceLine>();
            Mapper.CreateMap<SaleInvoiceLineCharge, SaleInvoiceLineCharge>();
            Mapper.CreateMap<SaleInvoiceHeaderCharge, SaleInvoiceHeaderCharge>();
            Mapper.CreateMap<SaleInvoiceHeaderDetail, SaleInvoiceHeaderDetail>();
            Mapper.CreateMap<SaleDispatchHeader, SaleDispatchHeader>();
            Mapper.CreateMap<SaleInvoiceLine, SaleInvoiceLine>();
            Mapper.CreateMap<SaleDispatchLine, SaleDispatchLine>();
            Mapper.CreateMap<SaleInvoiceHeader, SaleInvoiceHeader>();
            Mapper.CreateMap<PackingHeader, PackingHeader>();
            Mapper.CreateMap<CostCenter, CostCenter>();
            Mapper.CreateMap<Colour, Colour>();
            Mapper.CreateMap<CustomDetail, CustomDetail>();
            Mapper.CreateMap<DeliveryTerms, DeliveryTerms>();
            Mapper.CreateMap<DescriptionOfGoods, DescriptionOfGoods>();
            Mapper.CreateMap<Product, Product>();
            Mapper.CreateMap<BomDetail, BomDetail>();

            Mapper.CreateMap<ProductCategory, ProductCategory>();
            Mapper.CreateMap<ProductCollection, ProductCollection>();
            Mapper.CreateMap<ProductCategory, ProductCategory>();
            Mapper.CreateMap<ProductContentHeader, ProductContentHeader>();
            Mapper.CreateMap<ProductContentLine, ProductContentLine>();
            Mapper.CreateMap<ProductDesign, ProductDesign>();
            Mapper.CreateMap<ProductDesignPattern, ProductDesignPattern>();
            Mapper.CreateMap<ProductInvoiceGroup, ProductInvoiceGroup>();
            Mapper.CreateMap<ProductNature, ProductNature>();
            Mapper.CreateMap<ProductQuality, ProductQuality>();
            Mapper.CreateMap<ProductShape, ProductShape>();
            Mapper.CreateMap<ProductSizeType, ProductSizeType>();
            Mapper.CreateMap<ProductStyle, ProductStyle>();
            Mapper.CreateMap<ReportLine, ReportLine>();
            Mapper.CreateMap<SalesTaxGroupParty, SalesTaxGroupParty>();
            Mapper.CreateMap<SalesTaxGroupProduct, SalesTaxGroupProduct>();
            Mapper.CreateMap<ShipMethod, ShipMethod>();
            Mapper.CreateMap<Size, Size>();
            Mapper.CreateMap<JobReceiveLine, JobReceiveLine>();
            Mapper.CreateMap<RateListLine, RateListLine>();
            Mapper.CreateMap<RateListHeader, RateListHeader>();
            Mapper.CreateMap<DocumentTypeTimeExtension, DocumentTypeTimeExtensionViewModel>();
            Mapper.CreateMap<DocumentTypeTimeExtensionViewModel, DocumentTypeTimeExtension>();
            Mapper.CreateMap<DocumentTypeTimeExtension, DocumentTypeTimeExtension>();
            Mapper.CreateMap<ProductProcess, ProductProcess>();
            Mapper.CreateMap<SaleInvoiceSetting, SaleInvoiceSetting>();

            //////////////////////////////////////////End Rug ViewModel Mapping///////////////////////////////////////////////////


            //////////////////////////////////////////Module ViewModel Mapping///////////////////////////////////////////////////

            container.RegisterType<IRepository<MvcController>, Repository<MvcController>>();
            Mapper.CreateMap<Site, SiteViewModel>();
            Mapper.CreateMap<MvcController, MvcControllerViewModel>();
            Mapper.CreateMap<MvcControllerViewModel, MvcController>();
            Mapper.CreateMap<DocumentType, DocumentTypeViewModel>();
            Mapper.CreateMap<RolesDivisionViewModel, RolesDivision>();
            Mapper.CreateMap<RolesDivision, RolesDivisionViewModel>();
            Mapper.CreateMap<RolesSiteViewModel, RolesSite>();
            Mapper.CreateMap<RolesSite, RolesSiteViewModel>();



            //Mapper.Initialize(cfg =>
            //{
            //    cfg.CreateMap<Site, SiteViewModel>();
            //    cfg.CreateMap<MvcController, MvcControllerViewModel>();
            //    cfg.CreateMap<MvcControllerViewModel, MvcController>();
            //    cfg.CreateMap<DocumentType, DocumentTypeViewModel>();
            //    cfg.CreateMap<RolesDivisionViewModel, RolesDivision>();
            //    cfg.CreateMap<RolesDivision, RolesDivisionViewModel>();
            //    cfg.CreateMap<RolesSiteViewModel, RolesSite>();
            //    cfg.CreateMap<RolesSite, RolesSiteViewModel>();
            //});

            //////////////////////////////////////////End Module ViewModel Mapping///////////////////////////////////////////////////




            //Registering Mappers:
            Mapper.CreateMap<BusinessEntity, AgentViewModel>();
            Mapper.CreateMap<AgentViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, EmployeeViewModel>();
            Mapper.CreateMap<EmployeeViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, SupplierViewModel>();
            Mapper.CreateMap<SupplierViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, JobWorkerViewModel>();
            Mapper.CreateMap<JobWorkerViewModel, BusinessEntity>();

            Mapper.CreateMap<Person, JobWorkerViewModel>();
            Mapper.CreateMap<JobWorkerViewModel, Person >();

            Mapper.CreateMap<JobWorker, JobWorkerViewModel>();
            Mapper.CreateMap<JobWorkerViewModel, JobWorker>();


            Mapper.CreateMap<PersonAddress, JobWorkerViewModel>();
            Mapper.CreateMap<JobWorkerViewModel, PersonAddress>();


            Mapper.CreateMap<LedgerAccount, JobWorkerViewModel>();
            Mapper.CreateMap<JobWorkerViewModel, LedgerAccount>();

            Mapper.CreateMap<BusinessEntity, ManufacturerViewModel>();
            Mapper.CreateMap<ManufacturerViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, CourierViewModel>();
            Mapper.CreateMap<CourierViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, TransporterViewModel>();
            Mapper.CreateMap<TransporterViewModel, BusinessEntity>();

            Mapper.CreateMap<BusinessEntity, BuyerViewModel>();
            Mapper.CreateMap<BuyerViewModel, BusinessEntity>();

            Mapper.CreateMap<JobOrderHeader, JobOrderHeaderViewModel>();
            Mapper.CreateMap<JobOrderHeaderViewModel, JobOrderHeader>();

            Mapper.CreateMap<JobOrderLine, JobOrderLineViewModel>();
            Mapper.CreateMap<JobOrderLineViewModel, JobOrderLine>();

            Mapper.CreateMap<JobOrderSettings, JobOrderSettingsViewModel>();
            Mapper.CreateMap<JobOrderSettingsViewModel, JobOrderSettings>();

            Mapper.CreateMap<JobConsumptionSettings, JobConsumptionSettingsViewModel>();
            Mapper.CreateMap<JobConsumptionSettingsViewModel, JobConsumptionSettings>();

            Mapper.CreateMap<JobReceiveSettings, JobReceiveSettingsViewModel>();
            Mapper.CreateMap<JobReceiveSettingsViewModel, JobReceiveSettings>();

            Mapper.CreateMap<JobOrderPerk, PerkViewModel>();
            Mapper.CreateMap<PerkViewModel, JobOrderPerk>().ForMember(m => m.CreatedDate, x => x.Ignore()).ForMember(m => m.CreatedBy, x => x.Ignore())
                .ForMember(m => m.ModifiedDate, x => x.Ignore())
                .ForMember(m => m.ModifiedBy, x => x.Ignore());

            Mapper.CreateMap<Perk, PerkViewModel>();
            Mapper.CreateMap<PerkViewModel, Perk>();

            Mapper.CreateMap<LineChargeViewModel, CalculationProduct>().ForMember(m => m.CalculationProductId , x => x.Ignore());
            Mapper.CreateMap<CalculationProduct, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<LineChargeViewModel, JobOrderLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<JobOrderLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, JobOrderHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<JobOrderHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());           

            Mapper.CreateMap<LineChargeViewModel, JobInvoiceLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<JobInvoiceLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());
            
            Mapper.CreateMap<HeaderChargeViewModel, JobInvoiceHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<JobInvoiceHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<LineChargeViewModel, JobInvoiceReturnLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<JobInvoiceReturnLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, JobInvoiceReturnHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<JobInvoiceReturnHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<HeaderChargeViewModel, JobInvoiceAmendmentHeaderCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<JobInvoiceAmendmentHeaderCharge, HeaderChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());

            Mapper.CreateMap<LineChargeViewModel, JobInvoiceRateAmendmentLineCharge>().ForMember(m => m.Id, x => x.Ignore());
            Mapper.CreateMap<JobInvoiceRateAmendmentLineCharge, LineChargeViewModel>().ForMember(m => m.Id, x => x.Ignore());



            Mapper.CreateMap<JobReceiveHeader, JobReceiveHeaderViewModel>();
            Mapper.CreateMap<JobReceiveHeaderViewModel, JobReceiveHeader>();

            Mapper.CreateMap<JobReceiveLine, JobReceiveLineViewModel>();
            Mapper.CreateMap<JobReceiveLineViewModel, JobReceiveLine>();

            Mapper.CreateMap<JobReturnHeader, JobReturnHeaderViewModel>();
            Mapper.CreateMap<JobReturnHeaderViewModel, JobReturnHeader>();

            Mapper.CreateMap<JobReturnLine, JobReturnLineViewModel>();
            Mapper.CreateMap<JobReturnLineViewModel, JobReturnLine>();


            Mapper.CreateMap<JobInvoiceHeader, JobInvoiceHeaderViewModel>();
            Mapper.CreateMap<JobInvoiceHeaderViewModel, JobInvoiceHeader>();

            Mapper.CreateMap<JobInvoiceLine, JobInvoiceLineViewModel>();
            Mapper.CreateMap<JobInvoiceLineViewModel, JobInvoiceLine>();

            Mapper.CreateMap<JobOrderCancelHeader, JobOrderCancelHeaderViewModel>();
            Mapper.CreateMap<JobOrderCancelHeaderViewModel, JobOrderCancelHeader>();

            Mapper.CreateMap<JobOrderCancelLine, JobOrderCancelLineViewModel>();
            Mapper.CreateMap<JobOrderCancelLineViewModel, JobOrderCancelLine>();

            Mapper.CreateMap<JobOrderAmendmentHeader, JobOrderAmendmentHeaderViewModel>();
            Mapper.CreateMap<JobOrderAmendmentHeaderViewModel, JobOrderAmendmentHeader>();

            Mapper.CreateMap<JobOrderRateAmendmentLine, JobOrderRateAmendmentLineViewModel>();
            Mapper.CreateMap<JobOrderRateAmendmentLineViewModel, JobOrderRateAmendmentLine>();

            Mapper.CreateMap<JobInvoiceAmendmentHeader, JobInvoiceAmendmentHeaderViewModel>();
            Mapper.CreateMap<JobInvoiceAmendmentHeaderViewModel, JobInvoiceAmendmentHeader>();

            Mapper.CreateMap<JobInvoiceRateAmendmentLine, JobInvoiceRateAmendmentLineViewModel>();
            Mapper.CreateMap<JobInvoiceRateAmendmentLineViewModel, JobInvoiceRateAmendmentLine>();

            Mapper.CreateMap<JobReceiveByProduct, JobReceiveByProductViewModel>();
            Mapper.CreateMap<JobReceiveByProductViewModel, JobReceiveByProduct>();

            Mapper.CreateMap<JobReceiveBom, JobReceiveBomViewModel>();
            Mapper.CreateMap<JobReceiveBomViewModel, JobReceiveBom>();

            Mapper.CreateMap<JobInvoiceSettings, JobInvoiceSettingsViewModel>();
            Mapper.CreateMap<JobInvoiceSettingsViewModel, JobInvoiceSettings>();

            Mapper.CreateMap<CalculationFooterViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<CalculationProductViewModel, LineChargeViewModel>();

            Mapper.CreateMap<RateConversionSettings, RateConversionSettingsViewModel>();
            Mapper.CreateMap<RateConversionSettingsViewModel, RateConversionSettings>();

            Mapper.CreateMap<StockHeader, StockHeaderViewModel>();
            Mapper.CreateMap<StockHeaderViewModel, StockHeader>();

            Mapper.CreateMap<StockLineViewModel, StockLine>();
            Mapper.CreateMap<StockLine, StockLineViewModel>();

            Mapper.CreateMap<PersonContact, PersonContactViewModel>();
            Mapper.CreateMap<PersonContactViewModel, PersonContact>();

            Mapper.CreateMap<JobInvoiceHeaderViewModel, JobReceiveHeader>();
            Mapper.CreateMap<JobReceiveHeader, JobInvoiceHeaderViewModel>();

            Mapper.CreateMap<JobInvoiceLineViewModel, JobReceiveLine>();
            Mapper.CreateMap<JobReceiveLine, JobInvoiceLineViewModel>();

            Mapper.CreateMap<RateListViewModel, RateList>();
            Mapper.CreateMap<RateList, RateListViewModel>();

            Mapper.CreateMap<RateList, RateListHistory>();
            Mapper.CreateMap<RateListHistory, RateList>();

            Mapper.CreateMap<HeaderChargeViewModel, HeaderChargeViewModel>();
            Mapper.CreateMap<LineChargeViewModel, LineChargeViewModel>();

            Mapper.CreateMap<JobInvoiceReturnHeader, JobInvoiceReturnHeaderViewModel>();
            Mapper.CreateMap<JobInvoiceReturnHeaderViewModel, JobInvoiceReturnHeader>();

            Mapper.CreateMap<JobInvoiceReturnHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobInvoiceReturnHeader, DocumentUniqueId>();


            Mapper.CreateMap<JobOrderSettings, JobOrderSettings>();
            Mapper.CreateMap<JobInvoiceSettings, JobInvoiceSettings>();
            Mapper.CreateMap<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettings>();
            Mapper.CreateMap<JobOrderInspectionSettings, JobOrderInspectionSettings>();
            Mapper.CreateMap<JobReceiveQASettings, JobReceiveQASettings>();
            Mapper.CreateMap<JobReceiveSettings, JobReceiveSettings>();
            Mapper.CreateMap<JobOrderLine, JobOrderLine>();
            Mapper.CreateMap<JobReceiveLine, JobReceiveLine>();
            Mapper.CreateMap<JobOrderHeaderCharge, JobOrderHeaderCharge>();
            Mapper.CreateMap<JobOrderLineCharge, JobOrderLineCharge>();
            Mapper.CreateMap<JobOrderCancelLine, JobOrderCancelLine>();
            Mapper.CreateMap<JobInvoiceLine, JobInvoiceLine>();
            Mapper.CreateMap<JobInvoiceLineCharge, JobInvoiceLineCharge>();
            Mapper.CreateMap<JobInvoiceHeaderCharge, JobInvoiceHeaderCharge>();
            Mapper.CreateMap<JobReturnLine, JobReturnLine>();
            Mapper.CreateMap<PersonRegistration, PersonRegistration>();
            Mapper.CreateMap<LedgerAccount, LedgerAccount>();
            Mapper.CreateMap<PersonAddress, PersonAddress>();
            Mapper.CreateMap<Person, Person>();
            Mapper.CreateMap<BusinessEntity, BusinessEntity>();
            Mapper.CreateMap<JobWorker, JobWorker>();
            Mapper.CreateMap<StockLine, StockLine>();
            Mapper.CreateMap<JobOrderRateAmendmentLine, JobOrderRateAmendmentLine>();
            Mapper.CreateMap<JobInvoiceRateAmendmentLine, JobInvoiceRateAmendmentLine>();
            Mapper.CreateMap<StockHeader, StockHeader>();
            Mapper.CreateMap<JobInvoiceHeader, JobInvoiceHeader>();
            Mapper.CreateMap<JobReceiveHeader, JobReceiveHeader>();
            Mapper.CreateMap<JobInvoiceLineIndexViewModel,JobInvoiceLine>();
            Mapper.CreateMap<JobReceiveLineViewModel, JobReceiveLine>();
            Mapper.CreateMap<JobOrderCancelHeader, JobOrderCancelHeader>();
            Mapper.CreateMap<JobOrderCancelLineViewModel, JobOrderCancelLine>();
            Mapper.CreateMap<JobOrderAmendmentHeader, JobOrderAmendmentHeader>();
            Mapper.CreateMap<JobOrderRateAmendmentLineViewModel, JobOrderRateAmendmentLine>();
            Mapper.CreateMap<JobInvoiceAmendmentHeader, JobInvoiceAmendmentHeader>();
            Mapper.CreateMap<JobInvoiceRateAmendmentLineViewModel, JobInvoiceRateAmendmentLine>();
            Mapper.CreateMap<JobReceiveLineViewModel, JobReceiveLine>();
            Mapper.CreateMap<JobReturnLineIndexViewModel, JobReturnLine>();
            Mapper.CreateMap<JobOrderHeader, JobOrderHeader>();
            Mapper.CreateMap<JobOrderLineViewModel, JobOrderLine>();
            Mapper.CreateMap<JobOrderHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobOrderHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobInvoiceHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobInvoiceHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobInvoiceAmendmentHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobInvoiceAmendmentHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobOrderAmendmentHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobOrderAmendmentHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobOrderCancelHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobOrderCancelHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobOrderInspectionHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobOrderInspectionHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobReceiveQAHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobReceiveQAHeader, DocumentUniqueId>();
            Mapper.CreateMap<JobReceiveQAHeader, JobReceiveQAAttributeViewModel>();
            Mapper.CreateMap<JobReceiveQAAttributeViewModel, JobReceiveQAHeader>();

            Mapper.CreateMap<JobReceiveQAPenalty, JobReceiveQAPenaltyViewModel>();
            Mapper.CreateMap<JobReceiveQAPenaltyViewModel, JobReceiveQAPenalty>();


            Mapper.CreateMap<JobOrderInspectionRequestCancelHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobOrderInspectionRequestCancelHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobOrderInspectionRequestHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobOrderInspectionRequestHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobReceiveHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobReceiveHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobReturnHeaderViewModel, DocumentUniqueId>();
            Mapper.CreateMap<JobReturnHeader, DocumentUniqueId>();

            Mapper.CreateMap<JobOrderInspectionRequestHeader, JobOrderInspectionRequestHeaderViewModel>();
            Mapper.CreateMap<JobOrderInspectionRequestHeaderViewModel, JobOrderInspectionRequestHeader>();

            Mapper.CreateMap<JobOrderInspectionRequestLine, JobOrderInspectionRequestLineViewModel>();
            Mapper.CreateMap<JobOrderInspectionRequestLineViewModel, JobOrderInspectionRequestLine>();

            Mapper.CreateMap<JobOrderInspectionRequestSettings, JobOrderInspectionRequestSettingsViewModel>();
            Mapper.CreateMap<JobOrderInspectionRequestSettingsViewModel, JobOrderInspectionRequestSettings>();

            Mapper.CreateMap<JobOrderInspectionSettings, JobOrderInspectionSettingsViewModel>();
            Mapper.CreateMap<JobOrderInspectionSettingsViewModel, JobOrderInspectionSettings>();

            Mapper.CreateMap<JobOrderInspectionRequestHeader, JobOrderInspectionRequestHeader>();
            Mapper.CreateMap<JobOrderInspectionRequestLine, JobOrderInspectionRequestLine>();

            Mapper.CreateMap<JobOrderInspectionRequestCancelHeader, JobOrderInspectionRequestCancelHeaderViewModel>();
            Mapper.CreateMap<JobOrderInspectionRequestCancelHeaderViewModel, JobOrderInspectionRequestCancelHeader>();

            Mapper.CreateMap<JobOrderInspectionRequestCancelLine, JobOrderInspectionRequestCancelLineViewModel>();
            Mapper.CreateMap<JobOrderInspectionRequestCancelLineViewModel, JobOrderInspectionRequestCancelLine>();

            Mapper.CreateMap<JobOrderInspectionRequestCancelHeader, JobOrderInspectionRequestCancelHeader>();
            Mapper.CreateMap<JobOrderInspectionRequestCancelLine, JobOrderInspectionRequestCancelLine>();

            Mapper.CreateMap<JobOrderInspectionHeader, JobOrderInspectionHeaderViewModel>();
            Mapper.CreateMap<JobOrderInspectionHeaderViewModel, JobOrderInspectionHeader>();

            Mapper.CreateMap<JobOrderInspectionLine, JobOrderInspectionLineViewModel>();
            Mapper.CreateMap<JobOrderInspectionLineViewModel, JobOrderInspectionLine>();

            Mapper.CreateMap<JobOrderInspectionHeader, JobOrderInspectionHeader>();
            Mapper.CreateMap<JobOrderInspectionLine, JobOrderInspectionLine>();

            Mapper.CreateMap<JobReceiveQAHeader, JobReceiveQAHeaderViewModel>();
            Mapper.CreateMap<JobReceiveQAHeaderViewModel, JobReceiveQAHeader>();

            Mapper.CreateMap<JobReceiveQALine, JobReceiveQALineViewModel>();
            Mapper.CreateMap<JobReceiveQALineViewModel, JobReceiveQALine>();
            Mapper.CreateMap<JobReceiveQALine, JobReceiveQAAttributeViewModel>();
            Mapper.CreateMap<JobReceiveQAAttributeViewModel, JobReceiveQALine>();

            Mapper.CreateMap<JobReceiveQAHeader, JobReceiveQAHeader>();
            Mapper.CreateMap<JobReceiveQALine, JobReceiveQALine>();

            Mapper.CreateMap<JobReceiveQASettings, JobReceiveQASettingsViewModel>();
            Mapper.CreateMap<JobReceiveQASettingsViewModel, JobReceiveQASettings>();

            Mapper.CreateMap<JobInvoiceReturnHeader, JobInvoiceReturnHeader>();
            Mapper.CreateMap<JobInvoiceReturnLine, JobInvoiceReturnLine>();

            Mapper.CreateMap<JobInvoiceReturnHeaderViewModel, JobReturnHeader>();

            Mapper.CreateMap<JobInvoiceReturnLineViewModel, JobInvoiceReturnLine>();
            Mapper.CreateMap<JobInvoiceReturnLine, JobInvoiceReturnLineViewModel>();

            Mapper.CreateMap<JobInvoiceReturnLineViewModel, JobReturnLine>();
            Mapper.CreateMap<JobReturnLine, JobInvoiceReturnLineViewModel>();

            Mapper.CreateMap<JobInvoiceReturnLine, JobReturnLine>();

            Mapper.CreateMap<JobReceiveBom, JobReceiveBomViewModel>();
            Mapper.CreateMap<JobReceiveBomViewModel, JobReceiveBom>();
            
        }
    }
}
