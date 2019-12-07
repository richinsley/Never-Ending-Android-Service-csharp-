using Android.Content;
using Android.OS;
using Android.Util;
using System;

namespace NeverEndingAndroidService
{
    public class ProcessMainClass
    {
        public static String TAG = "ProcessMainClass";
        private static Intent serviceIntent = null;

        public ProcessMainClass()
        {
        }

        private void setServiceIntent(Context context)
        {
            if (serviceIntent == null)
            {
                serviceIntent = new Intent(context, Java.Lang.Class.FromType(typeof(MyService)));
            }
        }

        /**
         * launching the service
         */
        public void launchService(Context context)
        {
            if (context == null)
            {
                return;
            }
            setServiceIntent(context);
            // depending on the version of Android we eitehr launch the simple service (version<O)
            // or we start a foreground service
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                context.StartForegroundService(serviceIntent);
            }
            else
            {
                context.StartService(serviceIntent);
            }
            Log.Debug(TAG, "ProcessMainClass: start service go!!!!");
        }
    }
}