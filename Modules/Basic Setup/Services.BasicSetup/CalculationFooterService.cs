using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;
using Models.Company.ViewModels;
using System.Data.SqlClient;
using System.Configuration;

namespace Services.BasicSetup
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
        IEnumerable<HeaderChargeViewModel> GetCalculationFooterListSProc(int HeaderTableId, string HeaderTableName, string LineTableName);
    }

    public class CalculationFooterService : ICalculationFooterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CalculationFooter> _CalculationFooterRepository;

        public CalculationFooterService(IUnitOfWork unitOfWork, IRepository<CalculationFooter> CalculationFooter)
        {
            _unitOfWork = unitOfWork;
            _CalculationFooterRepository = CalculationFooter;
        }

        public CalculationFooterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CalculationFooterRepository = unitOfWork.Repository<CalculationFooter>();
        }


        public CalculationFooter Find(int id)
        {
            return _CalculationFooterRepository.Find(id);
        }
        public CalculationFooterViewModel GetCalculationFooter(int id)
        {
            return (from p in _CalculationFooterRepository.Instance
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
                    }

                        ).FirstOrDefault();


        }
        public IEnumerable<CalculationFooterViewModel> GetCalculationFooterListForDropDown()
        {
            return (from p in _CalculationFooterRepository.Instance
                    join t in _unitOfWork.Repository<Charge>().Instance on p.ChargeId equals t.ChargeId
                    select new CalculationFooterViewModel
                    {
                        Id = p.CalculationFooterLineId,
                        CalculationFooterName = t.ChargeName,
                    }
                        );
        }
        public IEnumerable<CalculationFooterViewModel> GetCalculationListForIndex(int id)
        {
            return (from p in _CalculationFooterRepository.Instance
                    where p.CalculationId == id
                    orderby p.CalculationFooterLineId
                    select new CalculationFooterViewModel
                    {
                        AddDeduct = p.AddDeduct,
                        AddDeductName = (p.AddDeduct == null ? "" : (p.AddDeduct == true ? "Add" : "Deduction")),
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

                    }
                        );
        }

        public CalculationFooter Create(CalculationFooter pt)
        {
            pt.ObjectState = ObjectState.Added;
            _CalculationFooterRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _CalculationFooterRepository.Delete(id);
        }

        public void Delete(CalculationFooter pt)
        {
            _CalculationFooterRepository.Delete(pt);
        }

        public void Update(CalculationFooter pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _CalculationFooterRepository.Update(pt);
        }

        public IEnumerable<CalculationFooter> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _CalculationFooterRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CalculationFooterLineId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<CalculationFooter> GetCalculationFooterList()
        {
            var pt = _CalculationFooterRepository.Query().Get().OrderBy(m => m.CalculationFooterLineId);
            return pt;
        }
        public IEnumerable<CalculationFooterViewModel> GetCalculationFooterList(int CalculationID, int DocumentTypeId, int SiteId, int DivisionId)
        {

            return (from p in _CalculationFooterRepository.Instance
                    join t in _unitOfWork.Repository<CalculationHeaderLedgerAccount>().Query().Get().Where(m=>m.DocTypeId==DocumentTypeId && m.SiteId==SiteId && m.DivisionId==DivisionId) on p.CalculationFooterLineId equals t.CalculationFooterId into table1 from tab1 in table1.DefaultIfEmpty()
                   where p.CalculationId==CalculationID
                   orderby p.Sr
                    select new CalculationFooterViewModel
                    {
                        AddDeduct=p.AddDeduct,
                        AffectCost=p.AffectCost,
                        CalculateOnId=p.CalculateOnId,
                        CalculateOnName=p.CalculateOn.ChargeName,
                        CalculateOnCode=p.CalculateOn.ChargeCode,
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
                    }
                        );
        }

        public CalculationFooter Add(CalculationFooter pt)
        {
            _CalculationFooterRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _CalculationFooterRepository.Instance
                        orderby p.CalculationFooterLineId
                        select p.CalculationFooterLineId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _CalculationFooterRepository.Instance
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
                temp = (from p in _CalculationFooterRepository.Instance
                        orderby p.CalculationFooterLineId
                        select p.CalculationFooterLineId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _CalculationFooterRepository.Instance
                        orderby p.CalculationFooterLineId
                        select p.CalculationFooterLineId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<HeaderChargeViewModel> GetCalculationFooterListSProc(int HeaderTableId, string HeaderTableName, string LineTableName)
        {
            SqlParameter SqlParameterHeaderTableId = new SqlParameter("@HeaderTableId", HeaderTableId);
            SqlParameter SqlParameterHeaderTableName = new SqlParameter("@HeaderTableName", HeaderTableName);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", LineTableName);

            IEnumerable<HeaderChargeViewModel> CalculationHeaderList = _unitOfWork.SqlQuery<HeaderChargeViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".CalculationHeaderCharge @HeaderTableId, @HeaderTableName, @LineTableName", SqlParameterHeaderTableId, SqlParameterHeaderTableName, SqlParameterLineTableName).ToList();

            return CalculationHeaderList;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
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
