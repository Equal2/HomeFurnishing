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
using Model.ViewModel;
using Model.ViewModels;

namespace Service
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
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<CalculationLineLedgerAccount> _CalculationLineLedgerAccountRepository;
        RepositoryQuery<CalculationLineLedgerAccount> CalculationLineLedgerAccountRepository;

        public CalculationLineLedgerAccountService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _CalculationLineLedgerAccountRepository = new Repository<CalculationLineLedgerAccount>(db);
            CalculationLineLedgerAccountRepository = new RepositoryQuery<CalculationLineLedgerAccount>(_CalculationLineLedgerAccountRepository);
        }


        public CalculationLineLedgerAccount Find(int id)
        {
            return _unitOfWork.Repository<CalculationLineLedgerAccount>().Find(id);
        }
        public CalculationLineLedgerAccountViewModel GetCalculationLineLedgerAccount(int id)
        {
            return (from p in db.CalculationLineLedgerAccount
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

            Records=(from p in db.CalculationLineLedgerAccount
                     join l in db.CalculationProduct on p.CalculationProductId equals l.CalculationProductId into CalculationProductTable from CalculationProductTab in CalculationProductTable.DefaultIfEmpty()
                    where p.CalculationId == id && p.DocTypeId==DocTypeId && p.SiteId==SiteId && p.DivisionId==DivisionId
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
                        Sr = CalculationProductTab.Sr
                    }).ToList();

            var PendingRecords = (from p in db.CalculationProduct
                                  join t in db.CalculationLineLedgerAccount.Where(m => m.DocTypeId == DocTypeId) on p.CalculationProductId equals t.CalculationProductId into table
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
                                      Sr = p.Sr
                                  });

            foreach (var item in PendingRecords)
                Records.Add(item);

            return (Records.OrderBy(m => m.Sr));
        }

        public CalculationLineLedgerAccount Create(CalculationLineLedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<CalculationLineLedgerAccount>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<CalculationLineLedgerAccount>().Delete(id);
        }

        public void Delete(CalculationLineLedgerAccount pt)
        {
            _unitOfWork.Repository<CalculationLineLedgerAccount>().Delete(pt);
        }

        public void Update(CalculationLineLedgerAccount pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<CalculationLineLedgerAccount>().Update(pt);
        }

        public IEnumerable<CalculationLineLedgerAccount> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<CalculationLineLedgerAccount>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.CalculationLineLedgerAccountId))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<CalculationLineLedgerAccount> GetCalculationLineLedgerAccountList()
        {
            var pt = _unitOfWork.Repository<CalculationLineLedgerAccount>().Query().Get().OrderBy(m=>m.CalculationLineLedgerAccountId);
            return pt;
        }
        public IEnumerable<CalculationLineLedgerAccountViewModel> GetCalculationLineLedgerAccountList(int CalculationID)
        {
            return (from p in db.CalculationLineLedgerAccount                    
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
            _unitOfWork.Repository<CalculationLineLedgerAccount>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.CalculationLineLedgerAccount
                        orderby p.CalculationLineLedgerAccountId
                        select p.CalculationLineLedgerAccountId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.CalculationLineLedgerAccount
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
                temp = (from p in db.CalculationLineLedgerAccount
                        orderby p.CalculationLineLedgerAccountId
                        select p.CalculationLineLedgerAccountId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.CalculationLineLedgerAccount
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

            return (from p in db.CalculationProduct
                    join t in db.Charge on p.ChargeId equals t.ChargeId
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
