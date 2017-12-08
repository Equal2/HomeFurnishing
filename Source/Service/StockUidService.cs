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
using Model.ViewModels;
using AutoMapper;

namespace Service
{
    public interface IStockUidService : IDisposable
    {
        StockUid Create(StockUid pt);
        void Delete(int id);
        void Update(StockUid pt);
        void DeleteStockUidForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId);
        void DeleteStockUidForDocLine(int DocLineId, int DocTypeId, int SiteId, int DivisionId);        
    }

    public class StockUidService : IStockUidService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<StockUid> _StockUidRepository;
        RepositoryQuery<StockUid> StockUidRepository;

        public StockUidService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StockUidRepository = new Repository<StockUid>(db);
            StockUidRepository = new RepositoryQuery<StockUid>(_StockUidRepository);
        }

        public StockUid GetStockUid(int pt)
        {
            return _unitOfWork.Repository<StockUid>().Find(pt);
        }        
        public StockUid Create(StockUid pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<StockUid>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<StockUid>().Delete(id);
        }

        public void Delete(StockUid pt)
        {
            _unitOfWork.Repository<StockUid>().Delete(pt);
        }


        public void Update(StockUid pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<StockUid>().Update(pt);
        }

        public IEnumerable<StockUid> GetStockUidList()
        {
            var pt = _unitOfWork.Repository<StockUid>().Query().Get();

            return pt;
        }

        public StockUid Add(StockUid pt)
        {
            _unitOfWork.Repository<StockUid>().Insert(pt);
            return pt;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<StockUid>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<StockUid> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public void DeleteStockUidForDocHeader(int DocHeaderId, int DocTypeId, int SiteId, int DivisionId)
        {
            IEnumerable<StockUid> StockUidList = (from L in db.StockUid
                                            where L.DocHeaderId == DocHeaderId && L.DocTypeId == DocTypeId && L.SiteId == SiteId && L.DivisionId == DivisionId
                                            select L).ToList();

            if (StockUidList != null)
            {
                foreach (StockUid item in StockUidList)
                {
                    Delete(item);
                }
            }
        }

        public void DeleteStockUidForDocLine(int DocLineId, int DocTypeId, int SiteId, int DivisionId)
        {
            IEnumerable<StockUid> StockUidList = (from L in db.StockUid
                                                  where L.DocLineId == DocLineId && L.DocTypeId == DocTypeId && L.SiteId == SiteId && L.DivisionId == DivisionId
                                                  select L).ToList();

            if (StockUidList != null)
            {
                foreach (StockUid item in StockUidList)
                {
                    Delete(item);
                }
            }
        }

        public void DeleteStockUidForDocLineDB(int DocLineId, int DocTypeId, int SiteId, int DivisionId,ref ApplicationDbContext Context)
        {
            IEnumerable<StockUid> StockUidList = (from L in Context.StockUid
                                                  where L.DocLineId == DocLineId && L.DocTypeId == DocTypeId && L.SiteId == SiteId && L.DivisionId == DivisionId
                                                  select L).ToList();

            if (StockUidList != null)
            {
                foreach (StockUid item in StockUidList)
                {
                    item.ObjectState = Model.ObjectState.Deleted;
                    Context.StockUid.Remove(item);
                }
            }
        }
    }
}
