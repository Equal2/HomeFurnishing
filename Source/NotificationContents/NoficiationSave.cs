using Data.Models;
using Model.Models;
//using Models.Login.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NotificationContents
{
    sealed class NotificationSave
    {

        public async Task SaveNotificationAsync(Notification nu, string UserName)
        {
            using (NotificationApplicationDbContext context = new NotificationApplicationDbContext())
            {

                nu.ObjectState = Model.ObjectState.Added;
                context.Notification.Add(nu);

                NotificationUser User = new NotificationUser();
                User.NotificationId = nu.NotificationId;
                User.UserName = UserName;
                User.ObjectState = Model.ObjectState.Added;
                context.NotificationUser.Add(User);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    try
                    {
                        using (StreamWriter writer =
                         new StreamWriter(@"c:\temp\NotificationException.txt", true))
                        {
                            writer.WriteLine(" {0} : " + ex.ToString(), DateTime.Now);
                            writer.Close();
                        }
                    }
                    catch (Exception x)
                    {

                    }
                }
            }
        }


    }
}
