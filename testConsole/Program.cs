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

namespace testConsole
{
    class Program
    {
        //private double d1, d2;
        //private AppServiceConnection connection = null;

        static void Main(string[] args)
        {
            string parameters = ApplicationData.Current.LocalSettings.Values["parameters"] as string;
            string ffmpegLocate = AppDomain.CurrentDomain.BaseDirectory +
            @"\ffmpeg\bin\ffmpeg.exe";

            ///Actually starting console
            //Console.Title = "CR001 Background";
            //Console.WriteLine("This process has access to the entire public desktop API surface");

            //if (File.Exists(ffmpegLocate))
            //{
            //    Console.WriteLine  ("\nFfmpeg found");
            //}
            //else
            //{
            //    Console.WriteLine  ("\nFfmpeg lost");
            //}

            var processInfo = new ProcessStartInfo
            {
                UseShellExecute = false, // change value to false
                FileName = ffmpegLocate,
                CreateNoWindow = true,
                //FileName = "cmd.exe",
                Arguments = parameters 
            };
            //Console.WriteLine("Starting child process...");
            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();
            }
            //Console.WriteLine("Press any key to exit ...");
            //Console.ReadKey();
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

        /// <summary>
        /// Handles the event when the desktop process receives a request from the UWP app
        /// </summary>
        //private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        //{
        //    // retrive the reg key name from the ValueSet in the request
        //    string key = args.Request.Message["KEY"] as string;
        //    int index = key.IndexOf('\\');
        //    if (index > 0)
        //    {
        //        // read the key values from the respective hive in the registry
        //        string hiveName = key.Substring(0, key.IndexOf('\\'));
        //        string keyName = key.Substring(key.IndexOf('\\') + 1);
        //        RegistryHive hive = RegistryHive.ClassesRoot;

        //        switch (hiveName)
        //        {
        //            case "HKLM":
        //                hive = RegistryHive.LocalMachine;
        //                break;
        //            case "HKCU":
        //                hive = RegistryHive.CurrentUser;
        //                break;
        //            case "HKCR":
        //                hive = RegistryHive.ClassesRoot;
        //                break;
        //            case "HKU":
        //                hive = RegistryHive.Users;
        //                break;
        //            case "HKCC":
        //                hive = RegistryHive.CurrentConfig;
        //                break;
        //        }

        //        using (RegistryKey regKey = RegistryKey.OpenRemoteBaseKey(hive, "").OpenSubKey(keyName))
        //        {
        //            // compose the response as ValueSet
        //            ValueSet response = new ValueSet();
        //            if (regKey != null)
        //            {
        //                foreach (string valueName in regKey.GetValueNames())
        //                {
        //                    response.Add(valueName, regKey.GetValue(valueName).ToString());
        //                }
        //            }
        //            else
        //            {
        //                response.Add("ERROR", "KEY NOT FOUND");
        //            }
        //            // send the response back to the UWP
        //            await args.Request.SendResponseAsync(response);
        //        }
        //    }
        //    else
        //    {
        //        ValueSet response = new ValueSet();
        //        response.Add("ERROR", "INVALID REQUEST");
        //        await args.Request.SendResponseAsync(response);
        //    }
        //}


    }
}
