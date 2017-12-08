using Model;
using Models.BasicSetup.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Customize.Models
{
    public class JobReceiveHeaderExtended : EntityBase
    {
        [Key]
        [ForeignKey("JobReceiveHeader")]
        public int JobReceiveHeaderId { get; set; }
        public JobReceiveHeader JobReceiveHeader { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
        public Decimal LoadingTime { get; set; }

        public string DyeingType { get; set; }
        public bool IsQCRequired { get; set; }

    }
}
