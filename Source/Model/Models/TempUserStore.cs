 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    //temprory user password store model
    //ToDo: need to remove after Identity 2.0 implimentation
    public class TempUserStore :EntityBase
    {
        [Key]
        public int TempUserStoreId { get; set; }

         [Required]
        public string UserId { get; set; }

         [Required]
        public string UserName { get; set; }

         [Required]
        public string Password { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
     
    }
}
