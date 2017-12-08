using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Models
{
    public class RouteLine : EntityBase, IHistoryLog
    {
        public RouteLine()
        {
        }

        [Key]
        public int RouteLineId { get; set; }

        [Display(Name = "Route"), Required]
        [ForeignKey("Route")]
        public int RouteId { get; set; }
        public virtual Route Route { get; set; }

        [Display(Name = "City"), Required]
        [ForeignKey("City")]
        public int CityId { get; set; }
        public virtual City City { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }

        [MaxLength(50)]
        public string OMSId { get; set; }
    }
}
