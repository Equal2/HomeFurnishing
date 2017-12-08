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
using System.Data.SqlClient;
using System.Configuration;

namespace Service
{
    public interface IPurchaseOrderAmendmentHeaderChargeService : IDisposable
    {
        PurchaseOrderAmendmentHeaderCharge Create(PurchaseOrderAmendmentHeaderCharge pt);
        void Delete(int id);
        void Delete(PurchaseOrderAmendmentHeaderCharge pt);
        PurchaseOrderAmendmentHeaderCharge Find(int id);
        IEnumerable<PurchaseOrderAmendmentHeaderCharge> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseOrderAmendmentHeaderCharge pt);
        PurchaseOrderAmendmentHeaderCharge Add(PurchaseOrderAmendmentHeaderCharge pt);
        IEnumerable<PurchaseOrderAmendmentHeaderCharge> GetPurchaseOrderAmendmentHeaderChargeList();
        Task<IEquatable<PurchaseOrderAmendmentHeaderCharge>> GetAsync();
        Task<PurchaseOrderAmendmentHeaderCharge> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<HeaderChargeViewModel> GetCalculationFooterList(int HeaderTableId);
    }

    public class PurchaseOrderAmendmentHeaderChargeService : IPurchaseOrderAmendmentHeaderChargeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        public PurchaseOrderAmendmentHeaderChargeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<HeaderChargeViewModel> GetCalculationFooterList(int HeaderTableId)
        {
            var temp=from p in db.PurchaseOrderAmendmentHeaderCharges
                       where p.HeaderTableId==HeaderTableId
                       orderby p.Sr
                       select new HeaderChargeViewModel
                       {
                           AddDeduct = p.AddDeduct,
                           AffectCost = p.AffectCost,
                           CalculateOnId = p.CalculateOnId,
                           CalculateOnName = p.CalculateOn.ChargeName,
                           CalculateOnCode = p.CalculateOn.ChargeCode,
                           Id = p.Id,
                           HeaderTableId=p.HeaderTableId,
                           ChargeId = p.ChargeId,
                           ChargeName = p.Charge.ChargeName,
                           ChargeCode = p.Charge.ChargeCode,
                           ChargeTypeId = p.ChargeTypeId,
                           ChargeTypeName = p.ChargeType.ChargeTypeName,
                           CostCenterId = p.CostCenterId,
                           CostCenterName = p.CostCenter.CostCenterName,
                           IncludedInBase = p.IncludedInBase,
                           LedgerAccountCrId = p.LedgerAccountCrId,
                           LedgerAccountCrName = p.LedgerAccountCr.LedgerAccountName,
                           LedgerAccountDrId = p.LedgerAccountDrId,
                           LedgerAccountDrName = p.LedgerAccountDr.LedgerAccountName,
                           ParentChargeId = p.ParentChargeId,
                           ProductChargeId = p.ProductChargeId,
                           ProductChargeName = p.ProductCharge.ChargeName,
                           ProductChargeCode = p.ProductCharge.ChargeCode,
                           Rate = p.Rate,
                           Amount = p.Amount,
                           Sr = p.Sr,
                           RateType = p.RateType,
                           IsVisible = p.IsVisible,
                       };

            return (temp);
        }

        public IEnumerable<HeaderChargeViewModel> GetCalculationFooterListSProc(int HeaderTableId, string HeaderTableName,string LineTableName)
        {
            SqlParameter SqlParameterHeaderTableId = new SqlParameter("@HeaderTableId", HeaderTableId);
            SqlParameter SqlParameterHeaderTableName = new SqlParameter("@HeaderTableName", HeaderTableName);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", LineTableName);

            IEnumerable<HeaderChargeViewModel> CalculationHeaderList = db.Database.SqlQuery<HeaderChargeViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".CalculationHeaderCharge @HeaderTableId, @HeaderTableName, @LineTableName", SqlParameterHeaderTableId, SqlParameterHeaderTableName, SqlParameterLineTableName).ToList();

            return CalculationHeaderList;
        }

        public PurchaseOrderAmendmentHeaderCharge Find(int id)
        {
            return (_unitOfWork.Repository<PurchaseOrderAmendmentHeaderCharge>().Find(id));
        }

        public PurchaseOrderAmendmentHeaderCharge Create(PurchaseOrderAmendmentHeaderCharge pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseOrderAmendmentHeaderCharge>().Insert(pt);

            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseOrderAmendmentHeaderCharge>().Delete(id);
        }

        public void Delete(PurchaseOrderAmendmentHeaderCharge pt)
        {
            _unitOfWork.Repository<PurchaseOrderAmendmentHeaderCharge>().Delete(pt);
        }

        public void Update(PurchaseOrderAmendmentHeaderCharge pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseOrderAmendmentHeaderCharge>().Update(pt);
        }

        public IEnumerable<PurchaseOrderAmendmentHeaderCharge> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseOrderAmendmentHeaderCharge>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseOrderAmendmentHeaderCharge> GetPurchaseOrderAmendmentHeaderChargeList()
        {
            var pt = _unitOfWork.Repository<PurchaseOrderAmendmentHeaderCharge>().Query().Get().OrderBy(m=>m.HeaderTableId);

            return pt;
        }

        public PurchaseOrderAmendmentHeaderCharge Add(PurchaseOrderAmendmentHeaderCharge pt)
        {
            _unitOfWork.Repository<PurchaseOrderAmendmentHeaderCharge>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseOrderAmendmentHeaderCharges
                        orderby p.HeaderTableId
                        select p.HeaderTableId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderAmendmentHeaderCharges
                        orderby p.HeaderTableId
                        select p.HeaderTableId).FirstOrDefault();
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

                temp = (from p in db.PurchaseOrderAmendmentHeaderCharges
                        orderby p.HeaderTableId
                        select p.HeaderTableId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderAmendmentHeaderCharges
                        orderby p.HeaderTableId
                        select p.HeaderTableId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseOrderAmendmentHeaderCharge>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderAmendmentHeaderCharge> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
