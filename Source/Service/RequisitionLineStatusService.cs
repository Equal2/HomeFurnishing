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

namespace Service
{
    public interface IRequisitionLineStatusService : IDisposable
    {
        RequisitionLineStatus Create(RequisitionLineStatus pt);
        void Delete(int id);
        void Delete(RequisitionLineStatus pt);
        RequisitionLineStatus Find(int id);
        IEnumerable<RequisitionLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(RequisitionLineStatus pt);
        RequisitionLineStatus Add(RequisitionLineStatus pt);
        IEnumerable<RequisitionLineStatus> GetRequisitionLineStatusList();

        // IEnumerable<RequisitionLineStatus> GetRequisitionLineStatusList(int buyerId);
        Task<IEquatable<RequisitionLineStatus>> GetAsync();
        Task<RequisitionLineStatus> FindAsync(int id);
        void CreateLineStatus(int id, ref ApplicationDbContext Context);
    }

    public class RequisitionLineStatusService : IRequisitionLineStatusService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<RequisitionLineStatus> _RequisitionLineStatusRepository;
        RepositoryQuery<RequisitionLineStatus> RequisitionLineStatusRepository;
        public RequisitionLineStatusService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _RequisitionLineStatusRepository = new Repository<RequisitionLineStatus>(db);
            RequisitionLineStatusRepository = new RepositoryQuery<RequisitionLineStatus>(_RequisitionLineStatusRepository);
        }


        public RequisitionLineStatus Find(int id)
        {
            return _unitOfWork.Repository<RequisitionLineStatus>().Find(id);
        }

        public RequisitionLineStatus Create(RequisitionLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<RequisitionLineStatus>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<RequisitionLineStatus>().Delete(id);
        }

        public void Delete(RequisitionLineStatus pt)
        {
            _unitOfWork.Repository<RequisitionLineStatus>().Delete(pt);
        }

        public void Update(RequisitionLineStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<RequisitionLineStatus>().Update(pt);
        }

        public IEnumerable<RequisitionLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<RequisitionLineStatus>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<RequisitionLineStatus> GetRequisitionLineStatusList()
        {
            var pt = _unitOfWork.Repository<RequisitionLineStatus>().Query().Get();

            return pt;
        }

        public RequisitionLineStatus Add(RequisitionLineStatus pt)
        {
            _unitOfWork.Repository<RequisitionLineStatus>().Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id,ref ApplicationDbContext Context)
        {
            RequisitionLineStatus Stat = new RequisitionLineStatus();
            Stat.RequisitionLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            Context.RequisitionLineStatus.Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            RequisitionLineStatus Stat = Find(id);
            Delete(Stat);
        }


        //CancelQtyUpdate Functions

        public void UpdateRequisitionQtyCancelMultiple(Dictionary<int, decimal> Qty, DateTime DocDate,ref ApplicationDbContext Context)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            var LineAndQty = (from p in Context.RequisitionCancelLine
                              where (IsdA).Contains(p.RequisitionLineId)
                              group p by p.RequisitionLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();

            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            var LineStatus = (from p in Context.RequisitionLineStatus
                              where IsdA.Contains(p.RequisitionLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.CancelQty = Qty[item.RequisitionLineId.Value] + (LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault().Qty);
                item.CancelDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.RequisitionLineStatus.Add(item);
            }
        }


        public void UpdateRequisitionQtyOnCancel(int id, int CancelLineId, DateTime DocDate, decimal Qty,ref ApplicationDbContext Context,bool IsDbBased)
        {

            var temp = (from p in Context.RequisitionCancelLine
                        where p.RequisitionLineId == id && p.RequisitionCancelLineId != CancelLineId
                        join t in Context.RequisitionCancelHeader on p.RequisitionCancelHeaderId equals t.RequisitionCancelHeaderId
                        select new
                        {
                            Qty = p.Qty,
                            Date = t.DocDate,
                        }).ToList();
            decimal qty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(RequisitionStatusQtyConstants.CancelQty, qty, date, id, ref Context, IsDbBased);

        }


        public void DeleteRequisitionQtyOnCancelMultiple(int id, ref ApplicationDbContext Context)
        {

            var LineAndQty = (from t in Context.RequisitionCancelLine
                              where t.RequisitionCancelHeaderId == id
                              group t by t.RequisitionLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();


            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId).ToArray();

            var RequisitionLineStatus = (from p in Context.RequisitionLineStatus
                                         where IsdA2.Contains(p.RequisitionLineId.Value)
                                         select p
                                        ).ToList();

            foreach (var item in RequisitionLineStatus)
            {
                item.CancelQty = item.CancelQty - (LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                Context.RequisitionLineStatus.Add(item);
            }

        }




        //UpdateQty on Issue
        public void UpdateRequisitionQtyIssueMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            var LineAndQty = (from p in Context.StockLine
                              where p.RequisitionLineId != null && (IsdA).Contains(p.RequisitionLineId.Value)
                              group p by p.RequisitionLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();

            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            var LineStatus = (from p in Context.RequisitionLineStatus
                              where IsdA.Contains(p.RequisitionLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.IssueQty = Qty[item.RequisitionLineId.Value] + (LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault().Qty);
                item.IssueDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                //Update(item);
                Context.RequisitionLineStatus.Add(item);
            }


        }


        public void UpdateRequisitionQtyOnIssue(int id, int IssueLineId, DateTime DocDate, decimal Qty, ref ApplicationDbContext Context, bool IsDbBased)
        {

            var temp = (from p in Context.StockLine
                        where p.RequisitionLineId == id && p.StockLineId != IssueLineId
                        join t in Context.StockHeader on p.StockHeaderId equals t.StockHeaderId
                        select new
                        {
                            Qty = p.Qty,
                            Date = t.DocDate,
                        }).ToList();
            decimal qty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(RequisitionStatusQtyConstants.IssueQty, qty, date, id, ref Context, IsDbBased);

        }


        public void DeleteRequisitionQtyOnIssueMultiple(int id, ref ApplicationDbContext Context)
        {

            var LineAndQty = (from t in Context.StockLine
                              where t.StockHeaderId == id && t.RequisitionLineId != null
                              group t by t.RequisitionLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();

            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId.Value).ToArray();

            var RequisitionLineStatus = (from p in Context.RequisitionLineStatus
                                         where IsdA2.Contains(p.RequisitionLineId.Value)
                                         select p
                                        ).ToList();

            foreach (var item in RequisitionLineStatus)
            {
                item.IssueQty = item.IssueQty - (LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                //Update(item);
                Context.RequisitionLineStatus.Add(item);
            }



        }


        //UpdateQty on Receive
        public void UpdateRequisitionQtyReceiveMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            var LineAndQty = (from p in Context.StockLine
                              where p.RequisitionLineId != null && (IsdA).Contains(p.RequisitionLineId.Value)
                              group p by p.RequisitionLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();

            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            var LineStatus = (from p in Context.RequisitionLineStatus
                              where IsdA.Contains(p.RequisitionLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.ReceiveQty = Qty[item.RequisitionLineId.Value] + (LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault().Qty);
                item.ReceiveDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                //Update(item);
                Context.RequisitionLineStatus.Add(item);
            }


        }


        public void UpdateRequisitionQtyOnReceive(int id, int ReceiveLineId, DateTime DocDate, decimal Qty, ref ApplicationDbContext Context, bool IsDbBased)
        {

            var temp = (from p in Context.StockLine
                        where p.RequisitionLineId == id && p.StockLineId != ReceiveLineId
                        join t in Context.StockHeader on p.StockHeaderId equals t.StockHeaderId
                        select new
                        {
                            Qty = p.Qty,
                            Date = t.DocDate,
                        }).ToList();
            decimal qty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(RequisitionStatusQtyConstants.ReceiveQty, qty, date, id, ref Context, IsDbBased);

        }


        public void DeleteRequisitionQtyOnReceiveMultiple(int id, ref ApplicationDbContext Context)
        {

            var LineAndQty = (from t in Context.StockLine
                              where t.StockHeaderId == id && t.RequisitionLineId != null
                              group t by t.RequisitionLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();

            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId.Value).ToArray();

            var RequisitionLineStatus = (from p in Context.RequisitionLineStatus
                                         where IsdA2.Contains(p.RequisitionLineId.Value)
                                         select p
                                        ).ToList();

            foreach (var item in RequisitionLineStatus)
            {
                item.ReceiveQty = item.ReceiveQty - (LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.RequisitionLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                //Update(item);
                Context.RequisitionLineStatus.Add(item);
            }



        }


        public void UpdateStatusQty(string QtyType, decimal Qty, DateTime date, int Id, ref ApplicationDbContext Context, bool IsDbBased)
        {

            RequisitionLineStatus Stat = Find(Id);

            switch (QtyType)
            {
                case RequisitionStatusQtyConstants.CancelQty:
                    {
                        Stat.CancelQty = Qty;
                        Stat.CancelDate = date;
                        break;
                    }
                case RequisitionStatusQtyConstants.IssueQty:
                    {
                        Stat.IssueQty = Qty;
                        Stat.IssueDate = date;
                        break;
                    }
                case RequisitionStatusQtyConstants.ReceiveQty:
                    {
                        Stat.ReceiveQty = Qty;
                        Stat.ReceiveDate = date;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            Stat.ObjectState = Model.ObjectState.Modified;
            if (IsDbBased)
                Context.RequisitionLineStatus.Add(Stat);
            else
                Update(Stat);


        }


        public void Dispose()
        {
        }


        public Task<IEquatable<RequisitionLineStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<RequisitionLineStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
