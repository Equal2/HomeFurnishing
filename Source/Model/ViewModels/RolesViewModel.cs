using System;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class RolesViewModel
    {
        public string RoleName { get; set; }
        public string RoleId { get; set; }

    }

    public class RoleSitePermissionViewModel
    {
        public string DivisionId { get; set; }
        public string SiteId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }

    }
    public class RolesSiteViewModel
    {
        public int RolesSiteId { get; set; }        
        public string RoleId { get; set; }
        public string RoleName { get; set; }        
        public int SiteId { get; set; }
        public string SiteName { get; set; }
    }
    public class RolesDivisionViewModel
    {
        public int RolesDivisionId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
    }
    public class RolesMenuViewModel
    {
        public int RolesMenuId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }        
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public bool FullHeaderPermission { get; set; }
        public bool FullLinePermission { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
    }

    public class CopyRolesViewModel
    {
        [Required]
        public string FromRoleId { get; set; }
        [Required]
        public string ToRoleId{get;set;}
    }
}
