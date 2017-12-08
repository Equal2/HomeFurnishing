using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class PerkViewModel
    {
        public int JobOrderPerkId { get; set; }
        public int JobOrderHeaderId { get; set; }

        [Display(Name = "Perk Id")]
        public int PerkId { get; set; }

        [MaxLength(50, ErrorMessage = "Perk Name cannot exceed 50 characters"), Required]
        [Display(Name = "Perk Name")]
        public string PerkName { get; set; }

        [Display(Name = "Base Description")]
        public string BaseDescription { get; set; }

        public decimal Base { get; set; }

        [Display(Name = "Worth Description")]
        public string WorthDescription { get; set; }

        public decimal Worth { get; set; }

        public Decimal CostConversionMultiplier { get; set; }


        [Display(Name = "Is Active ?")]
        public Boolean IsActive { get; set; }

        public bool IsEditableRate { get; set; }
    

    }
}
