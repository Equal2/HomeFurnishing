using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using System.Data.Entity;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModels;
using Model.ViewModel;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;
using Model.ViewModels;
using AutoMapper;

namespace Service
{
    public interface IJobReceiveQAAttributeService : IDisposable
    {
        JobReceiveQALine Create(JobReceiveQAAttributeViewModel pt, string UserName);
        void Delete(int id);
        void Update(JobReceiveQAAttributeViewModel pt, string UserName);
        IQueryable<JobReceivePendingToQAIndex> GetJobReceiveQAAttributeList(int DocTypeId, string Uname);//DocumentTypeId
        List<QAGroupLineLineViewModel> GetJobReceiveQAAttribute(int JobReceiveLineid);//JobReceiveLineId
        List<QAGroupLineLineViewModel> GetJobReceiveQAAttributeForEdit(int JobReceiveQALineid);
        Task<IEquatable<JobReceiveQAAttribute>> GetAsync();
        Task<JobReceiveQAAttribute> FindAsync(int id);
        JobReceiveQAAttributeViewModel GetJobReceiveLineDetail(int JobReceiveLineid);
        JobReceiveQAAttributeViewModel GetJobReceiveQAAttributeDetailForEdit(int JobReceiveQALineid);//JobReceiveQALineId

        LastValues GetLastValues(int DocTypeId);
        
    }

    public class JobReceiveQAAttributeService : IJobReceiveQAAttributeService
    {
        ApplicationDbContext db;

        IUnitOfWork _unitOfWork;
        IJobReceiveQAHeaderService _JobReceiveQAHeaderService;
        public JobReceiveQAAttributeService(ApplicationDbContext db)
        {

            this.db = db;
            _JobReceiveQAHeaderService = new JobReceiveQAHeaderService(db);
        }

        public JobReceiveQAAttributeService(IUnitOfWork uow)
        {
            _unitOfWork = uow;
        }

        public JobReceiveQAAttribute Find(int id)
        {
            return db.JobReceiveQAAttribute.Find(id);
        }

        public JobReceiveQALine Create(JobReceiveQAAttributeViewModel pt, string UserName)
        {
            JobReceiveQAHeader header = new JobReceiveQAHeader();
            header = Mapper.Map<JobReceiveQAAttributeViewModel, JobReceiveQAHeader>(pt);
            _JobReceiveQAHeaderService.Create(header, UserName);

            JobReceiveQALine Line = Mapper.Map<JobReceiveQAAttributeViewModel, JobReceiveQALine>(pt);
            Line.Sr = new JobReceiveQALineService(db,_unitOfWork).GetMaxSr(Line.JobReceiveQAHeaderId);
            Line.FailQty = Line.QAQty - Line.Qty;
            Line.FailDealQty = Line.FailQty * Line.UnitConversionMultiplier;
            new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnQA(Mapper.Map<JobReceiveQALineViewModel>(Line), pt.DocDate, ref db);
            new JobReceiveQALineService(db, _unitOfWork).Create(Line, UserName);

            JobReceiveQALineExtended LineExtended = new JobReceiveQALineExtended();
            LineExtended.JobReceiveQALineId = Line.JobReceiveQALineId;
            LineExtended.Length = pt.Length;
            LineExtended.Width = pt.Width;
            LineExtended.Height = pt.Height;
            LineExtended.ObjectState = ObjectState.Added;
            db.JobReceiveQALineExtended.Add(LineExtended);


            List<QAGroupLineLineViewModel> tem = pt.QAGroupLine;

            if (tem != null)
            {
                foreach (var item in tem)
                {
                    JobReceiveQAAttribute pa = new JobReceiveQAAttribute();
                    pa.JobReceiveQALineId = Line.JobReceiveQALineId;
                    pa.QAGroupLineId = item.QAGroupLineId;
                    pa.Value = item.Value;
                    pa.Remark = item.Remarks;
                    pa.CreatedBy = UserName;
                    pa.ModifiedBy = UserName;
                    pa.CreatedDate = DateTime.Now;
                    pa.ModifiedDate = DateTime.Now;
                    pa.ObjectState = ObjectState.Added;
                    db.JobReceiveQAAttribute.Add(pa);
                }
            }

            return Line;
        }

