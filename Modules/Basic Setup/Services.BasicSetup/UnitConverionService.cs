using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface IUnitConversionService : IDisposable
    {
        UnitConversion Create(UnitConversion pt);
        void Delete(int id);
        void Delete(UnitConversion pt);
        UnitConversion Find(int ptId);
        UnitConversion GetUnitConversion(int ProductId, string FromUnitId, string ToUnitId);        
        void Update(UnitConversion pt);
        UnitConversion Add(UnitConversion pt);
        IEnumerable<UnitConversion> GetUnitConversionList();
        IEnumerable<UnitConversionViewModel> GetProductUnitConversions(int id);
        Task<IEquatable<UnitConversion>> GetAsync();
        Task<UnitConversion> FindAsync(int id);
        int NextId(int id, int ProductId);
        int PrevId(int id, int ProductId);

    }

    public class UnitConversionService : IUnitConversionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<UnitConversion> _UnitConversionRepository;
        public UnitConversionService(IUnitOfWork unitOfWork, 
            IRepository<UnitConversion> unitConversion)
        {
            _unitOfWork = unitOfWork;
            _UnitConversionRepository = unitConversion;
        }

        public UnitConversionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _UnitConversionRepository = unitOfWork.Repository<UnitConversion>();
        }

        public UnitConversion Find(int pt)
        {
            //return UnitConversionRepository.Include(r => r.SalesOrder).Get().Where(i => i.SalesOrderDetailId == pt).FirstOrDefault();
            return _UnitConversionRepository.Find(pt);
        }


        public UnitConversion GetUnitConversion(int ProductId, string FromUnitId, string ToUnitId)
        {
            return _UnitConversionRepository.Query()
                                .Get().Where(i => i.ProductId ==ProductId && i.FromUnitId ==FromUnitId && i.ToUnitId == ToUnitId)
                                .FirstOrDefault();
        }

        public UnitConversion GetUnitConversionForUCF(int ProductId, string FromUnitId, string ToUnitId,int UnitConversionForId)
        {
            return _UnitConversionRepository.Query()
                                .Get().Where(i => i.ProductId == ProductId && i.FromUnitId == FromUnitId && i.ToUnitId == ToUnitId && i.UnitConversionForId==UnitConversionForId)
                                .FirstOrDefault();
        }
        public UnitConversion GetUnitConversion(int ProductId, int UnitConversionForId,string ToUnitId)
        {
            try
            {
                return _UnitConversionRepository.Query()
                    .Get().Where(i => i.ProductId == ProductId && i.UnitConversionForId == UnitConversionForId && i.ToUnitId == ToUnitId)
                    .FirstOrDefault();
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public UnitConversion GetUnitConversion(int ProductId, string FromUnitId,int UnitConversionForId, string ToUnitId)
        {
            return _UnitConversionRepository.Query()
                                .Get().Where(i => i.ProductId == ProductId && i.UnitConversionForId == UnitConversionForId && i.ToUnitId == ToUnitId&& i.FromUnitId==FromUnitId)
                                .FirstOrDefault();
        }



        public UnitConversion Create(UnitConversion pt)
        {
            pt.ObjectState = ObjectState.Added;
            _UnitConversionRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _UnitConversionRepository.Delete(id);
        }

        public void Delete(UnitConversion pt)
        {
            _UnitConversionRepository.Delete(pt);
        }

        public void Update(UnitConversion pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _UnitConversionRepository.Update(pt);
        }


        public IEnumerable<UnitConversion> GetUnitConversionList()
        {
            var pt = _UnitConversionRepository.Query().Get();

            return pt;
        }

        public UnitConversion Add(UnitConversion pt)
        {
            _UnitConversionRepository.Insert(pt);
            return pt;
        }

        public IEnumerable<UnitConversionViewModel> GetProductUnitConversions(int id)
        {
            return (from p in _UnitConversionRepository.Instance
                    where p.ProductId == id
                    select new UnitConversionViewModel
                    {
                        ProductId = p.ProductId,
                        FromQty=p.FromQty,
                        FromUnitId = p.FromUnitId,
                        FromUnitName=p.FromUnit.UnitName,
                        ToQty=p.ToQty,
                        ToUnitId = p.ToUnitId,
                        ToUnitName=p.ToUnit.UnitName,
                        UnitConversionForName=p.UnitConversionFor.UnitconversionForName,
                        UnitConversionId=p.UnitConversionId,      
                        UnitConversionForId = p.UnitConversionForId,
                        Description=p.Description,

                    }                    
                    );
        }
        public int NextId(int id, int ProductId)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _UnitConversionRepository.Instance
                        where p.ProductId == ProductId
                        orderby p.UnitConversionId
                        select p.UnitConversionId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _UnitConversionRepository.Instance
                        where p.ProductId == ProductId
                        orderby p.UnitConversionId
                        select p.UnitConversionId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id, int ProductId)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in _UnitConversionRepository.Instance
                        where p.ProductId == ProductId
                        orderby p.UnitConversionId
                        select p.UnitConversionId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _UnitConversionRepository.Instance
                        where p.ProductId == ProductId
                        orderby p.UnitConversionId
                        select p.UnitConversionId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<UnitConversion>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<UnitConversion> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
