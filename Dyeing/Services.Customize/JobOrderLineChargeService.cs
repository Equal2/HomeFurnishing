using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Models.Customize.Models;
using Models.Company.ViewModels;
using Infrastructure.IO;

namespace Services.Customize
{
    public interface IJobOrderLineChargeService : IDisposable
    {
        JobOrderLineCharge Create(JobOrderLineCharge pt);
        void Delete(int id);
        void Delete(JobOrderLineCharge pt);
        JobOrderLineCharge Find(int id);
        IEnumerable<JobOrderLineCharge> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderLineCharge pt);
        JobOrderLineCharge Add(JobOrderLineCharge pt);
        IEnumerable<JobOrderLineCharge> GetJobOrderLineChargeList();
        Task<IEquatable<JobOrderLineCharge>> GetAsync();
        Task<JobOrderLineCharge> FindAsync(int id);
        IEnumerable<LineChargeViewModel> GetCalculationProductList(int LineTableId);
        IEnumerable<LineChargeViewModel> GetCalculationProductListSProc(int LineTableId, string LineTableName);
        int NextId(int id);
        int PrevId(int id);
        int GetMaxProductCharge(int HeaderId, string LineTableName, string HeaderFieldName, string LineTableKeyField);
    }

    public class JobOrderLineChargeService : IJobOrderLineChargeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobOrderLineCharge> _JobOrderLineChargeRepository;
        public JobOrderLineChargeService(IUnitOfWork unitOfWork, IRepository<JobOrderLineCharge> JobOrderLineChargeRepo)
        {
            _unitOfWork = unitOfWork;
            _JobOrderLineChargeRepository = JobOrderLineChargeRepo;
        }

        public JobOrderLineChargeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobOrderLineChargeRepository = unitOfWork.Repository<JobOrderLineCharge>();
        }

        public JobOrderLineCharge Find(int id)
        {
            return (_JobOrderLineChargeRepository.Find(id));
        }

        public JobOrderLineCharge Create(JobOrderLineCharge pt)
        {
            pt.ObjectState = ObjectState.Added;
            _JobOrderLineChargeRepository.Add(pt);

            return pt;
        }
        public IEnumerable<LineChargeViewModel> GetCalculationProductList(int LineTableId)
        {
            return (from p in _JobOrderLineChargeRepository.Instance
                    where p.LineTableId == LineTableId
                    orderby p.Sr
                    select new LineChargeViewModel
                    {
                        AddDeduct = p.AddDeduct,
                        AffectCost = p.AffectCost,
                        CalculateOnId = p.CalculateOnId,
                        CalculateOnName = p.CalculateOn.ChargeName,
                        CalculateOnCode = p.CalculateOn.ChargeCode,
                        LineTableId = p.LineTableId,
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

        public int GetMaxProductCharge(int HeaderId, string LineTableName, string HeaderFieldName, string LineTableKeyField)
        {
            SqlParameter SqlParameterHeaderTableId = new SqlParameter("@HeaderTableKeyValue", HeaderId);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", LineTableName);
            SqlParameter SqlParameterHeaderFieldName = new SqlParameter("@HeaderFieldName", HeaderFieldName);
            SqlParameter SqlParameterLineTableFieldId = new SqlParameter("@LineTableKeyField", LineTableKeyField);

            int? CalculationLineList = Convert.ToInt32(_unitOfWork.SqlQuery<int?>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".procGetCalculationMaxLineId @HeaderTableKeyValue, @LineTableName,@HeaderFieldName,@LineTableKeyField", SqlParameterHeaderTableId, SqlParameterLineTableName, SqlParameterHeaderFieldName, SqlParameterLineTableFieldId).FirstOrDefault());

            return CalculationLineList ?? 0;

        }

        public IEnumerable<LineChargeViewModel> GetCalculationProductListSProc(int LineTableId, string LineTableName)
        {
            SqlParameter SqlParameterLineTableId = new SqlParameter("@LineTableld", LineTableId);
            SqlParameter SqlParameterLineTableName = new SqlParameter("@LineTableName", LineTableName);

            IEnumerable<LineChargeViewModel> CalculationLineList = _unitOfWork.SqlQuery<LineChargeViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".CalculationLineCharge @LineTableld, @LineTableName", SqlParameterLineTableId, SqlParameterLineTableName).ToList();

            return CalculationLineList;
        }
        public void Delete(int id)
        {
            _JobOrderLineChargeRepository.Delete(id);
        }

        public void Delete(JobOrderLineCharge pt)
        {
            _JobOrderLineChargeRepository.Delete(pt);
        }

        public void Update(JobOrderLineCharge pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _JobOrderLineChargeRepository.Update(pt);
        }

        public IEnumerable<JobOrderLineCharge> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _JobOrderLineChargeRepository
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobOrderLineCharge> GetJobOrderLineChargeList()
        {
            var pt = _JobOrderLineChargeRepository.Query().Get().OrderBy(m => m.LineTableId);

            return pt;
        }

        public JobOrderLineCharge Add(JobOrderLineCharge pt)
        {
            _JobOrderLineChargeRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _JobOrderLineChargeRepository.Instance
                        orderby p.LineTableId
                        select p.LineTableId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _JobOrderLineChargeRepository.Instance
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

                temp = (from p in _JobOrderLineChargeRepository.Instance
                        orderby p.LineTableId
                        select p.LineTableId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _JobOrderLineChargeRepository.Instance
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
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<JobOrderLineCharge>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderLineCharge> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
