using System.Collections.Generic;
using System.Linq;
using Surya.India.Data;
using Surya.India.Data.Infrastructure;
using Surya.India.Model.Models;
using Surya.India.Model.ViewModels;
using Surya.India.Core.Common;
using System;
using Surya.India.Model;
using System.Threading.Tasks;
using Surya.India.Data.Models;


namespace Surya.India.Service
{
    public interface ISupplierRatingService : IDisposable
    {
        IEnumerable<SupplierRating> GetSupplierRating();
    }

    public class SupplierRatingService : ISupplierRatingService
    {
        ApplicationDbContext db =new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public SupplierRatingService(IUnitOfWorkForService unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<SupplierRating> GetSupplierRating()
        {
            List<SupplierRating> supplierratings = new List<SupplierRating>();


            var TimeRating_Dispatch = _unitOfWork.Repository<PurchaseGoodsReceiptLine>().Query()
                             .Include(i => i.PurchaseGoodsReceiptHeader)
                             .Include(i => i.PurchaseOrderLine)
                             .Include(i => i.PurchaseOrderLine.PurchaseOrderHeader)
                             .Get().GroupBy(i => i.PurchaseOrderLine.PurchaseOrderHeaderId)
                             .Select(i => new { PurchaseOrderHeaderId = i.Key, TotalShipQty = i.Sum(o => o.Qty), LastShipDate = i.Max(o => o.PurchaseGoodsReceiptHeader.DocDate) });




            var TimeRating_TimelyShipperOrdersSupplierWise = _unitOfWork.Repository<PurchaseOrderLine>().Query()
                      .Include(i => i.PurchaseOrderHeader)
                      .Include(i => i.PurchaseOrderHeader.Supplier)
                      .Get()
                      .GroupJoin(TimeRating_Dispatch, x => x.PurchaseOrderHeaderId, y => y.PurchaseOrderHeaderId , (x, y) => new { y, x })
                      .SelectMany(i => i.y.DefaultIfEmpty(),  (x, y) => new
                        { 
                            PurchaseOrderHeaderId = x.x.PurchaseOrderHeaderId,
                            SupplierId = x.x.PurchaseOrderHeader.SupplierId,
                            SupplierName = x.x.PurchaseOrderHeader.Supplier.Name,
                            DueDate = x.x.PurchaseOrderHeader.DueDate,
                            LastShipDate = y.LastShipDate,
                            TotalOrderQty = x.x.Qty,
                            TotalShipQty = y.TotalShipQty
                        })
                        .GroupBy(i => i.PurchaseOrderHeaderId)
                        .Where(i => ((decimal?)i.Max(o => o.TotalShipQty) ?? 0) >= i.Sum(o => o.TotalOrderQty) && ((DateTime?)i.Max(o => o.LastShipDate) ?? DateTime.Now) <= i.Max(o => o.DueDate))
                        .Select(i => new
                        {
                            PurchaseOrderHeaderId = i.Key,
                            SupplierId = i.Max(o => o.SupplierId),
                            SupplierName = i.Max(o => o.SupplierName),
                            DueDate = i.Max(o => o.DueDate),
                            LastShipDate = (DateTime?)i.Max(o => o.LastShipDate) ?? DateTime.Now,
                            TotalOrderQty = i.Sum(o => o.TotalOrderQty),
                            TotalShipQty = (decimal?)i.Max(o => o.TotalShipQty) ?? 0
                        })
                        .GroupBy(i => i.SupplierId)
                        .Select(i => new
                        {
                            SupplierId = i.Key,
                            SupplierName = i.Max(o => o.SupplierName),
                            TimelyShipedOrders = i.Count()
                        });




            var TimeRating_TimelyShipperOrdersMax = TimeRating_TimelyShipperOrdersSupplierWise
                            .GroupBy(i => "x")
                            .Select(i => new { Dummy = i.Key, MaxTimelyShippedOrders = i.Max(o => o.TimelyShipedOrders) });



            var TimeRating = _unitOfWork.Repository<Supplier>().Query().Get()
                        .GroupJoin(TimeRating_TimelyShipperOrdersSupplierWise, m => m.PersonID, n => n.SupplierId, (m, n) => new { n, m })
                        .SelectMany(i => i.n.DefaultIfEmpty(), (m, n) => new
                        {
                            SupplierId = m.m.PersonID,
                            SupplierName = m.m.Name,
                            TimelyShipedOrders = (Int32?)n.TimelyShipedOrders ?? 0 
                        })
                        .GroupJoin(TimeRating_TimelyShipperOrdersMax, x => "1", y => "1", (x, y) => new { y, x })
                        .SelectMany(i => i.y.DefaultIfEmpty(), (x, y) => new
                        {
                            SupplierId = x.x.SupplierId,
                            SupplierName = x.x.SupplierName,
                            TimelyShipedOrders = x.x.TimelyShipedOrders,
                            MaxTimelyShippedOrders = (Int32?)y.MaxTimelyShippedOrders ?? 0   
                        });
                        


            

            
            foreach (var obj in TimeRating)
            {
                decimal TimeRatingVal = 0;
                if (obj.MaxTimelyShippedOrders != 0){ 
                     TimeRatingVal = Math.Round((Decimal) (obj.TimelyShipedOrders * 5 / obj.MaxTimelyShippedOrders ),2) ;
                }

                supplierratings.Add(new SupplierRating
                {
                    SupplierID = obj.SupplierId,
                    SupplierName = obj.SupplierName,
                    Time = TimeRatingVal,
                    Quality = 0,
                    InformationSharing = 0,
                    Creativity = 0,
                    Volume = 0
                });
            }



            //Volume Rating


            var VolumeRating_TotalDispatchSupplierWise = _unitOfWork.Repository<PurchaseInvoiceLine>().Query()
                           .Include(i => i.PurchaseInvoiceHeader)
                           .Include(i => i.PurchaseInvoiceHeader.Supplier)
                           .Include(i => i.PurchaseInvoiceHeader.Supplier.Currency)
                           .Get()
                           .GroupBy(i => i.PurchaseInvoiceHeader.SupplierId)
                           .Select(i => new { SupplierId = i.Key, SupplierName = i.Max(o => o.PurchaseInvoiceHeader.Supplier.Name), SupplierShipAmt = i.Sum(o => o.Amount)/i.Max(o =>o.PurchaseInvoiceHeader.Supplier.Currency.BaseCurrencyRate)});


            var VolumeRating_MaxDispatch = VolumeRating_TotalDispatchSupplierWise
                         .GroupBy(i => "x")
                         .Select(i => new { Dummy = i.Key, MaxShipAmt = i.Max(o => o.SupplierShipAmt) });


            var VolumeRating = VolumeRating_TotalDispatchSupplierWise
                           .GroupJoin(VolumeRating_MaxDispatch, x => "1", y => "1", (x, y) => new { y, x })
                           .SelectMany(i => i.y.DefaultIfEmpty(), (x, y) => new
                            {
                                SupplierId = x.x.SupplierId,
                                SupplierName = x.x.SupplierName,
                                SupplierShipAmt = (Decimal?)x.x.SupplierShipAmt ?? 0,
                                MaxShipAmt = (Decimal?)y.MaxShipAmt ?? 0  
                            });


      


            foreach (var obj in VolumeRating)
            {
                decimal VolumeRatingVal = 0;
                if (obj.MaxShipAmt != 0)
                {
                    VolumeRatingVal = Math.Round((Decimal)(obj.SupplierShipAmt * 5 / obj.MaxShipAmt), 2);
                }

                if (obj.SupplierShipAmt > 0)
                {
                    supplierratings.Add(new SupplierRating
                    {
                        SupplierID = obj.SupplierId,
                        SupplierName = obj.SupplierName,
                        Time = 0,
                        Quality = 0,
                        InformationSharing = 0,
                        Creativity = 0,
                        Volume = VolumeRatingVal
                    });

                }
            }







            //Volume Rating


            var CreativityRating_TotalSampleApproved = _unitOfWork.Repository<ProductSampleApproval>().Query()
                           .Include(i => i.ProductSampleShipment)
                           .Include(i => i.ProductSampleShipment.ProductSamplePhotoApproval)
                           .Include(i => i.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample)
                           .Include(i => i.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample.Supplier)
                           .Get()
                           .GroupBy(i => i.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample.Supplier.PersonID)
                           .Select(i => new { SupplierId = i.Key, SupplierName = i.Max(o => o.ProductSampleShipment.ProductSamplePhotoApproval.ProductSample.Supplier.Name), SupplierApprovedSamples = i.Count()});


            var CreativityRating_MaxSampleApproved = CreativityRating_TotalSampleApproved
                         .GroupBy(i => "x")
                         .Select(i => new { Dummy = i.Key, MaxApprovedSamples = i.Max(o => o.SupplierApprovedSamples) });


            var CreativityRating = CreativityRating_TotalSampleApproved
                           .GroupJoin(CreativityRating_MaxSampleApproved, x => "1", y => "1", (x, y) => new { y, x })
                           .SelectMany(i => i.y.DefaultIfEmpty(), (x, y) => new
                           {
                               SupplierId = x.x.SupplierId,
                               SupplierName = x.x.SupplierName,
                               SupplierApprovedSamples = x.x.SupplierApprovedSamples,
                               MaxApprovedSamples = y.MaxApprovedSamples
                           });



            foreach (var obj in CreativityRating)
            {
                decimal CreativityRatingVal = 0;
                if (obj.MaxApprovedSamples != 0)
                {
                    CreativityRatingVal = Math.Round((Decimal)(obj.SupplierApprovedSamples * 5 / obj.MaxApprovedSamples), 2);
                }
                supplierratings.Add(new SupplierRating
                {
                        SupplierID = obj.SupplierId,
                        SupplierName = obj.SupplierName,
                        Time = 0,
                        Quality = 0,
                        InformationSharing = 0,
                        Creativity = CreativityRatingVal,
                        Volume = 0
                  });

              }






            //Information Sharing Rating

            var purchaseorderheader = _unitOfWork.Repository<PurchaseOrderHeader>().Query().Include(i=>i.Supplier).Get();

            var InfoRating_TotalInfoShareSupplierWise = _unitOfWork.Repository<ActivityLog>().Query()
                               .Get()
                               .Where(i => i.ActivityType == PurchaseOrderActivityTypeConstants.ProgressChange)
                               .GroupJoin(purchaseorderheader, m => m.DocId, n => n.PurchaseOrderHeaderId, (m, n) => new { m, n })
                               .SelectMany(i => i.n.DefaultIfEmpty(), (x, y) => new
                               {
                                   SupplierId = y.SupplierId,
                                   SupplierName = y.Supplier.Name,
                                   TotalInfoShared = 1
                               })
                               .GroupBy(i => i.SupplierId)
                               .Select(i => new { SupplierId = i.Key, SupplierName = i.Max(o => o.SupplierName), InfoShared = i.Count() });


            var InfoRating_MaxInfoShare = InfoRating_TotalInfoShareSupplierWise
                         .GroupBy(i => "x")
                         .Select(i => new { Dummy = i.Key, MaxInfoShared = i.Max(o => o.InfoShared) });


            var InfoRating = InfoRating_TotalInfoShareSupplierWise
                       .GroupJoin(InfoRating_MaxInfoShare, x => "1", y => "1", (x, y) => new { y, x })
                       .SelectMany(i => i.y.DefaultIfEmpty(), (x, y) => new
                       {
                           SupplierId = x.x.SupplierId,
                           SupplierName = x.x.SupplierName,
                           SupplierInfoShared = x.x.InfoShared,
                           MaxInfoShared = y.MaxInfoShared
                       });

            
            foreach (var obj in InfoRating)
            {
                decimal InfoRatingVal = 0;
                if (obj.MaxInfoShared != 0)
                {
                    InfoRatingVal = Math.Round((Decimal)(obj.SupplierInfoShared * 5 / obj.MaxInfoShared), 2);
                }
                supplierratings.Add(new SupplierRating
                {
                        SupplierID = obj.SupplierId,
                        SupplierName = obj.SupplierName,
                        Time = 0,
                        Quality = 0,
                        InformationSharing = InfoRatingVal,
                        Creativity = 0,
                        Volume = 0
                  });
              }





            //Quality Rating


            var QualityRating_TotalQualityValSupplierWise = _unitOfWork.Repository<InspectionQaAttributes>().Query()
                           .Include(i => i.InspectionHeader)
                           .Include(i => i.InspectionHeader.Supplier)
                           .Get()
                           .GroupBy(i => i.InspectionHeaderId)
                           .Select(i => new
                           {
                               InspectionHeaderId = i.Key,
                               SupplierId = i.Max(o => o.InspectionHeader.SupplierId),
                               SupplierName = i.Max(o => o.InspectionHeader.Supplier.Name),
                               InspectionVal = i.Sum(o => o.Value) / i.Count()
                           })
                           .GroupBy(i => i.SupplierId)
                           .Select(i => new
                           {
                               SupplierId = i.Key,
                               SupplierName = i.Max(o => o.SupplierName),
                               InspectionVal = i.Sum(o => o.InspectionVal) 
                           });


            var QualityRating_MaxQualityVal = QualityRating_TotalQualityValSupplierWise
                     .GroupBy(i => "x")
                     .Select(i => new { Dummy = i.Key, MaxQualityVal = i.Max(o => o.InspectionVal) });


            var QualityRating = QualityRating_TotalQualityValSupplierWise
                   .GroupJoin(QualityRating_MaxQualityVal, x => "1", y => "1", (x, y) => new { y, x })
                   .SelectMany(i => i.y.DefaultIfEmpty(), (x, y) => new
                   {
                       SupplierId = x.x.SupplierId,
                       SupplierName = x.x.SupplierName,
                       InspectionVal = x.x.InspectionVal,
                       MaxQualityVal = y.MaxQualityVal
                   });


            foreach (var obj in QualityRating)
            {
                decimal QualityRatingVal = 0;
                if (obj.MaxQualityVal != 0)
                {
                    QualityRatingVal = Math.Round((Decimal)(obj.InspectionVal * 5 / obj.MaxQualityVal), 2);
                }
                supplierratings.Add(new SupplierRating
                {
                    SupplierID = obj.SupplierId,
                    SupplierName = obj.SupplierName,
                    Time = 0,
                    Quality = QualityRatingVal,
                    InformationSharing = 0,
                    Creativity = 0,
                    Volume = 0
                });
            }




            //Final Ratings

            List<SupplierRating> OverAllsupplierratings = new List<SupplierRating>();

            var OverAllRatings = supplierratings.GroupBy(i => i.SupplierID).
                           Select(i => new
                           {
                               SupplierID = i.Key,
                               SupplierName = i.Max(o => o.SupplierName),
                               Time = i.Sum(o => o.Time),
                               Quality = i.Sum(o => o.Quality),
                               InformationSharing = i.Sum(o => o.InformationSharing),
                               Creativity = i.Sum(o => o.Creativity),
                               Volume = i.Sum(o => o.Volume)
                           });


            foreach (var obj in OverAllRatings)
            {
                OverAllsupplierratings.Add(new SupplierRating
                    {
                        SupplierID = obj.SupplierID,
                        SupplierName = obj.SupplierName,
                        Time = obj.Time,
                        Quality = obj.Quality,
                        InformationSharing = obj.InformationSharing,
                        Creativity = obj.Creativity,
                        Volume = obj.Volume,
                        OverAllRating = Math.Round((obj.Time + obj.Quality + obj.InformationSharing + obj.Creativity + obj.Volume) / 5,2)
                    });
            }
            return OverAllsupplierratings;
        }

        public void Dispose()
        {
        }
    }
}
