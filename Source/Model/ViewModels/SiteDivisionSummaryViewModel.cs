using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class SiteDivisionSummaryViewModel
    {
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public string SiteName { get; set; }
        public string RoleId { get; set; }
        public string SiteColour { get; set; }
        public string DivisionColour { get; set; }
    }
}
