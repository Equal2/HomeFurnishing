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

namespace Service
{
    public interface IReportHeaderService : IDisposable
    {
        ReportHeader Create(ReportHeader pt);
        void Delete(int id);
        void Delete(ReportHeader pt);
        ReportHeader Find(int id);
        void Update(ReportHeader pt);
        ReportHeader Add(ReportHeader pt);
        IEnumerable<ReportHeader> GetReportHeaderList();
        ReportHeader GetReportHeader(int id);
        ReportHeader GetReportHeaderByName(string name);
        IEnumerable<ReportHeader> GetReportHeaderListForCopy(int id);
        List<string> GetSubReportProcList(int id);
    }

    public class ReportHeaderService : IReportHeaderService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;

        public ReportHeaderService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext();
            _unitOfWork = unitOfWork;
        }
        public ReportHeaderService(ApplicationDbContext db)
        {
            this.db = db;            
        }

        public ReportHeader Find(int id)
        {
            return _unitOfWork.Repository<ReportHeader>().Find(id);            
        }

        public ReportHeader Create(ReportHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ReportHeader>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ReportHeader>().Delete(id);
        }

        public void Delete(ReportHeader pt)
        {
            _unitOfWork.Repository<ReportHeader>().Delete(pt);
        }
        public ReportHeader GetReportHeader(int id)
        {
            return ((from p in db.ReportHeader
                     where p.ReportHeaderId == id
                     select p
                         ).FirstOrDefault());
        }
        public ReportHeader GetReportHeaderByName(string name)
        {
            return ((from p in db.ReportHeader
                     where p.ReportName == name
                     select p
                         ).FirstOrDefault());
        }

        public void Update(ReportHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ReportHeader>().Update(pt);
        }

        public IEnumerable<ReportHeader> GetReportHeaderList()
        {
            var pt = _unitOfWork.Repository<ReportHeader>().Query().Get();

            return pt;
        }

        public IEnumerable<ReportHeader> GetReportHeaderListForCopy(int id)
        {
            var pt = _unitOfWork.Repository<ReportHeader>().Query().Get().Where(m=>m.ReportHeaderId!=id);

            return pt;
        }

        public ReportHeader Add(ReportHeader pt)
        {
            _unitOfWork.Repository<ReportHeader>().Insert(pt);
            return pt;
        }

        public List<string> GetSubReportProcList(int id)
        {
            var pt = _unitOfWork.Repository<ReportHeader>().Query().Get().Where(m => m.ParentReportHeaderId  == id).Select (m=> m.SqlProc).ToList();

            return pt;
        }

        public void Dispose()
        {
        }

    }
}
