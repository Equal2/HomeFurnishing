using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;

using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModel;

namespace Service
{
    public interface ICalculationProductService : IDisposable
    {
        CalculationProduct Create(CalculationProduct pt);
        void Delete(int id);
        void Delete(CalculationProduct pt);
        CalculationProduct Find(int id);
        IEnumerable<CalculationProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(CalculationProduct pt);
        CalculationProduct Add(CalculationProduct pt);
        IEnumerable<CalculationProduct> GetCalculationProductList();
        IEnumerable<CalculationProductViewModel> GetCalculationProductList(int CalculationID, int DocumentTypeId, int SiteId, int DivisionId);
        Task<IEquatable<CalculationProduct>> GetAsync();
        Task<CalculationProduct> FindAsync(int id);
        IEnumerable<CalculationProductViewModel> GetCalculationListForIndex(int id);//CalculationId
        CalculationProductViewModel GetCalculationProduct(int id);//CalculationProductId
        IEnumerable<CalculationProductViewModel> GetCalculationProductListForDropDown();
        int NextId(int id);
        int PrevId(int id);
    }

    public class CalculationProductService : ICalculationProductService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<CalculationProduct> _CalculationProductRepository;
        RepositoryQuery<CalculationProduct> CalculationProductRepository;

        public CalculationProductService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CalculationProductRepository = new Repository<CalculationProduct>(db);
            CalculationProductRepository = new RepositoryQuery<CalculationProduct>(_CalculationProductRepository);
        }


        public CalculationProduct Find(int id)
        {
            return _unitOfWork.Repository<CalculationProduct>().Find(id);
        }
        public CalculationProductViewModel GetCalculationProduct(int id)
        {
            return (from p in db.CalculationProduct
                    where p.CalculationProductId == id
                    select new CalculationProductViewModel
                    {
                       AddDeduct=p.AddDeduct,
                       AffectCost=p.AffectCost,
                       CalculateOnId=p.CalculateOnId,
                       CalculationId=p.CalculationId,
                       Id=p.CalculationProductId,
                       ChargeId=p.ChargeId,
                       ChargeTypeId=p.ChargeTypeId,
                       CostCenterId=p.CostCenterId,
                       IncludedInBase=p.IncludedInBase,
                       IsActive=p.IsActive,
                       Amount=p.Amount,
                       RateType=p.RateType,
                       IsVisible=p.IsVisible,
                       ParentChargeId=p.ParentChargeId,
                       Rate=p.Rate,
                       Sr=p.Sr,
                       IncludedCharges=p.IncludedCharges,
                       IncludedChargesCalculation=p.IncludedChargesCalculation,
                    }

                        ).FirstOrDefault();


        }
        public IEnumerable<CalculationProductViewModel> GetCalculationProductListForDropDown()
        {
            return (from p in db.CalculationProduct
                    join t in db.Charge on p.ChargeId equals t.ChargeId
                    select new CalculationProductViewModel
                    {
                        Id = p.CalculationProductId,
                        CalculationProductName = t.ChargeName,
                    }
                        );
        }
        public IEnumerable<CalculationProductViewModel> GetCalculationListForIndex(int id)
        {
            var Query = (from p in db.CalculationProduct
                        where p.CalculationId == id
                        orderby p.Sr
                        select new CalculationProductViewModel
                        {
                            AddDeduct = p.AddDeduct,
                            //AddDeductName=(p.AddDeduct==null?"":(p.AddDeduct==true?"Add":"Deduction")),
                            //AddDeductName = p.AddDeduct,
                            AffectCost = p.AffectCost,
                            AffectCostName = (p.AffectCost == true ? "Yes" : "No"),
                            CalculateOnId = p.CalculateOnId,
                            CalculateOnName = p.CalculateOn.ChargeName,
                            Id = p.CalculationProductId,
                            ChargeName = p.Charge.ChargeName,
                            ChargeTypeName = p.ChargeType.ChargeTypeName,
                            CostCenterName = p.CostCenter.CostCenterName,
                            IncludedInBase = p.IncludedInBase,
                            IncludedInBaseName = (p.IncludedInBase == true ? "Yes" : "No"),
                            IsActive = p.IsActive,
                            Rate = p.Rate,
                            ParentChargeId = p.ParentChargeId,

                        }).ToList();

            var Result = Query.Select(p => new CalculationProductViewModel
            {
                AddDeduct = p.AddDeduct,               
                AddDeductName = (p.AddDeduct.HasValue ? Enum.GetName(typeof(AddDeductEnum), p.AddDeduct) : ""),
                AffectCost = p.AffectCost,
                AffectCostName = (p.AffectCost == true ? "Yes" : "No"),
                CalculateOnId = p.CalculateOnId,
                CalculateOnName = p.CalculateOnName,
                Id = p.Id,
                ChargeName = p.ChargeName,
                ChargeTypeName = p.ChargeTypeName,
                CostCenterName = p.CostCenterName,
                IncludedInBase = p.IncludedInBase,
                IncludedInBaseName = (p.IncludedInBaseName),
                IsActive = p.IsActive,
                Rate = p.Rate,
                ParentChargeId = p.ParentChargeId,
            }).AsEnumerable();

            return (Result);
        }

        public CalculationProduct Create(CalculationProduct pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<CalculationProduct>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<CalculationProduct>().Delete(id);
        }

        public void Delete(CalculationProduct pt)
        {
            _unitOfWork.Repository<CalculationProduct>().Delete(pt);
        }

        public void Update(CalculationProduct pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<CalculationProduct>().Update(pt);
        }

        public IEnumerable<CalculationProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<CalculationProduct>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CalculationProductId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<CalculationProduct> GetCalculationProductList()
        {
            var pt = _unitOfWork.Repository<CalculationProduct>().Query().Get().OrderBy(m=>m.CalculationProductId);
            return pt;
        }
        public IEnumerable<CalculationProductViewModel> GetCalculationProductList(int CalculationID, int DocumentTypeId, int SiteId, int DivisionId)
        {          

            return (from Cp in db.CalculationProduct                    
                    join Clla in db.CalculationLineLedgerAccount.Where(m=>m.DocTypeId==DocumentTypeId && m.SiteId == SiteId && m.DivisionId== DivisionId) on Cp.CalculationProductId equals Clla.CalculationProductId into CalculationLineLedgerAccountTable from CalculationLineLedgerAccountTab in CalculationLineLedgerAccountTable.DefaultIfEmpty()
                    where Cp.CalculationId == CalculationID
                    orderby Cp.Sr
                    select new CalculationProductViewModel
                    {
                        AddDeduct = Cp.AddDeduct,
                        AffectCost = Cp.AffectCost,
                        CalculateOnId = Cp.CalculateOnId,
                        CalculateOnName = Cp.CalculateOn.ChargeName,
                        CalculateOnCode = Cp.CalculateOn.ChargeCode,
                        CalculationId = Cp.CalculationId,
                        CalculationName = Cp.Calculation.CalculationName,
                        ChargeId = Cp.ChargeId,
                        ChargeName = Cp.Charge.ChargeName,
                        ChargeCode = Cp.Charge.ChargeCode,
                        ChargeTypeId = Cp.ChargeTypeId,
                        ChargeTypeName = Cp.ChargeType.ChargeTypeName,
                        CostCenterId = Cp.CostCenterId,
                        CostCenterName = Cp.CostCenter.CostCenterName,
                        IncludedInBase = Cp.IncludedInBase,
                        LedgerAccountCrId = CalculationLineLedgerAccountTab.LedgerAccountCrId,
                        LedgerAccountCrName  =  CalculationLineLedgerAccountTab.LedgerAccountCr.LedgerAccountName,
                        LedgerAccountDrId  =  CalculationLineLedgerAccountTab.LedgerAccountDrId,
                        LedgerAccountDrName  =  CalculationLineLedgerAccountTab.LedgerAccountDr.LedgerAccountName,
                        ContraLedgerAccountId = CalculationLineLedgerAccountTab.ContraLedgerAccountId,
                        ContraLedgerAccountName = CalculationLineLedgerAccountTab.ContraLedgerAccount.LedgerAccountName,
                        Rate = Cp.Rate,
                        Sr = Cp.Sr,
                        RateType = Cp.RateType,
                        IsVisible = Cp.IsVisible,
                        Amount = Cp.Amount,
                        ParentChargeId = Cp.ParentChargeId,
                        ElementId = "CALL_" + Cp.Charge.ChargeCode,
                        IncludedCharges = Cp.IncludedCharges,
                        IncludedChargesCalculation = Cp.IncludedChargesCalculation,
                        IsVisibleLedgerAccountCr = CalculationLineLedgerAccountTab.IsVisibleLedgerAccountCr,
                        IsVisibleLedgerAccountDr = CalculationLineLedgerAccountTab.IsVisibleLedgerAccountDr,
                        filterLedgerAccountGroupsCrId = CalculationLineLedgerAccountTab.filterLedgerAccountGroupsCrId,
                        filterLedgerAccountGroupsDrId = CalculationLineLedgerAccountTab.filterLedgerAccountGroupsDrId
                    });
        }

        public IEnumerable<CalculationProductViewModel> GetCalculationProductListWithChargeGroupSettings(int CalculationID, int DocumentTypeId, int SiteId, int DivisionId, int? ChargeGroupPersonId, int? ChargeGroupProductId)
        {
            var ChargeGroupSettings = from C in db.ChargeGroupSettings
                                      where C.ChargeGroupPersonId == ChargeGroupPersonId && C.ChargeGroupProductId == ChargeGroupProductId
                                      select C;

            int ChargeLedgerAccountId = new LedgerAccountService(_unitOfWork).Find(LedgerAccountConstants.Charge).LedgerAccountId;

            return (from p in db.CalculationProduct
                    join t in db.CalculationLineLedgerAccount.Where(m => m.DocTypeId == DocumentTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId) on p.CalculationProductId equals t.CalculationProductId into table1
                    join Cgs in ChargeGroupSettings on p.ChargeTypeId equals Cgs.ChargeTypeId into ChargeGroupSettingsTable
                    from ChargeGroupSettingsTab in ChargeGroupSettingsTable.DefaultIfEmpty()
                    from tab1 in table1.DefaultIfEmpty()
                    where p.CalculationId == CalculationID
                    orderby p.Sr
                    select new CalculationProductViewModel
                    {
                        AddDeduct = p.AddDeduct,
                        AffectCost = p.AffectCost,
                        CalculateOnId = p.CalculateOnId,
                        CalculateOnName = p.CalculateOn.ChargeName,
                        CalculateOnCode = p.CalculateOn.ChargeCode,
                        CalculationId = p.CalculationId,
                        CalculationName = p.Calculation.CalculationName,
                        ChargeId = p.ChargeId,
                        ChargeName = p.Charge.ChargeName,
                        ChargeCode = p.Charge.ChargeCode,
                        ChargeTypeId = p.ChargeTypeId,
                        ChargeTypeName = p.ChargeType.ChargeTypeName,
                        CostCenterId = p.CostCenterId,
                        CostCenterName = p.CostCenter.CostCenterName,
                        IncludedInBase = p.IncludedInBase,
                        LedgerAccountCrId = (tab1.LedgerAccountCrId == ChargeLedgerAccountId ? ChargeGroupSettingsTab.ChargeLedgerAccountId : tab1.LedgerAccountCrId),
                        LedgerAccountCrName = tab1.LedgerAccountCr.LedgerAccountName,
                        LedgerAccountDrId = (tab1.LedgerAccountDrId == ChargeLedgerAccountId ? ChargeGroupSettingsTab.ChargeLedgerAccountId : tab1.LedgerAccountDrId),
                        LedgerAccountDrName = tab1.LedgerAccountDr.LedgerAccountName,
                        ContraLedgerAccountId = tab1.ContraLedgerAccountId,
                        ContraLedgerAccountName = tab1.ContraLedgerAccount.LedgerAccountName,
                        Rate = ChargeGroupSettingsTab.ChargePer,
                        Sr = p.Sr,
                        RateType = p.RateType,
                        IsVisible = p.IsVisible,
                        Amount = p.Amount,
                        ParentChargeId = p.ParentChargeId,
                        ElementId = "CALL_" + p.Charge.ChargeCode,
                        IncludedCharges = p.IncludedCharges,
                        IncludedChargesCalculation = p.IncludedChargesCalculation,
                    });

        }

        public IEnumerable<CalculationProductViewModel> GetChargeRates(int CalculationID, int DocumentTypeId, int SiteId, int DivisionId, int ProcessId, int? ChargeGroupPersonId, int? ChargeGroupProductId, int? ProductId = null)
        {
            var ChargeGroupSettings = from C in db.ChargeGroupSettings
                                      where C.ChargeGroupPersonId == ChargeGroupPersonId && C.ChargeGroupProductId == ChargeGroupProductId && C.ProcessId == ProcessId
                                      select C;

            var PurchaseProcess = new ProcessService(_unitOfWork).Find(ProcessConstants.Purchase);

            if (ChargeGroupSettings.ToList().Count() == 0 && PurchaseProcess != null)
            {
                ChargeGroupSettings = from C in db.ChargeGroupSettings
                                      where C.ChargeGroupPersonId == ChargeGroupPersonId && C.ChargeGroupProductId == ChargeGroupProductId && C.ProcessId == PurchaseProcess.ProcessId
                                          select C;
            }


            int ChargeLedgerAccountId = new LedgerAccountService(_unitOfWork).Find(LedgerAccountConstants.Charge).LedgerAccountId;

            int? ProductLedgerAccountId = null;
            int? ChargeTypeId_SalesTaxTaxableAmount = null;
            if (ProductId != null)
            {
                var ProductLedgerAccount = (from L in db.LedgerAccount where L.ProductId == ProductId select L).FirstOrDefault();
                if (ProductLedgerAccount != null)
                    ProductLedgerAccountId = ProductLedgerAccount.LedgerAccountId;

                var ChargeType_SalesTaxTaxableAmount = (from Ct in db.ChargeType where Ct.ChargeTypeName == ChargeTypeConstants.SalesTaxableAmount select Ct).FirstOrDefault();
                if (ChargeType_SalesTaxTaxableAmount != null)
                    ChargeTypeId_SalesTaxTaxableAmount = ChargeType_SalesTaxTaxableAmount.ChargeTypeId;
            }

            return (from p in db.CalculationProduct
                    join t in db.CalculationLineLedgerAccount.Where(m => m.DocTypeId == DocumentTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId) on p.CalculationProductId equals t.CalculationProductId into CalculationLineLedgerAccountTable
                    from CalculationLineLedgerAccountTab in CalculationLineLedgerAccountTable.DefaultIfEmpty()
                    join Cgs in ChargeGroupSettings on p.ChargeTypeId equals Cgs.ChargeTypeId into ChargeGroupSettingsTable
                    from ChargeGroupSettingsTab in ChargeGroupSettingsTable.DefaultIfEmpty()
                    where p.CalculationId == CalculationID 
                    orderby p.Sr
                    select new CalculationProductViewModel
                    {
                        ChargeId = p.ChargeId,
                        LedgerAccountCrId = (CalculationLineLedgerAccountTab.LedgerAccountCrId == ChargeLedgerAccountId 
                                ? (p.ChargeTypeId == ChargeTypeId_SalesTaxTaxableAmount 
                                        ? (ProductLedgerAccountId ?? ChargeGroupSettingsTab.ChargeLedgerAccountId) 
                                        : ChargeGroupSettingsTab.ChargeLedgerAccountId)
                                : CalculationLineLedgerAccountTab.LedgerAccountCrId),
                        LedgerAccountDrId = (CalculationLineLedgerAccountTab.LedgerAccountDrId == ChargeLedgerAccountId 
                                ? (p.ChargeTypeId == ChargeTypeId_SalesTaxTaxableAmount 
                                        ? (ProductLedgerAccountId ?? ChargeGroupSettingsTab.ChargeLedgerAccountId) 
                                        : ChargeGroupSettingsTab.ChargeLedgerAccountId)
                                : CalculationLineLedgerAccountTab.LedgerAccountDrId),
                        Rate = (Decimal?)ChargeGroupSettingsTab.ChargePer ?? 0,
                        ChargeTypeId = ChargeGroupSettingsTab.ChargeTypeId
                    });

        }


        public IEnumerable<ChargeRateSettings> GetChargeRateSettingForValidation(int CalculationId, int DocTypeId, int SiteId, int DivisionId, int ProcessId, int ChargeGroupPersonId, int ChargeGroupProductId)
        {
            IEnumerable<ChargeRateSettings> ChargeRateSettingsList = (from p in db.CalculationProduct
                                                                      join t in db.CalculationLineLedgerAccount.Where(m => m.DocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId) on p.CalculationProductId equals t.CalculationProductId into table1
                                                                      from tab1 in table1.DefaultIfEmpty()
                                                                      join Cgs in db.ChargeGroupSettings.Where(m => m.ChargeGroupPersonId == ChargeGroupPersonId
                                                                             && m.ChargeGroupProductId == ChargeGroupProductId
                                                                             && m.ProcessId == ProcessId) on p.ChargeTypeId equals Cgs.ChargeTypeId into ChargeGroupSettingsTable
                                                                      from ChargeGroupSettingsTab in ChargeGroupSettingsTable.DefaultIfEmpty()
                                                                      where p.CalculationId == CalculationId && p.ChargeType.Category == ChargeTypeCategoryConstants.SalesTax
                                                                      select new ChargeRateSettings
                                                                      {
                                                                          ChargeId = p.ChargeId,
                                                                          ChargeName = p.Charge.ChargeName,
                                                                          LedgerAccountCrName = p.LedgerAccountCr.LedgerAccountName,
                                                                          LedgerAccountDrName = p.LedgerAccountDr.LedgerAccountName,
                                                                          ChargeGroupSettingId = ChargeGroupSettingsTab.ChargeGroupSettingsId,
                                                                          ChargePer = ChargeGroupSettingsTab.ChargePer,
                                                                          ChargeLedgerAccountId = ChargeGroupSettingsTab.ChargeLedgerAccountId,
                                                                      }).ToList();
            return ChargeRateSettingsList;
        }


        public CalculationProduct Add(CalculationProduct pt)
        {
            _unitOfWork.Repository<CalculationProduct>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.CalculationProduct
                        orderby p.CalculationProductId
                        select p.CalculationProductId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.CalculationProduct
                        orderby p.CalculationProductId
                        select p.CalculationProductId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.CalculationProduct
                        orderby p.CalculationProductId
                        select p.CalculationProductId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.CalculationProduct
                        orderby p.CalculationProductId
                        select p.CalculationProductId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<CalculationProduct>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CalculationProduct> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        //public void SaveProductCharge(CalculationLineCharge x) 
        //{            
        //    x.Id = 1;
        //    UnitOfWo




        //}

    }

    public class ChargeRateSettings
    {
        public int ChargeId { get; set; }
        public string ChargeName { get; set; }
        public string LedgerAccountDrName { get; set; }
        public string LedgerAccountCrName { get; set; }
        public int? ChargeGroupSettingId { get; set; }
        public Decimal? ChargePer { get; set; }
        public int? ChargeLedgerAccountId { get; set; }
    }
}
