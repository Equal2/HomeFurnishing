using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Notifier.ViewModels;

namespace Notifier.Hubs
{
    [HubName("Notify")]
    public class Notifications : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }

        public void SendUpdate(int val,string UserId)
        {
            Clients.User(UserId).SendUpdate(val);
        }

        public NotificationListViewModel PendingMessages()
        {
            var temp = RegisterChanges.GetUserUpdates(Context.User.Identity.Name);
            
            return temp;
        }        

        public void SetNotificationSeen()
        {
            RegisterChanges.SetNotificationSeen(Context.User.Identity.Name);
        }     

    }
}