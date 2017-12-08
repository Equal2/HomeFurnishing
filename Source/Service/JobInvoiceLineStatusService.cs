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
    public interface IJobInvoiceLineStatusService : IDisposable
    {
        JobInvoiceLineStatus Create(JobInvoiceLineStatus pt);
        void Delete(int id);
        void Delete(JobInvoiceLineStatus pt);
        JobInvoiceLineStatus Find(int id);
        void Update(JobInvoiceLineStatus pt);
        Task<IEquatable<JobInvoiceLineStatus>> GetAsync();
        Task<JobInvoiceLineStatus> FindAsync(int id);
    }

    public class JobInvoiceLineStatusService : IJobInvoiceLineStatusService
    {
        ApplicationDbContext db;
        public JobInvoiceLineStatusService(ApplicationDbContext db)
        {
            this.db = db;
        }


        public JobInvoiceLineStatus Find(int id)
        {
            return db.JobInvoiceLineStatus.Find(id);
        }

        public JobInvoiceLineStatus Create(JobInvoiceLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            db.JobInvoiceLineStatus.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            var temp = db.JobInvoiceLineStatus.Find(id);
            temp.ObjectState = Model.ObjectState.Deleted;
            db.JobInvoiceLineStatus.Remove(temp);
        }

        public void Delete(JobInvoiceLineStatus pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.JobInvoiceLineStatus.Remove(pt);
        }

        public void Update(JobInvoiceLineStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            db.JobInvoiceLineStatus.Add(pt);
        }

        public void CreateLineStatus(int id)
        {
            JobInvoiceLineStatus Stat = new JobInvoiceLineStatus();
            Stat.JobInvoiceLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            db.JobInvoiceLineStatus.Add(Stat);
        }

        public void DeleteLineStatus(int id)
        {
            JobInvoiceLineStatus Stat = Find(id);
            Delete(Stat);
        }

        //UpdateQty on Return
        public void UpdateJobInvoiceQtyReturnMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context, Dictionary<int, decimal> Weight)
        {
            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();

            var temp2 = (from p in Context.JobInvoiceLine
                         where (IsdA).Contains(p.JobInvoiceLineId)
                         select new
                         {
                             LineId = p.JobInvoiceLineId,
                             UnitConvMult = p.UnitConversionMultiplier,
                         }).ToList();

            var TEST3 = (from p in temp2
                         join t in Qty on p.LineId equals t.Key
                         join t2 in Weight on p.LineId equals t2.Key
                         group new { p, t, t2 } by p.LineId into g
                         select new
                         {
                             LineId = g.Key,
                             Qty = g.Sum(m => m.t.Value),
                             Weight = g.Sum(m => m.t2.Value),
                         }).ToList();

            int[] IsdA2 = null;
            IsdA2 = TEST3.Select(m => m.LineId).ToArray();

            var LineStatus = (from p in Context.JobInvoiceLineStatus
                              where IsdA2.Contains(p.JobInvoiceLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.ReturnQty = (item.ReturnQty.HasValue ? item.ReturnQty : 0) + (TEST3.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault() == null ? 0 : TEST3.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault().Qty);
                item.ReturnDealQty = (item.ReturnQty.HasValue ? item.ReturnQty : 0) * (temp2.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault() == null ? 0 : temp2.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault().UnitConvMult);
                item.ReturnWeight = (item.ReturnWeight.HasValue ? item.ReturnWeight : 0) + (TEST3.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault() == null ? 0 : TEST3.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault().Weight);
                item.ReturnDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobInvoiceLineStatus.Add(item);
            }
        }


        public void UpdateJobInvoiceQtyOnReturn(int id, int ReturnLineId, DateTime DocDate, decimal Qty, decimal Weight, ref ApplicationDbContext Context)
        {
            var Line = (from p in Context.JobInvoiceLine
                        where p.JobInvoiceLineId == id
                        select p).FirstOrDefault();

            var temp = (from p in Context.JobInvoiceReturnLine
                        where p.JobInvoiceLineId == Line.JobInvoiceLineId && p.JobInvoiceReturnLineId != ReturnLineId
                        join t2 in db.JobReturnLine on p.JobReturnLineId equals t2.JobReturnLineId
                        join t in Context.JobInvoiceReturnHeader on p.JobInvoiceReturnHeaderId equals t.JobInvoiceReturnHeaderId
                        select new
                        {
                            Qty = p.Qty,
                            Date = t.DocDate,
                            DealQty = p.DealQty,
                            Weight = t2.Weight,
                        }).ToList();
            decimal qty;
            decimal weight;
            decimal dealqty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                dealqty = qty * Line.UnitConversionMultiplier;
                date = DocDate;
                weight = Weight;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                weight = temp.Sum(m => m.Weight) + Weight;
                dealqty = temp.Sum(m => m.DealQty) + Qty * Line.UnitConversionMultiplier;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(JobStatusQtyConstants.ReturnQty, qty, dealqty, weight, date, Line.JobInvoiceLineId, ref Context, true);

        }

        public void DeleteJobInvoiceQtyOnReturnMultiple(int id, ref ApplicationDbContext Context)
        {

            var temp = (from p in Context.JobInvoiceReturnLine
                        where p.JobInvoiceReturnHeaderId == id
                        join t in db.JobReturnLine on p.JobReturnLineId equals t.JobReturnLineId
                        group new { p, t } by p.JobInvoiceLineId into g
                        select new
                        {
                            LineId = g.Key,
                            Qty = g.Sum(m => m.p.Qty),
                            UnitConvMultiplier = g.Max(m => m.p.UnitConversionMultiplier),
                            Weight = g.Sum(m => m.t.Weight),
                        }).ToList();

            int[] IsdA2 = null;
            IsdA2 = temp.Select(m => m.LineId).ToArray();

            var JobInvoiceLineStatus = (from p in Context.JobInvoiceLineStatus
                                        where IsdA2.Contains(p.JobInvoiceLineId.Value)
                                        select p
                                        ).ToList();

            foreach (var item in JobInvoiceLineStatus)
            {
                item.ReturnQty = item.ReturnQty - (temp.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault().Qty);
                item.ReturnDealQty = item.ReturnQty * (temp.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault().UnitConvMultiplier);
                item.ReturnWeight = item.ReturnWeight - (temp.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobInvoiceLineId).FirstOrDefault().Weight);
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobInvoiceLineStatus.Add(item);
            }

        }

        public void UpdateStatusQty(string QtyType, decimal Qty, decimal DealQty, decimal Weight, DateTime date, int Id, ref ApplicationDbContext Context, bool DbBased)
        {

            JobInvoiceLineStatus Stat = Find(Id);

            switch (QtyType)
            {
                case JobStatusQtyConstants.ReturnQty:
                    {
                        Stat.ReturnQty = Qty;
                        Stat.ReturnDealQty = DealQty;
                        Stat.ReturnDate = date;
                        Stat.ReturnWeight = Weight;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            Stat.ObjectState = Model.ObjectState.Modified;

            if (DbBased)
                Context.JobInvoiceLineStatus.Add(Stat);
            else
                Update(Stat);
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<JobInvoiceLineStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobInvoiceLineStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
