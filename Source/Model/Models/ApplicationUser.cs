using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class ApplicationUser : IdentityUser, IObjectState
    {
        public ApplicationUser()
        {

        }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        // HomeTown will be stored in the same table as Users
        //public string HomeTown { get; set; }

        // [Required]
        //public string Email { get; set; }

        // FirstName & LastName will be stored in a different table called MyUserInfo
        //public virtual UserInfo UserInfo { get; set; }


        //public string CreatedBy { get; set; }
        //public string ModifiedBy { get; set; }
        //public DateTime CreatedDate { get; set; }
        //public DateTime ModifiedDate { get; set; }

        [NotMapped]

        public ObjectState ObjectState { get; set; } //TODO: Renamed since a possible coflict with State entity column
    }

    public class UserInfo:EntityBase
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

}
