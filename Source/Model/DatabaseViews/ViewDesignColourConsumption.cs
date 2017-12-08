using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.DatabaseViews
{
    [Table("ViewDesignColourConsumption")]
    public class ViewDesignColourConsumption
    {
        [Key]
        public int ProductGroupId { get; set; }
        public string ProductGroupName { get; set; }
        public int ColourId { get; set; }
        public string ColourName { get; set; }
        public int ProductQualityId { get; set; }
        public string ProductQualityName { get; set; }
        public Decimal? Weight { get; set; }
        public int BomProductId { get; set; }

    }
}
