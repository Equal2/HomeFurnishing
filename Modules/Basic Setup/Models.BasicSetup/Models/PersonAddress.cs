using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Spatial;
using Model;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Company.Models;


namespace Models.BasicSetup.Models
{
    public class PersonAddress : EntityBase, IHistoryLog
    {
        [Key]
        public int PersonAddressID { get; set; }        

        [ForeignKey("Person"), Display(Name = "Person Name")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
                

        //[ForeignKey("AddressType"), Display(Name = "Contact Type")]
        //public int AddressTypeId { get; set; }
        //public virtual GenericType AddressType { get; set; }

        public string AddressType { get; set; }


        public string Address { get; set; }

        [Display(Name="City")]
        public int? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }


        [MaxLength(6, ErrorMessage = "{0} can not exceed {1} characters")]
        public string Zipcode { get; set; }
        
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
        
    }
}

