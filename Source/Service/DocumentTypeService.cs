using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Data;
using Data.Infrastructure;
using Model.Models;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using Model.ViewModels;
using Model.ViewModel;


namespace Service
{
    public interface IDocumentTypeService : IDisposable
    {
        DocumentType Create(DocumentType pt);
        void Delete(int id);
        void Delete(DocumentType pt);
        DocumentType Find(string Name);
        DocumentType Find(int id);
        IEnumerable<DocumentType> GetPagedList(int pageNumber, int pageSize, out int totalRecords);
        void Update(DocumentType pt);
        DocumentType Add(DocumentType pt);
        IQueryable<DocumentType> GetDocumentTypeList();
        IEnumerable<DocumentType> GetDocumentTypeList(string DocumentCategoryName);
        Task<IEquatable<DocumentType>> GetAsync();
        Task<DocumentType> FindAsync(int id);
        int NextId(int id);
        int PrevId(int id);

        IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentTypeHeaderAttribute(int DocumentTypeId);
    }

    public class DocumentTypeService : IDocumentTypeService
    {
        private ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;
        private readonly Repository<DocumentType> _DocumentTypeRepository;
        RepositoryQuery<DocumentType> DocumentTypeRepository;
        public DocumentTypeService(IUnitOfWorkForService unitOfWork)
        {
            this.db = new ApplicationDbContext();
            _unitOfWork = unitOfWork;
            _DocumentTypeRepository = new Repository<DocumentType>(db);
            DocumentTypeRepository = new RepositoryQuery<DocumentType>(_DocumentTypeRepository);
        }

        public DocumentType Find(string Name)
        {
            return (from p in db.DocumentType
                    where p.DocumentTypeName == Name
                    select p
                        ).FirstOrDefault();
        }


        public DocumentType Find(int id)
        {
            return db.DocumentType.Find(id);
        }

        public DocumentType Create(DocumentType pt)
        {
            pt.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<DocumentType>().Insert(pt);
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

            var DocTypes = from p in db.DocumentType
                           join t in db.DocumentTypeDivision.Where(m => m.DivisionId == DivisionId) on p.DocumentTypeId equals t.DocumentTypeId into table
                           from tabdiv in table.DefaultIfEmpty()
                           join t2 in db.DocumentTypeSite.Where(m => m.SiteId == SiteId) on p.DocumentTypeId equals t2.DocumentTypeId into table2
                           from tabsit in table2.DefaultIfEmpty()
                           where p.DocumentCategoryId == DocumentCategoryId && tabdiv == null && tabsit == null
                           orderby p.DocumentTypeName
                           select p;

            return (DocTypes).Include("DocumentCategory");
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
                temp = (from p in db.DocumentType
                        orderby p.DocumentTypeName
                        select p.DocumentTypeId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DocumentType
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

                temp = (from p in db.DocumentType
                        orderby p.DocumentTypeName
                        select p.DocumentTypeId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DocumentType
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

            NewDocNoViewModel NewDocNoViewModel = db.Database.SqlQuery<NewDocNoViewModel>("" + System.Configuration.ConfigurationManager.AppSettings["DataBaseSchema"] + ".GetNewDocNo @FieldName , @TableName ,@DocTypeId , @DocDate , @DivisionId , @SiteId ", SqlParameterFieldName, SqlParameterTableName, SqlParameterDocTypeId, SqlParameterDocDate, SqlParameterDivisionId, SqlParameterSiteId).FirstOrDefault();

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
            string FinStartDate = db.Database.SqlQuery<string>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProFromDate").FirstOrDefault();

            if (FinStartDate != null)
            {
                return DateTime.Parse(FinStartDate);
            }
            else
            {
                return DateTime.Now;
            }
        }

        public IEnumerable<DocumentTypeHeaderAttributeViewModel> GetDocumentTypeHeaderAttribute(int DocumentTypeId)
        {
            return (from p in _unitOfWork.Repository<DocumentTypeHeaderAttribute>().Instance
                    where p.DocumentTypeId == DocumentTypeId
                    select new DocumentTypeHeaderAttributeViewModel
                    {
                        DataType = p.DataType,
                        ListItem = p.ListItem,
                        Value = p.Value,
                        Name = p.Name,
                        DocumentTypeHeaderAttributeId = p.DocumentTypeHeaderAttributeId,
                        DocumentTypeId = p.DocumentTypeId,
                    });
        }


    }
}
