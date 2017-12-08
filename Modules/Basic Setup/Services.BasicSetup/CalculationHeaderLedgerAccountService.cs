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
    public interface ICalculationHeaderLedgerAccountService : IDisposable
    {
        CalculationHeaderLedgerAccount Create(CalculationHeaderLedgerAccount pt);
        void Delete(int id);
        void Delete(CalculationHeaderLedgerAccount pt);
        CalculationHeaderLedgerAccount Find(int id);
        IEnumerable<CalculationHeaderLedgerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(CalculationHeaderLedgerAccount pt);
        CalculationHeaderLedgerAccount Add(CalculationHeaderLedgerAccount pt);
        IEnumerable<CalculationHeaderLedgerAccountViewModel> GetCalculationHeaderLedgerAccountList(int CalculationID);
        Task<IEquatable<CalculationHeaderLedgerAccount>> GetAsync();
        Task<CalculationHeaderLedgerAccount> FindAsync(int id);
        IEnumerable<CalculationHeaderLedgerAccountViewModel> GetCalculationListForIndex(int id, int DocTypeId);//CalculationId
        CalculationHeaderLedgerAccountViewModel GetCalculationHeaderLedgerAccount(int id, int DocTypeId);//CalculationHeaderLedgerAccountId  
        CalculationHeaderLedgerAccountViewModel GetCalculationHeaderLedgerAccount(int id);//CalculationHeaderLedgerAccountId  
        IQueryable<CalculationHeaderLedgerAccountViewModel> GetHeaderIndex();
        IEnumerable<ComboBoxList> GetProductFooters(int id, string term);
        int NextId(int id);
        int PrevId(int id);
    }

    public class CalculationHeaderLedgerAccountService : ICalculationHeaderLedgerAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CalculationHeaderLedgerAccount> _CalculationHeaderLedgerAccountRepository;

        public CalculationHeaderLedgerAccountService(IUnitOfWork unitOfWork, IRepository<CalculationHeaderLedgerAccount> HeaderLedgerAcc)
        {
            _unitOfWork = unitOfWork;
            _CalculationHeaderLedgerAccountRepository = HeaderLedgerAcc;
        }
        public CalculationHeaderLedgerAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CalculationHeaderLedgerAccountRepository = unitOfWork.Repository<CalculationHeaderLedgerAccount>();
        }

        public CalculationHeaderLedgerAccount Find(int id)
        {
            return _CalculationHeaderLedgerAccountRepository.Find(id);
        }
        public CalculationHeaderLedgerAccountViewModel GetCalculationHeaderLedgerAccount(int id, int DocTypeId)
        {
            return (from p in _CalculationHeaderLedgerAccountRepository.Instance
                    where p.CalculationId == id && p.DocTypeId == DocTypeId
                    select new CalculationHeaderLedgerAccountViewModel
                    {
                        CalculationId = p.CalculationId,
                        DocTypeId = p.DocTypeId,
                        CalculationName = p.Calculation.CalculationName,
                        DocTypeName = p.DocType.DocumentTypeName,
                    }
                        ).Union(
                        from p in _unitOfWork.Repository<CalculationLineLedgerAccount>().Instance
                        where p.CalculationId == id && p.DocTypeId == DocTypeId
                        select new CalculationHeaderLedgerAccountViewModel
                        {
                            CalculationId = p.CalculationId,
                            DocTypeId = p.DocTypeId,
                            CalculationName = p.Calculation.CalculationName,
                            DocTypeName = p.DocType.DocumentTypeName,
                        }
                        ).FirstOrDefault();
        }

        public CalculationHeaderLedgerAccountViewModel GetCalculationHeaderLedgerAccount(int id)
        {
            return (from p in _CalculationHeaderLedgerAccountRepository.Instance
                    where p.CalculationHeaderLedgerAccountId == id
                    select new CalculationHeaderLedgerAccountViewModel
                    {
                        CalculationFooterId = p.CalculationFooterId,
                        CalculationFooterName = p.CalculationFooter.Charge.ChargeName,
                        CalculationHeaderLedgerAccountId = p.CalculationHeaderLedgerAccountId,
                        CalculationId = p.CalculationId,
                        CalculationName = p.Calculation.CalculationName,
                        CostCenterId = p.CostCenterId,
                        CostCenterName = p.CostCenter.CostCenterName,
                        DocTypeId = p.DocTypeId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        LedgerAccountCrId = p.LedgerAccountCrId,
                        LedgerAccountCrName = p.LedgerAccountCr.LedgerAccountName,
                        LedgerAccountDrId = p.LedgerAccountDrId,
                        LedgerAccountDrName = p.LedgerAccountDr.LedgerAccountName,
                        ContraLedgerAccountId = p.ContraLedgerAccountId,
                        ContraLedgerAccountName = p.ContraLedgerAccount.LedgerAccountName,
                    }
                        ).FirstOrDefault();
        }

        public IEnumerable<CalculationHeaderLedgerAccountViewModel> GetCalculationListForIndex(int id, int DocTypeId)
        {

            List<CalculationHeaderLedgerAccountViewModel> Records = new List<CalculationHeaderLedgerAccountViewModel>();

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];


            Records = (from p in _CalculationHeaderLedgerAccountRepository.Instance
                       where p.CalculationId == id && p.DocTypeId == DocTypeId && p.SiteId == SiteId && p.DivisionId == DivisionId
                       orderby p.CalculationHeaderLedgerAccountId
                       select new CalculationHeaderLedgerAccountViewModel
                       {
                           CalculationFooterId = p.CalculationFooterId,
                           CalculationFooterName = p.CalculationFooter.Charge.ChargeName,
                           CalculationHeaderLedgerAccountId = p.CalculationHeaderLedgerAccountId,
                           CalculationId = p.CalculationId,
                           CalculationName = p.Calculation.CalculationName,
                           CostCenterId = p.CostCenterId,
                           CostCenterName = p.CostCenter.CostCenterName,
                           DocTypeId = p.DocTypeId,
                           DocTypeName = p.DocType.DocumentTypeName,
                           LedgerAccountCrId = p.LedgerAccountCrId,
                           LedgerAccountCrName = p.LedgerAccountCr.LedgerAccountName,
                           LedgerAccountDrId = p.LedgerAccountDrId,
                           LedgerAccountDrName = p.LedgerAccountDr.LedgerAccountName,
                           ContraLedgerAccountName = p.ContraLedgerAccount.LedgerAccountName,
                           ContraLedgerAccountId = p.ContraLedgerAccountId,
                       }).ToList();

            var PendingRecords = (from p in _unitOfWork.Repository<CalculationFooter>().Instance
                                  join t in _CalculationHeaderLedgerAccountRepository.Query().Get().Where(m => m.DocTypeId == DocTypeId) on p.CalculationFooterLineId equals t.CalculationFooterId into table
                                  from tab in table.DefaultIfEmpty()
                                  where tab == null && p.CalculationId == id
                                  select new CalculationHeaderLedgerAccountViewModel
                                  {
                                      CalculationFooterId = p.CalculationFooterLineId,
                                      CalculationFooterName = p.Charge.ChargeName,
                                      CalculationId = p.CalculationId,
                                      CalculationName = p.Calculation.CalculationName,
                                      CostCenterId = p.CostCenterId,
                                      CostCenterName = p.CostCenter.CostCenterName,
                                      DocTypeId = DocTypeId,
                                  });

            foreach (var item in PendingRecords)
                Records.Add(item);

            return (Records);

        }

        public CalculationHeaderLedgerAccount Create(CalculationHeaderLedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Added;
            _CalculationHeaderLedgerAccountRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _CalculationHeaderLedgerAccountRepository.Delete(id);
        }

        public void Delete(CalculationHeaderLedgerAccount pt)
        {
            _CalculationHeaderLedgerAccountRepository.Delete(pt);
        }

        public void Update(CalculationHeaderLedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _CalculationHeaderLedgerAccountRepository.Update(pt);
        }

        public IEnumerable<CalculationHeaderLedgerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _CalculationHeaderLedgerAccountRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CalculationHeaderLedgerAccountId))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<CalculationHeaderLedgerAccountViewModel> GetCalculationHeaderLedgerAccountList(int CalculationID)
        {
            return (from p in _CalculationHeaderLedgerAccountRepository.Instance
                    where p.CalculationId == CalculationID
                    orderby p.CalculationHeaderLedgerAccountId
                    select new CalculationHeaderLedgerAccountViewModel
                    {
                        CalculationFooterId = p.CalculationFooterId,
                        CalculationFooterName = p.CalculationFooter.Charge.ChargeName,
                        CalculationHeaderLedgerAccountId = p.CalculationHeaderLedgerAccountId,
                        CalculationId = p.CalculationId,
                        CalculationName = p.Calculation.CalculationName,
                        CostCenterId = p.CostCenterId,
                        CostCenterName = p.CostCenter.CostCenterName,
                        DocTypeId = p.DocTypeId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        LedgerAccountCrId = p.LedgerAccountCrId,
                        LedgerAccountCrName = p.LedgerAccountCr.LedgerAccountName,
                        LedgerAccountDrId = p.LedgerAccountDrId,
                        LedgerAccountDrName = p.LedgerAccountDr.LedgerAccountName,
                        ContraLedgerAccountName = p.ContraLedgerAccount.LedgerAccountName,
                        ContraLedgerAccountId = p.ContraLedgerAccountId,
                    }
                        );
        }

        public CalculationHeaderLedgerAccount Add(CalculationHeaderLedgerAccount pt)
        {
            _unitOfWork.Repository<CalculationHeaderLedgerAccount>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _CalculationHeaderLedgerAccountRepository.Instance
                        orderby p.CalculationHeaderLedgerAccountId
                        select p.CalculationHeaderLedgerAccountId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _CalculationHeaderLedgerAccountRepository.Instance
                        orderby p.CalculationHeaderLedgerAccountId
                        select p.CalculationHeaderLedgerAccountId).FirstOrDefault();
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
                temp = (from p in _CalculationHeaderLedgerAccountRepository.Instance
                        orderby p.CalculationHeaderLedgerAccountId
                        select p.CalculationHeaderLedgerAccountId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _CalculationHeaderLedgerAccountRepository.Instance
                        orderby p.CalculationHeaderLedgerAccountId
                        select p.CalculationHeaderLedgerAccountId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IQueryable<CalculationHeaderLedgerAccountViewModel> GetHeaderIndex()
        {

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var Header = (from p in _CalculationHeaderLedgerAccountRepository.Instance
                          where p.SiteId == SiteId && p.DivisionId == DivisionId
                          group p by new { p.DocTypeId, p.CalculationId } into g
                          select new CalculationHeaderLedgerAccountViewModel
                          {
                              DocTypeId = g.Key.DocTypeId,
                              CalculationId = g.Key.CalculationId,
                              DocTypeName = g.Max(i => i.DocType.DocumentTypeName),
                              CalculationName = g.Max(i => i.Calculation.CalculationName),
                          });

            var Line = (from p in _unitOfWork.Repository<CalculationLineLedgerAccount>().Instance
                        where p.SiteId == SiteId && p.DivisionId == DivisionId
                        group p by new { p.DocTypeId, p.CalculationId } into g
                        select new CalculationHeaderLedgerAccountViewModel
                        {
                            DocTypeId = g.Key.DocTypeId,
                            CalculationId = g.Key.CalculationId,
                            DocTypeName = g.Max(i => i.DocType.DocumentTypeName),
                            CalculationName = g.Max(i => i.Calculation.CalculationName),
                        });

            return (Header.Union(Line).OrderBy(m => m.CalculationId));
        }

        public IEnumerable<ComboBoxList> GetProductFooters(int id, string term)
        {

            return (from p in _unitOfWork.Repository<CalculationFooter>().Instance
                    join t in _unitOfWork.Repository<Charge>().Instance on p.ChargeId equals t.ChargeId
                    where p.CalculationId == id && string.IsNullOrEmpty(term) ? 1 == 1 : t.ChargeName.ToLower().Contains(term.ToLower())
                    select new ComboBoxList
                    {
                        Id = p.CalculationFooterLineId,
                        PropFirst = t.ChargeName,
                    }
                        ).Take(25);

        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<CalculationHeaderLedgerAccount>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CalculationHeaderLedgerAccount> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
