using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Company.Models
{
    public class Country : EntityBase, IHistoryLog
    {
        public Country()
        {
            States = new List<State>();
        }

        [Key]
        [Display(Name = "Country Id")]
        public int CountryId { get; set; }

        [MaxLength(50, ErrorMessage = "Country Name cannot exceed 50 characters"), Required]
        [Index("IX_Country_Country", IsUnique = true)]
        [Display(Name = "Country Name")]
        public string CountryName { get; set; }

        public ICollection<State> States { get; set; }
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
