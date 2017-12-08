using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class ProductPhoto : EntityBase
    {
     
        [MaxLength(50), Required]
        public string ProductPhotoName { get; set; }

        public byte[] Photo { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
