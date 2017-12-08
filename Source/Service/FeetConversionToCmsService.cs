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
    public interface IFeetConversionToCmsService : IDisposable
    {
        FeetConversionToCms Create(FeetConversionToCms pt);
        void Delete(int id);
        void Delete(FeetConversionToCms pt);
        FeetConversionToCms Find(int id);
        IEnumerable<FeetConversionToCms> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(FeetConversionToCms pt);
        FeetConversionToCms Add(FeetConversionToCms pt);
        IEnumerable<FeetConversionToCms> GetFeetConversionToCmsList();

        // IEnumerable<FeetConversionToCms> GetFeetConversionToCmsList(int buyerId);
        Task<IEquatable<FeetConversionToCms>> GetAsync();
        Task<FeetConversionToCms> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);

        bool CheckForDocNoExists(int Feet, int Inch);
        bool CheckForDocNoExists(int Feet, int Inch, int Id);
    }

    public class FeetConversionToCmsService : IFeetConversionToCmsService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<FeetConversionToCms> _FeetConversionToCmsRepository;
        RepositoryQuery<FeetConversionToCms> FeetConversionToCmsRepository;
        public FeetConversionToCmsService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _FeetConversionToCmsRepository = new Repository<FeetConversionToCms>(db);
            FeetConversionToCmsRepository = new RepositoryQuery<FeetConversionToCms>(_FeetConversionToCmsRepository);
        }
       

        public FeetConversionToCms Find(int id)
        {
            return _unitOfWork.Repository<FeetConversionToCms>().Find(id);
        }

        public FeetConversionToCms Create(FeetConversionToCms pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<FeetConversionToCms>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<FeetConversionToCms>().Delete(id);
        }

        public void Delete(FeetConversionToCms pt)
        {
            _unitOfWork.Repository<FeetConversionToCms>().Delete(pt);
        }

        public void Update(FeetConversionToCms pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<FeetConversionToCms>().Update(pt);
        }

        public IEnumerable<FeetConversionToCms> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<FeetConversionToCms>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.FeetConversionToCmsId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<FeetConversionToCms> GetFeetConversionToCmsList()
        {
            var pt = _unitOfWork.Repository<FeetConversionToCms>().Query().Get().OrderBy(m => m.FeetConversionToCmsId);

            return pt;
        }

        public FeetConversionToCms Add(FeetConversionToCms pt)
        {
            _unitOfWork.Repository<FeetConversionToCms>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.FeetConversionToCms
                        orderby p.FeetConversionToCmsId
                        select p.FeetConversionToCmsId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.FeetConversionToCms
                        orderby p.FeetConversionToCmsId
                        select p.FeetConversionToCmsId).FirstOrDefault();
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

                temp = (from p in db.FeetConversionToCms
                        orderby p.FeetConversionToCmsId
                        select p.FeetConversionToCmsId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.FeetConversionToCms
                        orderby p.FeetConversionToCmsId
                        select p.FeetConversionToCmsId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public bool CheckForDocNoExists(int Feet, int Inch)
        {
            var temp = (from p in db.FeetConversionToCms
                        where p.Feet == Feet && p.Inch == Inch
                        select p).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }

        public bool CheckForDocNoExists(int Feet, int Inch, int Id)
        {
            var temp = (from p in db.FeetConversionToCms
                        where p.Feet == Feet && p.Inch == Inch && p.FeetConversionToCmsId != Id
                        select p).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }



        public void Dispose()
        {
        }


        public Task<IEquatable<FeetConversionToCms>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<FeetConversionToCms> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
