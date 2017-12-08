using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login.Models
{
    public class NotificationUser : EntityBase
    {

        [Key]
        public int NotificationUserId { get; set; }
        
        public int NotificationId { get; set; }

        [MaxLength(128)]
        public string UserName { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
