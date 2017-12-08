using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobOrderLineExtended : EntityBase
    {
        [Key]
        [ForeignKey("JobOrderLine")]
        public int JobOrderLineId { get; set; }
        public JobOrderLine JobOrderLine { get; set; }

        public Decimal? TestingQty { get; set; }
        public Decimal? SubRecipeQty { get; set; }

        //public int ExtendedTestValue1 { get; set; }
    }
}
