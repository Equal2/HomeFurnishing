using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Infrastructure;
using Model.Models;
using Model.ViewModels;
using Core.Common;
using System;
using Model;
using System.Threading.Tasks;
using Data.Models;
using Model.ViewModel;
using System.Data.SqlClient;
using System.Configuration;


namespace Service
{
    public interface IExcessMaterialLineService : IDisposable
    {
        ExcessMaterialLine Create(ExcessMaterialLine s);
        void Delete(int id);
        void Delete(ExcessMaterialLine s);
        ExcessMaterialLineViewModel GetExcessMaterialLine(int id);
        ExcessMaterialLine Find(int id);
        void Update(ExcessMaterialLine s);
        IEnumerable<ExcessMaterialLine> GetExcessMaterialLineforDelete(int id);
        IEnumerable<ExcessMaterialLineViewModel> GetExcessMaterialLineListForIndex(int id);
        int GetMaxSr(int id);
        UIDValidationViewModel GetBarCodeDetail(string ProductUidName);
    }

    public class ExcessMaterialLineService : IExcessMaterialLineService
    {
        ApplicationDbContext db;
        private readonly IUnitOfWorkForService _unitOfWork;

        public ExcessMaterialLineService(IUnitOfWorkForService unitOfWork, ApplicationDbContext Context)
        {
            db = Context;
            _unitOfWork = unitOfWork;
        }

        public ExcessMaterialLine Create(ExcessMaterialLine S)
        {
            S.ObjectState = ObjectState.Added;
            _unitOfWork.Repository<ExcessMaterialLine>().Insert(S);
            return S;
        }

        public void Delete(int id)
        {
            _unitOfWork.Repository<ExcessMaterialLine>().Delete(id);
        }

        public void Delete(ExcessMaterialLine s)
        {
            _unitOfWork.Repository<ExcessMaterialLine>().Delete(s);
        }

        public void Update(ExcessMaterialLine s)
        {
            s.ObjectState = ObjectState.Modified;
            _unitOfWork.Repository<ExcessMaterialLine>().Update(s);
        }


        public ExcessMaterialLineViewModel GetExcessMaterialLine(int id)
        {
            var temp = (from p in db.ExcessMaterialLine
                        join t in db.Product on p.ProductId equals t.ProductId into table
                        from tab in table.DefaultIfEmpty()
                        where p.ExcessMaterialLineId == id
                        select new ExcessMaterialLineViewModel
                        {
                            Qty = p.Qty,
                            Remark = p.Remark,
                            ExcessMaterialHeaderId = p.ExcessMaterialHeaderId,
                            ProductUidId = p.ProductUidId,
                            ProductUidName = p.ProductUid.ProductUidName,
                            ExcessMaterialLineId = p.ExcessMaterialLineId,
                            ProductName = tab.ProductName,
                            Dimension1Id = p.Dimension1Id,
                            Dimension2Id = p.Dimension2Id,
                            ProductId = p.ProductId,
                            ProcessId=p.ProcessId,
                            LotNo = p.LotNo,
                        }

                        ).FirstOrDefault();

            return temp;
        }

        public ExcessMaterialLine Find(int id)
        {
            return _unitOfWork.Repository<ExcessMaterialLine>().Find(id);
        }

        public IEnumerable<ExcessMaterialLine> GetExcessMaterialLineforDelete(int id)
        {
            return (from p in db.ExcessMaterialLine
                    where p.ExcessMaterialHeaderId == id
                    select p
                        );
        }

        public IEnumerable<ExcessMaterialLineViewModel> GetExcessMaterialLineListForIndex(int id)
        {

            return (from p in db.ExcessMaterialLine
                    join t in db.Product on p.ProductId equals t.ProductId
                    join u in db.Units on t.UnitId equals u.UnitName
                    where p.ExcessMaterialHeaderId == id
                    orderby p.Sr
                    select new ExcessMaterialLineViewModel
                    {
                        ProductName = t.ProductName,
                        Dimension1Name = p.Dimension1.Dimension1Name,
                        Dimension2Name = p.Dimension2.Dimension2Name,
                        LotNo = p.LotNo,
                        Qty = p.Qty,
                        Remark = p.Remark,
                        ExcessMaterialLineId = p.ExcessMaterialLineId,
                        ExcessMaterialHeaderId = p.ExcessMaterialHeaderId,
                        ProductUidName = p.ProductUid.ProductUidName,
                        ProcessName=p.Process.ProcessName,
                        ProcessId=p.ProcessId,
                        UnitId=u.UnitId,
                        UnitName=u.UnitName,
                        UnitDecimalPlaces=u.DecimalPlaces,
                    }
                        );

        }

        public int GetMaxSr(int id)
        {
            var Max = (from p in db.ExcessMaterialLine
                       where p.ExcessMaterialHeaderId == id
                       select p.Sr
                        );

            if (Max.Count() > 0)
                return Max.Max(m => m ?? 0) + 1;
            else
                return (1);
        }


        public UIDValidationViewModel GetBarCodeDetail(string ProductUidName)
        {

            UIDValidationViewModel temp = new UIDValidationViewModel();

            var UID = (from p in db.ProductUid
                       where p.ProductUidName == ProductUidName                       
                       select new UIDValidationViewModel
                       {
                           CurrenctGodownId = p.CurrenctGodownId,
                           CurrenctProcessId = p.CurrenctProcessId,
                           CurrentGodownName = p.CurrenctGodown.GodownName,
                           CurrentProcessName = p.CurrenctProcess.ProcessName,
                           GenDocDate = p.GenDocDate,
                           GenDocId = p.GenDocId,
                           GenDocNo = p.GenDocNo,
                           GenDocTypeId = p.GenDocTypeId,
                           GenDocTypeName = p.GenDocType.DocumentTypeName,
                           GenPersonId = p.GenPersonId,
                           GenPersonName = p.GenPerson.Person.Name,
                           IsActive = p.IsActive,
                           LastTransactionDocDate = p.LastTransactionDocDate,
                           LastTransactionDocId = p.LastTransactionDocId,
                           LastTransactionDocNo = p.LastTransactionDocNo,
                           LastTransactionDocTypeId = p.LastTransactionDocTypeId,
                           LastTransactionDocTypeName = p.LastTransactionDocType.DocumentTypeName,
                           LastTransactionPersonId = p.LastTransactionPersonId,
                           LastTransactionPersonName = p.LastTransactionPerson.Name,
                           ProductId = p.ProductId,
                           ProductName = p.Product.ProductName,
                           ProductUIDId = p.ProductUIDId,
                           ProductUidName = p.ProductUidName,
                           Status = p.Status,
                           LotNo = p.LotNo,
                           Dimension1Id = p.Dimension1Id,
                           Dimension1Name = p.Dimension1.Dimension1Name,
                           Dimension2Id = p.Dimension2Id,
                           Dimension2Name = p.Dimension2.Dimension2Name,                           
                       }).FirstOrDefault();

            if (UID == null)
            {
                UID = new UIDValidationViewModel();
                UID.ErrorType = "InvalidID";
                UID.ErrorMessage = "Invalid ProductUID";
            }         
            else
            {
                UID.ErrorType = "Success";
            }

            return UID;


        }

        public void Dispose()
        {
        }
    }
}
