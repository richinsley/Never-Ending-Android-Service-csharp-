using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using System;

namespace NeverEndingAndroidService.utilities
{
    public class MyNotification
    {
        private PendingIntent notificationPendingIntent;

        /**
         * This is the method called to create the Notification
         */
        public Notification setNotification(Context context, String title, String text, int icon)
        {
            if (notificationPendingIntent == null)
            {
                Intent notificationIntent = new Intent(context, Java.Lang.Class.FromType(typeof(MainActivity)));
                notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
                notificationPendingIntent = PendingIntent.GetActivity(context, 0, notificationIntent, 0);
            }

            Notification notification;
            NotificationManager notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

            // OREO
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                // Create the NotificationChannel, but only on API 26+ because
                // the NotificationChannel class is new and not in the support library
                Java.Lang.ICharSequence name = new Java.Lang.String("Permanent Notification");
                NotificationImportance importance = NotificationImportance.Low;

                String CHANNEL_ID = "uk.ac.shef.oak.channel";
                NotificationChannel channel = new NotificationChannel(CHANNEL_ID, name, importance);
                String description = "I would like to receive travel alerts and notifications for:";
                channel.Description = description;

                NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(context, CHANNEL_ID);
                if (notificationManager != null)
                {
                    notificationManager.CreateNotificationChannel(channel);
                }

                notification = notificationBuilder
                    //the log is PNG file format with a transparent background
                    .SetSmallIcon(icon)
                    .SetColor(ContextCompat.GetColor(context, Resource.Color.colorAccent))
                    .SetContentTitle(title)
                    .SetContentText(text)
                    .SetContentIntent(notificationPendingIntent)
                    .Build();

            }
            else if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                notification = new NotificationCompat.Builder(context, "channel")
                    // to be defined in the MainActivity of the app
                    .SetSmallIcon(icon)
                    .SetContentTitle(title)
                    .SetContentText(text)
                    .SetPriority(NotificationCompat.PriorityMin)
                    .SetContentIntent(notificationPendingIntent).Build();
            }
            else
            {
                notification = new NotificationCompat.Builder(context, "channel")
                    // to be defined in the MainActivity of the app
                    .SetSmallIcon(icon)
                    .SetContentTitle(title)
                    .SetContentText(text)
                    .SetPriority(NotificationCompat.PriorityMin)
                    .SetContentIntent(notificationPendingIntent).Build();
            }

            return notification;
        }
    }
}