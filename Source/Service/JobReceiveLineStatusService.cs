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

namespace Service
{
    public interface IJobReceiveLineStatusService : IDisposable
    {
        JobReceiveLineStatus Create(JobReceiveLineStatus pt);
        void Delete(int id);
        void Delete(JobReceiveLineStatus pt);
        JobReceiveLineStatus Find(int id);
        IEnumerable<JobReceiveLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(JobReceiveLineStatus pt);
        JobReceiveLineStatus Add(JobReceiveLineStatus pt);
        IEnumerable<JobReceiveLineStatus> GetJobReceiveLineStatusList();

        // IEnumerable<JobReceiveLineStatus> GetJobReceiveLineStatusList(int buyerId);
        Task<IEquatable<JobReceiveLineStatus>> GetAsync();
        Task<JobReceiveLineStatus> FindAsync(int id);
        void CreateLineStatus(int id, ref ApplicationDbContext context, bool IsDBbased);
        void DeleteJobReceiveQtyOnReturnMultipleDB(int id, ref ApplicationDbContext context);
        void UpdateJobReceiveQtyQAMultiple(List<JobReceiveQALineViewModel> Qty, DateTime DocDate, ref ApplicationDbContext Context);
    }

    public class JobReceiveLineStatusService : IJobReceiveLineStatusService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<JobReceiveLineStatus> _JobReceiveLineStatusRepository;
        RepositoryQuery<JobReceiveLineStatus> JobReceiveLineStatusRepository;
        public JobReceiveLineStatusService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _JobReceiveLineStatusRepository = new Repository<JobReceiveLineStatus>(db);
            JobReceiveLineStatusRepository = new RepositoryQuery<JobReceiveLineStatus>(_JobReceiveLineStatusRepository);
        }


        public JobReceiveLineStatus Find(int id)
        {
            return _unitOfWork.Repository<JobReceiveLineStatus>().Find(id);
        }

        public JobReceiveLineStatus Create(JobReceiveLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<JobReceiveLineStatus>().Insert(pt);
            return pt;
        }

        public JobReceiveLineStatus DBCreate(JobReceiveLineStatus pt)
        {
            pt.ObjectState = ObjectState.Added;
            db.JobReceiveLineStatus.Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<JobReceiveLineStatus>().Delete(id);
        }

        public void Delete(JobReceiveLineStatus pt)
        {
            _unitOfWork.Repository<JobReceiveLineStatus>().Delete(pt);
        }

        public void Update(JobReceiveLineStatus pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<JobReceiveLineStatus>().Update(pt);
        }

        public IEnumerable<JobReceiveLineStatus> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<JobReceiveLineStatus>()
                .Query()
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IEnumerable<JobReceiveLineStatus> GetJobReceiveLineStatusList()
        {
            var pt = _unitOfWork.Repository<JobReceiveLineStatus>().Query().Get();

            return pt;
        }

        public JobReceiveLineStatus Add(JobReceiveLineStatus pt)
        {
            _unitOfWork.Repository<JobReceiveLineStatus>().Insert(pt);
            return pt;
        }

        public void CreateLineStatus(int id, ref ApplicationDbContext context, bool IsDBbased)
        {
            JobReceiveLineStatus Stat = new JobReceiveLineStatus();
            Stat.JobReceiveLineId = id;
            Stat.ObjectState = Model.ObjectState.Added;
            if (IsDBbased)
                context.JobReceiveLineStatus.Add(Stat);
            else
                Add(Stat);
        }

        public void CreateLineStatusWithInvoice(int id, decimal Qty, decimal DealQty, DateTime DocDate, ref ApplicationDbContext context)
        {
            JobReceiveLineStatus Stat = new JobReceiveLineStatus();
            Stat.JobReceiveLineId = id;
            Stat.InvoiceQty = Qty;
            Stat.InvoiceDealQty = DealQty;
            Stat.InvoiceDate = DocDate;
            Stat.ObjectState = Model.ObjectState.Added;

            context.JobReceiveLineStatus.Add(Stat);

        }

        public void DeleteLineStatus(int id)
        {
            JobReceiveLineStatus Stat = Find(id);
            Delete(Stat);
        }

        //UpdateQty on Invoice
        public void UpdateJobReceiveQtyInvoiceMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {

            string Ids = null;
            Ids = string.Join(",", Qty.Select(m => m.Key.ToString()));

            int[] IsdA = null;
            IsdA = Ids.Split(",".ToCharArray()).Select(Int32.Parse).ToArray();

            var temp = (from p in Context.JobReceiveLine
                        where (IsdA).Contains(p.JobReceiveLineId)
                        group p by p.JobReceiveLineId into g
                        select new
                        {
                            LineId = g.Key,
                            Qty = g.Sum(m => m.Qty),
                        }).ToList();


            int[] Ids2 = null;
            Ids2 = temp.Select(m => m.LineId).ToArray();

            var UnitConVMuls = (from p in Context.JobReceiveLine
                                where (Ids2).Contains(p.JobReceiveLineId)
                                join t in Context.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                select new
                                {
                                    LineId = p.JobReceiveLineId,
                                    UnitConvMul = t.UnitConversionMultiplier
                                }).ToList();


            var LineStatus = (from p in Context.JobReceiveLineStatus
                              where (Ids2).Contains(p.JobReceiveLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.InvoiceQty = (item.InvoiceQty.HasValue ? item.InvoiceQty : 0) + (temp.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().Qty);
                item.InvoiceDealQty = item.InvoiceQty * (UnitConVMuls.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().UnitConvMul);
                item.InvoiceDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                //Context.JobReceiveLineStatus.Add(item);
            }
            Context.JobReceiveLineStatus.AddRange(LineStatus);

        }


        public void UpdateJobReceiveQtyOnInvoice(int id, int InvoiceLineId, DateTime DocDate, decimal Qty, ref ApplicationDbContext Context, bool IsDbBased)
        {
            int LineId = (from p in Context.JobReceiveLine
                          where p.JobReceiveLineId == id
                          select p.JobReceiveLineId).FirstOrDefault();

            var UnitConvMul = (from p in Context.JobReceiveLine
                               where p.JobReceiveLineId == LineId
                               join t in Context.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                               select t.UnitConversionMultiplier).FirstOrDefault();

            var temp = (from p in Context.JobInvoiceLine
                        where p.JobReceiveLine.JobReceiveLineId == LineId && p.JobInvoiceLineId != InvoiceLineId
                        join t in Context.JobInvoiceHeader on p.JobInvoiceHeaderId equals t.JobInvoiceHeaderId
                        select new
                        {
                            Qty = p.JobReceiveLine.Qty,
                            Date = t.DocDate,
                            DealQty = p.DealQty,
                        }).ToList();
            decimal qty;
            decimal dealqty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                dealqty = qty * UnitConvMul;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                dealqty = temp.Sum(m => m.DealQty) + Qty * UnitConvMul;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(JobStatusQtyConstants.InvoiceQty, qty, dealqty, date, LineId, ref Context, IsDbBased);

        }

        public void DeleteJobReceiveQtyOnInvoiceMultiple(int id, ref ApplicationDbContext Context)
        {

            var temp = (from p in Context.JobInvoiceLine
                        where p.JobInvoiceHeaderId == id
                        join t in Context.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                        group new { t, p } by t.JobReceiveLineId into g
                        select new
                        {
                            LineId = g.Key,
                            Qty = g.Sum(m => m.t.Qty),
                        }).ToList();



            int[] Ids2 = null;
            Ids2 = temp.Select(m => m.LineId).ToArray();


            var UnitConVMuls = (from p in Context.JobReceiveLine
                                where (Ids2).Contains(p.JobReceiveLineId)
                                join t in Context.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                select new
                                {
                                    LineId = p.JobReceiveLineId,
                                    UnitConvMul = t.UnitConversionMultiplier
                                }).ToList();

            var JobReceiveLineStatus = (from p in Context.JobReceiveLineStatus
                                        where Ids2.Contains(p.JobReceiveLineId.Value)
                                        select p
                                        ).ToList();

            foreach (var item in JobReceiveLineStatus)
            {
                item.InvoiceQty = item.InvoiceQty - (temp.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().Qty);
                item.InvoiceDealQty = item.InvoiceQty * (UnitConVMuls.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().UnitConvMul);
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobReceiveLineStatus.Add(item);
                //Update(item);
            }

            // Context.JobReceiveLineStatus.AddRange(JobReceiveLineStatus);

        }






        //UpdateQty on Return
        public void UpdateJobReceiveQtyReturnMultiple(Dictionary<int, decimal> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {
            int[] IsdA = null;
            IsdA = Qty.Select(m => m.Key).ToArray();

            var temp2 = (from p in Context.JobReceiveLine
                         where (IsdA).Contains(p.JobReceiveLineId)
                         select new
                         {
                             LineId = p.JobReceiveLineId,
                             RecevLineId = p.JobReceiveLineId,
                         }).ToList();




            var TEST3 = (from p in temp2
                         join t in Qty on p.RecevLineId equals t.Key
                         group new { p, t } by p.LineId into g
                         select new
                         {
                             LineId = g.Key,
                             Qty = g.Sum(m => m.t.Value),
                         }).ToList();

            int[] IsdA2 = null;
            IsdA2 = TEST3.Select(m => m.LineId).ToArray();


            var UnitConVMuls = (from p in Context.JobReceiveLine
                                where (IsdA2).Contains(p.JobReceiveLineId)
                                join t in Context.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                select new
                                {
                                    LineId = p.JobReceiveLineId,
                                    UnitConvMul = t.UnitConversionMultiplier
                                }).ToList();

            var LineStatus = (from p in Context.JobReceiveLineStatus
                              where IsdA2.Contains(p.JobReceiveLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                item.ReturnQty = (item.ReturnQty.HasValue ? item.ReturnQty : 0) + (TEST3.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault() == null ? 0 : TEST3.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().Qty);
                item.ReturnDealQty = (item.ReturnQty.HasValue ? item.ReturnQty : 0) * (UnitConVMuls.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().UnitConvMul);
                item.ReturnDate = DocDate;
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobReceiveLineStatus.Add(item);
            }


        }


        public void UpdateJobReceiveQtyOnReturn(int id, int ReturnLineId, DateTime DocDate, decimal Qty, ref ApplicationDbContext Context)
        {
            int LineId = (from p in Context.JobReceiveLine
                          where p.JobReceiveLineId == id
                          select p.JobReceiveLineId).FirstOrDefault();

            var UnitConvMul = (from p in Context.JobReceiveLine
                               where p.JobReceiveLineId == LineId
                               join t in Context.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                               select t.UnitConversionMultiplier).FirstOrDefault();

            var temp = (from p in Context.JobReturnLine
                        where p.JobReceiveLine.JobReceiveLineId == LineId && p.JobReturnLineId != ReturnLineId
                        join t in Context.JobReturnHeader on p.JobReturnHeaderId equals t.JobReturnHeaderId
                        select new
                        {
                            Qty = p.Qty,
                            Date = t.DocDate,
                            DealQty = p.DealQty,
                        }).ToList();
            decimal qty;
            decimal dealqty;
            DateTime date;
            if (temp.Count == 0)
            {
                qty = Qty;
                dealqty = qty * UnitConvMul;
                date = DocDate;
            }
            else
            {
                qty = temp.Sum(m => m.Qty) + Qty;
                dealqty = temp.Sum(m => m.DealQty) + Qty * UnitConvMul;
                date = temp.Max(m => m.Date);
            }
            UpdateStatusQty(JobStatusQtyConstants.ReturnQty, qty, dealqty, date, LineId, ref Context, true);

        }

        public void DeleteJobReceiveQtyOnReturnMultiple(int id, ref ApplicationDbContext Context)
        {

            var temp = (from p in Context.JobReturnLine
                        where p.JobReturnHeaderId == id
                        group p by p.JobReceiveLineId into g
                        select new
                        {
                            LineId = g.Key,
                            Qty = g.Sum(m => m.Qty),
                        }).ToList();

            int[] IsdA2 = null;
            IsdA2 = temp.Select(m => m.LineId).ToArray();

            var UnitConVMuls = (from p in Context.JobReceiveLine
                                where (IsdA2).Contains(p.JobReceiveLineId)
                                join t in Context.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                select new
                                {
                                    LineId = p.JobReceiveLineId,
                                    UnitConvMul = t.UnitConversionMultiplier
                                }).ToList();

            var JobReceiveLineStatus = (from p in Context.JobReceiveLineStatus
                                        where IsdA2.Contains(p.JobReceiveLineId.Value)
                                        select p
                                        ).ToList();

            foreach (var item in JobReceiveLineStatus)
            {
                item.ReturnQty = item.ReturnQty - (temp.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().Qty);
                item.ReturnDealQty = item.ReturnQty * (UnitConVMuls.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().UnitConvMul);
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobReceiveLineStatus.Add(item);
            }

        }

        public void DeleteJobReceiveQtyOnReturnMultipleDB(int id, ref ApplicationDbContext Context)
        {

            var temp = (from p in Context.JobReturnLine
                        where p.JobReturnHeaderId == id
                        join t in Context.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                        group p by t.JobReceiveLineId into g
                        select new
                        {
                            LineId = g.Key,
                            Qty = g.Sum(m => m.Qty),
                        }).ToList();

            int[] IsdA2 = null;
            IsdA2 = temp.Select(m => m.LineId).ToArray();

            var UnitConVMuls = (from p in Context.JobReceiveLine
                                where (IsdA2).Contains(p.JobReceiveLineId)
                                join t in Context.JobOrderLine on p.JobOrderLineId equals t.JobOrderLineId
                                select new
                                {
                                    LineId = p.JobReceiveLineId,
                                    UnitConvMul = t.UnitConversionMultiplier
                                }).ToList();

            var JobReceiveLineStatus = (from p in Context.JobReceiveLineStatus
                                        where IsdA2.Contains(p.JobReceiveLineId.Value)
                                        select p
                                        ).ToList();

            foreach (var item in JobReceiveLineStatus)
            {
                item.ReturnQty = item.ReturnQty - (temp.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault() == null ? 0 : temp.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().Qty);
                item.ReturnDealQty = item.ReturnQty * (UnitConVMuls.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault() == null ? 0 : UnitConVMuls.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault().UnitConvMul);
                item.ObjectState = Model.ObjectState.Modified;
                Context.JobReceiveLineStatus.Add(item);
            }

        }




        //UpdateQty on ReceiveQA
        public void UpdateJobReceiveQtyQAMultiple(List<JobReceiveQALineViewModel> Qty, DateTime DocDate, ref ApplicationDbContext Context)
        {

            int[] Ids2 = null;
            Ids2 = Qty.Select(m => m.JobReceiveLineId).ToArray();

            var LineStatus = (from p in Context.JobReceiveLineStatus
                              where (Ids2).Contains(p.JobReceiveLineId.Value)
                              select p).ToList();

            foreach (var item in LineStatus)
            {
                
                JobReceiveQALineViewModel recQaline = Qty.Where(m => m.JobReceiveLineId == item.JobReceiveLineId).FirstOrDefault();                

                item.QaDate = DocDate;
                item.QaFailQty = (item.QaFailQty ?? 0) + recQaline.FailQty;
                item.QaFailDealQty = (item.QaFailDealQty ?? 0) + recQaline.FailDealQty;
                item.QaWeight = (item.QaWeight ?? 0) + (recQaline == null ? 0 : recQaline.Weight);
                item.QaPenalty = (item.QaPenalty ?? 0) + (recQaline == null ? 0 : recQaline.PenaltyAmt);

                item.ObjectState = Model.ObjectState.Modified;
                //Context.JobReceiveLineStatus.Add(item);
            }
            Context.JobReceiveLineStatus.AddRange(LineStatus);

        }


        public void UpdateJobReceiveQtyOnQA(JobReceiveQALineViewModel QaLine, DateTime DocDate, ref ApplicationDbContext Context)
        {

            var temp = (from p in Context.JobReceiveQALine.AsNoTracking()
                        where p.JobReceiveLineId == QaLine.JobReceiveLineId && p.JobReceiveQALineId != QaLine.JobReceiveQALineId
                        join t in Context.JobReceiveQAHeader on p.JobReceiveQAHeaderId equals t.JobReceiveQAHeaderId
                        select new
                        {
                            FailQty = p.FailQty,
                            Date = t.DocDate,
                            FailDealQty = p.FailDealQty,                            
                            Weight = p.Weight,
                            Penalty = p.PenaltyAmt,

                        }).ToList();

            JobReceiveLineStatus Status = Context.JobReceiveLineStatus.Find(QaLine.JobReceiveLineId);

            if (temp.Count == 0)
            {
                Status.QaFailQty = QaLine.FailQty;
                Status.QaDate = DocDate;
                Status.QaFailDealQty = QaLine.FailDealQty;
                Status.QaWeight = QaLine.Weight;
                Status.QaPenalty = QaLine.PenaltyAmt;
            }
            else
            {
                Status.QaDate = DocDate;
                Status.QaFailQty = temp.Sum(m => m.FailQty) + (QaLine.FailQty);
                Status.QaFailDealQty = temp.Sum(m => m.FailDealQty) + QaLine.FailDealQty;
                Status.QaWeight = temp.Sum(m => m.Weight) + QaLine.Weight;
                Status.QaPenalty = temp.Sum(m => m.Penalty) + QaLine.PenaltyAmt;
            }

            Status.ObjectState = Model.ObjectState.Modified;
            Context.JobReceiveLineStatus.Add(Status);

        }

        public void DeleteJobReceiveQtyOnQAMultiple(int id, ref ApplicationDbContext Context)
        {

            var temp = (from p in Context.JobReceiveQALine.AsNoTracking()
                        where p.JobReceiveQAHeaderId == id
                        join t in Context.JobReceiveLine on p.JobReceiveLineId equals t.JobReceiveLineId
                        group new { t, p } by t.JobReceiveLineId into g
                        select new
                        {
                            LineId = g.Key,
                            FailQty = g.Sum(m => m.p.FailQty),
                            FailDealQty = g.Sum(m => m.p.FailDealQty),
                            Penalty = g.Sum(m => m.p.PenaltyAmt),
                            Weight = g.Sum(m => m.p.Weight),
                        }).ToList();



            int[] Ids2 = null;
            Ids2 = temp.Select(m => m.LineId).ToArray();

            var JobReceiveLineStatus = (from p in Context.JobReceiveLineStatus
                                        where Ids2.Contains(p.JobReceiveLineId.Value)
                                        select p
                                        ).ToList();

            foreach (var item in JobReceiveLineStatus)
            {
                var recline = temp.Where(m => m.LineId == item.JobReceiveLineId).FirstOrDefault();

                item.QaFailQty = (item.QaFailQty ?? 0) - (recline == null ? 0 : recline.FailQty);
                item.QaFailDealQty = (item.QaFailDealQty ?? 0) - (recline == null ? 0 : recline.FailDealQty);
                item.QaWeight = (item.QaWeight ?? 0) - (recline == null ? 0 : recline.Weight);
                item.QaPenalty = (item.QaPenalty ?? 0) - (recline == null ? 0 : recline.Penalty);

                item.ObjectState = Model.ObjectState.Modified;
                Context.JobReceiveLineStatus.Add(item);
            }

        }




        public void UpdateStatusQty(string QtyType, decimal Qty, decimal DealQty, DateTime date, int Id, ref ApplicationDbContext Context, bool DbBased)
        {

            JobReceiveLineStatus Stat = Find(Id);

            switch (QtyType)
            {
                case JobStatusQtyConstants.ReturnQty:
                    {
                        Stat.ReturnQty = Qty;
                        Stat.ReturnDealQty = DealQty;
                        Stat.ReturnDate = date;
                        break;
                    }
                case JobStatusQtyConstants.InvoiceQty:
                    {
                        Stat.InvoiceQty = Qty;
                        Stat.InvoiceDealQty = DealQty;
                        Stat.InvoiceDate = date;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            Stat.ObjectState = Model.ObjectState.Modified;

            if (DbBased)
                Context.JobReceiveLineStatus.Add(Stat);
            else
                Update(Stat);


        }


        public void Dispose()
        {
        }


        public Task<IEquatable<JobReceiveLineStatus>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveLineStatus> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
