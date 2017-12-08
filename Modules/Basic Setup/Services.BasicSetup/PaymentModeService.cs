using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Threading.Tasks;
using Models.Company.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;
using Models.BasicSetup.Models;

namespace Services.BasicSetup
{
    public interface IPaymentModeService : IDisposable
    {
        PaymentMode Create(PaymentMode pt);
        void Delete(int id);
        void Delete(PaymentMode pt);
        PaymentMode Find(string Name);
        PaymentMode Find(int id);
        IEnumerable<PaymentMode> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PaymentMode pt);
        PaymentMode Add(PaymentMode pt);
        IEnumerable<PaymentMode> GetPaymentModeList();
        IQueryable<PaymentModeViewModel> GetPaymentModeListForIndex();
        PaymentModeViewModel GetPaymentModeListForEdit(int id);
        Task<IEquatable<PaymentMode>> GetAsync();
        Task<PaymentMode> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        bool CheckForNameExists(string Name);
        bool CheckForNameExists(string Name, int Id);

        #region HelpList Getter
        /// <summary>
        /// *General Function*
        /// This function will create the help list for Projects
        /// </summary>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum);
        #endregion

        #region HelpList Setters
        /// <summary>
        /// *General Function*
        /// This function will return the object in (Id,Text) format based on the Id
        /// </summary>
        /// <param name="Id">Primarykey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult GetValue(int Id);

