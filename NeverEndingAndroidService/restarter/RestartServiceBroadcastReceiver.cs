using Android.Annotation;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Android.Util;
using System;

namespace NeverEndingAndroidService.restarter
{
    [BroadcastReceiver(Enabled = true, Exported = true, Label = "StartMyServiceAtBootReceiver")]
    [IntentFilter(new[] { "android.intent.action.BOOT_COMPLETED" }, Categories = new[] { Intent.CategoryDefault })]
    [IntentFilter(new[] { "android.intent.action.QUICKBOOT_POWERON" }, Categories = new[] { Intent.CategoryDefault })]
    [IntentFilter(new[] { "android.intent.action.MY_PACKAGE_REPLACED" }, Categories = new[] { Intent.CategoryDefault })]
    [IntentFilter(new[] { "uk.ac.shef.oak.activity_recognition.sensor_service.RestartSensor" }, Categories = new[] { Intent.CategoryDefault })]
    public class RestartServiceBroadcastReceiver : BroadcastReceiver
    {
        public static String TAG = "RestartServiceBroadcastReceiver";
        private static JobScheduler jobScheduler;
        private RestartServiceBroadcastReceiver restartSensorServiceReceiver;

        public override void OnReceive(Context context, Intent intent)
        {
            Log.Debug(TAG, "about to start timer " + context.ToString());
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                scheduleJob(context);
            }
            else
            {
                registerRestarterReceiver(context);
                ProcessMainClass bck = new ProcessMainClass();
                bck.launchService(context);
            }
        }

        [TargetApi(Value = 21)]
        public static void scheduleJob(Context context)
        {
            if (jobScheduler == null)
            {
                jobScheduler = (JobScheduler)context.GetSystemService(Context.JobSchedulerService);
            }
            ComponentName componentName = new ComponentName(context, Java.Lang.Class.FromType(typeof(MyJobService)));

            // setOverrideDeadline runs it immediately - you must have at least one constraint
            // https://stackoverflow.com/questions/51064731/firing-jobservice-without-constraints
            JobInfo jobInfo = new JobInfo.Builder(1, componentName)
                .SetOverrideDeadline(0)
                .SetPersisted(true)
                .Build();

            jobScheduler.Schedule(jobInfo);
        }

        public static void reStartTracker(Context context)
        {
            // restart the never ending service
            Log.Info(TAG, "Restarting tracker");
            Intent broadcastIntent = new Intent(Globals.RESTART_INTENT);
            context.SendBroadcast(broadcastIntent);
        }

        private void registerRestarterReceiver(Context context)
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
                    context.UnregisterReceiver(restartSensorServiceReceiver);
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
                    context.RegisterReceiver(restartSensorServiceReceiver, filter);
                }
                catch (Exception e)
                {
                    try
                    {
                        context.ApplicationContext.RegisterReceiver(restartSensorServiceReceiver, filter);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            });

            new Handler().PostDelayed(r, 1000);
        }
    }
}