        public void Update(JobReceiveQAAttributeViewModel pt, string UserName)
        {
            JobReceiveQAHeader header = new JobReceiveQAHeaderService(db).Find(pt.JobReceiveQAHeaderId);
            header.DocDate = pt.DocDate;
            header.DocNo = pt.DocNo;
            header.QAById = pt.QAById;
            header.Remark = pt.Remark;
            _JobReceiveQAHeaderService.Update(header, UserName);

            JobReceiveQALine Line = new JobReceiveQALineService(db, _unitOfWork).Find(pt.JobReceiveQALineId);
            Line.Marks = pt.Marks;
            Line.PenaltyRate = pt.PenaltyRate;
            Line.PenaltyAmt = pt.PenaltyAmt;
            Line.Weight = pt.Weight;
            Line.UnitConversionMultiplier = pt.UnitConversionMultiplier;
            Line.DealQty = pt.DealQty;
            Line.FailQty = Line.QAQty - Line.Qty;
            Line.FailDealQty = Line.FailQty * Line.UnitConversionMultiplier;
            new JobReceiveLineStatusService(_unitOfWork).UpdateJobReceiveQtyOnQA(Mapper.Map<JobReceiveQALineViewModel>(Line), pt.DocDate, ref db);
            new JobReceiveQALineService(db, _unitOfWork).Update(Line, UserName);


            JobReceiveQALineExtended LineExtended = (from Ld in db.JobReceiveQALineExtended where Ld.JobReceiveQALineId == pt.JobReceiveQALineId select Ld).FirstOrDefault();
            if (LineExtended != null)
            {
                LineExtended.Length = pt.Length;
                LineExtended.Width = pt.Width;
                LineExtended.Height = pt.Height;
                LineExtended.ObjectState = ObjectState.Modified;
                db.JobReceiveQALineExtended.Add(LineExtended);
            }


            List<QAGroupLineLineViewModel> tem = pt.QAGroupLine;

            if (tem != null)
            {
                foreach (var item in tem)
                {
                    if (item.JobReceiveQAAttributeId != null && item.JobReceiveQAAttributeId != 0)
                    {
                        JobReceiveQAAttribute pa = Find((int)item.JobReceiveQAAttributeId);
                        pa.QAGroupLineId = item.QAGroupLineId;
                        pa.Value = item.Value;
                        pa.Remark = item.Remarks;
                        pa.ModifiedBy = UserName;
                        pa.ModifiedDate = DateTime.Now;
                        pa.ObjectState = ObjectState.Modified;
                        db.JobReceiveQAAttribute.Add(pa);
                    }
                    else
                    {
                        JobReceiveQAAttribute pa = new JobReceiveQAAttribute();
                        pa.JobReceiveQALineId = Line.JobReceiveQALineId;
                        pa.QAGroupLineId = item.QAGroupLineId;
                        pa.Value = item.Value;
                        pa.Remark = item.Remarks;
                        pa.CreatedBy = UserName;
                        pa.ModifiedBy = UserName;
                        pa.CreatedDate = DateTime.Now;
                        pa.ModifiedDate = DateTime.Now;
                        pa.ObjectState = ObjectState.Added;
                        db.JobReceiveQAAttribute.Add(pa);
                    }
                }
            }
        }




        public IQueryable<JobReceivePendingToQAIndex> GetJobReceiveQAAttributeList(int DocTypeId, string Uname)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var settings = new JobReceiveQASettingsService(db).GetJobReceiveQASettingsForDocument(DocTypeId, DivisionId, SiteId);

            string[] contraDocTypes = null;
            if (!string.IsNullOrEmpty(settings.filterContraDocTypes)) { contraDocTypes = settings.filterContraDocTypes.Split(",".ToCharArray()); }
            else { contraDocTypes = new string[] { "NA" }; }

