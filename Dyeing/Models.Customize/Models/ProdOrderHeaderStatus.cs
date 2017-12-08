using Model;
using Models.BasicSetup.Models;
using Models.Company.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Customize.Models
{
    public class ProdOrderHeaderStatus : EntityBase
    {
        [Key]
        [ForeignKey("ProdOrderHeader")]
        public int? ProdOrderHeaderId { get; set; }
        public virtual ProdOrderHeader ProdOrderHeader { get; set; }
    }
}
