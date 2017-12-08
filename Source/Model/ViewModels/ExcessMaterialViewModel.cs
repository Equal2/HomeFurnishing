using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Model.ViewModel;

namespace Model.ViewModels
{
    public class ExcessMaterialHeaderViewModel 
    {
        public int ExcessMaterialHeaderId { get; set; }
        public int DocTypeId { get; set; }
        public string DocTypeName { get; set; }
        public DateTime DocDate { get; set; }
        public string DocNo { get; set; }
        public int DivisionId { get; set; }
        public string DivisionName { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public int? PersonId { get; set; }
        public string PersonName { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public int? GodownId { get; set; }
        public string GodownName { get; set; }
        public string Remark { get; set; }
        public int Status { get; set; }
        public int? ReviewCount { get; set; }
        public string ReviewBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string LockReason { get; set; }
        public bool? Reviewed { get; set; }
        public ExcessMaterialSettingsViewModel ExcessMaterialSettings { get; set; }
    }


    public class ExcessMaterialLineViewModel
    {
        public int ExcessMaterialLineId { get; set; }
        public int ExcessMaterialHeaderId { get; set; }
        public string ExcessMaterialHeaderDocNo { get; set; }
        public int? ProductUidId { get; set; }
        public string ProductUidName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public decimal UnitDecimalPlaces { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string LotNo { get; set; }
        public Decimal Qty { get; set; }
        public string Remark { get; set; }
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string LockReason { get; set; }
        public int? Sr { get; set; }
        public ExcessMaterialSettingsViewModel ExcessMaterialSettings { get; set; }
    }

}
