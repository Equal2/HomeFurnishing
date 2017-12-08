using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.SqlServer;
using Models.BasicSetup.Models;
using Models.BasicSetup.ViewModels;
using Infrastructure.IO;
using ProjLib.Constants;

namespace Services.BasicSetup
{
    public interface IStockHeaderService : IDisposable
    {
        StockHeader Create(StockHeader s);
        void Delete(int id);
        void Delete(StockHeader s);
        StockHeader Find(int id);
        void Update(StockHeader s);
        string GetMaxDocNo();
        StockHeader FindByDocNo(string Docno);
        StockHeader FindByDocHeader(int? DocHeaderId, int? StockHeaderId, int DocTypeId, int SiteId, int DivisionId);
        IQueryable<StockHeaderViewModel> GetStockHeaderList(int DocTypeId, string UName);
        IQueryable<StockHeaderViewModel> GetStockHeaderListPendingToSubmit(int DocTypeId, string UName);
        IQueryable<StockHeaderViewModel> GetStockHeaderListPendingToReview(int DocTypeId, string UName);
        StockHeaderViewModel GetStockHeader(int id);

        void UpdateStockHeader(StockHeaderViewModel S);
    }
    public class StockHeaderService : IStockHeaderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<StockHeader> _stockHeaderRepository;

        public StockHeaderService(IUnitOfWork unit,IRepository<StockHeader> stockheaderRepo)
        {
            _unitOfWork = unit;
            _stockHeaderRepository = stockheaderRepo;
        }
        public StockHeaderService(IUnitOfWork unit)
        {
            _unitOfWork = unit;
            _stockHeaderRepository = unit.Repository<StockHeader>();
        }

        public StockHeader Create(StockHeader s)
        {
            s.ObjectState = ObjectState.Added;
            _stockHeaderRepository.Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _stockHeaderRepository.Delete(id);
        }

        public void Delete(StockHeader s)
        {
            _stockHeaderRepository.Delete(s);
        }
        public void Update(StockHeader s)
        {
            s.ObjectState = ObjectState.Modified;
            _stockHeaderRepository.Update(s);
        }
        public StockHeader Find(int id)
        {
            return _stockHeaderRepository.Find(id);
        }


        public StockHeader FindByDocNo(string Docno)
        {
            return _stockHeaderRepository.Query().Get().Where(m => m.DocNo == Docno).FirstOrDefault();

        }

