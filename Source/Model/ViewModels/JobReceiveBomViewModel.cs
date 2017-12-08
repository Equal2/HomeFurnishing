using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
    public class JobReceiveBomViewModel
    {
        public int JobReceiveBomId { get; set; }
        public int JobReceiveHeaderId { get; set; }
        public string JobReceiveHeaderDocNo { get; set; }
        public int ? JobReceiveLineId { get; set; }


        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        public decimal Qty { get; set; }
        [MaxLength(50)]
        public string LotNo { get; set; }

        [Display(Name = "Dimension1")]
        public int? Dimension1Id { get; set; }
        public string Dimension1Name { get; set; }

        [Display(Name = "Dimension2")]
        public int? Dimension2Id { get; set; }
        public string Dimension2Name { get; set; }




        [Display(Name = "Dimension3")]
        public int? Dimension3Id { get; set; }
        public string Dimension3Name { get; set; }

        [Display(Name = "Dimension4")]
        public int? Dimension4Id { get; set; }
        public string Dimension4Name { get; set; }



        public string UnitId { get; set; }
        public string UnitName { get; set; }
        public byte UnitDecimalPlaces { get; set; }
        public int? CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public JobReceiveSettingsViewModel JobReceiveSettings { get; set; }
    }
}
