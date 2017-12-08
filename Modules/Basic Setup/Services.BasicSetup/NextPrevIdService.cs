using System.Linq;
using System;
using System.Data.SqlClient;
using System.Configuration;
using Infrastructure.IO;

namespace Services.BasicSetup
{
    public interface INextPrevIdService : IDisposable
    {
        int GetNextPrevId(int DocId, int DocTypeId, string UserName, string ForAction, string HeaderTableName, string HeaderTablePK, string NextPrev);
    }

    public class NextPrevIdService : INextPrevIdService
    {
        private readonly IUnitOfWork _unitOfWork;
     
        public NextPrevIdService(IUnitOfWork unitofWork)
        {
            _unitOfWork = unitofWork;
        }


        public int GetNextPrevId(int DocId, int DocTypeId, string UserName, string ForAction, string HeaderTableName, string HeaderTablePK, string NextPrev)
        {
            SqlParameter SqlParameterUserName = new SqlParameter("@UserName", UserName);
            SqlParameter SqlParameterForAction = new SqlParameter("@ForAction", ForAction);
            SqlParameter SqlParameterHeaderTableName = new SqlParameter("@HeaderTableName", HeaderTableName);
            SqlParameter SqlParameterHeaderTablePkFieldName = new SqlParameter("@HeaderTablePkFieldName", HeaderTablePK);
            SqlParameter SqlParameterDocId = new SqlParameter("@DocId", DocId);
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocTypeId", DocTypeId);
            SqlParameter SqlParameterNextPrevious = new SqlParameter("@NextPrevious", NextPrev);

            int Id = _unitOfWork.SqlQuery<int>("" + ConfigurationManager.AppSettings["DataBaseSchema"] + ".spGetNextPreviousId @UserName, @ForAction, @HeaderTableName, @HeaderTablePkFieldName, @DocId, @DocTypeId, @NextPrevious", SqlParameterUserName, SqlParameterForAction, SqlParameterHeaderTableName, SqlParameterHeaderTablePkFieldName, SqlParameterDocId, SqlParameterDocTypeId, SqlParameterNextPrevious).FirstOrDefault();

            return Id;
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
       
    }
}
