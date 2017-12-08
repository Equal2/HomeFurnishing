using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Model.Models
{
    public class Update
    {
        [ScaffoldColumn(false)]
        public int UpdateId { get; set; }

        public string Updatemsg { get; set; }

        public double? status { get; set; }

        public int GoalId { get; set; }

        public DateTime UpdateDate { get; set; }

    

        public Update()
        {
            UpdateDate = DateTime.Now;

        }

        [MaxLength(50)]
        public string OMSId { get; set; }

    }
}
