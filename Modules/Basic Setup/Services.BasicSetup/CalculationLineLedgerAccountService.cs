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
    public interface ICalculationLineLedgerAccountService : IDisposable
    {
        CalculationLineLedgerAccount Create(CalculationLineLedgerAccount pt);
        void Delete(int id);
        void Delete(CalculationLineLedgerAccount pt);
        CalculationLineLedgerAccount Find(int id);
        IEnumerable<CalculationLineLedgerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(CalculationLineLedgerAccount pt);
        CalculationLineLedgerAccount Add(CalculationLineLedgerAccount pt);
        IEnumerable<CalculationLineLedgerAccount> GetCalculationLineLedgerAccountList();
        IEnumerable<CalculationLineLedgerAccountViewModel> GetCalculationLineLedgerAccountList(int CalculationID);
        Task<IEquatable<CalculationLineLedgerAccount>> GetAsync();
        Task<CalculationLineLedgerAccount> FindAsync(int id);
        IEnumerable<CalculationLineLedgerAccountViewModel> GetCalculationListForIndex(int id,int DocTypeId);//CalculationId
        CalculationLineLedgerAccountViewModel GetCalculationLineLedgerAccount(int id);//CalculationLineLedgerAccountId
        IEnumerable<ComboBoxList> GetCalculationProducts(int id, string term);
        int NextId(int id);
        int PrevId(int id);
    }

    public class CalculationLineLedgerAccountService : ICalculationLineLedgerAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CalculationLineLedgerAccount> _CalculationLineLedgerAccountRepository;

        public CalculationLineLedgerAccountService(IUnitOfWork unitOfWork, IRepository<CalculationLineLedgerAccount> LineLedgerAccRepo)
        {
            _unitOfWork = unitOfWork;
            _CalculationLineLedgerAccountRepository = LineLedgerAccRepo;
        }
        public CalculationLineLedgerAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CalculationLineLedgerAccountRepository = unitOfWork.Repository<CalculationLineLedgerAccount>();
        }

        public CalculationLineLedgerAccount Find(int id)
        {
            return _unitOfWork.Repository<CalculationLineLedgerAccount>().Find(id);
        }
        public CalculationLineLedgerAccountViewModel GetCalculationLineLedgerAccount(int id)
        {
            return (from p in _CalculationLineLedgerAccountRepository.Instance
                    where p.CalculationLineLedgerAccountId == id
                    select new CalculationLineLedgerAccountViewModel
                    {
                       CalculationId=p.CalculationId,
                       CalculationLineLedgerAccountId=p.CalculationLineLedgerAccountId,
                       CalculationName=p.Calculation.CalculationName,
                       CalculationProductId=p.CalculationProductId,
                       CalculationProductName=p.CalculationProduct.Charge.ChargeName,
                       CostCenterId=p.CostCenterId,
                       CostCenterName=p.CostCenter.CostCenterName,
                       DocTypeId=p.DocTypeId,
                       DocTypeName=p.DocType.DocumentTypeName,
                       LedgerAccountCrId=p.LedgerAccountCrId,
                       LedgerAccountCrName=p.LedgerAccountCr.LedgerAccountName,
                       LedgerAccountDrId=p.LedgerAccountDrId,
                       LedgerAccountDrName=p.LedgerAccountDr.LedgerAccountName,
                       ContraLedgerAccountName = p.ContraLedgerAccount.LedgerAccountName,
                       ContraLedgerAccountId = p.ContraLedgerAccountId,
                    }


                        ).FirstOrDefault();


        }

        public IEnumerable<CalculationLineLedgerAccountViewModel> GetCalculationListForIndex(int id, int DocTypeId)
        {
            List<CalculationLineLedgerAccountViewModel> Records=new List<CalculationLineLedgerAccountViewModel>();

            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            Records = (from p in _CalculationLineLedgerAccountRepository.Instance
                    where p.CalculationId == id && p.DocTypeId==DocTypeId && p.SiteId==SiteId && p.DivisionId==DivisionId
                    orderby p.CalculationLineLedgerAccountId
                    select new CalculationLineLedgerAccountViewModel
                    {
                        CalculationId = p.CalculationId,
                        CalculationLineLedgerAccountId = p.CalculationLineLedgerAccountId,
                        CalculationName = p.Calculation.CalculationName,
                        CalculationProductId = p.CalculationProductId,
                        CalculationProductName = p.CalculationProduct.Charge.ChargeName,
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

            var PendingRecords = (from p in _unitOfWork.Repository<CalculationProduct>().Instance
                                  join t in _CalculationLineLedgerAccountRepository.Query().Get().Where(m => m.DocTypeId == DocTypeId) on p.CalculationProductId equals t.CalculationProductId into table
                                  from tab in table.DefaultIfEmpty()
                                  where tab == null && p.CalculationId == id
                                  select new CalculationLineLedgerAccountViewModel
                                  {
                                      CalculationProductId = p.CalculationProductId,
                                      CalculationProductName = p.Charge.ChargeName,
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

        public CalculationLineLedgerAccount Create(CalculationLineLedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Added;
            _CalculationLineLedgerAccountRepository.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _CalculationLineLedgerAccountRepository.Delete(id);
        }

        public void Delete(CalculationLineLedgerAccount pt)
        {
            _CalculationLineLedgerAccountRepository.Delete(pt);
        }

        public void Update(CalculationLineLedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _CalculationLineLedgerAccountRepository.Update(pt);
        }

        public IEnumerable<CalculationLineLedgerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _CalculationLineLedgerAccountRepository
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CalculationLineLedgerAccountId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<CalculationLineLedgerAccount> GetCalculationLineLedgerAccountList()
        {
            var pt = _CalculationLineLedgerAccountRepository.Query().Get().OrderBy(m => m.CalculationLineLedgerAccountId);
            return pt;
        }
        public IEnumerable<CalculationLineLedgerAccountViewModel> GetCalculationLineLedgerAccountList(int CalculationID)
        {
            return (from p in _CalculationLineLedgerAccountRepository.Instance
                    where p.CalculationId == CalculationID
                    orderby p.CalculationLineLedgerAccountId
                    select new CalculationLineLedgerAccountViewModel
                    {
                        CalculationId = p.CalculationId,
                        CalculationLineLedgerAccountId = p.CalculationLineLedgerAccountId,
                        CalculationName = p.Calculation.CalculationName,
                        CalculationProductId = p.CalculationProductId,
                        CalculationProductName = p.CalculationProduct.Charge.ChargeName,
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

        public CalculationLineLedgerAccount Add(CalculationLineLedgerAccount pt)
        {
            _CalculationLineLedgerAccountRepository.Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _CalculationLineLedgerAccountRepository.Instance
                        orderby p.CalculationLineLedgerAccountId
                        select p.CalculationLineLedgerAccountId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _CalculationLineLedgerAccountRepository.Instance
                        orderby p.CalculationLineLedgerAccountId
                        select p.CalculationLineLedgerAccountId).FirstOrDefault();
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
                temp = (from p in _CalculationLineLedgerAccountRepository.Instance
                        orderby p.CalculationLineLedgerAccountId
                        select p.CalculationLineLedgerAccountId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _CalculationLineLedgerAccountRepository.Instance
                        orderby p.CalculationLineLedgerAccountId
                        select p.CalculationLineLedgerAccountId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public IEnumerable<ComboBoxList> GetCalculationProducts(int id, string term)
        {

            return (from p in _unitOfWork.Repository<CalculationProduct>().Instance
                    join t in _unitOfWork.Repository<Charge>().Instance on p.ChargeId equals t.ChargeId
                    where p.CalculationId == id && string.IsNullOrEmpty(term) ? 1 == 1 : t.ChargeName.ToLower().Contains(term.ToLower())
                    select new ComboBoxList
                    {
                        Id = p.CalculationProductId,
                        PropFirst = t.ChargeName,
                    }
                      ).Take(25);

        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<CalculationLineLedgerAccount>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CalculationLineLedgerAccount> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

    }
}
