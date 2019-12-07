using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using System;

namespace NeverEndingAndroidService
{
    [Service(Enabled = true)]
    public class MyService : Service
    {
        protected static int NOTIFICATION_ID = 1337;
        private static String TAG = "Service";
        private static Service mCurrentService;
        private int counter = 0;

        public MyService() : base()
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                restartForeground();
            }
            mCurrentService = this;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);

            Log.Debug(TAG, "restarting Service !!");
            counter = 0;

            // it has been killed by Android and now it is restarted. We must make sure to have reinitialised everything
            if (intent == null)
            {
                ProcessMainClass bck = new ProcessMainClass();
                bck.launchService(this);
            }

            // make sure you call the startForeground on onStartCommand because otherwise
            // when we hide the notification on onScreen it will nto restart in Android 6 and 7
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                restartForeground();
            }

            startTimer();

            // return start sticky so if it is killed by android, it will be restarted with Intent null
            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        /**
         * it starts the process in foreground. Normally this is done when screen goes off
         * THIS IS REQUIRED IN ANDROID 8 :
         * "The system allows apps to call Context.startForegroundService()
         * even while the app is in the background.
         * However, the app must call that service's startForeground() method within five seconds
         * after the service is created."
         */
        public void restartForeground()
        {
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                Log.Info(TAG, "restarting foreground");
                try
                {
                    utilities.MyNotification notification = new utilities.MyNotification();
                    StartForeground(NOTIFICATION_ID, notification.setNotification(this, "Service notification", "This is the service's notification", Resource.Drawable.ic_sleep));
                    Log.Info(TAG, "restarting foreground successful");
                    startTimer();
                }
                catch (Exception e)
                {
                    Log.Error(TAG, "Error in notification " + e.Message);
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Info(TAG, "onDestroy called");
            // restart the never ending service
            Intent broadcastIntent = new Intent(Globals.RESTART_INTENT);
            SendBroadcast(broadcastIntent);
            stoptimertask();
        }

        /**
         * this is called when the process is killed by Android
         *
         * @param rootIntent
         */
        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);
            Log.Info(TAG, "onTaskRemoved called");
            // restart the never ending service
            Intent broadcastIntent = new Intent(Globals.RESTART_INTENT);
            SendBroadcast(broadcastIntent);
            // do not call stoptimertask because on some phones it is called asynchronously
            // after you swipe out the app and therefore sometimes
            // it will stop the timer after it was restarted
            // stoptimertask();
        }

        /*
         * static to avoid multiple timers to be created when the service is called several times
         */
        private static System.Timers.Timer timer;

        public void startTimer()
        {
            Log.Info(TAG, "Starting timer");

            //set a new Timer - if one is already running, cancel it to avoid two running at the same time
            stoptimertask();

            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Enabled = true;
            timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
            {
                Log.Info("in timer", "in timer ++++  " + (counter++));
            };
            timer.Start();
        }

        public void stoptimertask()
        {
            //stop the timer, if it's not already null
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }
        }

        public Service CurrentService
        {
            get { return mCurrentService; }
            set { mCurrentService = value; }
        }
    }
}
