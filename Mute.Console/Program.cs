using CSCore.CoreAudioAPI;
using Mute.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Mute.Console
{
    class Program
    {
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private static ProcessMonitor monitor;
        static void Main(string[] args)
        {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            monitor = ProcessMonitor.GetInstance();
            monitor.OnWindowChanged += Mon_OnWindowChanged;
            monitor.OnStopped += Monitor_OnStopped;
            while (true)
            {
                Thread.Sleep(100);
            }
        }

        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                monitor?.Dispose();
            }
            return false;
        }

        private static void Monitor_OnStopped(object sender, EventArgs e)
        {
            using var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render);
            using var sessionEnumerator = sessionManager.GetSessionEnumerator();
            foreach (var session in sessionEnumerator)
            {

                using var simpleVolume = session.QueryInterface<SimpleAudioVolume>();
                using var sessionControl = session.QueryInterface<AudioSessionControl2>();
                simpleVolume.IsMuted = false;
            }
        }



        private static void Mon_OnWindowChanged(Process proc)
        {
            System.Console.Clear();
            System.Console.WriteLine($"Active process : {proc.ProcessName}({proc.MainWindowHandle})");
            using var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render);
            using var sessionEnumerator = sessionManager.GetSessionEnumerator();
            foreach (var session in sessionEnumerator)
            {

                using var simpleVolume = session.QueryInterface<SimpleAudioVolume>();
                using var sessionControl = session.QueryInterface<AudioSessionControl2>();
                System.Console.WriteLine($"--- {sessionControl.Process.ProcessName} + {sessionControl.Process.MainWindowHandle}");
                if (sessionControl.ProcessID == proc.Id)
                {
                    simpleVolume.IsMuted = false;
                    simpleVolume.MasterVolume = 1f;
                }
                else if (proc.ProcessName == "ApplicationFrameHost" && sessionControl.Process.ProcessName == "WWAHost")
                {
                    simpleVolume.IsMuted = false;
                    simpleVolume.MasterVolume = 1f;
                }
                else
                {
                    simpleVolume.IsMuted = true;
                }
            }

            //var app = proc.ProcessName;
            //var apps = EnumerateApplications().ToArray();
            //foreach (string name in EnumerateApplications())
            //{
            //    System.Console.WriteLine("name:" + name);
            //    if (name == app)
            //    {
            //        // display mute state & volume level (% of master)
            //        System.Console.WriteLine("Mute:" + GetApplicationMute(app));
            //        System.Console.WriteLine("Volume:" + GetApplicationVolume(app));

            //        // mute the application
            //        SetApplicationMute(name, false);

            //        // set the volume to half of master volume (50%)
            //        SetApplicationVolume(name, 100);
            //    }
            //    else
            //    {
            //        SetApplicationMute(name, true);
            //    }
            //}
        }

        private static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
        {
            using (MMDeviceEnumerator enumerator = new MMDeviceEnumerator())
            {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia))
                {
                    System.Console.WriteLine("DefaultDevice: " + device.FriendlyName);
                    var sessionManager = AudioSessionManager2.FromMMDevice(device);
                    return sessionManager;
                }
            }
        }

        private static IEnumerable<AudioSessionManager2> EnumerateAudioSessionManager2(DataFlow dataFlow)
        {
            using (MMDeviceEnumerator enumerator = new MMDeviceEnumerator())
            {
                using (var devices = enumerator.EnumAudioEndpoints(dataFlow, DeviceState.Active))
                {
                    foreach (var device in devices)
                    {
                        System.Console.WriteLine("DefaultDevice: " + device.FriendlyName);
                        var sessionManager = AudioSessionManager2.FromMMDevice(device);
                        yield return sessionManager;
                    }

                }
            }
        }
    }
}
