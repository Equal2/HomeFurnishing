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
    public interface IJobOrderBomService : IDisposable
    {
        JobOrderBom Create(JobOrderBom s);
        void Delete(int id);
        void Delete(JobOrderBom s);
        JobOrderBom Find(int id);
        void Update(JobOrderBom s);
        IEnumerable<JobOrderBom> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, decimal Qty);
        IEnumerable<JobOrderBom> GetBomForLine(int id);
    }

    public class JobOrderBomService : IJobOrderBomService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public JobOrderBomService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public JobOrderBom Create(JobOrderBom S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobOrderBom>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobOrderBom>().Delete(id);
        }

        public void Delete(JobOrderBom s)
        {
            _unitOfWork.Repository<JobOrderBom>().Delete(s);
        }

        public void Update(JobOrderBom s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobOrderBom>().Update(s);
        }


    
        public JobOrderBom Find(int id)
        {
            return _unitOfWork.Repository<JobOrderBom>().Find(id);
        }

        public IEnumerable<JobOrderBom> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, decimal Qty)
        {


            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterDim1Id = new SqlParameter("@Dimension1Id", Dimension1Id);
            SqlParameter SqlParameterDim2Id = new SqlParameter("@Dimension2Id", Dimension2Id);
            SqlParameter SqlParameterProcessId = new SqlParameter("@ProcessId", Dimension2Id);
            SqlParameter SqlParameterQty = new SqlParameter("@Qty", Qty);

            IEnumerable<JobOrderBom> PendingOrderQtyForPacking = db.Database.SqlQuery<JobOrderBom>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetBomForWeaving @ProductId, @Dimension1Id, @Dimension2Id, @ProcessId, @Qty", SqlParameterProductId, SqlParameterDim1Id, SqlParameterDim2Id, SqlParameterProcessId, SqlParameterQty);

            return PendingOrderQtyForPacking;

        }

        public IEnumerable<JobOrderBom> GetBomForLine(int id)
        {
            return (from p in db.JobOrderBom
                    where p.JobOrderLineId == id
                    select p
                        );
        }

        public IEnumerable<JobOrderBom> GetBomForHeader(int id)
        {
            return (from p in db.JobOrderBom
                    where p.JobOrderHeaderId == id && p.JobOrderLineId == null
                    select p
                        );
        }

        public void Dispose()
        {
        }
    }
}
