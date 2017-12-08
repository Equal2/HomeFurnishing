using System;
using Infrastructure.IO;
using Models.Company.Models;

namespace Services.Customize
{
    public interface INotificationService : IDisposable
    {
        Site GetSite(int Id);
        Division GetDivision(int Id);
    }

    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        public NotificationService(IUnitOfWork uow)
        {
            _unitOfWork = uow;
        }

        public Site GetSite(int Id)
        {
            return (_unitOfWork.Repository<Site>().Find(Id));
        }

        public Division GetDivision(int Id)
        {
            return _unitOfWork.Repository<Division>().Find(Id);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
