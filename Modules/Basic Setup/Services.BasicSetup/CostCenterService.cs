using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Infrastructure.IO;
using Models.BasicSetup.ViewModels;

namespace Services.BasicSetup
{
    public interface ICostCenterService : IDisposable
    {
        CostCenter Create(CostCenter pt);
        void Delete(int id);
        void Delete(CostCenter pt);
        CostCenter Find(string Name);
        CostCenter Find(int id);
        CostCenter Find(string Name, int DivisionId, int SiteId, int DocTypeId);
        IEnumerable<CostCenter> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(CostCenter pt);
        CostCenter Add(CostCenter pt);
        IEnumerable<CostCenter> GetCostCenterList();
        Task<IEquatable<CostCenter>> GetAsync();
        Task<CostCenter> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);

        ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum);
        ComboBoxResult GetValue(int Id);
    }

    public class CostCenterService : ICostCenterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CostCenter> _CostCenterRepository;
        public CostCenterService(IUnitOfWork unitOfWork, IRepository<CostCenter> costCenterRepo)
        {
            _unitOfWork = unitOfWork;
            _CostCenterRepository = costCenterRepo;
        }
        public CostCenterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CostCenterRepository = unitOfWork.Repository<CostCenter>();
        }

        public CostCenter Find(string Name)
        {
            return _CostCenterRepository.Query().Get().Where(i => i.CostCenterName == Name).FirstOrDefault();
        }

        public CostCenter Find(string Name, int DivisionId, int SiteId, int DocTypeId)
        {
            return _CostCenterRepository.Query().Get().Where(i => i.CostCenterName == Name && i.DivisionId == DivisionId && i.SiteId == SiteId && i.DocTypeId == DocTypeId).FirstOrDefault();
        }


        public CostCenter Find(int id)
        {
            return _CostCenterRepository.Find(id);            
        }

        public CostCenter Create(CostCenter pt)
        {
            pt.ObjectState = ObjectState.Added;
            _CostCenterRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _CostCenterRepository.Delete(id);
        }

        public void Delete(CostCenter pt)
        {
            _CostCenterRepository.Delete(pt);
        }

        public void Update(CostCenter pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _CostCenterRepository.Update(pt);
        }

        public IEnumerable<CostCenter> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _CostCenterRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CostCenterName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<CostCenter> GetCostCenterList()
        {
            var pt = _CostCenterRepository.Query().Get().OrderBy(m => m.CostCenterName);

            return pt;
        }

        public CostCenter Add(CostCenter pt)
        {
            _CostCenterRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _CostCenterRepository.Instance
                        orderby p.CostCenterName
                        select p.CostCenterId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _CostCenterRepository.Instance
                        orderby p.CostCenterName
                        select p.CostCenterId).FirstOrDefault();
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

                temp = (from p in _CostCenterRepository.Instance
                        orderby p.CostCenterName
                        select p.CostCenterId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _CostCenterRepository.Instance
                        orderby p.CostCenterName
                        select p.CostCenterId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }



        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _CostCenterRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.CostCenterName.ToLower().Contains(searchTerm.ToLower())))
                        && pr.IsActive == true
                        orderby pr.CostCenterName
                        select new ComboBoxResult
                        {
                            text = pr.CostCenterName,
                            id = pr.CostCenterId.ToString()
                        }
              );

            var temp = list
               .Skip(pageSize * (pageNum - 1)).Take(pageSize).ToList();

            var count = list.Count();

            ComboBoxPagedResult Data = new ComboBoxPagedResult();
            Data.Results = temp;
            Data.Total = count;

            return Data;
        }

        public ComboBoxResult GetValue(int Id)
        {
            ComboBoxResult ProductJson = new ComboBoxResult();

            IEnumerable<CostCenter> CostCenters = from pr in _CostCenterRepository.Instance
                                                  where pr.CostCenterId == Id
                                                  select pr;

            ProductJson.id = CostCenters.FirstOrDefault().CostCenterId.ToString();
            ProductJson.text = CostCenters.FirstOrDefault().CostCenterName;

            return ProductJson;
        }



        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<CostCenter>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CostCenter> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
