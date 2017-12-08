using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Login.Models
{
    public class UserApplication
    {
        [Key]
        public int UserApplicationId { get; set; }

        [ForeignKey("Application")]
        public int ApplicationId { get; set; }
        public virtual Application Application { get; set; }
        public string UserId { get; set; }
        public DateTime? InvitationDate { get; set; }
        public string InvitationBy { get; set; }
        public string UserRole { get; set; }
        public int? SiteId { get; set; }
        public int? DivisionId { get; set; }


        //public string RefereeId { get; set; }

        //[ForeignKey("UserReferral")]
        //public Guid ReferralId { get; set; }
        //public virtual UserReferral  UserReferral{ get; set; }

    }
}