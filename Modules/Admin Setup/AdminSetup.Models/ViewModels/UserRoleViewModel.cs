using Model;
using System;

namespace AdminSetup.Models.ViewModels
{
    public class UserRoleViewModel : EntityBase
    {        
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FromUserId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; }
        public string RolesList { get; set; }
        public string RoleIdList { get; set; }
        public DateTime? ExpiryDate { get; set; }

    }
}
