using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;
//using Windows.UI.Notifications;

namespace ocraFingerprint
{
    public class DisplayNotificationClass
    {
        public void displayNotfication(string TextMassage)
        {
            Console.WriteLine("Notification : ", TextMassage);
        //    new ToastContentBuilder()
          //        .AddText("Fingerprint Scanner")
            //      .AddText(TextMassage)
              //    .AddAppLogoOverride((new Uri("C:\\Users\\Yayal\\Documents\\OCRA Project\\FpBackgroundService\\fingerprintImage2.JPG")), ToastGenericAppLogoCrop.Circle)
                //  .Show();
        }
    }
}

