using Model.Models;
using Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ViewModels
{
    public class ReportMasterViewModel
    {
        public ReportMasterViewModel()
        {
            QtyColumnCaption = "Qty";
            UnitColumnCaption = "Unit";
            DealQtyColumnCaption = "DealQty";
            DealUnitColumnCaption = "DealUnit";
        }
        public int ReportHeaderId { get; set; }
        public bool closeOnSelect { get; set; }
        public ReportHeader ReportHeader { get; set; }        
        public List<ReportLine> ReportLine{ get; set; }
        public ReportHeaderCompanyDetail ReportHeaderCompanyDetail { get; set; }

        public string ReportTitle { get; set; }

        public string QtyColumnCaption { get; set; }
        public string UnitColumnCaption { get; set; }
        public string DealQtyColumnCaption { get; set; }
        public string DealUnitColumnCaption { get; set; }
        

    }

    public class ReportHeaderCompanyDetail
    {
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string CityName { get; set; }
        public string Phone { get; set; }
        public string LogoBlob { get; set; }
    }
}
