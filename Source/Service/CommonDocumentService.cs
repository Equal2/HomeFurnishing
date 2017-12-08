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
using System.Data.SqlClient;
using System.Configuration;

namespace Service
{
    public interface ICommonDocumentService : IDisposable
    {
        int NextId(int id, string TableName, string UserName, string Status);
        int PrevId(int id, string TableName, string UserName, string Status);
    }

    public class CommonDocumentService : ICommonDocumentService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public CommonDocumentService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public int NextId(int id, string TableName, string UserName, string Status)
        {
            SqlParameter SqlParameterDocId = new SqlParameter("@DocId", id);
            SqlParameter SqlParameterTableName = new SqlParameter("@TableName", TableName);
            SqlParameter SqlParameterUserName = new SqlParameter("@UserName", UserName);
            SqlParameter SqlParameterStatus = new SqlParameter("@Status", Status);

            int NextId = db.Database.SqlQuery<int>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetStockForPacking @DocId, @TableName, @UserName, @Status", SqlParameterDocId, SqlParameterTableName, SqlParameterUserName, SqlParameterStatus).FirstOrDefault();

            return NextId;
        }

        public int PrevId(int id, string TableName, string UserName, string Status)
        {

            SqlParameter SqlParameterDocId = new SqlParameter("@DocId", id);
            SqlParameter SqlParameterTableName = new SqlParameter("@TableName", TableName);
            SqlParameter SqlParameterUserName = new SqlParameter("@UserName", UserName);
            SqlParameter SqlParameterStatus = new SqlParameter("@Status", Status);

            int PrevId = db.Database.SqlQuery<int>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".ProcGetStockForPacking @DocId, @TableName, @UserName, @Status", SqlParameterDocId, SqlParameterTableName, SqlParameterUserName, SqlParameterStatus).FirstOrDefault();

            return PrevId;
        }

        public void Dispose()
        {
        }
    }
}
