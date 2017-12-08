using Core.Common;
using Data.Models;
using Model.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
//using Models.Login.Models;

namespace NotificationContents
{
    public class RegistrationNotification
    {
        public async Task CreateRegistrationNotificationAsync(string UserId)
        {
            Notification NN = new Notification();

            //ApplicationUser user = new ApplicationUser(); ;

            //using (LoginApplicationDbContext db = new LoginApplicationDbContext())
            //{
            //    user = (from p in db.Users
            //            where p.Id == UserId
            //            select p).FirstOrDefault();
            //}

            NN.NotificationSubjectId = (int)NotificationSubjectConstants.UserRegistered;
            NN.CreatedBy = "System";
            NN.CreatedDate = DateTime.Now;
            NN.ExpiryDate = DateTime.Now.AddDays(7);
            NN.IsActive = true;
            NN.ModifiedBy = "System";
            NN.ModifiedDate = DateTime.Now;
            //NN.NotificationText = "New user " + user.UserName + " registered";
            NN.NotificationText = "New user registered";

            NotificationSave ns = new NotificationSave();
            await ns.SaveNotificationAsync(NN, "madhan");
        }
    }
}