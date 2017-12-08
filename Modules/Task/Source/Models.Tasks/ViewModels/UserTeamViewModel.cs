using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Model.Tasks.ViewModel
{
    public class UserTeamViewModel
    {
        public int UserTeamId { get; set; }

        [Required, MaxLength(5)]
        public string Srl { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        [MaxLength(255), Display(Name = "Manager"), Required]        
        public string User { get; set; }

        [MaxLength(255), Display(Name = "Team"), Required]
        public string TeamUser { get; set; }       
    }

    public class UserIndexViewModel
    {
        public string UserName { get; set; }        
        public string UserId { get; set; }
    }

    public class ProjectIndexViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int ? UserTeamCount { get; set; }
    }

}
