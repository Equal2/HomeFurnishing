using System.Collections.Generic;
using System.Linq;
using Surya.India.Data;
using Surya.India.Data.Infrastructure;
using Surya.India.Model.Models;

using Surya.India.Core.Common;
using System;
using Surya.India.Model;
using System.Threading.Tasks;
using Surya.India.Data.Models;

namespace Surya.India.Service
{
    public interface IStreamService : IDisposable
    {
        Sch_Stream Create(Sch_Stream pt);
        void Delete(int id);
        void Delete(Sch_Stream pt);
        Sch_Stream Find(string Name);
        Sch_Stream Find(int id);
        IEnumerable<Sch_Stream> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(Sch_Stream pt);
        Sch_Stream Add(Sch_Stream pt);
        IEnumerable<Sch_Stream> GetStreamList();

        IEnumerable<Sch_Stream> GetStreamList(int ProgramId);

        // IEnumerable<Sch_Stream> GetStreamList(int buyerId);
        Task<IEquatable<Sch_Stream>> GetAsync();
        Task<Sch_Stream> FindAsync(int id);
        Sch_Stream GetStreamByName(string terms);
        int NextId(int id);
        int PrevId(int id);
    }

    public class StreamService : IStreamService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<Sch_Stream> _StreamRepository;
        RepositoryQuery<Sch_Stream> StreamRepository;
        public StreamService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _StreamRepository = new Repository<Sch_Stream>(db);
            StreamRepository = new RepositoryQuery<Sch_Stream>(_StreamRepository);
        }
        public Sch_Stream GetStreamByName(string terms)
        {
            return (from p in db.Sch_Stream
                    where p.StreamName == terms
                    select p).FirstOrDefault();
        }

        public Sch_Stream Find(string Name)
        {
            return StreamRepository.Get().Where(i => i.StreamName == Name).FirstOrDefault();
        }


        public Sch_Stream Find(int id)
        {
            return _unitOfWork.Repository<Sch_Stream>().Find(id);
        }

        public Sch_Stream Create(Sch_Stream pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<Sch_Stream>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<Sch_Stream>().Delete(id);
        }

        public void Delete(Sch_Stream pt)
        {
            _unitOfWork.Repository<Sch_Stream>().Delete(pt);
        }

        public void Update(Sch_Stream pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<Sch_Stream>().Update(pt);
        }

        public IEnumerable<Sch_Stream> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<Sch_Stream>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.StreamName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<Sch_Stream> GetStreamList()
        {
            var pt = _unitOfWork.Repository<Sch_Stream>().Query().Get().OrderBy(m=>m.StreamName);

            return pt;
        }

        public IEnumerable<Sch_Stream> GetStreamList(int ProgramId)
        {
            var pt = _unitOfWork.Repository<Sch_Stream>().Query().Get().Where(i => i.ProgramId == ProgramId).OrderBy(m => m.StreamName);

            return pt;
        }

        public Sch_Stream Add(Sch_Stream pt)
        {
            _unitOfWork.Repository<Sch_Stream>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.Sch_Stream
                        orderby p.StreamName
                        select p.StreamId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.Sch_Stream
                        orderby p.StreamName
                        select p.StreamId).FirstOrDefault();
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

                temp = (from p in db.Sch_Stream
                        orderby p.StreamName
                        select p.StreamId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.Sch_Stream
                        orderby p.StreamName
                        select p.StreamId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<Sch_Stream>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Sch_Stream> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
