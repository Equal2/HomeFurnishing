using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ReportUIDValues : EntityBase
    {
        [Key]
        public int Id { get; set; }
        public Guid UID { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
