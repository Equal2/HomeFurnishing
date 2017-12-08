using System.ComponentModel.DataAnnotations;

// New namespace imports:
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.ViewModels
{
    public class DesignColourConsumptionHeaderViewModel : EntityBase
    {
        public int BaseProductId { get; set; }
        public string BaseProductName { get; set; }

        [Display(Name = "Design"),Required]
        public int ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }

        [Display(Name = "Colour"), Required]
        public int ColourId { get; set; }
        public string ColourName { get; set; }

        [Display(Name = "Quality")]
        public int ProductQualityId { get; set; }
        public string ProductQualityName { get; set; }

        public string EntryMode { get; set; }

        public Decimal? Weight { get; set; }
    }

    
}
