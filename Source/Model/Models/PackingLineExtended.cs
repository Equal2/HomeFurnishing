﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class PackingLineExtended : EntityBase
    {
        [Key]
        [ForeignKey("PackingLine")]
        [Display(Name = "PackingLine")]
        public int PackingLineId { get; set; }
        public virtual PackingLine PackingLine { get; set; }
        public Decimal? Length { get; set; }
        public Decimal? Width { get; set; }
        public Decimal? Height { get; set; }

    }
}
