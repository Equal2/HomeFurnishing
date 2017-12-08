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
        IEnumerable<LineChargeViewModel> GetCalculationProductListSProc(int LineTableId, string LineTableName);
        int GetMaxProductCharge(int HeaderId, string LineTableName, string HeaderFieldName, string LineTableKeyField);
    }

    public class CalculationProductService : ICalculationProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CalculationProduct> _CalculationProductRepository;

        public CalculationProductService(IUnitOfWork unitOfWork, IRepository<CalculationProduct> calculationProduct)
        {
            _unitOfWork = unitOfWork;
            _CalculationProductRepository = calculationProduct;
        }
        public CalculationProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CalculationProductRepository = unitOfWork.Repository<CalculationProduct>();
        }


        public CalculationProduct Find(int id)
        {
            return _CalculationProductRepository.Find(id);
        }
        public CalculationProductViewModel GetCalculationProduct(int id)
        {
            return (from p in _CalculationProductRepository.Instance
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
                       Sr=p.Sr
                    }

                        ).FirstOrDefault();


        }
        public IEnumerable<CalculationProductViewModel> GetCalculationProductListForDropDown()
        {
            return (from p in _CalculationProductRepository.Instance
                    join t in _unitOfWork.Repository<Charge>().Instance on p.ChargeId equals t.ChargeId
                    select new CalculationProductViewModel
                    {
                        Id = p.CalculationProductId,
                        CalculationProductName = t.ChargeName,
                    }
                        );
        }
        public IEnumerable<CalculationProductViewModel> GetCalculationListForIndex(int id)
        {
            return (from p in _CalculationProductRepository.Instance
                    where p.CalculationId == id
                    orderby p.CalculationProductId
                    select new CalculationProductViewModel
                    {
                        AddDeduct=p.AddDeduct,
                        AddDeductName=(p.AddDeduct==null?"":(p.AddDeduct==true?"Add":"Deduction")),
                        AffectCost=p.AffectCost,
                        AffectCostName=(p.AffectCost==true?"Yes":"No"),
                        CalculateOnId=p.CalculateOnId,
                        CalculateOnName=p.CalculateOn.ChargeName,                        
                        Id=p.CalculationProductId,
                        ChargeName=p.Charge.ChargeName,
                        ChargeTypeName=p.ChargeType.ChargeTypeName,
                        CostCenterName=p.CostCenter.CostCenterName,
                        IncludedInBase=p.IncludedInBase,
                        IncludedInBaseName=(p.IncludedInBase==true?"Yes":"No"),
                        IsActive=p.IsActive,
                        Rate=p.Rate,
                        ParentChargeId=p.ParentChargeId,
                        
                    }
                        );
        }

        public CalculationProduct Create(CalculationProduct pt)
        {
            pt.ObjectState = ObjectState.Added;
            _CalculationProductRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _CalculationProductRepository.Delete(id);
        }

        public void Delete(CalculationProduct pt)
        {
            _CalculationProductRepository.Delete(pt);
        }

        public void Update(CalculationProduct pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _CalculationProductRepository.Update(pt);
        }

        public IEnumerable<CalculationProduct> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _CalculationProductRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CalculationProductId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<CalculationProduct> GetCalculationProductList()
        {
            var pt = _CalculationProductRepository.Query().Get().OrderBy(m => m.CalculationProductId);
            return pt;
        }
        public IEnumerable<CalculationProductViewModel> GetCalculationProductList(int CalculationID, int DocumentTypeId, int SiteId, int DivisionId)
        {

            return (from p in _CalculationProductRepository.Instance
                    join t in _unitOfWork.Repository<CalculationLineLedgerAccount>().Query().Get().Where(m=>m.DocTypeId==DocumentTypeId && m.SiteId == SiteId && m.DivisionId== DivisionId) on p.CalculationProductId equals t.CalculationProductId into table1 from tab1 in table1.DefaultIfEmpty()
                    where p.CalculationId == CalculationID
                    orderby p.Sr
                    select new CalculationProductViewModel
                    {
                        AddDeduct=p.AddDeduct,
                        AffectCost=p.AffectCost,
                        CalculateOnId=p.CalculateOnId,
                        CalculateOnName=p.CalculateOn.ChargeName,
                        CalculateOnCode=p.CalculateOn.ChargeCode,
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
                        LedgerAccountCrId=tab1.LedgerAccountCrId,
                        LedgerAccountCrName = tab1.LedgerAccountCr.LedgerAccountName,
                        LedgerAccountDrId = tab1.LedgerAccountDrId,
                        LedgerAccountDrName = tab1.LedgerAccountDr.LedgerAccountName,
                        ContraLedgerAccountId=tab1.ContraLedgerAccountId,
                        ContraLedgerAccountName=tab1.ContraLedgerAccount.LedgerAccountName,
                        Rate=p.Rate,
                        Sr=p.Sr,
                        RateType=p.RateType,
                        IsVisible=p.IsVisible,
                        Amount=p.Amount,
                        ParentChargeId=p.ParentChargeId,
                        ElementId="CALL_"+p.Charge.ChargeCode,

                    }
                        );
        }

        public CalculationProduct Add(CalculationProduct pt)
        {
            _CalculationProductRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _CalculationProductRepository.Instance
                        orderby p.CalculationProductId
                        select p.CalculationProductId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _CalculationProductRepository.Instance
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
                temp = (from p in _CalculationProductRepository.Instance
                        orderby p.CalculationProductId
                        select p.CalculationProductId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _CalculationProductRepository.Instance
                        orderby p.CalculationProductId
                        select p.CalculationProductId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<LineChargeViewModel> GetCalculationProductListSProc(int LineTableId, string LineTableName)
        {
            SqlParameter SqlParameterLineTableId = new SqlParameter("@LineTableld", LineTableId);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", LineTableName);

            IEnumerable<LineChargeViewModel> CalculationLineList = _unitOfWork.SqlQuery<LineChargeViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".CalculationLineCharge @LineTableld, @LineTableName", SqlParameterLineTableId, SqlParameterLineTableName).ToList();

            return CalculationLineList;
        }

        public int GetMaxProductCharge(int HeaderId, string LineTableName, string HeaderFieldName, string LineTableKeyField)
        {
            SqlParameter SqlParameterHeaderTableId = new SqlParameter("@HeaderTableKeyValue", HeaderId);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", LineTableName);
            SqlParameter SqlParameterHeaderFieldName = new SqlParameter("@HeaderFieldName", HeaderFieldName);
            SqlParameter SqlParameterLineTableFieldId = new SqlParameter("@LineTableKeyField", LineTableKeyField);

            int? CalculationLineList = Convert.ToInt32(_unitOfWork.SqlQuery<int?>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".procGetCalculationMaxLineId @HeaderTableKeyValue, @LineTableName,@HeaderFieldName,@LineTableKeyField", SqlParameterHeaderTableId, SqlParameterLineTableName, SqlParameterHeaderFieldName, SqlParameterLineTableFieldId).FirstOrDefault());

            return CalculationLineList ?? 0;

        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }



        public Task<IEquatable<CalculationProduct>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CalculationProduct> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        
    }
}
