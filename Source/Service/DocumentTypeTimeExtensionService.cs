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
using Model.ViewModels;

namespace Service
{
    public interface IDocumentTypeTimeExtensionService : IDisposable
    {
        DocumentTypeTimeExtension Create(DocumentTypeTimeExtension pt,string UserName);
        void Delete(int id);
        void Delete(DocumentTypeTimeExtension pt);
        void Update(DocumentTypeTimeExtension pt,string UserName);
        IQueryable<DocumentTypeTimeExtensionViewModel> GetDocumentTypeTimeExtensionList(string UName);
        Task<IEquatable<DocumentTypeTimeExtension>> GetAsync();
        Task<DocumentTypeTimeExtension> FindAsync(int id);
        DocumentTypeTimeExtension Find(int id);
        int NextId(int id);
        int PrevId(int id);
        IQueryable<ComboBoxResult> GetDocTypesHelpList(string Type,string SearchTerm);
        DocumentTypeTimeExtensionViewModel GetHistory(string UserName);
    }

    public class DocumentTypeTimeExtensionService : IDocumentTypeTimeExtensionService
    {
        private ApplicationDbContext db;
        public DocumentTypeTimeExtensionService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public DocumentTypeTimeExtension Find(int id)
        {
            return db.DocumentTypeTimeExtension.Find(id);
        }

        public DocumentTypeTimeExtension Create(DocumentTypeTimeExtension pt, string UserName)
        {
            pt.CreatedBy = UserName;
            pt.CreatedDate = DateTime.Now;
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;

            pt.ObjectState = ObjectState.Added;
            db.DocumentTypeTimeExtension.Add(pt);

            return pt;
        }

        public void Delete(int id)
        {
            DocumentTypeTimeExtension pd = db.DocumentTypeTimeExtension.Find(id);

            pd.ObjectState = Model.ObjectState.Deleted;
            db.DocumentTypeTimeExtension.Remove(pd);
        }

        public void Delete(DocumentTypeTimeExtension pt)
        {
            pt.ObjectState = Model.ObjectState.Deleted;
            db.DocumentTypeTimeExtension.Remove(pt);
        }

        public void Update(DocumentTypeTimeExtension pt, string UserName)
        {
            pt.ModifiedBy = UserName;
            pt.ModifiedDate = DateTime.Now;
            pt.ObjectState = ObjectState.Modified;
            db.DocumentTypeTimeExtension.Add(pt);
        }

        public IQueryable<DocumentTypeTimeExtensionViewModel> GetDocumentTypeTimeExtensionList(string UName)
        {
            List<string> UserRoles = (List<string>)System.Web.HttpContext.Current.Session["Roles"];
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            bool Admin = UserRoles.Contains("Admin");

            return (from p in db.DocumentTypeTimeExtension
                    where p.SiteId == SiteId && p.DivisionId == DivisionId && (Admin ? 1 == 1 : p.CreatedBy == UName)
                    orderby p.DocumentTypeTimeExtensionId descending
                    select new DocumentTypeTimeExtensionViewModel
                    {
                        DocTypeId = p.DocTypeId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        DivisionId = p.DivisionId,
                        SiteId = p.SiteId,
                        DocDate = p.DocDate,
                        DocumentTypeTimeExtensionId = p.DocumentTypeTimeExtensionId,
                        ExpiryDate = p.ExpiryDate,
                        NoOfRecords = p.NoOfRecords,
                        Reason = p.Reason,
                        Type = p.Type,
                        UserName = p.UserName,
                    });

        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {
                temp = (from p in db.DocumentTypeTimeExtension
                        orderby p.DocumentTypeTimeExtensionId
                        select p.DocumentTypeTimeExtensionId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();
            }
            else
            {
                temp = (from p in db.DocumentTypeTimeExtension
                        orderby p.DocumentTypeTimeExtensionId
                        select p.DocumentTypeTimeExtensionId).FirstOrDefault();
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

                temp = (from p in db.DocumentTypeTimeExtension
                        orderby p.DocumentTypeTimeExtensionId
                        select p.DocumentTypeTimeExtensionId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.DocumentTypeTimeExtension
                        orderby p.DocumentTypeTimeExtensionId
                        select p.DocumentTypeTimeExtensionId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }


        public IQueryable<ComboBoxResult> GetDocTypesHelpList(string Type,string SearchTerm)
        {

            var Query = from p in db.DocumentType
                        select new
                        {
                            Id = p.DocumentTypeId,
                            Name = p.DocumentTypeName,
                            GatePassGenerated=p.SupportGatePass,
                        };

            if (Type == DocumentTimePlanTypeConstants.GatePassCancel || Type == DocumentTimePlanTypeConstants.GatePassCreate)
                Query = Query.Where(m => m.GatePassGenerated == true);

            if (!string.IsNullOrEmpty(SearchTerm))
                Query = Query.Where(m => m.Name.ToLower().Contains(SearchTerm.ToLower()));

            return from p in Query
                   where p.GatePassGenerated == p.GatePassGenerated
                   orderby p.Name
                   select new ComboBoxResult
                   {
                       id = p.Id.ToString(),
                       text = p.Name,
                   };


        }

        public DocumentTypeTimeExtensionViewModel GetHistory(string UserName)
        {
            return (from p in db.DocumentTypeTimeExtension
                    where p.CreatedBy == UserName
                    orderby p.DocumentTypeTimeExtensionId descending
                    select new DocumentTypeTimeExtensionViewModel
                    {
                        NoOfRecords = p.NoOfRecords,
                        DocTypeId = p.DocTypeId,
                        DocTypeName = p.DocType.DocumentTypeName,
                        ExpiryDate = p.ExpiryDate,
                        DocDate = p.DocDate,
                        Type = p.Type,
                        UserName = p.UserName,
                        Reason = p.Reason,
                    }).FirstOrDefault();
        }

        public void Dispose()
        {
        }


        public Task<IEquatable<DocumentTypeTimeExtension>> GetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<DocumentTypeTimeExtension> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