            return (from L in db.ViewJobReceiveBalanceForQA
                    join Jrl in db.JobReceiveLine on L.JobReceiveLineId equals Jrl.JobReceiveLineId into JobReceiveLineTable
                    from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                    join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                    orderby JobReceiveHeaderTab.DocDate, JobReceiveHeaderTab.DocNo
                    where JobReceiveHeaderTab.SiteId == SiteId && JobReceiveHeaderTab.DivisionId == DivisionId 
                    && (string.IsNullOrEmpty(settings.filterContraDocTypes) ? 1 == 1 : contraDocTypes.Contains(JobReceiveHeaderTab.DocTypeId.ToString()))
                    select new JobReceivePendingToQAIndex
                    {
                        JobReceiveHeaderId =JobReceiveHeaderTab.JobReceiveHeaderId,
                        JobReceiveLineId = L.JobReceiveLineId,
                        DocTypeName = JobReceiveHeaderTab.DocType.DocumentTypeName,
                        DocDate = JobReceiveHeaderTab.DocDate,
                        DocNo = JobReceiveHeaderTab.DocNo,
                        JobWorkerName = JobReceiveHeaderTab.JobWorker.Name,
                        ProductName = JobReceiveLineTab.JobOrderLine.Product.ProductName,
                        ProductUidName = JobReceiveLineTab.ProductUid.ProductUidName,
                        DocTypeId = DocTypeId
                    }
                );
        }

        public List<QAGroupLineLineViewModel> GetJobReceiveQAAttribute(int JobReceiveLineid)
        {
            List<QAGroupLineLineViewModel> JobReceiveQAAttribute = (from L in db.JobReceiveLine
                                                                    join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable
                                                                    from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                                                                    join Jol in db.JobOrderLine on L.JobOrderLineId equals Jol.JobOrderLineId into JobOrderLineTable
                                                                    from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                                                                    join Pp in db.ProductProcess on new { X1 = JobOrderLineTab.ProductId, X2 = JobReceiveHeaderTab.ProcessId } equals new { X1 = Pp.ProductId, X2 = (Pp.ProcessId ?? 0) } into ProductProcessTable
                                                                    from ProductProcessTab in ProductProcessTable.DefaultIfEmpty()
                                                                    join QAGl in db.QAGroupLine on ProductProcessTab.QAGroupId equals QAGl.QAGroupId into QAGroupLineTable
                                                                    from QAGroupLineTab in QAGroupLineTable.DefaultIfEmpty()
                                                                    where L.JobReceiveLineId == JobReceiveLineid && ((int?)QAGroupLineTab.QAGroupLineId ?? 0) != 0
                                                                    select new QAGroupLineLineViewModel
                                                                    {
                                                                        QAGroupLineId = QAGroupLineTab.QAGroupLineId,
                                                                        DefaultValue = QAGroupLineTab.DefaultValue,
                                                                        Value = QAGroupLineTab.DefaultValue,
                                                                        Name = QAGroupLineTab.Name,
                                                                        DataType = QAGroupLineTab.DataType,
                                                                        ListItem = QAGroupLineTab.ListItem
                                                                    }).ToList();


            return JobReceiveQAAttribute;
        }

        public List<QAGroupLineLineViewModel> GetJobReceiveQAAttributeForEdit(int JobReceiveQALineid)
        {


            List<QAGroupLineLineViewModel> JobReceiveQAAttribute = (from L in db.JobReceiveQAAttribute 
                                                                    where L.JobReceiveQALineId == JobReceiveQALineid
                                                                    select new QAGroupLineLineViewModel
                                                                    {
                                                                        JobReceiveQAAttributeId = L.JobReceiveQAAttributeId,
                                                                        QAGroupLineId = L.QAGroupLineId,
                                                                        DefaultValue = L.Value,
                                                                        Value = L.Value,
                                                                        Name = L.QAGroupLine.Name,
                                                                        DataType = L.QAGroupLine.DataType,
                                                                        ListItem = L.QAGroupLine.ListItem,
                                                                        Remarks = L.Remark
                                                                    }).ToList();


            return JobReceiveQAAttribute;
        }

