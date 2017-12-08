using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Models.Customize.Models;
using Infrastructure.IO;
using Models.Company.ViewModels;

namespace Services.Customize
{
    public interface IJobOrderHeaderChargeService : IDisposable
    {
        JobOrderHeaderCharge Create(JobOrderHeaderCharge pt);
        void Delete(int id);
        void Delete(JobOrderHeaderCharge pt);
        JobOrderHeaderCharge Find(int id);
        IEnumerable<JobOrderHeaderCharge> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobOrderHeaderCharge pt);
        JobOrderHeaderCharge Add(JobOrderHeaderCharge pt);
        IEnumerable<JobOrderHeaderCharge> GetJobOrderHeaderChargeList();
        Task<IEquatable<JobOrderHeaderCharge>> GetAsync();
        Task<JobOrderHeaderCharge> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<HeaderChargeViewModel> GetCalculationFooterList(int HeaderTableId);
        void UpdateHeaderCharges(List<HeaderChargeViewModel> temp);
    }

    public class JobOrderHeaderChargeService : IJobOrderHeaderChargeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<JobOrderHeaderCharge> _jobOrderHeaderChargeRepository;
        public JobOrderHeaderChargeService(IUnitOfWork unitOfWork, IRepository<JobOrderHeaderCharge> joborderHEaderChargeRepo)
        {
            _unitOfWork = unitOfWork;
            _jobOrderHeaderChargeRepository = joborderHEaderChargeRepo;
        }

        public JobOrderHeaderChargeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _jobOrderHeaderChargeRepository = unitOfWork.Repository<JobOrderHeaderCharge>();
        }

        public IEnumerable<HeaderChargeViewModel> GetCalculationFooterList(int HeaderTableId)
        {
            var temp=from p in _jobOrderHeaderChargeRepository.Instance
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

            IEnumerable<HeaderChargeViewModel> CalculationHeaderList = _unitOfWork.SqlQuery<HeaderChargeViewModel>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".CalculationHeaderCharge @HeaderTableId, @HeaderTableName, @LineTableName", SqlParameterHeaderTableId, SqlParameterHeaderTableName, SqlParameterLineTableName).ToList();

            return CalculationHeaderList;
        }

        public JobOrderHeaderCharge Find(int id)
        {
            return (_jobOrderHeaderChargeRepository.Find(id));
        }

        public JobOrderHeaderCharge Create(JobOrderHeaderCharge pt)
        {
            pt.ObjectState = ObjectState.Added;
            _jobOrderHeaderChargeRepository.Insert(pt);

            return pt;
        }

        public void Delete(int id)
        {
            _jobOrderHeaderChargeRepository.Delete(id);
        }

        public void Delete(JobOrderHeaderCharge pt)
        {
            _jobOrderHeaderChargeRepository.Delete(pt);
        }

        public void Update(JobOrderHeaderCharge pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _jobOrderHeaderChargeRepository.Update(pt);
        }

        public IEnumerable<JobOrderHeaderCharge> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _jobOrderHeaderChargeRepository
                .Query()               
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobOrderHeaderCharge> GetJobOrderHeaderChargeList()
        {
            var pt = _jobOrderHeaderChargeRepository.Query().Get().OrderBy(m => m.HeaderTableId);

            return pt;
        }

        public JobOrderHeaderCharge Add(JobOrderHeaderCharge pt)
        {
            _jobOrderHeaderChargeRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _jobOrderHeaderChargeRepository.Instance
                        orderby p.HeaderTableId
                        select p.HeaderTableId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _jobOrderHeaderChargeRepository.Instance
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

                temp = (from p in _jobOrderHeaderChargeRepository.Instance
                        orderby p.HeaderTableId
                        select p.HeaderTableId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _jobOrderHeaderChargeRepository.Instance
                        orderby p.HeaderTableId
                        select p.HeaderTableId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void UpdateHeaderCharges(List<HeaderChargeViewModel> temp)
        {
            foreach (var item in temp)
            {
                var header = Find(item.Id);
                header.Rate = item.Rate;
                header.Amount = item.Amount;
                Update(header);
            }

            _unitOfWork.Save();

        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<JobOrderHeaderCharge>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobOrderHeaderCharge> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
