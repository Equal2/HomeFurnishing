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

namespace Service
{
    public interface IChargeGroupPersonCalculationService : IDisposable
    {
        ChargeGroupPersonCalculation Create(ChargeGroupPersonCalculation pt);
        void Delete(int id);
        void Delete(ChargeGroupPersonCalculation pt);
        ChargeGroupPersonCalculation Find(int id);
        IEnumerable<ChargeGroupPersonCalculation> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(ChargeGroupPersonCalculation pt);
        ChargeGroupPersonCalculation Add(ChargeGroupPersonCalculation pt);
        IEnumerable<ChargeGroupPersonCalculationViewModel> GetChargeGroupPersonCalculationList(int id);

        ChargeGroupPersonCalculationViewModel GetChargeGroupPersonCalculationForEdit(int id);
        Task<IEquatable<ChargeGroupPersonCalculation>> GetAsync();
        Task<ChargeGroupPersonCalculation> FindAsync(int id);
        int? GetChargeGroupPersonCalculation(int DocTypeId, int ChargeGroupPersonId, int Siteid, int DivisionId);
        bool CheckForChargeGroupPersonCalculationExists(int DocTypeId, int? ChargeGroupPersonId, int? SiteId, int? DivisionId, int ChargeGroupPersonCalculationId);
        bool CheckForChargeGroupPersonCalculationExists(int DocTypeId, int? ChargeGroupPersonId, int? SiteId, int? DivisionId);
        int NextId(int id);
        int PrevId(int id);
    }

