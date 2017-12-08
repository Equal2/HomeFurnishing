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
    public interface ICalculationFooterService : IDisposable
    {
        CalculationFooter Create(CalculationFooter pt);
        void Delete(int id);
        void Delete(CalculationFooter pt);
        CalculationFooter Find(int id);
        IEnumerable<CalculationFooter> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(CalculationFooter pt);
        CalculationFooter Add(CalculationFooter pt);
        IEnumerable<CalculationFooter> GetCalculationFooterList();
        IEnumerable<CalculationFooterViewModel> GetCalculationFooterList(int CalculationID, int DocumentTypeId, int SiteId, int DivisionId);
        Task<IEquatable<CalculationFooter>> GetAsync();
        Task<CalculationFooter> FindAsync(int id);
        IEnumerable<CalculationFooterViewModel> GetCalculationListForIndex(int id);//CalculationId
        CalculationFooterViewModel GetCalculationFooter(int id);//CalculationFooterId
        IEnumerable<CalculationFooterViewModel> GetCalculationFooterListForDropDown();
        int NextId(int id);
        int PrevId(int id);
    }

    public class CalculationFooterService : ICalculationFooterService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<CalculationFooter> _CalculationFooterRepository;
        RepositoryQuery<CalculationFooter> CalculationFooterRepository;

        public CalculationFooterService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CalculationFooterRepository = new Repository<CalculationFooter>(db);
            CalculationFooterRepository = new RepositoryQuery<CalculationFooter>(_CalculationFooterRepository);
        }


        public CalculationFooter Find(int id)
        {
            return _unitOfWork.Repository<CalculationFooter>().Find(id);
        }
        public CalculationFooterViewModel GetCalculationFooter(int id)
        {
            return (from p in db.CalculationFooter
                    where p.CalculationFooterLineId == id
                    select new CalculationFooterViewModel
                    {
                        AddDeduct = p.AddDeduct,
                        AffectCost = p.AffectCost,
                        CalculateOnId = p.CalculateOnId,
                        CalculationId = p.CalculationId,
                        Id = p.CalculationFooterLineId,
                        ChargeId = p.ChargeId,
                        ProductChargeId=p.ProductChargeId,
                        ChargeTypeId = p.ChargeTypeId,
                        CostCenterId = p.CostCenterId,
                        IncludedInBase = p.IncludedInBase,
                        IsActive = p.IsActive,
                        RateType=p.RateType,
                        IsVisible=p.IsVisible,
                        Amount=p.Amount,
                        ParentChargeId = p.ParentChargeId,
                        Rate = p.Rate,
                        Sr=p.Sr,
                        IncludedCharges=p.IncludedCharges,
                        IncludedChargesCalculation=p.IncludedChargesCalculation,
                    }

                        ).FirstOrDefault();


        }
        public IEnumerable<CalculationFooterViewModel> GetCalculationFooterListForDropDown()
        {
            return (from p in db.CalculationFooter
                    join t in db.Charge on p.ChargeId equals t.ChargeId
                    select new CalculationFooterViewModel
                    {
                        Id = p.CalculationFooterLineId,
                        CalculationFooterName = t.ChargeName,
                    }
                        );
        }
        public IEnumerable<CalculationFooterViewModel> GetCalculationListForIndex(int id)
        {

            var Query = (from p in db.CalculationFooter
                        where p.CalculationId == id
                        orderby p.Sr
                        select new CalculationFooterViewModel
                        {
                            AddDeduct = p.AddDeduct,
                            //AddDeductName = (p.AddDeduct == null ? "" : (p.AddDeduct == true ? "Add" : "Deduction")),
                            //AddDeductName = p.AddDeduct,
                            AffectCost = p.AffectCost,
                            AffectCostName = (p.AffectCost == true ? "Yes" : "No"),
                            CalculateOnId = p.CalculateOnId,
                            CalculateOnName = p.CalculateOn.ChargeName,
                            Id = p.CalculationFooterLineId,
                            ChargeName = p.Charge.ChargeName,
                            ChargeTypeName = p.ChargeType.ChargeTypeName,
                            CostCenterName = p.CostCenter.CostCenterName,
                            IncludedInBase = p.IncludedInBase,
                            IncludedInBaseName = (p.IncludedInBase == true ? "Yes" : "No"),
                            IsActive = p.IsActive,
                            Rate = p.Rate,
                            ParentChargeId = p.ParentChargeId,

                        }).ToList();
            var Result=Query.Select(p => new CalculationFooterViewModel
            {
                AddDeduct = p.AddDeduct,
                //AddDeductName = (p.AddDeduct == null ? "" : (p.AddDeduct == true ? "Add" : "Deduction")),
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
                IncludedInBaseName = (p.IncludedInBase == true ? "Yes" : "No"),
                IsActive = p.IsActive,
                Rate = p.Rate,
                ParentChargeId = p.ParentChargeId,
            }).AsEnumerable();
         
            return (Result);
        }

        public CalculationFooter Create(CalculationFooter pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<CalculationFooter>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<CalculationFooter>().Delete(id);
        }

        public void Delete(CalculationFooter pt)
        {
            _unitOfWork.Repository<CalculationFooter>().Delete(pt);
        }

        public void Update(CalculationFooter pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<CalculationFooter>().Update(pt);
        }

        public IEnumerable<CalculationFooter> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<CalculationFooter>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CalculationFooterLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<CalculationFooter> GetCalculationFooterList()
        {
            var pt = _unitOfWork.Repository<CalculationFooter>().Query().Get().OrderBy(m => m.CalculationFooterLineId);
            return pt;
        }
        public IEnumerable<CalculationFooterViewModel> GetCalculationFooterList(int CalculationID, int DocumentTypeId, int SiteId, int DivisionId)
        {
            return (from p in db.CalculationFooter
                    join t in db.CalculationHeaderLedgerAccount.Where(m=>m.DocTypeId==DocumentTypeId && m.SiteId==SiteId && m.DivisionId==DivisionId) on p.CalculationFooterLineId equals t.CalculationFooterId into table1 from tab1 in table1.DefaultIfEmpty()
                   where p.CalculationId==CalculationID
                   orderby p.Sr
                    select new CalculationFooterViewModel
                    {
                        AddDeduct=p.AddDeduct,
                        AffectCost=p.AffectCost,
                        CalculateOnId=p.CalculateOnId,
                        CalculateOnName=p.CalculateOn.ChargeName,
                        CalculateOnCode=p.CalculateOn.ChargeCode,
                        //Id=p.CalculationFooterLineId,
                        CalculationFooterName=p.Charge.ChargeName,
                        CalculationId=p.CalculationId,
                        CalculationName=p.Calculation.CalculationName,
                        ChargeId=p.ChargeId,
                        ChargeName=p.Charge.ChargeName,
                        ChargeCode=p.Charge.ChargeCode,
                        ChargeTypeId=p.ChargeTypeId,
                        ChargeTypeName=p.ChargeType.ChargeTypeName,
                        CostCenterId=p.CostCenterId,
                        CostCenterName=p.CostCenter.CostCenterName,
                        IncludedInBase=p.IncludedInBase,
                        LedgerAccountCrId = tab1.LedgerAccountCrId,
                        LedgerAccountCrName = tab1.LedgerAccountCr.LedgerAccountName,
                        LedgerAccountDrId = tab1.LedgerAccountDrId,
                        LedgerAccountDrName = tab1.LedgerAccountDr.LedgerAccountName,
                        ContraLedgerAccountId=tab1.ContraLedgerAccountId,
                        ContraLedgerAccountName=tab1.ContraLedgerAccount.LedgerAccountName,
                        ParentChargeId=p.ParentChargeId,
                        ProductChargeId=p.ProductChargeId,
                        ProductChargeName=p.ProductCharge.ChargeName,
                        ProductChargeCode=p.ProductCharge.ChargeCode,
                        Rate=p.Rate,
                        Amount=p.Amount,
                        Sr=p.Sr,
                        RateType=p.RateType,
                        IsVisible=p.IsVisible,
                        IncludedCharges = p.IncludedCharges,
                        IncludedChargesCalculation = p.IncludedChargesCalculation,
                    }
                        );
        }

        public CalculationFooter Add(CalculationFooter pt)
        {
            _unitOfWork.Repository<CalculationFooter>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.CalculationFooter
                        orderby p.CalculationFooterLineId
                        select p.CalculationFooterLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.CalculationFooter
                        orderby p.CalculationFooterLineId
                        select p.CalculationFooterLineId).FirstOrDefault();
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
                temp = (from p in db.CalculationFooter
                        orderby p.CalculationFooterLineId
                        select p.CalculationFooterLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.CalculationFooter
                        orderby p.CalculationFooterLineId
                        select p.CalculationFooterLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<CalculationFooter>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CalculationFooter> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
