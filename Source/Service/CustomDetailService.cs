using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.ViewModels;
using System.Data.SqlClient;
using System.Configuration;

namespace Service
{
    public interface ICustomDetailService : IDisposable
    {
        CustomDetail Create(CustomDetail s);
        void Delete(int id);
        void Delete(CustomDetail s);
        CustomDetail GetCustomDetail(int id);

        CustomDetail Find(int id);
        IQueryable<CustomDetailViewModel> GetCustomDetailViewModelForIndex();
        IQueryable<CustomDetailViewModel> GetCustomDetailListPendingToSubmit();
        IQueryable<CustomDetailViewModel> GetCustomDetailListPendingToApprove();
        CustomDetailViewModel GetCustomDetailViewModel(int id);

        IEnumerable<ComboBoxList> GetInvoicePendingForCustomDetail(int CustomDetailId, DateTime DocDate);
        
        void Update(CustomDetail s);
        string GetMaxDocNo();
        CustomDetail FindByDocNo(string Docno, int DivisionId, int SiteId);
        int NextId(int id);
        int PrevId(int id);

       
    }
    public class CustomDetailService : ICustomDetailService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public CustomDetailService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

     public CustomDetail Create(CustomDetail s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<CustomDetail>().Insert(s);
            return s;
        }

       public void Delete(int id)
     {
         _unitOfWork.Repository<CustomDetail>().Delete(id);
     }
       public void Delete(CustomDetail s)
        {
            _unitOfWork.Repository<CustomDetail>().Delete(s);
        }
       public void Update(CustomDetail s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<CustomDetail>().Update(s);            
        }

       public CustomDetail GetCustomDetail(int id)
        {
            return _unitOfWork.Repository<CustomDetail>().Query().Get().Where(m => m.CustomDetailId == id).FirstOrDefault();
        }

       public CustomDetail Find(int id)
       {
           return _unitOfWork.Repository<CustomDetail>().Find(id);
       }

       public CustomDetail FindByDocNo(string Docno, int DivisionId, int SiteId)
       {
           return _unitOfWork.Repository<CustomDetail>().Query().Get().Where(m => m.DocNo == Docno && m.DivisionId == DivisionId && m.SiteId == SiteId ).FirstOrDefault();

       }