    public class ChargeGroupPersonCalculationService : IChargeGroupPersonCalculationService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<ChargeGroupPersonCalculation> _ChargeGroupPersonCalculationRepository;
        RepositoryQuery<ChargeGroupPersonCalculation> ChargeGroupPersonCalculationRepository;
        public ChargeGroupPersonCalculationService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _ChargeGroupPersonCalculationRepository = new Repository<ChargeGroupPersonCalculation>(db);
            ChargeGroupPersonCalculationRepository = new RepositoryQuery<ChargeGroupPersonCalculation>(_ChargeGroupPersonCalculationRepository);
        }




        public ChargeGroupPersonCalculation Find(int id)
        {
            return _unitOfWork.Repository<ChargeGroupPersonCalculation>().Find(id);
        }

        public ChargeGroupPersonCalculation Create(ChargeGroupPersonCalculation pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ChargeGroupPersonCalculation>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ChargeGroupPersonCalculation>().Delete(id);
        }

        public void Delete(ChargeGroupPersonCalculation pt)
        {
            _unitOfWork.Repository<ChargeGroupPersonCalculation>().Delete(pt);
        }

        public void Update(ChargeGroupPersonCalculation pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ChargeGroupPersonCalculation>().Update(pt);
        }

        public IEnumerable<ChargeGroupPersonCalculation> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<ChargeGroupPersonCalculation>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.ChargeGroupPersonCalculationId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public int? GetChargeGroupPersonCalculation(int DocTypeId, int ChargeGroupPersonId, int Siteid, int DivisionId)
        {
            var ChargeGroupPersonCalculationSiteDivisionWise = (from H in db.ChargeGroupPersonCalculation
                      where H.DocTypeId == DocTypeId && H.ChargeGroupPersonId == ChargeGroupPersonId && H.SiteId == Siteid && H.DivisionId == DivisionId
                      select new ChargeGroupPersonCalculationViewModel
                      {
                          ChargeGroupPersonCalculationId = H.ChargeGroupPersonCalculationId,
                          DocTypeId = H.DocTypeId,
                          DocTypeName = H.DocType.DocumentTypeName,
                          ChargeGroupPersonId = H.ChargeGroupPersonId,
                          ChargeGroupPersonName = H.ChargeGroupPerson.ChargeGroupPersonName,
                          CalculationId = H.CalculationId,
                          CalculationName = H.Calculation.CalculationName,
                          CreatedBy = H.CreatedBy,
                          ModifiedBy = H.ModifiedBy
                      }).FirstOrDefault();

            if (ChargeGroupPersonCalculationSiteDivisionWise != null)
            {
                return ChargeGroupPersonCalculationSiteDivisionWise.CalculationId;
            }
            else
            {
                var ChargeGroupPersonCalculation = (from H in db.ChargeGroupPersonCalculation
                                                    where H.DocTypeId == DocTypeId && H.ChargeGroupPersonId == ChargeGroupPersonId
                                                    select new ChargeGroupPersonCalculationViewModel
                                                    {
                                                        ChargeGroupPersonCalculationId = H.ChargeGroupPersonCalculationId,
                                                        DocTypeId = H.DocTypeId,
                                                        DocTypeName = H.DocType.DocumentTypeName,
                                                        ChargeGroupPersonId = H.ChargeGroupPersonId,
                                                        ChargeGroupPersonName = H.ChargeGroupPerson.ChargeGroupPersonName,
                                                        CalculationId = H.CalculationId,
                                                        CalculationName = H.Calculation.CalculationName,
                                                        CreatedBy = H.CreatedBy,
                                                        ModifiedBy = H.ModifiedBy
                                                    }).FirstOrDefault();
                if (ChargeGroupPersonCalculation != null)
                {
                    return ChargeGroupPersonCalculation.CalculationId;
                }
                else
                {
                    return null;
                }
            }
        }



        public IEnumerable<ChargeGroupPersonCalculationViewModel> GetChargeGroupPersonCalculationList(int id)
        {
            var pt = (from H in db.ChargeGroupPersonCalculation
                      where H.ChargeGroupPersonId == id
                      select new ChargeGroupPersonCalculationViewModel
                      {
                          ChargeGroupPersonCalculationId = H.ChargeGroupPersonCalculationId,
                          DocTypeId = H.DocTypeId,
                          DocTypeName = H.DocType.DocumentTypeName,
                          ChargeGroupPersonId = H.ChargeGroupPersonId,
                          ChargeGroupPersonName = H.ChargeGroupPerson.ChargeGroupPersonName,
                          CalculationId = H.CalculationId,
                          CalculationName = H.Calculation.CalculationName,
                          SiteId = H.SiteId,
                          SiteName = H.Site.SiteName,
                          DivisionId = H.DivisionId,
                          DivisionName = H.Division.DivisionName,
                          CreatedBy = H.CreatedBy,
                          ModifiedBy = H.ModifiedBy
                      }).ToList();

            return pt;
        }

        public ChargeGroupPersonCalculation Add(ChargeGroupPersonCalculation pt)
        {
            _unitOfWork.Repository<ChargeGroupPersonCalculation>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.ChargeGroupPersonCalculation
                        orderby p.ChargeGroupPersonCalculationId
                        select p.ChargeGroupPersonCalculationId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.ChargeGroupPersonCalculation
                        orderby p.ChargeGroupPersonCalculationId
                        select p.ChargeGroupPersonCalculationId).FirstOrDefault();
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

                temp = (from p in db.ChargeGroupPersonCalculation
                        orderby p.ChargeGroupPersonCalculationId
                        select p.ChargeGroupPersonCalculationId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.ChargeGroupPersonCalculation
                        orderby p.ChargeGroupPersonCalculationId
                        select p.ChargeGroupPersonCalculationId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ChargeGroupPersonCalculationViewModel GetChargeGroupPersonCalculationForEdit(int id)
        {
            ChargeGroupPersonCalculationViewModel Vm = (from H in db.ChargeGroupPersonCalculation
                                                        where H.ChargeGroupPersonCalculationId == id
                                                        select new ChargeGroupPersonCalculationViewModel
                                                        {
                                                            ChargeGroupPersonCalculationId = H.ChargeGroupPersonCalculationId,
                                                            DocTypeId = H.DocTypeId,
                                                            DocTypeName = H.DocType.DocumentTypeName,
                                                            ChargeGroupPersonId = H.ChargeGroupPersonId,
                                                            ChargeGroupPersonName = H.ChargeGroupPerson.ChargeGroupPersonName,
                                                            CalculationId = H.CalculationId,
                                                            CalculationName = H.Calculation.CalculationName,
                                                            SiteId = H.SiteId,
                                                            SiteName = H.Site.SiteName,
                                                            DivisionId = H.DivisionId,
                                                            DivisionName = H.Division.DivisionName,
                                                            CreatedBy = H.CreatedBy,
                                                            ModifiedBy = H.ModifiedBy
                                                        }).FirstOrDefault();
            return Vm;
        }

        public bool CheckForChargeGroupPersonCalculationExists(int DocTypeId, int? ChargeGroupPersonId, int? SiteId, int? DivisionId, int ChargeGroupPersonCalculationId)
        {
            ChargeGroupPersonCalculation Vm = (from H in db.ChargeGroupPersonCalculation
                                   where H.DocTypeId == DocTypeId && H.ChargeGroupPersonId == ChargeGroupPersonId && H.SiteId == SiteId 
                                           && H.DivisionId == DivisionId 
                                           && H.ChargeGroupPersonCalculationId != ChargeGroupPersonCalculationId
                                   select H).FirstOrDefault();
            if (Vm != null)
                return true;
            else 
                return false;
        }

        public bool CheckForChargeGroupPersonCalculationExists(int DocTypeId, int? ChargeGroupPersonId, int? SiteId, int? DivisionId)
        {
            ChargeGroupPersonCalculation Vm = (from H in db.ChargeGroupPersonCalculation
                                   where H.DocTypeId == DocTypeId && H.ChargeGroupPersonId == ChargeGroupPersonId && H.SiteId == SiteId
                                        && H.DivisionId == DivisionId 
                                   select H).FirstOrDefault();
            if (Vm != null)
                return true;
            else 
                return false;
        }


        public void Dispose()
        {
        }


        public Task<IEquatable<ChargeGroupPersonCalculation>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ChargeGroupPersonCalculation> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
