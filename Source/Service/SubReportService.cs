using System.Collections.Generic;
using System.Linq;
using Data.Infrastructure;
using Model.Models;
using System;
using Model;
using Data.Models;
using Model.ViewModel;

namespace Service
{
    public interface ISubReportService : IDisposable
    {
        SubReport Create(SubReport pt);
        void Delete(int id);
        void Delete(SubReport pt);
        SubReport Find(int id);
        void Update(SubReport pt);
        SubReport Add(SubReport pt);
        SubReport GetSubReport(int id);
        IEnumerable<SubReportViewModel> GetSubReportList(int id);
    }

    public class SubReportService : ISubReportService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;

        public SubReportService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext();
            _unitOfWork = unitOfWork;
        }
        public SubReport Find(int id)
        {
            return _unitOfWork.Repository<SubReport>().Find(id);
        }

        public SubReport Create(SubReport pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<SubReport>().Insert(pt);
            return pt;
        }
        public SubReport GetSubReport(int id)
        {
            return ((from p in db.SubReport
                     where p.SubReportId == id
                     select p).FirstOrDefault()
                        );
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<SubReport>().Delete(id);
        }

        public void Delete(SubReport pt)
        {
            _unitOfWork.Repository<SubReport>().Delete(pt);
        }

        public void Update(SubReport pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<SubReport>().Update(pt);
        }

        public SubReport Add(SubReport pt)
        {
            _unitOfWork.Repository<SubReport>().Insert(pt);
            return pt;
        }

        public IEnumerable<SubReportViewModel> GetSubReportList(int id)
        {
            return (_unitOfWork.Repository<SubReport>().Query().Get().Where(m => m.ReportHeaderId == id).Select(m => new SubReportViewModel
            {
                ReportHeaderId = m.ReportHeaderId,
                ReportHeaderName = m.ReportHeader.ReportName,
                SqlProc = m.SqlProc,
                SubReportId = m.SubReportId,
                SubReportName = m.SubReportName
            })).ToList();
        }

        public void Dispose()
        {
        }

    }
}
