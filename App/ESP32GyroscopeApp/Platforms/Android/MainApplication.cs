using Android.App;
using Android.Bluetooth;
using Android.Runtime;

namespace ESP32GyroscopeApp
{
    [Application]
    public class MainApplication : MauiApplication
    {
        [Obsolete]
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
