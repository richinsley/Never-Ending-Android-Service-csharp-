using Android.Annotation;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Android.Util;
using System;

namespace NeverEndingAndroidService.restarter
{
    [Service(Permission = "android.permission.BIND_JOB_SERVICE")]
    [TargetApi(Value = 21)]
    public class MyJobService : JobService
    {
        private static String TAG = "MyJobService";
        private static RestartServiceBroadcastReceiver restartSensorServiceReceiver;
        private static MyJobService instance;
        private static JobParameters jobParameters;

        public override bool OnStartJob(JobParameters jobParameters)
        {
            ProcessMainClass bck = new ProcessMainClass();
            bck.launchService(this);
            registerRestarterReceiver();
            instance = this;
            MyJobService.jobParameters = jobParameters;

            return false;
        }

        private void registerRestarterReceiver()
        {
            // the context can be null if app just installed and this is called from restartsensorservice
            // https://stackoverflow.com/questions/24934260/intentreceiver-components-are-not-allowed-to-register-to-receive-intents-when
            // Final decision: in case it is called from installation of new version (i.e. from manifest, the application is
            // null. So we must use context.registerReceiver. Otherwise this will crash and we try with context.getApplicationContext
            if (restartSensorServiceReceiver == null)
            {
                restartSensorServiceReceiver = new RestartServiceBroadcastReceiver();
            }
            else try
                {
                    UnregisterReceiver(restartSensorServiceReceiver);
                }
                catch (Exception e)
                {
                    // not registered
                }

            Java.Lang.Runnable r = new Java.Lang.Runnable(() =>
            {
                // we register the  receiver that will restart the background service if it is killed
                // see onDestroy of Service
                IntentFilter filter = new IntentFilter();
                filter.AddAction(Globals.RESTART_INTENT);
                try
                {
                    RegisterReceiver(restartSensorServiceReceiver, filter);
                }
                catch (Exception e)
                {
                    try
                    {
                        Android.App.Application.Context.RegisterReceiver(restartSensorServiceReceiver, filter);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            new Handler().PostDelayed(r, 1000);
        }

        public override bool OnStopJob(JobParameters jobParameters)
        {
            Log.Info(TAG, "Stopping job");
            Intent broadcastIntent = new Intent(Globals.RESTART_INTENT);
            SendBroadcast(broadcastIntent);

            Java.Lang.Runnable r = new Java.Lang.Runnable(() =>
            {
                UnregisterReceiver(restartSensorServiceReceiver);
            });

            new Handler().PostDelayed(r, 1000);

            return false;
        }

        // i'm not sure how this gets invoked (if ever)
        public static void stopJob(Context context)
        {
            if (instance != null && jobParameters != null)
            {
                try
                {
                    instance.UnregisterReceiver(restartSensorServiceReceiver);
                }
                catch (Exception e)
                {
                    // not registered
                }
                Log.Info(TAG, "Finishing job");
                instance.JobFinished(jobParameters, true);
            }
        }

    }
}