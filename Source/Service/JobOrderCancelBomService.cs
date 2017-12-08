using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
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
    public interface IJobOrderCancelBomService : IDisposable
    {
        JobOrderCancelBom Create(JobOrderCancelBom s);
        void Delete(int id);
        void Delete(JobOrderCancelBom s);
        JobOrderCancelBom Find(int id);
        void Update(JobOrderCancelBom s);
        IEnumerable<JobOrderCancelBom> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, decimal Qty);
        IEnumerable<JobOrderCancelBom> GetBomForLine(int id);
    }

    public class JobOrderCancelBomService : IJobOrderCancelBomService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public JobOrderCancelBomService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public JobOrderCancelBom Create(JobOrderCancelBom S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderCancelBom>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderCancelBom>().Delete(id);
        }

        public void Delete(JobOrderCancelBom s)
        {
            _unitOfWork.Repository<JobOrderCancelBom>().Delete(s);
        }

        public void Update(JobOrderCancelBom s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderCancelBom>().Update(s);
        }


    
        public JobOrderCancelBom Find(int id)
        {
            return _unitOfWork.Repository<JobOrderCancelBom>().Find(id);
        }

        public IEnumerable<JobOrderCancelBom> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, decimal Qty)
        {


            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterDim1Id = new SqlParameter("@Dimension1Id", Dimension1Id);
            SqlParameter SqlParameterDim2Id = new SqlParameter("@Dimension2Id", Dimension2Id);
            SqlParameter SqlParameterProcessId = new SqlParameter("@ProcessId", Dimension2Id);
            SqlParameter SqlParameterQty = new SqlParameter("@Qty", Qty);

            IEnumerable<JobOrderCancelBom> PendingOrderQtyForPacking = db.Database.SqlQuery<JobOrderCancelBom>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetBomForWeaving @ProductId, @Dimension1Id, @Dimension2Id, @ProcessId, @Qty", SqlParameterProductId, SqlParameterDim1Id, SqlParameterDim2Id, SqlParameterProcessId, SqlParameterQty);

            return PendingOrderQtyForPacking;

        }

        public IEnumerable<JobOrderCancelBom> GetBomForLine(int id)
        {
            return (from p in db.JobOrderCancelBom
                    where p.JobOrderCancelLineId == id
                    select p
                        );
        }
        public void Dispose()
        {
        }
    }
}