        public JobReceiveQAAttributeViewModel GetJobReceiveLineDetail(int JobReceiveLineid)
        {
            JobReceiveQAAttributeViewModel JobReceiveLineDetail = (from L in db.JobReceiveLine
                                                                   join H in db.JobReceiveHeader on L.JobReceiveHeaderId equals H.JobReceiveHeaderId into JobReceiveHeaderTable
                                                                   from JobReceiveHeaderTab in JobReceiveHeaderTable.DefaultIfEmpty()
                                                                   join Jol in db.JobOrderLine on L.JobOrderLineId equals Jol.JobOrderLineId into JobOrderLineTable
                                                                   from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                                                                   where L.JobReceiveLineId == JobReceiveLineid
                                                                   select new JobReceiveQAAttributeViewModel
                                                                           {
                                                                               JobReceiveLineId = L.JobReceiveLineId,
                                                                               JobWorkerId = JobReceiveHeaderTab.JobWorkerId,
                                                                               ProductUidId = L.ProductUidId,
                                                                               ProductUidName = L.ProductUid.ProductUidName,
                                                                               ProductId = JobOrderLineTab.ProductId,
                                                                               ProductName = JobOrderLineTab.Product.ProductName,
                                                                               QAQty = L.Qty,
                                                                               InspectedQty = L.Qty,
                                                                               Qty = L.Qty,
                                                                               UnitId = JobOrderLineTab.Product.UnitId,
                                                                               DealUnitId = L.DealUnitId,
                                                                               UnitConversionMultiplier = L.UnitConversionMultiplier,
                                                                               DealQty = L.DealQty,
                                                                               Weight = L.Weight,
                                                                               UnitDecimalPlaces = JobOrderLineTab.Product.Unit.DecimalPlaces,
                                                                               DealUnitDecimalPlaces = JobOrderLineTab.DealUnit.DecimalPlaces,
                                                                               DocTypeId = JobReceiveHeaderTab.DocTypeId
                                                                           }).FirstOrDefault();

            
            if (JobReceiveLineDetail != null)
            {
                ProductDimensions ProductDimensions = new ProductService(_unitOfWork).GetProductDimensions(JobReceiveLineDetail.ProductId, JobReceiveLineDetail.DealUnitId, JobReceiveLineDetail.DocTypeId);
                if (ProductDimensions != null)
                {
                    JobReceiveLineDetail.Length = ProductDimensions.Length;
                    JobReceiveLineDetail.Width = ProductDimensions.Width;
                    JobReceiveLineDetail.Height = ProductDimensions.Height;
                    JobReceiveLineDetail.DimensionUnitDecimalPlaces = ProductDimensions.DimensionUnitDecimalPlaces;
                }
            }

            return JobReceiveLineDetail;
        }

