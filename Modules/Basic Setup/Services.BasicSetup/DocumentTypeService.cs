using System.Collections.Generic;
using System.Linq;
using System;
using Model;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using Models.Company.Models;
using Infrastructure.IO;
using Models.BasicSetup.Models;
using Models.Company.ViewModels;

namespace Services.BasicSetup
{
    public interface IDocumentTypeService : IDisposable
    {
        DocumentType Create(DocumentType pt);
        void Delete(int id);
        void Delete(DocumentType pt);
        DocumentType Find(string Name);
        DocumentType Find(int id);
        DocumentType FindByName(string DocumentTypeName);
        IEnumerable<DocumentType> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DocumentType pt);
        DocumentType Add(DocumentType pt);
        IQueryable<DocumentType> GetDocumentTypeList();
        IEnumerable<DocumentType> GetDocumentTypeList(string DocumentCategoryName);
        Task<IEquatable<DocumentType>> GetAsync();
        Task<DocumentType> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);
        IEnumerable<DocumentType> FindByDocumentCategory(int DocumentCategoryId);
        string FGetNewDocNo(string FieldName, string TableName, int DocTypeId, DateTime DocDate, int DivisionId, int SiteId);
    }

    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<DocumentType> _DocumentTypeRepository;
        public DocumentTypeService(IUnitOfWork unitOfWork, IRepository<DocumentType> doctypeRepo)
        {
            _unitOfWork = unitOfWork;
            _DocumentTypeRepository = doctypeRepo;
        }

        public DocumentTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _DocumentTypeRepository = unitOfWork.Repository<DocumentType>();
        }

        public DocumentType Find(string Name)
        {
            return (from p in _DocumentTypeRepository.Instance
                    where p.DocumentTypeName == Name
                    select p
                        ).FirstOrDefault();
        }


        public DocumentType Find(int id)
        {
            return _DocumentTypeRepository.Find(id);
        }

        public DocumentType Create(DocumentType pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DocumentType>().Add(pt);
            return pt;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<DocumentType>().Delete(id);
        }

        public void Delete(DocumentType pt)
        {
            _unitOfWork.Repository<DocumentType>().Delete(pt);
        }

        public void Update(DocumentType pt)
        {
            pt.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<DocumentType>().Update(pt);
        }

        public IEnumerable<DocumentType> GetPagedList(int pageNumber, int pageSize, out int totalRecords)
        {
            var so = _unitOfWork.Repository<DocumentType>()
                .Query()
                .OrderBy(q => q.OrderBy(c => c.DocumentTypeName))
                .GetPage(pageNumber, pageSize, out totalRecords);

            return so;
        }

        public IQueryable<DocumentType> GetDocumentTypeList()
        {
            var pt = _unitOfWork.Repository<DocumentType>().Query().Get().OrderBy(m => m.DocumentTypeName);

            return pt;
        }

        public IEnumerable<DocumentType> GetDocumentTypeList(string DocumentCategoryName)
        {
            var pt = _unitOfWork.Repository<DocumentType>().Query().Get()
                            .Where(i => i.DocumentCategory.DocumentCategoryName == DocumentCategoryName);

            return pt;
        }

        public IEnumerable<DocumentType> FindByDocumentCategory(int DocumentCategoryId)
        {
            //return _unitOfWork.Repository<DocumentType>().Query().Get().Where(i => i.DocumentCategoryId  == DocumentCategoryId );
            //return db.DocumentType.Where(i => i.DocumentCategoryId == DocumentCategoryId);

            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];

            var DocTypes = from p in _DocumentTypeRepository.Instance
                           join t in _unitOfWork.Repository<DocumentTypeDivision>().Query().Get().Where(m => m.DivisionId == DivisionId) on p.DocumentTypeId equals t.DocumentTypeId into table
                           from tabdiv in table.DefaultIfEmpty()
                           join t2 in _unitOfWork.Repository<DocumentTypeSite>().Instance.Where(m => m.SiteId == SiteId) on p.DocumentTypeId equals t2.DocumentTypeId into table2
                           from tabsit in table2.DefaultIfEmpty()
                           where p.DocumentCategoryId == DocumentCategoryId && tabdiv == null && tabsit == null
                           orderby p.DocumentTypeName
                           select p;

            return (DocTypes).Include(m=>m.DocumentCategory);
        }



        public DocumentType FindByName(string DocumentTypeName)
        {
            return _unitOfWork.Repository<DocumentType>().Query().Get().Where(i => i.DocumentTypeName == DocumentTypeName).FirstOrDefault();
        }


        public DocumentType Add(DocumentType pt)
        {
            _unitOfWork.Repository<DocumentType>().Insert(pt);
            return pt;
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in _DocumentTypeRepository.Instance
                        orderby p.DocumentTypeName
                        select p.DocumentTypeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in _DocumentTypeRepository.Instance
                        orderby p.DocumentTypeName
                        select p.DocumentTypeId).FirstOrDefault();
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

                temp = (from p in _DocumentTypeRepository.Instance
                        orderby p.DocumentTypeName
                        select p.DocumentTypeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in _DocumentTypeRepository.Instance
                        orderby p.DocumentTypeName
                        select p.DocumentTypeId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }


        public Task<IEquatable<DocumentType>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DocumentType> FindAsync(int id)
        {
            throw new NotImplementedException();
        }




        public string FGetNewDocNo(string FieldName, string TableName, int DocTypeId, DateTime DocDate, int DivisionId, int SiteId)
        {
            SqlParameter SqlParameterFieldName = new SqlParameter("@FieldName", FieldName);
            SqlParameter SqlParameterTableName = new SqlParameter("@TableName", TableName);
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocTypeId", DocTypeId);
            SqlParameter SqlParameterDocDate = new SqlParameter("@DocDate", DocDate);
            SqlParameter SqlParameterDivisionId = new SqlParameter("@DivisionId", DivisionId);
            SqlParameter SqlParameterSiteId = new SqlParameter("@SiteId", SiteId);

            NewDocNoViewModel NewDocNoViewModel = _unitOfWork.SqlQuery<NewDocNoViewModel>("" + System.Configuration.ConfigurationManager.AppSettings["DataBaseSchema"] + ".GetNewDocNo @FieldName , @TableName ,@DocTypeId , @DocDate , @DivisionId , @SiteId ", SqlParameterFieldName, SqlParameterTableName, SqlParameterDocTypeId, SqlParameterDocDate, SqlParameterDivisionId, SqlParameterSiteId).FirstOrDefault();

            if (NewDocNoViewModel != null)
            {
                return NewDocNoViewModel.NewDocNo;
            }
            else
            {
                return null;
            }
        }

        public DateTime GetFinYearStart()
        {
            string FinStartDate = _unitOfWork.SqlQuery<string>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProFromDate").FirstOrDefault();

            if (FinStartDate != null)
            {
                return DateTime.Parse(FinStartDate);
            }
            else
            {
                return DateTime.Now;
            }
        }
    }

    public class NewDocNoViewModel
    {
        public string NewDocNo { get; set; }
    }
}
