using System;

namespace MComponents
{
    public static class Notificator
    {
        public static Action<Notification> NotificationHandler;

        internal static void InvokeNotification(bool pIsError, string pText)
        {
            if (NotificationHandler == null)
                return;

            NotificationHandler.Invoke(new Notification()
            {
                IsError = pIsError,
                Text = pText
            });
        }
    }
}
