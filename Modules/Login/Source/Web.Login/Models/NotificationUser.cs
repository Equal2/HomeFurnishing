using Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Login.Models
{
    public class NotificationUser : EntityBase
    {

        [Key]
        public int NotificationUserId { get; set; }

        [ForeignKey("Notification")]
        public int NotificationId { get; set; }
        public virtual Notification Notification { get; set; }

        [MaxLength(128), Required]
        public string UserName { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
