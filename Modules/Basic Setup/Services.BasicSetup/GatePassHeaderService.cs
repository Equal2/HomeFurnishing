using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface IGatePassHeaderService : IDisposable
    {
        GatePassHeader Create(GatePassHeader pt);
        void Delete(int id);
        void Delete(GatePassHeader pt);
        GatePassHeader Find(string Name);
        GatePassHeader Find(int id);
        IEnumerable<GatePassHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(GatePassHeader pt);
        GatePassHeader Add(GatePassHeader pt);                
        Task<IEquatable<GatePassHeader>> GetAsync();
        Task<GatePassHeader> FindAsync(int id);        
    }

    public class GatePassHeaderService : IGatePassHeaderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<GatePassHeader> _GatePassHeaderRepository;
        public GatePassHeaderService(IUnitOfWork unitOfWork,IRepository<GatePassHeader> gatePassHeaderRepo)
        {
            _unitOfWork = unitOfWork;
            _GatePassHeaderRepository = gatePassHeaderRepo;
        }
        public GatePassHeaderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _GatePassHeaderRepository = unitOfWork.Repository<GatePassHeader>();
        }
        public GatePassHeader Find(string Name)
        {
            return _GatePassHeaderRepository.Query().Get().Where(i => i.DocNo == Name).FirstOrDefault();
        }


        public GatePassHeader Find(int id)
        {
            return _GatePassHeaderRepository.Find(id);
        }

        public GatePassHeader Create(GatePassHeader pt)
        {
            pt.ObjectState = ObjectState.Added;
            _GatePassHeaderRepository.Insert(pt);
            return pt;
        }     

        public void Delete(int id)
        {
            _GatePassHeaderRepository.Delete(id);
        }

        public void Delete(GatePassHeader pt)
        {
            _GatePassHeaderRepository.Delete(pt);
        }

        public void Update(GatePassHeader pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _GatePassHeaderRepository.Update(pt);
        }

        public IEnumerable<GatePassHeader> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _GatePassHeaderRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocNo))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public GatePassHeader Add(GatePassHeader pt)
        {
            _GatePassHeaderRepository.Insert(pt);
            return pt;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<GatePassHeader>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<GatePassHeader> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
