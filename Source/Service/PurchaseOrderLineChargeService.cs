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
    public interface IPurchaseOrderLineChargeService : IDisposable
    {
        PurchaseOrderLineCharge Create(PurchaseOrderLineCharge pt);
        void Delete(int id);
        void Delete(PurchaseOrderLineCharge pt);
        PurchaseOrderLineCharge Find(int id);
        IEnumerable<PurchaseOrderLineCharge> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseOrderLineCharge pt);
        PurchaseOrderLineCharge Add(PurchaseOrderLineCharge pt);
        IEnumerable<PurchaseOrderLineCharge> GetPurchaseOrderLineChargeList();

        // IEnumerable<PurchaseOrderLineCharge> GetPurchaseOrderLineChargeList(int buyerId);
        Task<IEquatable<PurchaseOrderLineCharge>> GetAsync();
        Task<PurchaseOrderLineCharge> FindAsync(int id);
        IEnumerable<LineChargeViewModel> GetCalculationProductList(int LineTableId);
        IEnumerable<LineChargeViewModel> GetCalculationProductListSProc(int LineTableId, string LineTableName);
        int NextId(int id);
        int PrevId(int id);
        int GetMaxProductCharge(int HeaderId, string LineTableName, string HeaderFieldName, string LineTableKeyField);
    }

    public class PurchaseOrderLineChargeService : IPurchaseOrderLineChargeService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseOrderLineCharge> _PurchaseOrderLineChargeRepository;
        RepositoryQuery<PurchaseOrderLineCharge> PurchaseOrderLineChargeRepository;
        public PurchaseOrderLineChargeService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public PurchaseOrderLineCharge Find(int id)
        {
            return (_unitOfWork.Repository<PurchaseOrderLineCharge>().Find(id));
        }

        public PurchaseOrderLineCharge Create(PurchaseOrderLineCharge pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseOrderLineCharge>().Insert(pt);

            return pt;
        }
        public IEnumerable<LineChargeViewModel> GetCalculationProductList(int LineTableId)
        {
            return (from p in db.PurchaseOrderLineCharge
                    where p.LineTableId== LineTableId
                    orderby p.Sr
                    select new LineChargeViewModel
                    {
                        AddDeduct = p.AddDeduct,
                        AffectCost = p.AffectCost,
                        CalculateOnId = p.CalculateOnId,
                        CalculateOnName = p.CalculateOn.ChargeName,
                        CalculateOnCode = p.CalculateOn.ChargeCode,                       
                        LineTableId=p.LineTableId,
                        Id = p.Id,
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
                        Rate = p.Rate,
                        Sr = p.Sr,
                        RateType = p.RateType,
                        IsVisible = p.IsVisible,
                        Amount = p.Amount,
                        ParentChargeId = p.ParentChargeId,                      

                    }
                      );
        }

        public int GetMaxProductCharge(int HeaderId,string LineTableName,string HeaderFieldName,string LineTableKeyField)
        {
            //var temp=from p in db.PurchaseOrderLineCharge
            //        where p.LineTableId == (from p1 in db.PurchaseOrderLine
            //                                orderby p1.PurchaseOrderLineId descending
            //                                    where p1.PurchaseOrderHeaderId==HeaderId
            //                                    select p1.PurchaseOrderLineId
            //                                    ).FirstOrDefault()
            //        orderby p.Sr
            //        select new LineChargeViewModel
            //        {
            //            AddDeduct = p.AddDeduct,
            //            AffectCost = p.AffectCost,
            //            CalculateOnId = p.CalculateOnId,
            //            CalculateOnName = p.CalculateOn.ChargeName,
            //            CalculateOnCode = p.CalculateOn.ChargeCode,
            //            LineTableId = p.LineTableId,
            //            Id = p.Id,
            //            ChargeId = p.ChargeId,
            //            ChargeName = p.Charge.ChargeName,
            //            ChargeCode = p.Charge.ChargeCode,
            //            ChargeTypeId = p.ChargeTypeId,
            //            ChargeTypeName = p.ChargeType.ChargeTypeName,
            //            CostCenterId = p.CostCenterId,
            //            CostCenterName = p.CostCenter.CostCenterName,
            //            IncludedInBase = p.IncludedInBase,
            //            LedgerAccountCrId = p.LedgerAccountCrId,
            //            LedgerAccountCrName = p.LedgerAccountCr.LedgerAccountName,
            //            LedgerAccountDrId = p.LedgerAccountDrId,
            //            LedgerAccountDrName = p.LedgerAccountDr.LedgerAccountName,
            //            Rate = p.Rate,
            //            Sr = p.Sr,
            //            RateType = p.RateType,
            //            IsVisible = p.IsVisible,
            //            Amount = 0,
            //            ParentChargeId = p.ParentChargeId,
            //        };
            //return temp;

            SqlParameter SqlParameterHeaderTableId = new SqlParameter("@HeaderTableKeyValue", HeaderId);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", LineTableName);
            SqlParameter SqlParameterHeaderFieldName = new SqlParameter("@HeaderFieldName", HeaderFieldName);
            SqlParameter SqlParameterLineTableFieldId = new SqlParameter("@LineTableKeyField", LineTableKeyField);

            int ? CalculationLineList = Convert.ToInt32(db.Database.SqlQuery<int ?>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".procGetCalculationMaxLineId @HeaderTableKeyValue, @LineTableName,@HeaderFieldName,@LineTableKeyField", SqlParameterHeaderTableId, SqlParameterLineTableName, SqlParameterHeaderFieldName, SqlParameterLineTableFieldId).FirstOrDefault());

            return CalculationLineList??0;

        }

        public IEnumerable<LineChargeViewModel> GetCalculationProductListSProc(int LineTableId,string LineTableName)
        {
            SqlParameter SqlParameterLineTableId = new SqlParameter("@LineTableld", LineTableId);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", LineTableName);

            IEnumerable<LineChargeViewModel> CalculationLineList = db.Database.SqlQuery<LineChargeViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".CalculationLineCharge @LineTableld, @LineTableName", SqlParameterLineTableId, SqlParameterLineTableName).ToList();

            return CalculationLineList;
        }
        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseOrderLineCharge>().Delete(id);
        }

        public void Delete(PurchaseOrderLineCharge pt)
        {
            _unitOfWork.Repository<PurchaseOrderLineCharge>().Delete(pt);
        }

        public void Update(PurchaseOrderLineCharge pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseOrderLineCharge>().Update(pt);
        }

        public IEnumerable<PurchaseOrderLineCharge> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseOrderLineCharge>()
                .Query()               
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseOrderLineCharge> GetPurchaseOrderLineChargeList()
        {
            var pt = _unitOfWork.Repository<PurchaseOrderLineCharge>().Query().Get().OrderBy(m=>m.LineTableId);

            return pt;
        }

        public PurchaseOrderLineCharge Add(PurchaseOrderLineCharge pt)
        {
            _unitOfWork.Repository<PurchaseOrderLineCharge>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.PurchaseOrderLineCharge
                        orderby p.LineTableId
                        select p.LineTableId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderLineCharge
                        orderby p.LineTableId
                        select p.LineTableId).FirstOrDefault();
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

                temp = (from p in db.PurchaseOrderLineCharge
                        orderby p.LineTableId
                        select p.LineTableId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseOrderLineCharge
                        orderby p.LineTableId
                        select p.LineTableId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseOrderLineCharge>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderLineCharge> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
