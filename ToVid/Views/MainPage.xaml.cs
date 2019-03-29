using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace ToVid.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        ///pre-declare things
        StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
        private string ffmepgLocate = AppDomain.CurrentDomain.BaseDirectory +
            @"\Services\ffmpeg\bin\ffmpeg.exe";
        string audioSend = "";
        string videoSend = "";
        string imageSend = "";


        public MainPage()
        {
            InitializeComponent();
            ///allow textboxes to be tapped
            audioIn.AddHandler(TappedEvent, new TappedEventHandler(AudioIn_Tapped), true);
            imageIn.AddHandler(TappedEvent, new TappedEventHandler(ImageIn_Tapped), true);
            videoOut.AddHandler(TappedEvent, new TappedEventHandler(VideoOut_Tapped), true);
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

        private async void AudioIn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Desktop,
                CommitButtonText = "Get Audio"
            };
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".wav");
            openPicker.FileTypeFilter.Add("*");

            StorageFile selectedFile = await openPicker.PickSingleFileAsync();

            if (selectedFile != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(selectedFile);
                /// Application now has read/write access to the picked file
                audioIn.Text = selectedFile.Path;
            }
        }

        private async void ImageIn_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FileOpenPicker imagePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Desktop,
                CommitButtonText = "Get Image"
            };
            imagePicker.FileTypeFilter.Add(".jpg");
            imagePicker.FileTypeFilter.Add(".png");
            imagePicker.FileTypeFilter.Add("*");

            StorageFile selectedFile = await imagePicker.PickSingleFileAsync();

            if (selectedFile != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(selectedFile);
                /// Application now has read/write access to the picked file
                ///send ORIGINAL path to UI
                imageIn.Text = selectedFile.Path;
            }
        }

        private async void VideoOut_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop
            };
            /// Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Plain Text", new List <string> () {
                ".mkv"
            });
            /// Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "output";
            savePicker.CommitButtonText = "Save here";
            

            Windows.Storage.StorageFile selectedFile = await savePicker.PickSaveFileAsync();
            if (selectedFile != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(selectedFile);
                videoOut.Text = selectedFile.Path;
                //Regex rx = new Regex(@"([\w]:\\.+\\)*(.+)\.(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                //MatchCollection matches = rx.Matches(videoOut.Text);

                /// Prevent updates to the remote version of the file until
                /// we finish making changes and call CompleteUpdatesAsync.
                //Windows.Storage.CachedFileManager.DeferUpdates(file);
                /// write to file
                //await Windows.Storage.FileIO.WriteTextAsync(file, file.Name);
                /// Let Windows know that we're finished changing the file so
                /// the other app can update the remote version of the file.
                /// Completing updates may require Windows to ask for user input.
                //Windows.Storage.Provider.FileUpdateStatus status =
                //    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                //if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                //{
                //    this.stat.Text += "\nFile " + file.Name + " was saved.";
                //}
            }
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ///Prevent edit
            imageIn.IsReadOnly = true;
            audioIn.IsReadOnly = true;
            videoOut.IsReadOnly = true;
            imageIn.Tapped -= ImageIn_Tapped;
            audioIn.Tapped -= AudioIn_Tapped;
            videoOut.Tapped -= VideoOut_Tapped;

            stat.Text += $"\nAudio: {audioIn.Text}\nImage: {imageIn.Text}\nOutput to: {videoOut.Text}";

            ///check if ffmpeg is located
            //if (File.Exists(ffmepgLocate))
            //{
            //    stat.Text += "\nFfmpeg found";
            //}
            //else
            //{
            //    stat.Text += "\nFfmpeg lost";
            //}


            ///Open Temp folder, if doesn't exist add a new one
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFolder temp = await localFolder.CreateFolderAsync("Temp", CreationCollisionOption.OpenIfExists);

            ///IMAGE WORK

            ///copy file into app's loading area
            StorageFile imageFileIn = await StorageFile.GetFileFromPathAsync(imageIn.Text);
            await imageFileIn.CopyAsync(temp, imageFileIn.Name, NameCollisionOption.ReplaceExisting);
            ///send NEW path to ffmpeg
            StorageFile imageLoad = await temp.GetFileAsync(imageFileIn.Name);
            imageSend = imageLoad.Path;

            ///AUDIO WORK

            ///copy file into app's loading area
            StorageFile audioFileIn = await StorageFile.GetFileFromPathAsync(audioIn.Text);
            await audioFileIn.CopyAsync(temp, audioFileIn.Name, NameCollisionOption.ReplaceExisting);
            ///send ORIGINAL path to UI, NEW path to ffmpeg
            StorageFile audioLoad = await temp.GetFileAsync(audioFileIn.Name);
            audioSend = audioLoad.Path;

            ///OUTPUT WORK

            ///prep file path

            StorageFile videoLoad = await StorageFile.GetFileFromPathAsync(videoOut.Text);
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(videoLoad);

            ///Output file to temp first
            //videoSend = temp.Path + "\\" + videoLoad.Name;
            videoSend = videoLoad.Path;

            ///prepare ffmpeg
            string ffmpegArg = $"-y -loop 1 -framerate 2 -i \"" + imageSend + "\" -i \"" + audioSend + "\" -c:v libx264 -tune stillimage -c:a aac -b:a 192k -pix_fmt yuv420p -shortest \"" + videoSend + "\"";
            //string strCmdText = @"-y -loop 1 -framerate 2 -i Assets/StoreLogo.png -i Assets/BBC.mp3 -c:v libx264 -tune stillimage -c:a aac -b:a 192k -pix_fmt yuv420p -shortest output.mkv -y  2> out.txt";
            Process process = new Process();
            process.StartInfo.FileName = ffmepgLocate;
            process.StartInfo.Arguments = ffmpegArg; //-ss 0 -i output.mp4 -t 10 -an -y test.mp4
            process.StartInfo.UseShellExecute = false;
            //process.StartInfo.RedirectStandardOutput = true;
            //process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;

            //process.StartInfo.CreateNoWindow = true;

            stat.Text += "\n" + ffmpegArg + "\n";

            ///start ffmpeg

            stat.Text += "\nBegin processing, it takes (at least) several minutes.\n";


            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {


                /// store command line parameters in local settings
                /// so the Lancher can retrieve them and pass them on
                ApplicationData.Current.LocalSettings.Values["parameters"] = ffmpegArg;

                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Parameters");
            }

            //await Task.Run(() =>
            //{
            //    process.Start();
            //    process.Exited += new EventHandler(Process_Exited);
            //    //string output = process.StandardOutput.ReadToEnd();
            //    //stat.Text += output;
            //    //string err = process.StandardError.ReadToEnd();
            //    //stat.Text += err;

            //}
            //    );

            //string output = process.StandardOutput.ReadToEnd();
            //stat.Text += output;
            //string err = process.StandardError.ReadToEnd();
            //stat.Text += err;

            ///output ffmpeg log





        //}
        //private async void Process_Exited(object sender, System.EventArgs e)
        //{
        ///Wait until ffmpeg ends
        //    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
        //    {
        stat.Text += "\nEnd";
                //if (File.Exists(videoSend))
                //{
                //    stat.Text += "\nOutput found";
                //}
                //else
                //{
                //    stat.Text += "\nOutput lost";
                //}
                //await Launcher.LaunchFolderPathAsync(videoSend);


                ///saving file OUTSIDE
                //stat.Text += "\nCopying " + videoSend;
                //StorageFile finalOut = await StorageFile.GetFileFromPathAsync(videoSend);
                //string filePath = Path.GetDirectoryName(videoOut.Text);
                //StorageFolder finalPath = await StorageFolder.GetFolderFromPathAsync(filePath);


                ///Copying file from inside to outside
                
                //StorageFile videoOuted = await StorageFile.GetFileFromPathAsync(videoSend);
                //StorageFile videoLoadFin = await StorageFile.GetFileFromPathAsync(videoOut.Text);
                //await videoOuted.CopyAndReplaceAsync(videoLoadFin);

        stat.Text += "\nSaved to " + videoOut.Text;


        imageIn.IsReadOnly = false;
        audioIn.IsReadOnly = false;
        videoOut.IsReadOnly = false;
        imageIn.IsTapEnabled = true;
        audioIn.IsTapEnabled = true;
        videoOut.IsTapEnabled = true;

            //);
        //}
        }

        private void Stat_TextChanged(object sender, TextChangedEventArgs e)
        {
            var grid = (Grid)VisualTreeHelper.GetChild(stat, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                break;
            }
        }

    }
}
