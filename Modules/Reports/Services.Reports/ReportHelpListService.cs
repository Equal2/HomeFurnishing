using Infrastructure.IO;
using Models.BasicSetup.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Services.Reports
{
    public interface IReportHelpListService : IDisposable
    {
        #region Getters
        /// <summary>
        /// *General Function*
        /// This function will create the help list for Reports
        /// </summary>
        /// <param name="SqlProcGet">Sql Procedure for getting the help list</param>
        /// <param name="searchTerm">user search term</param>
        /// <param name="pageSize">no of records to fetch for each page</param>
        /// <param name="pageNum">current page size </param>
        /// <returns>ComboBoxPagedResult</returns>
        CustomComboBoxPagedResult GetSelect2HelpList(string SqlProcGet, string searchTerm, int pageSize, int pageNum);
        #endregion

        #region Setters

       
        /// <summary>
        /// *General Function*
        /// This function will return list of object based on the Ids
        /// </summary>
        /// <param name="SqlProcSet">Sql procedure for getting helplist</param>
        /// <param name="Id">PrimaryKey of the record(',' seperated string)</param>
        /// <returns>IEnumerable of ComboBoxResult</returns>
        List<ComboBoxResult> SetSelct2Data(string Id, string SqlProcSet);

        /// <summary>
        /// *General Function*
        /// This function will return list of object based on the Ids
        /// </summary>
        /// <param name="SqlProcSet">Sql procedure for getting helplist</param>
        /// <param name="Id">PrimaryKey of the record</param>
        /// <returns>ComboBoxResult</returns>
        ComboBoxResult SetSingleSelect2Data(int Id, string SqlProcSet);

        /// <summary>
        /// *General Function*
        /// This function will return Date from the Sql procedure
        /// </summary>
        /// <param name="SqlProcSet">Sql procedure for getting helplist</param>
        /// <returns>string</returns>
        string SetDate(string SqlProcSet);

        #endregion
    }

    public class ReportHelpListService : IReportHelpListService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportHelpListService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public CustomComboBoxPagedResult GetSelect2HelpList(string SqlProcGet, string searchTerm, int pageSize, int pageNum)
        {
            int SiteId = (int)System.Web.HttpContext.Current.Session["SiteId"];
            int DivisionId = (int)System.Web.HttpContext.Current.Session["DivisionId"];

            if (SqlProcGet.Contains(" ") == true)
                SqlProcGet = SqlProcGet + " ,";

            string mQry;

            mQry = " " + SqlProcGet + " @SearchString = '" + searchTerm + "', @PageSize =" + pageSize.ToString() + ", @PageNo =" + (pageNum - 1).ToString() + ", @SiteId= " + SiteId.ToString() + ", @DivisionId= " + DivisionId.ToString();
            IEnumerable<CustomComboBoxResult> Select2List = _unitOfWork.SqlQuery<CustomComboBoxResult>(mQry).ToList();

            CustomComboBoxPagedResult pagedAttendees = new CustomComboBoxPagedResult();
            pagedAttendees.Results = Select2List.ToList();
            pagedAttendees.Total = Select2List.Count() > 0 ? Select2List.FirstOrDefault().RecCount : 0;

            return pagedAttendees;

        }

        public List<ComboBoxResult> SetSelct2Data(string Id, string SqlProcSet)
        {
            if (SqlProcSet.Contains(" ") == true)
                SqlProcSet = SqlProcSet + " ,";

            List<ComboBoxResult> ProductJson = _unitOfWork.SqlQuery<ComboBoxResult>(" " + SqlProcSet + " @Ids = \'" + Id + "\'").ToList();

            return ProductJson;
        }

        public ComboBoxResult SetSingleSelect2Data(int Id, string SqlProcSet)
        {
            SqlParameter SqlParameterDocId = new SqlParameter("@Ids", Id);
            ComboBoxResult ProductJson = _unitOfWork.SqlQuery<ComboBoxResult>(" " + SqlProcSet + " @Ids ", SqlParameterDocId).FirstOrDefault();

            return ProductJson;
        }

        public string SetDate(string SqlProcSet)
        {
            return _unitOfWork.SqlQuery<string>(SqlProcSet).FirstOrDefault();
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
