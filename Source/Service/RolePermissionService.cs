using System.Collections.Generic;
using System.Linq;
using Data.Infrastructure;
using Model.ViewModel;
using System;
using Data.Models;
using System.Data.SqlClient;
using Model.ViewModels;

namespace Service
{
    public interface IRolePermissionService : IDisposable
    {
        IEnumerable<RolePermissionViewModel> RolePermissionDetail(string RoleId);
        IEnumerable<RoleProcessPermissionViewModel> RoleProcessPermissionDetail(string RoleId, int DocTypeId);
    }

    public class RolePermissionService : IRolePermissionService
    {
        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

      

        public RolePermissionService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;            
        }





        public IEnumerable<RolePermissionViewModel> RolePermissionDetail(string RoleId)
        {
            SqlParameter SqlParameterRoleId = new SqlParameter("@RoleId", RoleId);

            string mQry = "";

            mQry = @"SELECT D.DocumentTypeId, D.DocumentTypeName, 
                    Max(Ca.ControllerName) AS ControllerName, 
                    Max(CASE WHEN Ca.DisplayName = 'Add' THEN Ca.ActionName END) AS AddActionName, 
                    Convert(BIT,Sum(CASE WHEN Ca.DisplayName = 'Add' THEN IsNull(VRolesDocTypes.IsPermissionGranted,0) END)) AS [Add],
                    Max(CASE WHEN Ca.DisplayName = 'Edit' THEN Ca.ActionName END) AS EditActionName, 
                    Convert(BIT,Sum(CASE WHEN Ca.DisplayName = 'Edit' THEN IsNull(VRolesDocTypes.IsPermissionGranted,0) END)) AS [Edit],
                    Max(CASE WHEN Ca.DisplayName = 'Delete' THEN Ca.ActionName END) AS DeleteActionName, 
                    Convert(BIT,Sum(CASE WHEN Ca.DisplayName = 'Delete' THEN IsNull(VRolesDocTypes.IsPermissionGranted,0) END)) AS [Delete],
                    Max(CASE WHEN Ca.DisplayName = 'Print' THEN Ca.ActionName END) AS PrintActionName, 
                    Convert(BIT,Sum(CASE WHEN Ca.DisplayName = 'Print' THEN IsNull(VRolesDocTypes.IsPermissionGranted,0) END)) AS [Print],
                    Max(CASE WHEN Ca.DisplayName = 'Submit' THEN Ca.ActionName END) AS SubmitActionName, 
                    Convert(BIT,Sum(CASE WHEN Ca.DisplayName = 'Submit' THEN IsNull(VRolesDocTypes.IsPermissionGranted,0) END)) AS [Submit]
                    FROM Web.ControllerActions Ca
                    LEFT JOIN Web.DocumentTypes D ON Ca.ControllerName = D.ControllerName
                    LEFT JOIN (SELECT 1 AS IsPermissionGranted, Rd.DocTypeId, Rd.ControllerName, Rd.ActionName
			                    FROM Web.RolesDocTypes Rd
			                    WHERE Rd.RoleId = @RoleId
                    ) AS VRolesDocTypes ON D.DocumentTypeId = VRolesDocTypes.DocTypeId
		                    AND Ca.ControllerName = VRolesDocTypes.ControllerName
		                    AND Ca.ActionName = VRolesDocTypes.ActionName
                    WHERE D.DocumentTypeId IS NOT NULL
                    AND Ca.DisplayName IN ('Add', 'Edit', 'Delete', 'Print', 'Submit')
                    GROUP BY D.DocumentTypeId, D.DocumentTypeName 
                    Order By D.DocumentTypeName ";

            IEnumerable<RolePermissionViewModel> RolePermissionViewModel = db.Database.SqlQuery<RolePermissionViewModel>(mQry, SqlParameterRoleId).ToList();


            return RolePermissionViewModel;

        }

