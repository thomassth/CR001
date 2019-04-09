using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ToVid.Views
{
    // TODO WTS: Remove this sample page when/if it's not needed.
    // This page is an sample of how to launch a specific page in response to a protocol launch and pass it a value.
    // It is expected that you will delete this page once you have changed the handling of a protocol launch to meet
    // your needs and redirected to another of your pages.
    public sealed partial class SchemeActivationSamplePage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<string> Parameters { get; } = new ObservableCollection<string>();

        public static BackgroundTaskDeferral AppServiceDeferral = null;
        public static AppServiceConnection Connection = null;
        //public static event EventHandler AppServiceDisconnected;
        //public static event EventHandler<AppServiceTriggerDetails> AppServiceConnected;
        public static bool IsForeground = false;

        public SchemeActivationSamplePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameters = e.Parameter as Dictionary<string, string>;
            if (parameters != null)
            {
                Initialize(parameters);
            }
        }

        public void Initialize(Dictionary<string, string> parameters)
        {
            Parameters.Clear();
            foreach (var param in parameters)
            {
                if (param.Key == "ticks" && long.TryParse(param.Value, out long ticks))
                {
                    var dateTime = new DateTime(ticks);
                    Parameters.Add($"{param.Key}: {dateTime.ToString()}");
                }
                else
                {
                    Parameters.Add($"{param.Key}: {param.Value}");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string ffmpegArg = @"ffmpeg -y -loop 1 -framerate 2 -i 'C: \Users\thoma\AppData\Local\Packages\b0379a39 - e70e - 4c21 - 8f94 - ffb23dcfa746_gkx4qx1bgg8j8\LocalState\Temp\AddCast@2x.png' -i 'C:\Users\thoma\AppData\Local\Packages\b0379a39 - e70e - 4c21 - 8f94 - ffb23dcfa746_gkx4qx1bgg8j8\LocalState\Temp\01 Pompeii.mp3' -c:v libx264 -tune stillimage -c:a aac -b:a 192k -pix_fmt yuv420p -shortest 'C:\Users\thoma\AppData\Local\Packages\b0379a39 - e70e - 4c21 - 8f94 - ffb23dcfa746_gkx4qx1bgg8j8\LocalState\Temp\1.mkv'";

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {


                // store command line parameters in local settings
                // so the Lancher can retrieve them and pass them on
                ApplicationData.Current.LocalSettings.Values["parameters"] = ffmpegArg;

                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Parameters");
            }


        }

        //public void OnBackgroundActivated (BackgroundActivatedEventArgs args)
        //{
        //    //base.OnBackgroundActivated(args);

        //    if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
        //    {
        //        // only accept connections from callers in the same package
        //        if (details.CallerPackageFamilyName == Package.Current.Id.FamilyName)
        //        {
        //            // connection established from the fulltrust process
        //            AppServiceDeferral = args.TaskInstance.GetDeferral();
        //            args.TaskInstance.Canceled += OnTaskCanceled;

        //            Connection = details.AppServiceConnection;
        //            AppServiceConnected?.Invoke(this, args.TaskInstance.TriggerDetails as AppServiceTriggerDetails);
        //        }
        //    }
        //}

        /// <summary>
        /// Task canceled here means the app service client is gone
        /// </summary>
        //private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        //{
        //    AppServiceDeferral?.Complete();
        //    AppServiceDeferral = null;
        //    Connection = null;
        //    AppServiceDisconnected?.Invoke(this, null);
        //}


    }
}
