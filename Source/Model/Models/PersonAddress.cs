using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Spatial;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class PersonAddress : EntityBase, IHistoryLog
    {
        [Key]
        public int PersonAddressID { get; set; }        

        [ForeignKey("Person"), Display(Name = "Person Name")]
        public int PersonId { get; set; }
        public virtual Person Person { get; set; }
        
        public string AddressType { get; set; }
               
        public string Address { get; set; }

        [Display(Name="City")]
        public int? CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        [Display(Name = "Area")]
        public int? AreaId { get; set; }
        [ForeignKey("AreaId")]
        public virtual Area Area { get; set; }

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
