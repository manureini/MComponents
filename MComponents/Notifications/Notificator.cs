using System;

namespace MComponents.Notifications
{
    public static class Notificator
    {
        internal static void InvokeNotification(IServiceProvider pServiceProvider, bool pIsError, string pLocalizedText)
        {
            var notificationService = (IMNotificationService)pServiceProvider.GetService(typeof(IMNotificationService));

            if (notificationService == null)
                return;

            notificationService.ShowNotification(new Notification()
            {
                IsError = pIsError,
                Text = pLocalizedText
            });
        }
    }
}
