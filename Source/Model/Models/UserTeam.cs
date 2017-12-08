using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Tasks.Models
{
    public class UserTeam : EntityBase, IHistoryLog
    {

        [Key]
        public int UserTeamId { get; set; }

        [Required, MaxLength(5)]
        public string Srl { get; set; }


        [ForeignKey("Project"), Display(Name = "Project")]
        [Index("IX_UserTeam_TeamName", IsUnique = true, Order = 1)]
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; }

        [MaxLength(255), Display(Name = "Manager"),Required]
        [Index("IX_UserTeam_TeamName", IsUnique = true, Order = 2)]
        public string User { get; set; }
                
        [MaxLength(255), Display(Name = "Team"),Required ]
        [Index("IX_UserTeam_TeamName", IsUnique = true, Order = 3)]
        public string TeamUser { get; set; }
        
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Modified By")]
        public string ModifiedBy { get; set; }
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDate { get; set; }
    }
}
