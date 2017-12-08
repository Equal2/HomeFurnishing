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
    public interface IJobReceiveBomService : IDisposable
    {
        JobReceiveBom Create(JobReceiveBom s);
        void Delete(int id);
        void Delete(JobReceiveBom s);
        JobReceiveBom Find(int id);
        void Update(JobReceiveBom s);
        IEnumerable<JobReceiveBom> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, decimal Qty);
        IEnumerable<JobReceiveBom> GetBomForLine(int id);
        IEnumerable<JobReceiveBom> GetBomForHeader(int id);
        JobReceiveBomViewModel GetJobReceiveBom(int id);
    }

    public class JobReceiveBomService : IJobReceiveBomService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public JobReceiveBomService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public JobReceiveBom Create(JobReceiveBom S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobReceiveBom>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobReceiveBom>().Delete(id);
        }

        public void Delete(JobReceiveBom s)
        {
            _unitOfWork.Repository<JobReceiveBom>().Delete(s);
        }

        public void Update(JobReceiveBom s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobReceiveBom>().Update(s);
        }



        public JobReceiveBom Find(int id)
        {
            return _unitOfWork.Repository<JobReceiveBom>().Find(id);
        }

        public IEnumerable<JobReceiveBom> GetBomPostingDataForWeaving(int ProductId, int? Dimension1Id, int? Dimension2Id, int? Dimension3Id, int? Dimension4Id, int? ProcessId, decimal Qty)
        {


            SqlParameter SqlParameterProductId = new SqlParameter("@ProductId", ProductId);
            SqlParameter SqlParameterDim1Id = new SqlParameter("@Dimension1Id", Dimension1Id);
            SqlParameter SqlParameterDim2Id = new SqlParameter("@Dimension2Id", Dimension2Id);
            SqlParameter SqlParameterProcessId = new SqlParameter("@ProcessId", Dimension2Id);
            SqlParameter SqlParameterQty = new SqlParameter("@Qty", Qty);

            IEnumerable<JobReceiveBom> PendingOrderQtyForPacking = db.Database.SqlQuery<JobReceiveBom>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetBomForWeaving @ProductId, @Dimension1Id, @Dimension2Id, @ProcessId, @Qty", SqlParameterProductId, SqlParameterDim1Id, SqlParameterDim2Id, SqlParameterProcessId, SqlParameterQty);

            return PendingOrderQtyForPacking;

        }

        public IEnumerable<JobReceiveBom> GetBomForLine(int id)
        {
            return (from p in db.JobReceiveBom
                    where p.JobReceiveLineId == id
                    select p
                        );
        }

        public IEnumerable<JobReceiveBom> GetBomForHeader(int id)
        {
            return (from p in db.JobReceiveBom
                    where p.JobReceiveHeaderId == id
                    select p
                        );
        }

        public JobReceiveBomViewModel GetJobReceiveBom(int id)
        {
            return (from p in db.JobReceiveBom
                    where p.JobReceiveBomId == id
                    select new JobReceiveBomViewModel
                    {
                        Dimension1Id = p.Dimension1Id,
                        Dimension2Id = p.Dimension2Id,
                        Dimension3Id = p.Dimension3Id,
                        Dimension4Id = p.Dimension4Id,
                        JobReceiveBomId = p.JobReceiveBomId,
                        JobReceiveHeaderId = p.JobReceiveHeaderId,
                        JobReceiveLineId=p.JobReceiveLineId,
                        LotNo = p.LotNo,
                        ProductId = p.ProductId,
                        Qty = p.Qty,
                        UnitId = p.Product.UnitId,
                    }
                       ).FirstOrDefault();

        }
        public void Dispose()
        {
        }
    }
}
