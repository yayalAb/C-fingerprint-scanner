using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
//using Windows.UI.Notifications;

namespace ReadFingerprintDemo
{
    public class DisplayNotificationClass
    {
        public void displayNotfication(string TextMassage)
        {
           Console.WriteLine($"Notification : {TextMassage}");
            //new ToastContentBuilder()
            //      .AddText("fingerprint scanner")
            //      .AddText(TextMassage)
            //      .AddAppLogoOverride((new Uri("fingerprintimage.jpg")), ToastGenericAppLogoCrop.Circle)
            //      .Show();
        }
    }
}

