using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Models
{
    public class City : EntityBase, IHistoryLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int CityId { get; set; }
        [Display (Name="Name")]
        [MaxLength(50, ErrorMessage = "City Name cannot exceed 50 characters"), Required]
        [Index("IX_City_CityName", IsUnique = true)]
        public string CityName { get; set; }
                     
        [Display(Name = "State")]
        public int StateId { get; set; }
        [ForeignKey("StateId")]
        public virtual State State { get; set; }
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