        public JobReceiveQAAttributeViewModel GetJobReceiveQAAttributeDetailForEdit(int JobReceiveQALineid)
        {
            JobReceiveQAAttributeViewModel JobReceiveQALineDetail = (from L in db.JobReceiveQALine
                                                                     join H in db.JobReceiveQAHeader on L.JobReceiveQAHeaderId equals H.JobReceiveQAHeaderId into JobReceiveQAHeaderTable
                                                                     from JobReceiveQAHeaderTab in JobReceiveQAHeaderTable.DefaultIfEmpty()
                                                                     join Jrl in db.JobReceiveLine on L.JobReceiveLineId equals Jrl.JobReceiveLineId into JobReceiveLineTable
                                                                     from JobReceiveLineTab in JobReceiveLineTable.DefaultIfEmpty()
                                                                     join Jol in db.JobOrderLine on JobReceiveLineTab.JobOrderLineId equals Jol.JobOrderLineId into JobOrderLineTable
                                                                     from JobOrderLineTab in JobOrderLineTable.DefaultIfEmpty()
                                                                     join Ld in db.JobReceiveQALineExtended on L.JobReceiveQALineId equals Ld.JobReceiveQALineId into JobReceiveQALineDetailTable
                                                                     from JobReceiveQALineDetailTab in JobReceiveQALineDetailTable.DefaultIfEmpty()
                                                                     where L.JobReceiveQALineId == JobReceiveQALineid
                                                                     select new JobReceiveQAAttributeViewModel
                                                                     {
                                                                         JobReceiveQALineId = L.JobReceiveQALineId,
                                                                         JobReceiveQAHeaderId = L.JobReceiveQAHeaderId,
                                                                         JobReceiveLineId = L.JobReceiveLineId,
                                                                         JobWorkerId = JobReceiveQAHeaderTab.JobWorkerId,
                                                                         ProductUidId = L.ProductUidId,
                                                                         ProductUidName = L.ProductUid.ProductUidName,
                                                                         ProductId = JobOrderLineTab.ProductId,
                                                                         ProductName = JobOrderLineTab.Product.ProductName,
                                                                         QAQty = L.Qty,
                                                                         InspectedQty = L.Qty,
                                                                         Qty = L.Qty,
                                                                         UnitId = JobOrderLineTab.Product.UnitId,
                                                                         DealUnitId = JobReceiveLineTab.DealUnitId,
                                                                         UnitConversionMultiplier = L.UnitConversionMultiplier,
                                                                         DealQty = L.DealQty,
                                                                         Weight = L.Weight,
                                                                         UnitDecimalPlaces = JobOrderLineTab.Product.Unit.DecimalPlaces,
                                                                         DealUnitDecimalPlaces = JobOrderLineTab.DealUnit.DecimalPlaces,
                                                                         PenaltyRate = L.PenaltyRate,
                                                                         PenaltyAmt = L.PenaltyAmt,
                                                                         DivisionId = JobReceiveQAHeaderTab.DivisionId,
                                                                         SiteId = JobReceiveQAHeaderTab.SiteId,
                                                                         ProcessId = JobReceiveQAHeaderTab.ProcessId,
                                                                         DocDate = JobReceiveQAHeaderTab.DocDate,
                                                                         DocTypeId = JobReceiveQAHeaderTab.DocTypeId,
                                                                         DocNo = JobReceiveQAHeaderTab.DocNo,
                                                                         QAById = JobReceiveQAHeaderTab.QAById,
                                                                         Remark = JobReceiveQAHeaderTab.Remark,
                                                                         Length = JobReceiveQALineDetailTab.Length,
                                                                         Width = JobReceiveQALineDetailTab.Width,
                                                                         Height = JobReceiveQALineDetailTab.Height
                                                                     }).FirstOrDefault();

            if (JobReceiveQALineDetail != null)
            {
                ProductDimensions ProductDimensions = new ProductService(_unitOfWork).GetProductDimensions(JobReceiveQALineDetail.ProductId, JobReceiveQALineDetail.DealUnitId, JobReceiveQALineDetail.DocTypeId);
                if (ProductDimensions != null)
                {
                    JobReceiveQALineDetail.DimensionUnitDecimalPlaces = ProductDimensions.DimensionUnitDecimalPlaces;
                }
            }

            return JobReceiveQALineDetail;
        }

        public LastValues GetLastValues(int DocTypeId)
        {
            var DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            var SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];


            var temp = (from H in db.JobReceiveQAHeader
                        where H.DocTypeId == DocTypeId && H.SiteId == SiteId && H.DivisionId == DivisionId
                        orderby H.JobReceiveQAHeaderId descending
                        select new LastValues
                        {
                            QAById = H.QAById
                        }).FirstOrDefault();

            return temp;
        }

        public void Delete(int id)
        {
            JobReceiveQAAttribute Temp = db.JobReceiveQAAttribute.Find(id);
            Temp.ObjectState = Model.ObjectState.Deleted;

            db.JobReceiveQAAttribute.Remove(Temp);
        }
        

        public void Dispose()
        {
        }


        public Task<IEquatable<JobReceiveQAAttribute>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<JobReceiveQAAttribute> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }

    public class LastValues
    {
        public int? QAById { get; set; }
        public int? JobReceiveById { get; set; }
        public int? JobWorkerId { get; set; }
        public DateTime DocDate { get; set; }
    }


}