        /// <summary>
        /// *General Function*
        /// This function will return list of object in (Id,Text) format based on the Ids
        /// </summary>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>List<ComboBoxResult></returns>
        List<ComboBoxResult> GetListCsv(string Id);
        #endregion
    }

    public class PaymentModeService : IPaymentModeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PaymentMode> _PaymentModeRepository;
        public PaymentModeService(IUnitOfWork unitOfWork, IRepository<PaymentMode> PaymentModeRepo)
        {
            _unitOfWork = unitOfWork;
            _PaymentModeRepository = PaymentModeRepo;
        }
        public PaymentModeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _PaymentModeRepository = unitOfWork.Repository<PaymentMode>();
        }

        public PaymentMode Find(string Name)
        {
            return _PaymentModeRepository.Query().Get().Where(i => i.PaymentModeName == Name).FirstOrDefault();
        }


        public PaymentMode Find(int id)
        {
            return _unitOfWork.Repository<PaymentMode>().Find(id);
        }

        public PaymentMode Create(PaymentMode pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PaymentMode>().Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PaymentMode>().Delete(id);
        }

        public void Delete(PaymentMode pt)
        {
            _unitOfWork.Repository<PaymentMode>().Delete(pt);
        }

        public void Update(PaymentMode pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PaymentMode>().Update(pt);
        }

        public IEnumerable<PaymentMode> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PaymentMode>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.PaymentModeName))                
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PaymentMode> GetPaymentModeList()
        {
            var pt = _unitOfWork.Repository<PaymentMode>().Query().Get().OrderBy(m => m.PaymentModeName);

            return pt;
        }

        public IQueryable<PaymentModeViewModel> GetPaymentModeListForIndex()
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var PaymentModeLedgerAccounts = from P in _unitOfWork.Repository<PaymentModeLedgerAccount>().Instance
                                             where P.SiteId == SiteId && P.DivisionId == DivisionId
                                             select P;

            var pt = from P in _unitOfWork.Repository<PaymentMode>().Instance
                     join Pl in PaymentModeLedgerAccounts on P.PaymentModeId equals Pl.PaymentModeId into PaymentModeLedgerAccountsTable
                     from PaymentModeLedgerAccountsTab in PaymentModeLedgerAccountsTable.DefaultIfEmpty()
                     join L in _unitOfWork.Repository<LedgerAccount>().Instance on PaymentModeLedgerAccountsTab.LedgerAccountId equals L.LedgerAccountId into LedgerAccountTable from LedgerAccountTab in LedgerAccountTable.DefaultIfEmpty()
                     orderby P.PaymentModeName
                     select new PaymentModeViewModel
                     {
                         PaymentModeId = P.PaymentModeId,
                         PaymentModeName = P.PaymentModeName,
                         PaymentModeLedgerAccountId = PaymentModeLedgerAccountsTab.PaymentModeLedgerAccountId,
                         LedgerAccountId = PaymentModeLedgerAccountsTab.LedgerAccountId,
                         LedgerAccountName = LedgerAccountTab.LedgerAccountName,
                         SiteId = PaymentModeLedgerAccountsTab.SiteId,
                         DivisionId = PaymentModeLedgerAccountsTab.DivisionId,
                     };

            return pt;
        }


        public PaymentModeViewModel GetPaymentModeListForEdit(int PaymentModeId)
        {
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var PaymentModeLedgerAccounts = from P in _unitOfWork.Repository<PaymentModeLedgerAccount>().Instance
                                             where P.SiteId == SiteId && P.DivisionId == DivisionId && P.PaymentModeId == PaymentModeId
                                             select P;

            var pt = (from P in _unitOfWork.Repository<PaymentMode>().Instance
                      join Pl in PaymentModeLedgerAccounts on P.PaymentModeId equals Pl.PaymentModeId into PaymentModeLedgerAccountsTable
                     from PaymentModeLedgerAccountsTab in PaymentModeLedgerAccountsTable.DefaultIfEmpty()
                     where P.PaymentModeId == PaymentModeId
                     select new PaymentModeViewModel
                     {
                         PaymentModeId = P.PaymentModeId,
                         PaymentModeName = P.PaymentModeName,
                         PaymentModeLedgerAccountId = PaymentModeLedgerAccountsTab.PaymentModeLedgerAccountId,
                         LedgerAccountId = PaymentModeLedgerAccountsTab.LedgerAccountId,
                         SiteId = PaymentModeLedgerAccountsTab.SiteId,
                         DivisionId = PaymentModeLedgerAccountsTab.DivisionId,
                     }).FirstOrDefault();

            return pt;
        }

        public PaymentMode Add(PaymentMode pt)
        {
            _unitOfWork.Repository<PaymentMode>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _PaymentModeRepository.Instance
                        orderby p.PaymentModeName
                        select p.PaymentModeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _PaymentModeRepository.Instance
                        orderby p.PaymentModeName
                        select p.PaymentModeId).FirstOrDefault();
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

                temp = (from p in _PaymentModeRepository.Instance
                        orderby p.PaymentModeName
                        select p.PaymentModeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _PaymentModeRepository.Instance
                        orderby p.PaymentModeName
                        select p.PaymentModeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public ComboBoxPagedResult GetList(string searchTerm, int pageSize, int pageNum)
        {
            var list = (from pr in _PaymentModeRepository.Instance
                        where (string.IsNullOrEmpty(searchTerm) ? 1 == 1 : (pr.PaymentModeName.ToLower().Contains(searchTerm.ToLower())))
                        orderby pr.PaymentModeName
                        select new ComboBoxResult
                        {
                            text = pr.PaymentModeName,
                            id = pr.PaymentModeId.ToString()
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

            IEnumerable<PaymentMode> PaymentModes = from pr in _PaymentModeRepository.Instance
                                            where pr.PaymentModeId == Id
                                            select pr;

            ProductJson.id = PaymentModes.FirstOrDefault().PaymentModeId.ToString();
            ProductJson.text = PaymentModes.FirstOrDefault().PaymentModeName;

            return ProductJson;
        }

        public List<ComboBoxResult> GetListCsv(string Ids)
        {
            string[] subStr = Ids.Split(',');
            List<ComboBoxResult> ProductJson = new List<ComboBoxResult>();
            for (int i = 0; i < subStr.Length; i++)
            {
                int temp = Convert.ToInt32(subStr[i]);
                IEnumerable<PaymentMode> PaymentModes = from pr in _PaymentModeRepository.Instance
                                                where pr.PaymentModeId == temp
                                                select pr;
                ProductJson.Add(new ComboBoxResult()
                {
                    id = PaymentModes.FirstOrDefault().PaymentModeId.ToString(),
                    text = PaymentModes.FirstOrDefault().PaymentModeName
                });
            }
            return ProductJson;
        }
        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<PaymentMode>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PaymentMode> FindAsync(int id)
        {
            throw new NotImplementedException();
        }

        public bool CheckForNameExists(string Name)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _PaymentModeRepository.Instance
                        where pr.PaymentModeName == Name 
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;

        }
        public bool CheckForNameExists(string Name, int Id)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            var temp = (from pr in _PaymentModeRepository.Instance
                        where pr.PaymentModeName == Name && pr.PaymentModeId != Id 
                        select pr).FirstOrDefault();
            if (temp == null)
                return false;
            else
                return true;
        }
    }
}
