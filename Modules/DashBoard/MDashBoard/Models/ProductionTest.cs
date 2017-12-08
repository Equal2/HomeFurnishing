using System.Collections.Generic;

namespace Model.ViewModel
{
    public class ProductionTest
    {

         public  string  DivisionName { get; set; } 
         public string  ProductCategoryName { get; set; }
         public decimal ? Qty { get; set; }
         public decimal ? Area { get; set; }      
    }

   
    public class MDashBoardPacking
    {
        public string  DivisionName { get; set; }
        public string  Name { get; set; }
        public decimal ? Qty { get; set; }
        public decimal ? Area { get; set; }
             
    }

    public  class MDashBoardDyeing
    {
        public string  DivisionName { get; set; }
        public string  Location { get; set; }
        public decimal ? Qty { get; set; }
    }

    public class MDashBoardPurchase
    {
        public string  DivisionName { get; set; }
        public string  ProductCategoryName { get; set; }
        public decimal ? Qty { get; set; }
        public decimal ? Area { get; set; }
    }

    public class DashBoardTables
    {
        public DashBoardTables()
        {
            ProductionTest = new List<ProductionTest>();
            Packing = new List<MDashBoardPacking>();
            Dyeing = new List<MDashBoardDyeing>();
            Purchase = new List<MDashBoardPurchase>();
        }
        public List<ProductionTest> ProductionTest { get; set; }
        public List<MDashBoardPacking> Packing { get; set; }
        public List<MDashBoardDyeing> Dyeing { get; set; }
        public List<MDashBoardPurchase> Purchase { get; set; }
    }

}
