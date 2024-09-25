using Microsoft.Toolkit.Uwp.Notifications;

namespace WPass.Utility.OtherHandler
{
    public class ToastHelper
    {
        public static void Show(string title, string content)
        {
            // Construct the toast content
            var toastContent = new ToastContentBuilder()
                .AddText(title)
                .AddText(content);
            //.GetToastContent();

            // Show the toast
            toastContent.Show();


            //var toastNotification = new ToastNotification(toastContent.GetXml())
            //{
            //    ExpirationTime = DateTime.Now.AddSeconds(3),
            //};

            //ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }
    }
}
