using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using Model.Models;
using System;
using Microsoft.AspNet.Identity;

namespace Model.ViewModels
{
    public class ReviewSampleViewModel
    {
        [Key]
        public int ProductSampleId { get; set; }

        [MaxLength(50), Required]
        [Display(Name = "Sample Name")]
        public string SampleName { get; set; }

        [Display(Name = "Sample Description")]
        public string SampleDescription { get; set; }

        [Display(Name = "Email Date"), DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MMM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EmailDate { get; set; }

        public string SupplierName { get; set; }
        public byte[] ProductPicture { get; set; }



    }
}
