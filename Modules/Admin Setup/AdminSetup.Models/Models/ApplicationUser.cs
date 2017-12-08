using Microsoft.AspNet.Identity.EntityFramework;
using Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminSetup.Models.Models
{
    public class ApplicationUser : IdentityUser, IObjectState
    {
        public ApplicationUser()
        {
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [NotMapped]

        public ObjectState ObjectState { get; set; }
    }
}
