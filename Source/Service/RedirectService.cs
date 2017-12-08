using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNet.Identity.EntityFramework;
using Data;
using AutoMapper;
using Model.ViewModel;
using Data.Infrastructure;
using Model.Models;

namespace Service
{
    public interface IRedirectService : IDisposable
    {
        DocumentTypeViewModel GetDocumentType(int Id);
    }

    public class RedirectService : IRedirectService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RedirectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public DocumentTypeViewModel GetDocumentType(int Id)
        {
            var obj = _unitOfWork.Repository<DocumentType>().Find(Id);

            return Mapper.Map<DocumentTypeViewModel>(obj);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }

       
    }
}
