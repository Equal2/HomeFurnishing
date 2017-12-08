using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Models.Reports.Models;
using Infrastructure.IO;
using Models.Reports.DatabaseViews;
using Microsoft.Reporting.WebForms;

namespace Service
{
    public interface IReportUIDValuesService : IDisposable
    {
        ReportUIDValues Create(ReportUIDValues pt);
        void InsertRange(List<ReportParameter> Params, Guid Uid);
        void DeleteRange(Guid Uid);
        void Delete(int id);
        void Delete(ReportUIDValues pt);
        ReportUIDValues Find(int id);
        void Update(ReportUIDValues pt);
        ReportUIDValues Add(ReportUIDValues pt);
        IEnumerable<ReportUIDValues> GetReportUIDValuesList();
        ReportUIDValues GetReportUIDValues(int id);
    }

    public class ReportUIDValuesService : IReportUIDValuesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<ReportUIDValues> _ReportUIDValuesRepository;

        public ReportUIDValuesService(IUnitOfWork unitOfWork, IRepository<ReportUIDValues> ReportUIDValuesRepo)
        {
            _unitOfWork = unitOfWork;
            _ReportUIDValuesRepository = ReportUIDValuesRepo;
        }

        public ReportUIDValues Find(int id)
        {
            return _unitOfWork.Repository<ReportUIDValues>().Find(id);
        }

        public ReportUIDValues Create(ReportUIDValues pt)
        {
            pt.ObjectState = Model.ObjectState.Added;
            _unitOfWork.Repository<ReportUIDValues>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ReportUIDValues>().Delete(id);
        }

        public void Delete(ReportUIDValues pt)
        {
            _unitOfWork.Repository<ReportUIDValues>().Delete(pt);
        }
        public ReportUIDValues GetReportUIDValues(int id)
        {
            return ((from p in _ReportUIDValuesRepository.Instance
                     where p.Id == id
                     select p
                         ).FirstOrDefault());
        }

        public void Update(ReportUIDValues pt)
        {
            pt.ObjectState = Model.ObjectState.Modified;
            _unitOfWork.Repository<ReportUIDValues>().Update(pt);
        }

        public IEnumerable<ReportUIDValues> GetReportUIDValuesList()
        {
            var pt = _unitOfWork.Repository<ReportUIDValues>().Query().Get();

            return pt;
        }

        public ReportUIDValues Add(ReportUIDValues pt)
        {
            _unitOfWork.Repository<ReportUIDValues>().Insert(pt);
            return pt;
        }

        public string GetMenuName(int Id)
        {
            return _unitOfWork.Repository<_Menus>().Find(Id).MenuName;
        }

        public void InsertRange(List<ReportParameter> Params, Guid Uid)
        {
            foreach (var item in Params)
            {
                ReportUIDValues rid = new ReportUIDValues();
                rid.UID = Uid;
                rid.Type = item.Name;
                rid.Value = item.Values[0];
                rid.ObjectState = Model.ObjectState.Added;
                Create(rid);
            }
            _unitOfWork.Save();
        }

        public void DeleteRange(Guid Uid)
        {
            var Items = _ReportUIDValuesRepository.Query().Get().Where(m => m.UID == Uid).ToList();

            foreach (var item in Items)
            {
                item.ObjectState = Model.ObjectState.Deleted;
                Delete(item);
            }
            _unitOfWork.Save();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

    }
}
