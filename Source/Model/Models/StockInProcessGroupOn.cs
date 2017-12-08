using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class StockInProcessGroupOn : EntityBase
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100),Required]
        public string Items { get; set; }
    }
}
