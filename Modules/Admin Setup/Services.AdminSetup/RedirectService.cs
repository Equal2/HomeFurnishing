using System.Collections.Generic;
using System.Linq;
using System;
using Infrastructure.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using Data;
using Models.Company.Models;
using Models.Company.ViewModels;
using AutoMapper;

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
