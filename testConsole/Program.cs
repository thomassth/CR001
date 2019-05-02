using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Reflection;
using Windows.ApplicationModel.AppService;
using System.Collections.ObjectModel;
using Windows.ApplicationModel;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using Windows.Foundation.Collections;
using System.Windows.Threading;
using System.Threading;

using Windows.ApplicationModel.Background;

namespace testConsole
{
    class Program
    {
        string outputStore = ApplicationData.Current.TemporaryFolder.ToString() + @"\output.txt";
        static BackgroundTaskDeferral backgroundTaskDeferral;

        private String[] inventoryItems = new string[] { "Done", "Not Done" };
        private double[] inventoryPrices = new double[] { 1, 0 };

        bool doneFfmpeg = false;

        static AppServiceConnection inventoryService;

        static string Text = "";

        //private double d1, d2;
        //private AppServiceConnection connection = null;
         static void Main(string[] args)
        {
            ///Store the output
            ///Actually starting console
            //Console.Title = "CR001 Background";
            //Console.WriteLine("This process has access to the entire public desktop API surface");
            //if (File.Exists(ffmpegLocate))
            //{
            //    Console.WriteLine("\nFfmpeg found");
            //}
            //else
            //{
            //    Console.WriteLine("\nFfmpeg lost");
            //}
            runFFMPEG();
            //Console.ReadKey();
        }

        private static void runFFMPEG()
        {
            string parameters = ApplicationData.Current.LocalSettings.Values["parameters"] as string;
            string ffmpegLocate = AppDomain.CurrentDomain.BaseDirectory +
            @"\ffmpeg\bin\ffmpeg.exe";

            var processInfo = new ProcessStartInfo
            {
                UseShellExecute = false, // change value to false
                FileName = ffmpegLocate,
                CreateNoWindow = true,
                //FileName = "cmd.exe",
                RedirectStandardError = true,
                RedirectStandardOutput = true, // Is a MUST!
                Arguments = parameters
            };
            //Console.WriteLine("Starting child process...");
            using (var process = Process.Start(processInfo))
            {
                var p = new Process();
                p.StartInfo = processInfo;
                p.EnableRaisingEvents = true;




                p.OutputDataReceived += OutputDataReceived;
                p.ErrorDataReceived += ErrorDataReceived;

                p.Start();

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();

                p.OutputDataReceived -= OutputDataReceived;
                p.ErrorDataReceived -= ErrorDataReceived;
                Environment.Exit(0);


            }
            //Console.WriteLine("Press any key to exit ...");

            //sendEndSignalAsync().GetAwaiter().GetResult();


        }

        static async Task sendEndSignalAsync()
        {
            // Add the app connection.
            if (inventoryService == null)
            {
                inventoryService = new AppServiceConnection();

                // Here, we use the app service name defined in the app service 
                // provider's Package.appxmanifest file in the <Extension> section.
                inventoryService.AppServiceName = "com.microsoft.inventory";

                // Use Windows.ApplicationModel.Package.Current.Id.FamilyName 
                // within the app service provider to get this value.
                inventoryService.PackageFamilyName = Windows.ApplicationModel.Package.Current.Id.FamilyName;

                var status = await inventoryService.OpenAsync();

                if (status != AppServiceConnectionStatus.Success)
                {
                    Text += "\nFailed to connect";
                    inventoryService = null;
                    return;
                }
            }

            /// Send the message.
            //int idx = int.Parse(sendViaAppService);
            string thing = "End";
            var message = new ValueSet();
            //message.Add("Command", "Item");
            //message.Add("ID", idx);
            message.Add("status",thing);
            AppServiceResponse response = await inventoryService.SendMessageAsync(message);
            //string result = "";        }
        }

        private static void ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {

            string outputStore = ApplicationData.Current.LocalSettings.Values["outfile"].ToString();

            //using (System.IO.StreamWriter file =
            //new System.IO.StreamWriter(outputStore, true))
            //{
            //    file.WriteLine(e.Data);
            //}
        }

        private static void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string outputStore = ApplicationData.Current.LocalSettings.Values["outfile"].ToString();

            //using (System.IO.StreamWriter file =
            //new System.IO.StreamWriter(outputStore, true))
            //{
            //    file.WriteLine(e.Data);
            //}
        }


        /// Handles the event when the desktop process receives a request from the UWP app
        private async void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            // This function is called when the app service receives a request.
            // Get a deferral because we use an awaitable API below to respond to the message
            // and we don't want this call to get canceled while we are waiting.
            var messageDeferral = args.GetDeferral();
            ///App services use ValueSet objects to exchange information.
            ValueSet message = args.Request.Message;
            ValueSet returnData = new ValueSet();

            string command = message["Command"] as string;
            int? inventoryIndex = message["ID"] as int?;

            if (inventoryIndex.HasValue &&
                inventoryIndex.Value >= 0 &&
                inventoryIndex.Value < inventoryItems.GetLength(0))
            {
                switch (command)
                {
                    case "Price":
                        {
                            returnData.Add("Result", inventoryPrices[inventoryIndex.Value]);
                            returnData.Add("Status", "OK");
                            break;
                        }

                    case "Item":
                        {
                            returnData.Add("Result", inventoryItems[inventoryIndex.Value]);
                            returnData.Add("Status", "OK");
                            break;
                        }

                    default:
                        {
                            returnData.Add("Status", "Fail: unknown command");
                            break;
                        }
                }
            }
            else
            {
                returnData.Add("Status", "Fail: Index out of range");
            }

            try
            {
                // Return the data to the caller.
                await args.Request.SendResponseAsync(returnData);
            }
            catch (Exception e)
            {
                // Your exception handling code here.
            }
            finally
            {
                // Complete the deferral so that the platform knows that we're done responding to the app service call.
                // Note for error handling: this must be called even if SendResponseAsync() throws an exception.
                messageDeferral.Complete();
            }
        }



        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (backgroundTaskDeferral != null)
            {
                // Complete the service deferral.
                backgroundTaskDeferral.Complete();
            }
        }



        /// <summary>
        /// Open connection to UWP app service
        /// </summary>
        //private async void InitializeAppServiceConnection()
        //{
        //    connection = new AppServiceConnection();
        //    connection.AppServiceName = "SampleInteropService";
        //    connection.PackageFamilyName = Package.Current.Id.FamilyName;
        //    connection.RequestReceived += Connection_RequestReceived;
        //    connection.ServiceClosed += Connection_ServiceClosed;

        //    AppServiceConnectionStatus status = await connection.OpenAsync();
        //    if (status != AppServiceConnectionStatus.Success)
        //    {
        //        // something went wrong ...
        //        MessageBox.Show(status.ToString());
        //        this.IsEnabled = false;
        //    }
        //}

        /// <summary>
        /// Handles the event when the app service connection is closed
        /// </summary>
        //private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        //{
        //    //connection to the UWP lost, so we shut down the desktop process
        //    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
        //    {
        //        Application.Current.Shutdown();
        //    }));
        //}
    }
}