        public IEnumerable<RoleProcessPermissionViewModel> RoleProcessPermissionDetail(string RoleId, int DocTypeId)
        {
            SqlParameter SqlParameterRoleId = new SqlParameter("@RoleId", RoleId);
            SqlParameter SqlParameterDocTypeId = new SqlParameter("@DocTypeId", DocTypeId);

            string mQry = "";

            mQry = @"SELECT D.DocumentTypeId, P.ProcessId, P.ProcessName,
                        Convert(BIT,IsNull(VRolesDocTypeProcess.IsPermissionGranted,0)) AS IsActive
                        FROM Web.DocumentTypes D
                        LEFT JOIN Web.AspNetRoles R ON 1=1
                        LEFT JOIN Web.Processes P ON 1=1
                        LEFT JOIN (SELECT 1 AS IsPermissionGranted, Rdp.RoleId, Rdp.DocTypeId, Rdp.ProcessId
                                    FROM Web.RolesDocTypeProcesses Rdp
                                    WHERE Rdp.RoleId = @RoleId AND Rdp.DocTypeId = @DocTypeId
                        ) AS VRolesDocTypeProcess ON R.Id = VRolesDocTypeProcess.RoleId
                                AND D.DocumentTypeId = VRolesDocTypeProcess.DocTypeId
                                AND P.ProcessId = VRolesDocTypeProcess.ProcessId
                        WHERE R.Id = @RoleId AND D.DocumentTypeId = @DocTypeId ";

            IEnumerable<RoleProcessPermissionViewModel> RoleProcessPermissionViewModel = db.Database.SqlQuery<RoleProcessPermissionViewModel>(mQry, SqlParameterRoleId, SqlParameterDocTypeId).ToList();

            return RoleProcessPermissionViewModel;
        }

        //public bool IsActionAllowed(List<string> UserRoles, int DocTypeId, int? ProcessId, string ControllerName, string ActionName)
        //{
        //    bool IsAllowed = true;

        //    var RolesDocType = (from L in db.RolesDocType 
        //                        join R in db.Roles on L.RoleId equals R.Id
        //                        where UserRoles.Contains(R.Name) && L.DocTypeId == DocTypeId 
        //                            && L.ControllerName == ControllerName && L.ActionName == ActionName 
        //                        select L).FirstOrDefault();

        //    if (RolesDocType == null)
        //    {
        //        IsAllowed = false;
        //    }
        //    else
        //    {
        //        if (ProcessId != null)
        //        {
        //            var RolesDocTypeProcess = (from L in db.RolesDocTypeProcess
        //                                       join R in db.AspNetRole on L.RoleId equals R.Id
        //                                       where UserRoles.Contains(R.Name) && L.DocTypeId == DocTypeId 
        //                                            && L.ProcessId == ProcessId 
        //                                       select L).FirstOrDefault();
        //            if (RolesDocTypeProcess == null)
        //                IsAllowed = false;
        //        }
        //    }

        //    return IsAllowed;
        //}


        public bool IsActionAllowed(List<string> UserRoles, int DocTypeId, int? ProcessId, string ControllerName, string ActionName)
        {
            bool IsAllowed = true;

            var ExistingData = (from L in db.RolesDocType select L).FirstOrDefault();
            if (ExistingData == null)
                return true;

            foreach(string RoleName in UserRoles)
            {
                var RolesDocType = (from L in db.RolesDocType
                                    join R in db.Roles on L.RoleId equals R.Id
                                    where R.Name == RoleName && L.DocTypeId == DocTypeId
                                        && L.ControllerName == ControllerName && L.ActionName == ActionName
                                    select L).FirstOrDefault();

                if (RolesDocType == null)
                {
                    IsAllowed = false;
                }
                else
                {
                    if (ProcessId != null)
                    {
                        var RolesDocTypeProcess_Any = (from L in db.RolesDocTypeProcess
                                                       join R in db.Roles on L.RoleId equals R.Id
                                                   where R.Name == RoleName && L.DocTypeId == DocTypeId
                                                   select L).FirstOrDefault();
                        if (RolesDocTypeProcess_Any != null)
                        {
                            var RolesDocTypeProcess = (from L in db.RolesDocTypeProcess
                                                       join R in db.Roles on L.RoleId equals R.Id
                                                       where R.Name == RoleName && L.DocTypeId == DocTypeId
                                                            && L.ProcessId == ProcessId
                                                       select L).FirstOrDefault();
                            if (RolesDocTypeProcess == null)
                                IsAllowed = false;
                            else
                                IsAllowed = true;
                        }
                        else
                        {
                            IsAllowed = true;
                        }
                    }
                }
            }

            return IsAllowed;
        }

        public void Dispose()
        {
        }
    }


    public class RolePermissionViewModel
    {
        public int DocumentTypeId { get; set; }
        public string DocumentTypeName { get; set; }
        public string ControllerName { get; set; }
        public string AddActionName { get; set; }
        public bool Add { get; set; }
        public string EditActionName { get; set; }
        public bool Edit { get; set; }
        public string DeleteActionName { get; set; }
        public bool Delete { get; set; }
        public string PrintActionName { get; set; }
        public bool Print { get; set; }
        public string SubmitActionName { get; set; }
        public bool Submit { get; set; }

    }


    public class RoleProcessPermissionViewModel
    {
        public int DocumentTypeId { get; set; }
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public bool IsActive { get; set; }
    }
}

