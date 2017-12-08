using Microsoft.AspNet.Identity.EntityFramework;
using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminSetup.Models.Models
{
    public class UserRole : EntityBase
    {
        [Key]
        public int UserRoleId { get; set; }
        
        [MaxLength(128)]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public virtual IdentityUser User { get; set; }
                
        [MaxLength(128)]
        public string RoleId { get; set; }

        public DateTime? ExpiryDate { get; set; }

    }
}
