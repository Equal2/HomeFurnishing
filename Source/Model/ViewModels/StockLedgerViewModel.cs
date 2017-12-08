using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Models;
using System.ComponentModel.DataAnnotations;

namespace Model.ViewModel
{
   
    public class StockLedgerViewModel
    {
        public Int64 Id { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public string Dimension1TypeName { get; set; }
        public string Dimension2TypeName { get; set; }
        public string GodownName { get; set; }
        public string ProcessName { get; set; }
        public string ProductName { get; set; }
        public string Dimension1Name { get; set; }
        public string Dimension2Name { get; set; }
        public string PartyName { get; set; }
        public string LotNo { get; set; }
        public string DocNo { get; set; }
        public string DocDate { get; set; }
        public string TransactionType { get; set; }
        public string UnitName { get; set; }
        public byte ? UnitDecimalPlaces { get; set; }
        public string SiteName { get; set; }
        public string DivisionName { get; set; }
        public decimal Opening { get; set; }
        public decimal RecQty { get; set; }
        public decimal IssQty { get; set; }
        public int ? DocTypeId { get; set; }
        public int ? DocHeaderId { get; set; }
        public decimal Balance { get; set; }
       
    }

}
