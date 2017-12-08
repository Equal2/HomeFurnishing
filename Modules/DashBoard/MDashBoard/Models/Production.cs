using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MDashBoard.Models
{
 

   #region Dyeing 
    public class DyeingOrder
    {
        public string Type { get; set; }
        public string Head { get; set; }
        public decimal? Value { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; } 
    }

    public class DyeingDataset
    {
        public DyeingDataset()
          {
              Month = new List<DyeingOrder>();
              JobWorker = new List<DyeingOrder>();            
          }
        public List<DyeingOrder> Month { get; set; }
        public List<DyeingOrder> JobWorker { get; set; }
    }

#endregion 


    #region Packing
    public class Packing
    {
        public string 	Type {get;set;}
	    public string Head {get;set;}	    
	    public decimal? Area {get;set;}
	    public int SiteId {get;set;}
	    public int DivisionId {get;set;}
    }

    public class PackingDataset
    {
        public PackingDataset()
        {
            Month=new List<Packing>();
            Name=new List<Packing>();
        }
        public List<Packing> Month{get;set;}
        public List<Packing> Name{get;set;}
    }
    #endregion 


    #region Purchase
    public class Purchase
    {
        public string Type {get;set;}
	    public string Head {get;set;}	    
	    public decimal? Area {get;set;}
	    public int SiteId {get;set;}
	    public int DivisionId {get;set;}
    }

    public class PurchaseDataSet
    {
        public PurchaseDataSet()
        {
            Month = new List<Purchase>();           
            Construction = new List<Purchase>();
        }
        public List<Purchase> Month { get; set; }
       public List<Purchase> Construction { get; set; }
    }
        #endregion

    #region Production
    public class Production
    {
        public string Type { get; set; }
        public string Head { get; set; }
        public decimal? Value { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
        public string Short { get; set; }
    }
    public class Dataset
      {
          public List<Production> Month { get; set; }
          public List<Production> JobWorker { get; set; }
          public List<Production> Construction { get; set; }

          public Dataset()
          {
              Month = new List<Production>();
              JobWorker = new List<Production>();
              Construction = new List<Production>();
          }
      }
#endregion 


    #region SubProduction
    public class SubProduction
    {
       public string   ProductCategoryName { get; set; }
	    public string Month1  { get; set; }
	    public string Month  { get; set; }
	    public int?  SiteId { get; set; }
	    public int? DivisionId { get; set; }
	    public double?  Area { get; set; }
	    public string Jobworker{ get; set; }
        public string DATE { get; set; }
        public string Name { get; set; }

        public string MinDate { get; set; }
        public string MaxDate { get; set; }
        public double? Qty { get; set; }

        public string SupplierName { get; set; }

        public int? Week { get; set; }
        public string UnitName { get; set; }
        public string ProcessName { get; set; }
        public double? TodayArea { get; set; }
        public int? ProcessSr { get; set; }

        public double? DyeingPlan { get; set; }
        public double? DyeingOrder { get; set; }

        #region charts
        public double? CurrentArea { get; set; }
        public double? PreviousArea { get; set; }
        #endregion 

        public double? AreaTobeIssue { get; set; }
        public double? BalanceQty { get; set; }
        public double? IssueQty { get; set; }
        public int? color { get; set; }
        public double? Percentage { get; set; }


        public List<JobworkerDetail> NameDetails { get; set; }

    }

    public class JobworkerDetail
    {
        public string Name { get; set; }
        public string DateTime { get; set; }
        public double ? Area { get; set; }
        public double? Qty { get; set; }
        public string CostCenter { get; set; }
        public string JobWorker { get; set; }
    }
     public class SubProductionDataset
     {
         public List<SubProduction> Month { get; set; }
         public List<SubProduction> Jobworker { get; set; }
         public List<SubProduction> Construction { get; set; }
         public List<SubProduction> MinMaxDate { get; set; }
         public List<SubProduction> Name { get; set; }

         public List<SubProduction> SupplierName { get; set; }
         public List<SubProduction> Week { get; set; }
         public List<SubProduction> Day { get; set; }
         public List<SubProduction> ProcessName { get; set; }

         
      
         public SubProductionDataset()
         {
             Month = new List<SubProduction>();
             Jobworker = new List<SubProduction>();
             Construction = new List<SubProduction>();
             MinMaxDate = new List<SubProduction>();
             Name = new List<SubProduction>();
             SupplierName = new List<SubProduction>();
             Week = new List<SubProduction>();
             Day = new List<SubProduction>();
             ProcessName = new List<SubProduction>();
             
         }

     }
    #endregion

    #region Finishing
    public class finishing
    {
        public string Type { get; set; }
        public string Head { get; set; }
        public decimal? Value { get; set; }
        public int SiteId { get; set; }
        public int DivisionId { get; set; }
    }

    public class FinishingDataset
    {
        public  FinishingDataset()
        {
            Process = new List<finishing>();
            Construction = new List<finishing>();
        }
        public List<finishing> Process { get; set; }
        public List<finishing> Construction { get; set; }
    }
#endregion Finishing
    
#region Month data
    public class MonthDtatSet
    {
        public List<SubProduction> ProductionMonth { get; set; }
        public List<SubProduction> ProductionToday { get; set; }        

        public List<SubProduction> DyeingReceiveMonth { get; set; }
        public List<SubProduction> DyeingReceiveToday { get; set; }

        public List<SubProduction> PurchaseReceiveMonth { get; set; }
        public List<SubProduction> PurchaseReceiveToday { get; set; }


        public List<SubProduction> PackingMonth { get; set; }
        public List<SubProduction> PackingToday { get; set; }

        public List<SubProduction> TopMonthProduction { get; set; }
        public List<SubProduction> TopYearProduction { get; set; }

           #region Chart
        public List<SubProduction> ChartProduction { get; set; }
          #endregion 
        public List<SubProduction> OrderToBeIssue { get; set; }
        public List<SubProduction> OnLoom { get; set; }
        public List<SubProduction> DyeingBal { get; set; }
        public List<SubProduction> DyeingPlan { get; set; }
        public List<SubProduction> MaterialReqBalIssue { get; set; }

        #region TodayIssueReceive
        public List<SubProduction> WeavingOrder { get; set; }
        public List<SubProduction> WeavingReceive { get; set; }
        public List<SubProduction> DyeingReceive { get; set; }
        public List<SubProduction> DyeingOrder { get; set; }
        public List<SubProduction> WeavingMaterialIssue { get; set; }
        #endregion



        public MonthDtatSet()
        {

            ProductionMonth = new List<SubProduction>();
            ProductionToday = new List<SubProduction>();

            DyeingReceiveMonth = new List<SubProduction>();
            DyeingReceiveToday = new List<SubProduction>();

            PurchaseReceiveMonth = new List<SubProduction>();
            PurchaseReceiveToday = new List<SubProduction>();

            PackingMonth = new List<SubProduction>();
            PackingToday = new List<SubProduction>();

            TopMonthProduction = new List<SubProduction>();
            TopYearProduction = new List<SubProduction>();

            #region Chart
            ChartProduction = new List<SubProduction>();          
            #endregion 
            OrderToBeIssue = new List<SubProduction>();
            OnLoom = new List<SubProduction>();
            DyeingBal = new List<SubProduction>();
            DyeingPlan = new List<SubProduction>();
            MaterialReqBalIssue = new List<SubProduction>();

            #region TodayIssueReceive
            WeavingOrder= new List<SubProduction>();
            WeavingReceive= new List<SubProduction>();
            DyeingReceive= new List<SubProduction>();
            DyeingOrder= new List<SubProduction>();
            WeavingMaterialIssue= new List<SubProduction>();           
        #endregion
        }
    }
#endregion



}