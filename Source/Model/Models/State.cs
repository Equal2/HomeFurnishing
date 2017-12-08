using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class State : EntityBase, IHistoryLog
    {
        public State()
        {
            Cities  = new List<City>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int StateId { get; set; }

        [Display(Name = "Code")]
        [MaxLength(20, ErrorMessage = "State Code cannot exceed 20 characters"), Required]
        [Index("IX_State_StateCode", IsUnique = true)]
        public string StateCode { get; set; }

        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "State Name cannot exceed 50 characters"), Required]
        [Index("IX_State_StateName", IsUnique = true)]
        public string StateName { get; set; }


        [ForeignKey("Country")]
        [Display(Name = "Country")]
        public int CountryId { get; set; }
        public virtual Country Country { get; set; }

        public ICollection<City> Cities { get; set; }
        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

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
