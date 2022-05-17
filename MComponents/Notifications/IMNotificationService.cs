using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MComponents.Notifications
{
    public interface IMNotificationService
    {
        void ShowNotification(Notification pNotification);
    }
}
