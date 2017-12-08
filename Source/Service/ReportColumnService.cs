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
    public interface IReportColumnService : IDisposable
    {
        ReportColumn Create(ReportColumn pt);
        void Delete(int id);
        void Delete(ReportColumn pt);
        ReportColumn Find(int id);
        void Update(ReportColumn pt);
        ReportColumn Add(ReportColumn pt);
        IEnumerable<ReportColumnViewModel> GetReportColumnList(int ReportHeaderid, string ReportType);
        ReportColumn GetReportColumn(int id);
        ReportColumn GetReportColumnByName(string Name, int HeaderID);
        IEnumerable<ReportColumn> GetREportColumnListFromReportColumn(int ReportColumnId);
    }

    public class ReportColumnService : IReportColumnService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;

        public ReportColumnService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext();
            _unitOfWork = unitOfWork;
        }
        public ReportColumn Find(int id)
        {
            return _unitOfWork.Repository<ReportColumn>().Find(id);
        }

        public ReportColumn Create(ReportColumn pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ReportColumn>().Insert(pt);
            return pt;
        }
        public ReportColumn GetReportColumn(int id)
        {
            return ((from p in db.ReportColumn
                     where p.ReportColumnId == id
                     select p).FirstOrDefault()
                        );
        }
        public ReportColumn GetReportColumnByName(string Name, int HeaderID)
        {
            return (from p in db.ReportColumn
                    where p.ReportHeaderId == HeaderID && p.FieldName == Name
                    select p
                        ).FirstOrDefault();
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ReportColumn>().Delete(id);
        }

        public void Delete(ReportColumn pt)
        {
            _unitOfWork.Repository<ReportColumn>().Delete(pt);
        }

        public void Update(ReportColumn pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ReportColumn>().Update(pt);
        }

        public IEnumerable<ReportColumnViewModel> GetReportColumnList(int id, string ReportType)
        {

            if (!string.IsNullOrEmpty(ReportType))
            {
                var pt = _unitOfWork.Repository<ReportColumn>().Query().Get().Where(m => m.ReportHeaderId == id && m.SubReport.SubReportName == ReportType).OrderBy(m => m.Serial);

                var records= pt.Select(m => new
                {
                    AggregateFunc = m.AggregateFunc,
                    DisplayName = m.DisplayName,
                    FieldName = m.FieldName,
                    field = m.FieldName,
                    name = m.DisplayName,
                    id = m.FieldName,
                    IsVisible = m.IsVisible,
                    minWidth = m.minWidth,
                    ReportColumnId = m.ReportColumnId,
                    ReportHeaderId = m.ReportHeaderId,
                    SubReportId = m.SubReportId,
                    SubReportName=m.SubReport.SubReportName,
                    SubReportHeaderId=m.SubReportHeaderId,
                    TestAlignment = m.TestAlignment,
                    width = m.width,
                    IsDocNo=m.IsDocNo
                }).ToList();

                //return records;

                return records.Select((m => new ReportColumnViewModel
                {
                    hasTotal = m.AggregateFunc.ToLower().Contains("sum") ? true : false,
                    hasTotalName = m.AggregateFunc.ToLower().Contains("sum") ? true : (string.IsNullOrEmpty(m.AggregateFunc) ? false : true),
                    cssClass=m.TestAlignment,
                    headerCssClass = m.TestAlignment,
                    DisplayName = m.DisplayName,
                    FieldName = m.FieldName,
                    field = m.FieldName,
                    name = m.DisplayName,
                    id = m.FieldName,
                    IsVisible = m.IsVisible,
                    minWidth = !string.IsNullOrEmpty(m.minWidth) ? (int ?)Convert.ToInt32(m.minWidth) : null,
                    ReportColumnId = m.ReportColumnId,
                    ReportHeaderId = m.ReportHeaderId,
                    SubReportId = m.SubReportId,
                    SubReportName = m.SubReportName,
                    SubReportHeaderId = m.SubReportHeaderId,
                    TestAlignment = m.TestAlignment,
                    width = !string.IsNullOrEmpty(m.width) ? (int?)Convert.ToInt32(m.width) : null,
                    IsDocNo = m.IsDocNo
                }));

            }
            else
            {
                var pt = _unitOfWork.Repository<ReportColumn>().Query().Get().Where(m => m.ReportHeaderId == id).GroupBy(m => m.SubReport.ReportHeaderId);



                var records = pt.Select(m => new
                {
                    AggregateFunc = m.FirstOrDefault().AggregateFunc,
                    DisplayName = m.FirstOrDefault().DisplayName,
                    FieldName = m.FirstOrDefault().FieldName,
                    field = m.FirstOrDefault().FieldName,
                    name = m.FirstOrDefault().DisplayName,
                    id = m.FirstOrDefault().FieldName,
                    IsVisible = m.FirstOrDefault().IsVisible,
                    minWidth = m.FirstOrDefault().minWidth,
                    ReportColumnId = m.FirstOrDefault().ReportColumnId,
                    ReportHeaderId = m.FirstOrDefault().ReportHeaderId,
                    SubReportId = m.FirstOrDefault().SubReportId,
                    SubReportName = m.FirstOrDefault().SubReport.SubReportName,
                    SubReportHeaderId = m.FirstOrDefault().SubReportHeaderId,
                    TestAlignment = m.FirstOrDefault().TestAlignment,
                    width = m.FirstOrDefault().width,
                    Sr=m.FirstOrDefault().Serial,
                    IsDocNo = m.FirstOrDefault().IsDocNo
                }).OrderBy(m=>m.Sr).ToList();

                return records.Select((m => new ReportColumnViewModel
                {
                    hasTotal = m.AggregateFunc.ToLower().Contains("sum") ? true : false,
                    hasTotalName = m.AggregateFunc.ToLower().Contains("sum") ? true : (string.IsNullOrEmpty(m.AggregateFunc) ? false : true),
                    cssClass = m.TestAlignment,
                    headerCssClass = m.TestAlignment,
                    DisplayName = m.DisplayName,
                    FieldName = m.FieldName,
                    field = m.FieldName,
                    name = m.DisplayName,
                    id = m.FieldName,
                    IsVisible = m.IsVisible,
                    minWidth = !string.IsNullOrEmpty(m.minWidth) ? (int?)Convert.ToInt32(m.minWidth) : null,
                    ReportColumnId = m.ReportColumnId,
                    ReportHeaderId = m.ReportHeaderId,
                    SubReportId = m.SubReportId,
                    SubReportName = m.SubReportName,
                    SubReportHeaderId = m.SubReportHeaderId,
                    TestAlignment = m.TestAlignment,
                    width = !string.IsNullOrEmpty(m.width) ? (int?)Convert.ToInt32(m.width) : null,
                    IsDocNo = m.IsDocNo
                }));
            }

        }

        public IEnumerable<ReportColumn> GetREportColumnListFromReportColumn(int ReportColumnId)
        {
            var rc=Find(ReportColumnId);
            return _unitOfWork.Repository<ReportColumn>().Query().Get().Where(m => m.ReportHeaderId == rc.ReportHeaderId && m.SubReportId == rc.SubReportId).ToList();
            //return _unitOfWork.Repository<ReportColumn>().Query().Get().Where(m => m.ReportHeaderId == rc.ReportHeaderId && m.SubReportHeaderId == rc.SubReportHeaderId && m.SubReportId == rc.SubReportId).ToList();
        }

        public ReportColumn Add(ReportColumn pt)
        {
            _unitOfWork.Repository<ReportColumn>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }

    }
}
