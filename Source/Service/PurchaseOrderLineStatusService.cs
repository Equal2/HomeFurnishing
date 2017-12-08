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
    public interface IPurchaseOrderLineStatusService : IDisposable
    {
        PurchaseOrderLineStatus Create(PurchaseOrderLineStatus pt);
        void Delete(int id);
        void Delete(PurchaseOrderLineStatus pt);
        PurchaseOrderLineStatus Find(int id);
        IEnumerable<PurchaseOrderLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(PurchaseOrderLineStatus pt);
        PurchaseOrderLineStatus Add(PurchaseOrderLineStatus pt);
        IEnumerable<PurchaseOrderLineStatus> GetPurchaseOrderLineStatusList();

        // IEnumerable<PurchaseOrderLineStatus> GetPurchaseOrderLineStatusList(int buyerId);
        Task<IEquatable<PurchaseOrderLineStatus>> GetAsync();
        Task<PurchaseOrderLineStatus> FindAsync(int id);
    }

    public class PurchaseOrderLineStatusService : IPurchaseOrderLineStatusService
    {
        ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<PurchaseOrderLineStatus> _PurchaseOrderLineStatusRepository;
        RepositoryQuery<PurchaseOrderLineStatus> PurchaseOrderLineStatusRepository;
        public PurchaseOrderLineStatusService(IUnitOfWorkForService unitOfWork)
        {
            this.db= new ApplicationDbContext();
            _unitOfWork = unitOfWork;
            _PurchaseOrderLineStatusRepository = new Repository<PurchaseOrderLineStatus>(db);
            PurchaseOrderLineStatusRepository = new RepositoryQuery<PurchaseOrderLineStatus>(_PurchaseOrderLineStatusRepository);
        }


        public PurchaseOrderLineStatus Find(int id)
        {
            return _unitOfWork.Repository<PurchaseOrderLineStatus>().Find(id);
        }

        public PurchaseOrderLineStatus Create(PurchaseOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseOrderLineStatus>().Insert(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseOrderLineStatus>().Delete(id);
        }

        public void Delete(PurchaseOrderLineStatus pt)
        {
            _unitOfWork.Repository<PurchaseOrderLineStatus>().Delete(pt);
        }

        public void Update(PurchaseOrderLineStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseOrderLineStatus>().Update(pt);
        }

        public IEnumerable<PurchaseOrderLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<PurchaseOrderLineStatus>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<PurchaseOrderLineStatus> GetPurchaseOrderLineStatusList()
        {
            var pt = _unitOfWork.Repository<PurchaseOrderLineStatus>().Query().Get();

            return pt;
        }

        public PurchaseOrderLineStatus Add(PurchaseOrderLineStatus pt)
        {
            _unitOfWork.Repository<PurchaseOrderLineStatus>().Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id, ref ApplicationDbContext Context)
        {
            PurchaseOrderLineStatus Stat = new PurchaseOrderLineStatus();
            Stat.PurchaseOrderLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            Context.PurchaseOrderLineStatus.Add(Stat);
            //Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            PurchaseOrderLineStatus Stat = Find(id);
            Delete(Stat);
        }


        //CancelQtyUpdate Functions

        public void UpdatePurchaseQtyCancelMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {
            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            var LineAndQty = (from p in Context.PurchaseOrderCancelLine
                              where (IsdA).Contains(p.PurchaseOrderLineId)
                              group p by p.PurchaseOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();

            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            var LineStatus = (from p in Context.PurchaseOrderLineStatus
                              where IsdA.Contains(p.PurchaseOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.CancelQty = Qty[item.PurchaseOrderLineId.Value] + (LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault().Qty);
                item.CancelDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.PurchaseOrderLineStatus.Add(item);
                //Update(item);
            }
        }


        public void UpdatePurchaseQtyOnCancel(int id, int CancelLineId, DateTime DocDate, decimal Qty, ref ApplicationDbContext Context, bool IsDbBased)
        {

            var temp = (from p in db.PurchaseOrderCancelLine
                        where p.PurchaseOrderLineId == id && p.PurchaseOrderCancelLineId != CancelLineId
                        join t in db.PurchaseOrderCancelHeader on p.PurchaseOrderCancelHeaderId equals t.PurchaseOrderCancelHeaderId
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
            UpdateStatusQty(PurchaseStatusQtyConstants.CancelQty, qty, date, id, ref Context, IsDbBased);

        }


        public void DeletePurchaseQtyOnCancelMultiple(int id, ref ApplicationDbContext Context)
        {

            var LineAndQty = (from t in Context.PurchaseOrderCancelLine
                              where t.PurchaseOrderCancelHeaderId == id
                              group t by t.PurchaseOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();


            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId).ToArray();

            var PurchaseOrderLineStatus = (from p in Context.PurchaseOrderLineStatus
                                           where IsdA2.Contains(p.PurchaseOrderLineId.Value)
                                           select p
                                        ).ToList();

            foreach (var item in PurchaseOrderLineStatus)
            {
                item.CancelQty = item.CancelQty - (LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                Context.PurchaseOrderLineStatus.Add(item);
                //Update(item);
            }

        }



        //Rate Amendment Functions

        public void UpdatePurchaseRateOnAmendmentMultiple(Dictionary<int, decimal> Rate, DateTime DocDate, ref ApplicationDbContext Context)
        {
            int[] IsdA = null;
            IsdA = Rate.Select(m => m.Key).ToArray();

            var LineStatus = (from p in Context.PurchaseOrderLineStatus
                              where IsdA.Contains(p.PurchaseOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.RateAmendmentRate = Rate[item.PurchaseOrderLineId.Value];
                item.RateAmendmentDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.PurchaseOrderLineStatus.Add(item);
            }


        }


        public void UpdatePurchaseRateOnAmendment(int id, int AmendmentLineId, DateTime DocDate, decimal Rate, ref ApplicationDbContext Context)
        {

            UpdateStatusQty(PurchaseStatusQtyConstants.AmendmentRate, Rate, DocDate, id, ref Context, true);

        }


        public void DeletePurchaseRateOnAmendmentMultiple(int id, ref ApplicationDbContext Context)
        {

            var temp = (from t in Context.PurchaseOrderRateAmendmentLine
                        where t.PurchaseOrderAmendmentHeaderId == id
                        join p in Context.PurchaseOrderLine on t.PurchaseOrderLineId equals p.PurchaseOrderLineId
                        group new { t, p } by t.PurchaseOrderLineId into g
                        select new
                        {
                            LineId = g.Key,
                        }).ToList();

            int[] IsdA2 = null;
            IsdA2 = temp.Select(m => m.LineId).ToArray();

            var PurchaseOrderLineStatus = (from p in Context.PurchaseOrderLineStatus
                                      where IsdA2.Contains(p.PurchaseOrderLineId.Value)
                                      select p
                                        ).ToList();

            foreach (var item in PurchaseOrderLineStatus)
            {
                item.RateAmendmentRate = 0;
                item.RateAmendmentDate = null;
                item.ObjectState = Model.ObjectState.Modified;
                Context.PurchaseOrderLineStatus.Add(item);
            }
        }


        //UpdateQty on Receive
        public void UpdatePurchaseQtyReceiveMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            var LineAndQty = (from p in Context.PurchaseGoodsReceiptLine
                              where (IsdA).Contains(p.PurchaseOrderLineId.Value)
                              group p by p.PurchaseOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty),
                              }).ToList();


            var LineStatus = (from p in Context.PurchaseOrderLineStatus
                              where IsdA.Contains(p.PurchaseOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.ReceiveQty = Qty[item.PurchaseOrderLineId.Value] + (LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault().Qty);
                item.ReceiveDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                //Update(item);
                Context.PurchaseOrderLineStatus.Add(item);
            }


        }


        public void UpdatePurchaseQtyOnReceive(int id, int ReceiveLineId, DateTime DocDate, decimal Qty, ref ApplicationDbContext Context, bool IsDbBased)
        {

            var temp = (from p in db.PurchaseGoodsReceiptLine
                        where p.PurchaseOrderLineId == id && p.PurchaseGoodsReceiptLineId != ReceiveLineId
                        join t in db.PurchaseGoodsReceiptHeader on p.PurchaseGoodsReceiptHeaderId equals t.PurchaseGoodsReceiptHeaderId
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
            UpdateStatusQty(PurchaseStatusQtyConstants.ReceiveQty, qty, date, id, ref Context, IsDbBased);

        }


        public void DeletePurchaseQtyOnReceiveMultiple(int id, ref ApplicationDbContext Context)
        {

            var LineAndQty = (from t in Context.PurchaseGoodsReceiptLine
                              where t.PurchaseGoodsReceiptHeaderId == id
                              group t by t.PurchaseOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();

            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId ?? 0).ToArray();

            var PurchaseOrderLineStatus = (from p in Context.PurchaseOrderLineStatus
                                           where IsdA2.Contains(p.PurchaseOrderLineId.Value)
                                           select p
                                        ).ToList();

            foreach (var item in PurchaseOrderLineStatus)
            {
                item.ReceiveQty = item.ReceiveQty - (LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                //Update(item);
                Context.PurchaseOrderLineStatus.Add(item);
            }



        }



        //UpdateQty on Invoice
        public void UpdatePurchaseQtyInvoiceMultiple(Dictionary<int, decimal> Qty, DateTime DocDate,ref ApplicationDbContext Context)
        {

            string Ids = null;
            Ids = string.Join(",", Qty.Select(m => m.Key.ToString()));

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            //var temp = (from p in db.PurchaseGoodsReceiptLine
            //            where (IsdA).Contains(p.PurchaseGoodsReceiptLineId.ToString())
            //            group p by p.PurchaseOrderLineId into g
            //            select new
            //            {
            //                LineId = g.Key,
            //                Qty = g.Sum(m => m.Qty),
            //            }).ToList();

            var ReceiveAndLines = (from p in db.PurchaseGoodsReceiptLine
                                   where (IsdA).Contains(p.PurchaseGoodsReceiptLineId)
                                   select new
                                   {
                                       LineId = p.PurchaseOrderLineId,
                                       RecevLineId = p.PurchaseGoodsReceiptLineId,
                                   }).ToList();


            var LineAndQty = (from p in ReceiveAndLines
                              join t in Qty on p.RecevLineId equals t.Key
                              group new { p, t } by p.LineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.t.Value),
                              }).ToList();

            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId ?? 0).ToArray();

            var LineStatus = (from p in Context.PurchaseOrderLineStatus
                              where IsdA2.Contains(p.PurchaseOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.InvoiceQty = (item.InvoiceQty.HasValue ? item.InvoiceQty : 0) + (LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault().Qty);
                item.InvoiceDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.PurchaseOrderLineStatus.Add(item);
                //Update(item);
            }


        }


        public void UpdatePurchaseQtyOnInvoice(int id, int InvoiceLineId, DateTime DocDate, decimal Qty,ref ApplicationDbContext Context,bool IsDbBased)
        {
            var LineId = (from p in Context.PurchaseGoodsReceiptLine
                          where p.PurchaseGoodsReceiptLineId == id
                          select p.PurchaseOrderLineId).FirstOrDefault();

            if (LineId.HasValue)
            {
                var temp = (from p in Context.PurchaseInvoiceLine
                            where p.PurchaseGoodsReceiptLine.PurchaseOrderLineId == LineId && p.PurchaseInvoiceLineId != InvoiceLineId
                            join t in Context.PurchaseInvoiceHeader on p.PurchaseInvoiceHeaderId equals t.PurchaseInvoiceHeaderId
                            select new
                            {
                                Qty = p.PurchaseGoodsReceiptLine.Qty,
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
                UpdateStatusQty(PurchaseStatusQtyConstants.InvoiceQty, qty, date, LineId.Value, ref Context, IsDbBased);
            }
        }

        public void DeletePurchaseQtyOnInvoiceMultiple(int id,ref ApplicationDbContext Context)
        {

            var LineAndQty = (from p in Context.PurchaseInvoiceLine
                              where p.PurchaseInvoiceHeaderId == id
                              join t in Context.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t.PurchaseGoodsReceiptLineId
                              group t by t.PurchaseOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();



            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId ?? 0).ToArray();

            var PurchaseOrderLineStatus = (from p in db.PurchaseOrderLineStatus
                                           where IsdA2.Contains(p.PurchaseOrderLineId.Value)
                                           select p
                                        ).ToList();

            foreach (var item in PurchaseOrderLineStatus)
            {
                item.InvoiceQty = item.InvoiceQty - (LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                Context.PurchaseOrderLineStatus.Add(item);
                //Update(item);
            }



        }






        //UpdateQty on Return
        public void UpdatePurchaseQtyReturnMultiple(Dictionary<int, decimal> Qty, DateTime DocDate,ref ApplicationDbContext Context)
        {

            string Ids = null;
            Ids = string.Join(",", Qty.Select(m => m.Key.ToString()));

            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();


            //var temp = (from p in db.PurchaseReceiveLine
            //            where (IsdA).Contains(p.PurchaseReceiveLineId.ToString())
            //            group p by p.PurchaseOrderLineId into g
            //            select new
            //            {
            //                LineId = g.Key,
            //            }).ToList();

            var ReceiveAndLines = (from p in Context.PurchaseGoodsReceiptLine
                                   where (IsdA).Contains(p.PurchaseGoodsReceiptLineId)
                                   select new
                                   {
                                       LineId = p.PurchaseOrderLineId,
                                       RecevLineId = p.PurchaseGoodsReceiptLineId,
                                   }).ToList();




            var LineAndQty = (from p in ReceiveAndLines
                              join t in Qty on p.RecevLineId equals t.Key
                              group new { p, t } by p.LineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.t.Value),
                              }).ToList();

            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId ?? 0).ToArray();

            var LineStatus = (from p in Context.PurchaseOrderLineStatus
                              where IsdA2.Contains(p.PurchaseOrderLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.ReturnQty = (item.ReturnQty.HasValue ? item.ReturnQty : 0) + (LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault().Qty);
                item.ReturnDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.PurchaseOrderLineStatus.Add(item);
                //Update(item);
            }


        }


        public void UpdatePurchaseQtyOnReturn(int id, int ReturnLineId, DateTime DocDate, decimal Qty,ref ApplicationDbContext Context,bool IsDbBased)
        {
            var LineId = (from p in Context.PurchaseGoodsReceiptLine
                          where p.PurchaseGoodsReceiptLineId == id
                          select p.PurchaseOrderLineId).FirstOrDefault();

            if (LineId.HasValue)
            {
                var temp = (from p in Context.PurchaseGoodsReturnLine
                            where p.PurchaseGoodsReceiptLine.PurchaseOrderLineId == LineId && p.PurchaseGoodsReturnLineId != ReturnLineId
                            join t in Context.PurchaseGoodsReturnHeader on p.PurchaseGoodsReturnHeaderId equals t.PurchaseGoodsReturnHeaderId
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
                UpdateStatusQty(PurchaseStatusQtyConstants.ReturnQty, qty, date, LineId.Value, ref Context, IsDbBased);
            }
        }

        public void DeletePurchaseQtyOnReturnMultiple(int id, ref ApplicationDbContext Context)
        {

            var LineAndQty = (from p in Context.PurchaseGoodsReturnLine
                              where p.PurchaseGoodsReturnHeaderId == id
                              join t in Context.PurchaseGoodsReceiptLine on p.PurchaseGoodsReceiptLineId equals t.PurchaseGoodsReceiptLineId
                              group p by t.PurchaseOrderLineId into g
                              select new
                              {
                                  LineId = g.Key,
                                  Qty = g.Sum(m => m.Qty)
                              }).ToList();



            string Ids2 = null;
            Ids2 = string.Join(",", LineAndQty.Select(m => m.LineId.ToString()));

            int[] IsdA2 = null;
            IsdA2 = LineAndQty.Select(m => m.LineId ?? 0).ToArray();

            var PurchaseOrderLineStatus = (from p in Context.PurchaseOrderLineStatus
                                           where IsdA2.Contains(p.PurchaseOrderLineId.Value)
                                           select p
                                        ).ToList();

            foreach (var item in PurchaseOrderLineStatus)
            {
                item.ReturnQty = item.ReturnQty - (LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault() == null ? 0 : LineAndQty.Where(m => m.LineId == item.PurchaseOrderLineId).FirstOrDefault().Qty);
                item.ObjectState = Model.ObjectState.Modified;
                Context.PurchaseOrderLineStatus.Add(item);
                //Update(item);
            }



        }




        public void UpdateStatusQty(string QtyType, decimal Qty, DateTime date, int Id, ref ApplicationDbContext Context, bool IsDbBased)
        {

            PurchaseOrderLineStatus Stat = Find(Id);

            switch (QtyType)
            {
                case PurchaseStatusQtyConstants.CancelQty:
                    {
                        Stat.CancelQty = Qty;
                        Stat.CancelDate = date;
                        break;
                    }
                case PurchaseStatusQtyConstants.ReceiveQty:
                    {
                        Stat.ReceiveQty = Qty;
                        Stat.ReceiveDate = date;
                        break;
                    }
                case PurchaseStatusQtyConstants.InvoiceQty:
                    {
                        Stat.InvoiceQty = Qty;
                        Stat.InvoiceDate = date;
                        break;
                    }
                case PurchaseStatusQtyConstants.ReturnQty:
                    {
                        Stat.ReturnQty = Qty;
                        Stat.ReturnDate = date;
                        break;
                    }
                case JobStatusQtyConstants.AmendmentRate:
                    {
                        Stat.RateAmendmentRate = Qty;
                        Stat.RateAmendmentDate = date;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            Stat.ObjectState = Model.ObjectState.Modified;

            if (IsDbBased)
            {
                Context.PurchaseOrderLineStatus.Add(Stat);
            }
            else
            {
                Update(Stat);
            }

        }


        public void Dispose()
        {
        }


        public Task<IEquatable<PurchaseOrderLineStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PurchaseOrderLineStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
