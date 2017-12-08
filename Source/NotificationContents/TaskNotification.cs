using Model.Models;
using System.Threading.Tasks;
//using Models.Login.Models;

namespace NotificationContents
{
    public class TaskNotification
    {
        public async Task CreateTaskNotificationAsync(Notification NN, string User)
        {
            NotificationSave ns = new NotificationSave();
            await ns.SaveNotificationAsync(NN, User);
        }
    }
}