        public StockHeader FindByDocHeader(int? DocHeaderId, int? StockHeaderId, int DocTypeId, int SiteId, int DivisionId)
        {
            if (DocHeaderId != null)
                return _stockHeaderRepository.Query().Get().Where(m => m.DocHeaderId == DocHeaderId && m.DocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault();
            else
                return _stockHeaderRepository.Query().Get().Where(m => m.StockHeaderId == StockHeaderId && m.DocTypeId == DocTypeId && m.SiteId == SiteId && m.DivisionId == DivisionId).FirstOrDefault();

        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _stockHeaderRepository.Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public IQueryable<StockHeaderViewModel> GetStockHeaderList(int DocTypeId, string UName)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            return (from p in _stockHeaderRepository.Instance
                    orderby p.DocDate descending, p.DocNo descending
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && p.DocTypeId == DocTypeId
                    select new StockHeaderViewModel
                    {
                        CurrencyName = p.Currency.Name,
                        DivisionName = p.Division.DivisionName,
                        DocDate = p.DocDate,
                        DocHeaderId = p.DocHeaderId,
                        //MachineName = p.Machine.MachineName,
                        CostCenterName = p.CostCenter.CostCenterName,
                        DocNo = p.DocNo,
                        DocTypeId = p.DocTypeId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        FromGodownName = p.FromGodown.GodownName,
                        GodownName = p.Godown.GodownName,
                        PersonName = p.Person.Name,
                        ProcessName = p.Process.ProcessName,
                        Remark = p.Remark,
                        Status = p.Status,
                        StockHeaderId = p.StockHeaderId,
                        ModifiedBy = p.ModifiedBy,
                        GatePassDocNo = p.GatePassHeader.DocNo,
                        GatePassHeaderId = p.GatePassHeaderId,
                        GatePassDocDate = p.GatePassHeader.DocDate,
                        GatePassStatus = (p.GatePassHeaderId != null ? p.GatePassHeader.Status : 0),
                        ReviewCount = p.ReviewCount,
                        ReviewBy = p.ReviewBy,
                        Reviewed = (SqlFunctions.CharIndex(UName, p.ReviewBy) > 0),
                    }
                        );

        }

        public IQueryable<StockHeaderViewModel> GetStockHeaderListPendingToSubmit(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var StockHeader = GetStockHeaderList(id, Uname).AsQueryable();

            var PendingToSubmit = from p in StockHeader
                                  where p.Status == (int)StatusConstants.Drafted || p.Status == (int)StatusConstants.Import || p.Status == (int)StatusConstants.Modified && (p.ModifiedBy == Uname || UserRoles.Contains("Admin"))
                                  select p;
            return PendingToSubmit;

        }

        public IQueryable<StockHeaderViewModel> GetStockHeaderListPendingToReview(int id, string Uname)
        {

            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            var StockHeader = GetStockHeaderList(id, Uname).AsQueryable();

            var PendingToReview = from p in StockHeader
                                  where p.Status == (int)StatusConstants.Submitted && (SqlFunctions.CharIndex(Uname, (p.ReviewBy ?? "")) == 0)
                                  select p;
            return PendingToReview;

        }

        public StockHeaderViewModel GetStockHeader(int id)
        {

            return (from p in _stockHeaderRepository.Instance
                    where p.StockHeaderId == id
                    select new StockHeaderViewModel
                    {
                        CurrencyId = p.CurrencyId,
                        DocDate = p.DocDate,
                        DocHeaderId = p.DocHeaderId,
                        DocNo = p.DocNo,
                        DocTypeId = p.DocTypeId,
                        MachineId = p.MachineId,
                        CostCenterId = p.CostCenterId,
                        FromGodownId = p.FromGodownId,
                        SiteId = p.SiteId,
                        DivisionId = p.DivisionId,
                        GodownId = p.GodownId,
                        PersonId = p.PersonId,
                        ProcessId = p.ProcessId,
                        Remark = p.Remark,
                        Status = p.Status,
                        StockHeaderId = p.StockHeaderId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        GatePassHeaderId = p.GatePassHeaderId,
                        GatePassDocNo = p.GatePassHeader.DocNo,
                        GatePassStatus = (p.GatePassHeader == null ? 0 : p.GatePassHeader.Status),
                        GatePassDocDate = p.GatePassHeader.DocDate,
                        ModifiedBy = p.ModifiedBy,
                        CreatedDate = p.CreatedDate,
                        LockReason = p.LockReason,
                    }
                        ).FirstOrDefault();

        }

        public void UpdateStockHeader(StockHeaderViewModel S)
        {
            StockHeader StockHeader = Find(S.StockHeaderId);

            StockHeader.DocTypeId = S.DocTypeId;
            StockHeader.DocDate = S.DocDate;
            StockHeader.DocNo = S.DocNo;
            StockHeader.DivisionId = S.DivisionId;
            StockHeader.SiteId = S.SiteId;
            StockHeader.CurrencyId = S.CurrencyId;
            StockHeader.PersonId = S.PersonId;
            StockHeader.ProcessId = S.ProcessId;
            StockHeader.FromGodownId = S.FromGodownId;
            StockHeader.GodownId = S.GodownId;
            StockHeader.Remark = S.Remark;
            StockHeader.Status = S.Status;
            StockHeader.ModifiedBy = S.ModifiedBy;
            StockHeader.ModifiedDate = S.ModifiedDate;
            StockHeader.CostCenterId = S.CostCenterId;
            StockHeader.MachineId = S.MachineId;

            Update(StockHeader);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}