using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;

// The following permissions need to be added to the manifest via project settings
// RECEIVE_LAUNCH_BROADCASTS
// RECEIVE_BOOT_COMPLETED
// FOREGROUND_SERVICE

// Android Q restrictions
// https://developer.android.com/guide/components/activities/background-starts#warning-messages

namespace NeverEndingAndroidService
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                restarter.RestartServiceBroadcastReceiver.scheduleJob(Application.Context);
            }
            else
            {
                ProcessMainClass bck = new ProcessMainClass();
                bck.launchService(Application.Context);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}