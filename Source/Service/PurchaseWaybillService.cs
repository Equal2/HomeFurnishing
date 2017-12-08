using Data.Infrastructure;
using Data.Models;
using Model;
using Model.Models;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.ViewModels;
using Model.ViewModel;

namespace Service
{
    public interface IPurchaseWaybillService : IDisposable
    {
        PurchaseWaybill Create(PurchaseWaybill s);
        void Delete(int id);
        void Delete(PurchaseWaybill s);
        PurchaseWaybillViewModel GetPurchaseWaybill(int id);
        PurchaseWaybill Find(int id);       
        void Update(PurchaseWaybill s);
        IQueryable<PurchaseWaybillViewModel> GetPurchaseWaybillList(int id);
        string GetMaxDocNo();
        PurchaseWaybill FindByDocNo(string Docno, int DivisionId, int SiteId);
        int NextId(int id);
        int PrevId(int id);


    }
    public class PurchaseWaybillService : IPurchaseWaybillService
    {

        ApplicationDbContext db = new ApplicationDbContext();
        private readonly IUnitOfWorkForService _unitOfWork;

        public PurchaseWaybillService(IUnitOfWorkForService unit)
        {
            _unitOfWork = unit;
        }

        public PurchaseWaybill Create(PurchaseWaybill s)
        {
            s.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<PurchaseWaybill>().Insert(s);
            return s;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<PurchaseWaybill>().Delete(id);
        }
        public void Delete(PurchaseWaybill s)
        {
            _unitOfWork.Repository<PurchaseWaybill>().Delete(s);
        }
        public void Update(PurchaseWaybill s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<PurchaseWaybill>().Update(s);
        }

        public PurchaseWaybillViewModel GetPurchaseWaybill(int id)
        {
            return (from p in db.PurchaseWaybill
                    where p.PurchaseWaybillId == id
                    select new PurchaseWaybillViewModel
                    {
                        ActualWeight=p.ActualWeight,
                        ChargedWeight=p.ChargedWeight,
                        ConsigneeName=p.Consignee.Name,
                        ConsignerId=p.ConsignerId,
                        ContainerNo=p.ContainerNo,
                        DeliveryPoint=p.DeliveryPoint,
                        DivisionId=p.DivisionId,
                        DocDate=p.DocDate,
                        DocNo=p.DocNo,
                        DocTypeId=p.DocTypeId,
                        EntryNo=p.EntryNo,
                        DocumentTypeName=p.DocType.DocumentTypeName,
                        EstimatedDeliveryDate=p.EstimatedDeliveryDate,
                        FreightAmt=p.FreightAmt,
                        FreightType=p.FreightType,
                        FromCityId=p.FromCityId,
                        IsDoorDelivery=p.IsDoorDelivery,
                        IsPOD=p.IsPOD,
                        NoOfPackages=p.NoOfPackages,
                        OtherCharges=p.OtherCharges,
                        PrivateMark=p.PrivateMark,
                        ProductDescription=p.ProductDescription,
                        PurchaseWaybillId=p.PurchaseWaybillId,
                        ReferenceDocNo=p.ReferenceDocNo,
                        Remark=p.Remark,
                        ServiceTax=p.ServiceTax,
                        ServiceTaxPer=p.ServiceTaxPer,
                        ShipMethodId=p.ShipMethodId,
                        SiteId=p.SiteId,
                        ToCityId=p.ToCityId,
                        SiteName=p.Site.SiteName,
                        ToCityName=p.ToCity.CityName,
                        TotalAmount=p.TotalAmount,
                        TransporterId=p.TransporterId,
                        TransporterName=p.Transporter.Person.Name,

                    }

                        ).FirstOrDefault();
        }

        public PurchaseWaybill Find(int id)
        {
            return _unitOfWork.Repository<PurchaseWaybill>().Find(id);
        }

        public PurchaseWaybill FindByDocNo(string Docno, int DivisionId, int SiteId)
        {
            return _unitOfWork.Repository<PurchaseWaybill>().Query().Get().Where(m => m.DocNo == Docno && m.DivisionId == DivisionId && m.SiteId == SiteId).FirstOrDefault();

        }
        public IQueryable<PurchaseWaybillViewModel> GetPurchaseWaybillList(int id)
        {
            return (from p in db.PurchaseWaybill
                    where p.DocTypeId==id
                    orderby p.DocDate descending, p.DocNo descending                    
                    select new PurchaseWaybillViewModel
                    {
                        DocNo = p.DocNo,
                        DocDate = p.DocDate,
                        ContainerNo = p.ContainerNo,
                        ConsigneeName = p.Consignee.Name,
                        PurchaseWaybillId=p.PurchaseWaybillId,
                    }
                        );
        }

        public int NextId(int id)
        {
            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.PurchaseWaybill
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseWaybillId).AsEnumerable().SkipWhile(p => p != id).Skip(1).FirstOrDefault();


            }
            else
            {
                temp = (from p in db.PurchaseWaybill
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseWaybillId).FirstOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public int PrevId(int id)
        {

            int temp = 0;
            if (id != 0)
            {

                temp = (from p in db.PurchaseWaybill
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseWaybillId).AsEnumerable().TakeWhile(p => p != id).LastOrDefault();
            }
            else
            {
                temp = (from p in db.PurchaseWaybill
                        orderby p.DocDate descending, p.DocNo descending
                        select p.PurchaseWaybillId).AsEnumerable().LastOrDefault();
            }
            if (temp != 0)
                return temp;
            else
                return id;
        }

        public string GetMaxDocNo()
        {
            int x;
            var maxVal = _unitOfWork.Repository<PurchaseWaybill>().Query().Get().Select(i => i.EntryNo).DefaultIfEmpty().ToList().Select(sx => int.TryParse(sx, out x) ? x : 0).Max();
            return (maxVal + 1).ToString();
        }

        public void Dispose()
        {
        }
    }
}