       public int NextId(int id)
       {
           int temp = 0;
           if (id != 0)
           {

               temp = (from p in db.CustomDetail
                       orderby p.DocDate descending, p.DocNo descending
                       select p.CustomDetailId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


           }
           else
           {
               temp = (from p in db.CustomDetail
                       orderby p.DocDate descending, p.DocNo descending
                       select p.CustomDetailId).FirstOrDefault();
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

               temp = (from p in db.CustomDetail
                       orderby p.DocDate descending, p.DocNo descending
                       select p.CustomDetailId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
           }
           else
           {
               temp = (from p in db.CustomDetail
                       orderby p.DocDate descending, p.DocNo descending
                       select p.CustomDetailId).AsEnumerable().LastOrDefault();
           }
           if (temp != 0)
               return temp;
           else
               return id;
       }

       public string GetMaxDocNo()
       {
           int x;
           var maxVal = _unitOfWork.Repository<CustomDetail>().Query().Get().Select(i => i.DocNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
           return (maxVal + 1).ToString();
       }

       public void Dispose()
       {
       }

       public IQueryable<CustomDetailViewModel> GetCustomDetailViewModelForIndex()
       {
           IQueryable<CustomDetailViewModel> CustomDetail = from H in db.CustomDetail
                                                            join s in db.SaleInvoiceHeader on H.SaleInvoiceHeaderId equals s.SaleInvoiceHeaderId into SaleInvoiceHeaderTable
                                                            from SaleInvoiceHeaderTab in SaleInvoiceHeaderTable.DefaultIfEmpty()
                                                            join d in db.DocumentType on H.DocTypeId equals d.DocumentTypeId into DocumentTypeTable
                                                            from DocumentTypeTab in DocumentTypeTable.DefaultIfEmpty()
                                                            orderby H.CustomDetailId
                                                            select new CustomDetailViewModel
                                                            {
                                                                CustomDetailId = H.CustomDetailId,
                                                                DocDate = H.DocDate,
                                                                DocNo = H.DocNo,
                                                                DocTypeName = DocumentTypeTab.DocumentTypeName,
                                                                SaleInvoiceHeaderDocNo = SaleInvoiceHeaderTab.DocNo,
                                                                TRNo = H.TRNo,
                                                                TRDate = H.TRDate,
                                                                Remark = H.Remark,
                                                            };

           return CustomDetail;
       }


       public IQueryable<CustomDetailViewModel> GetCustomDetailList()
        {
            IQueryable<CustomDetailViewModel> CustomDetaillist = from H in db.CustomDetail
                                                                 orderby H.DocDate descending, H.DocNo descending
                                                                 select new CustomDetailViewModel
                                                                 {
                                                                     CustomDetailId = H.CustomDetailId,
                                                                     DocDate = H.DocDate,
                                                                     DocNo = H.DocNo,
                                                                     Remark = H.Remark,
                                                                 };

            return CustomDetaillist;                             
        }
        
        public IQueryable<CustomDetailViewModel> GetCustomDetailListPendingToSubmit()
       {
           IQueryable<CustomDetailViewModel> CustomDetaillistpendingtosubmit = from H in db.CustomDetail
                                                                               join s in db.SaleInvoiceHeader on H.SaleInvoiceHeaderId equals s.SaleInvoiceHeaderId into SaleInvoiceHeaderTable
                                                                               from SaleInvoiceHeaderTab in SaleInvoiceHeaderTable.DefaultIfEmpty()
                                                                               orderby H.DocDate descending, H.DocNo descending
                                                                               where H.Status == (int)StatusConstants.Drafted || H.Status == (int)StatusConstants.Modified
                                                                               select new CustomDetailViewModel
                                                                               {
                                                                                   CustomDetailId = H.CustomDetailId,
                                                                                   DocDate = H.DocDate,
                                                                                   DocNo = H.DocNo,
                                                                                   SaleInvoiceHeaderDocNo = SaleInvoiceHeaderTab.DocNo,
                                                                                   TRNo = H.TRNo,
                                                                                   TRDate = H.TRDate,
                                                                                   Remark = H.Remark,
                                                                               };
           return CustomDetaillistpendingtosubmit;   
       }

        public IQueryable<CustomDetailViewModel> GetCustomDetailListPendingToApprove()
        {
            IQueryable<CustomDetailViewModel> CustomDetaillistpendingtoapprove = from H in db.CustomDetail
                                                                                 join s in db.SaleInvoiceHeader on H.SaleInvoiceHeaderId equals s.SaleInvoiceHeaderId into SaleInvoiceHeaderTable
                                                                                 from SaleInvoiceHeaderTab in SaleInvoiceHeaderTable.DefaultIfEmpty()
                                                                                 orderby H.DocDate descending, H.DocNo descending
                                                                                 where H.Status == (int)StatusConstants.Submitted || H.Status == (int)StatusConstants.ModificationSubmitted
                                                                                 select new CustomDetailViewModel
                                                                                 {
                                                                                     CustomDetailId = H.CustomDetailId,
                                                                                     DocDate = H.DocDate,
                                                                                     DocNo = H.DocNo,
                                                                                     SaleInvoiceHeaderDocNo = SaleInvoiceHeaderTab.DocNo,
                                                                                     TRNo = H.TRNo,
                                                                                     TRDate = H.TRDate,
                                                                                     Remark = H.Remark,
                                                                                 };

            return CustomDetaillistpendingtoapprove;   
        }


        public CustomDetailViewModel GetCustomDetailViewModel(int id)
        {
            CustomDetailViewModel packingheaderlistpendingtoapprove = (from H in db.CustomDetail
                                                                       join s in db.SaleInvoiceHeader on H.SaleInvoiceHeaderId equals s.SaleInvoiceHeaderId into SaleInvoiceHeaderTable
                                                                       from SaleInvoiceHeaderTab in SaleInvoiceHeaderTable.DefaultIfEmpty()
                                                                       orderby H.DocDate descending, H.DocNo descending
                                                                       where H.CustomDetailId == id
                                                                       select new CustomDetailViewModel
                                                                       {
                                                                           CustomDetailId = H.CustomDetailId,
                                                                           DocDate = H.DocDate,
                                                                           DocNo = H.DocNo,
                                                                           SaleInvoiceHeaderDocNo = SaleInvoiceHeaderTab.DocNo,
                                                                           TRNo = H.TRNo,
                                                                           TRDate = H.TRDate,
                                                                           Remark = H.Remark,
                                                                       }).FirstOrDefault();

            return packingheaderlistpendingtoapprove;
        }

        //public IEnumerable<ComboBoxList> GetInvoicePendingForCustomDetail(int CustomDetailId, DateTime DocDate)
        //{
        //    IEnumerable<ComboBoxList> packingheader = from H in db.ViewSaleInvoicePendingForCustomDetail
        //                                              where H.DocDate <= DocDate
        //                                              select new ComboBoxList
        //                                              {
        //                                                  Id = H.SaleInvoiceHeaderId,
        //                                                  PropFirst = H.SaleInvoiceNo
        //                                              };
        //    return packingheader;
        //}

        public IEnumerable<ComboBoxList> GetInvoicePendingForCustomDetail(int CustomDetailId, DateTime DocDate)
        {
            SqlParameter SqlParameterDocDate = new SqlParameter("@DocDate", DocDate);
            SqlParameter SqlParameterCustomDetailId = new SqlParameter("@CustomDetailId", CustomDetailId);

            IEnumerable<PendingSaleInvoiceList> svm = db.Database.SqlQuery<PendingSaleInvoiceList>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetPendingInvoiceForCustomEntry @CustomDetailId, @DocDate", SqlParameterCustomDetailId, SqlParameterDocDate).ToList();

            IEnumerable<ComboBoxList> PendingInvoiceList = (from H in svm
                                                            select new ComboBoxList
                                                            {
                                                                Id = H.SaleInvoiceHeaderId,
                                                                PropFirst = H.SaleInvoiceNo
                                                            }).ToList();


            return PendingInvoiceList;
        }

    }

    public class PendingSaleInvoiceList
    {
        public int SaleInvoiceHeaderId { get; set; }
        public string SaleInvoiceNo { get; set; }
    }
}
