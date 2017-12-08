using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
   
    public class StockInHandViewModel
    {
        public Int64 Id { get; set; }
        public int SiteId { get; set; }
        public int ? DivisionId { get; set; }
        public string LotNo { get; set; }
        public string ProductName { get; set; }
        public string ProductGroupName { get; set; }
        public int ? ProductId { get; set; }
        public string UnitName { get; set; }
        public int ? UnitDecimalPlaces { get; set; }
        public string SiteName { get; set; }
        public string DivisionName { get; set; }
        public string GodownName { get; set; }
        public int ? GodownId { get; set; }
        public int ? ProcessId { get; set; }
        public int ? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int ? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }
        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }
        public string ProcessName { get; set; }
        public string Dimension1TypeName { get; set; }
        public string Dimension2TypeName { get; set; }
        public decimal Opening { get; set; }
        public decimal RecQty { get; set; }
        public decimal IssQty { get; set; }
        public decimal BalQty { get; set; }

        public string ReportName { get; set; }
        public string ReportTitle { get; set; }

        public string SubReportProcList { get; set; }

        public string Name { get; set; }

        public int? PersonId { get; set; }

    }



}
