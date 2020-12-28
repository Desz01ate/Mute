using CSCore.CoreAudioAPI;
using Mute.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mute.Sessions;
using Mute.Helpers;
using System.IO;
using Newtonsoft.Json;

namespace Mute
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ProcessMonitor monitor;
        private readonly object lockObject = new object();
        private readonly IReadOnlyList<string> exceptions;
        public MainWindow()
        {
            InitializeComponent();

            using var cacheFs = CacheFileManager.GetFileCache("exclude.json");
            using var streamReader = new StreamReader(cacheFs);
            exceptions = JsonConvert.DeserializeObject<List<string>>(streamReader.ReadToEnd());

            monitor = ProcessMonitor.GetInstance();
            monitor.OnWindowChanged += Mon_OnWindowChanged;
            monitor.OnStopped += Monitor_OnStopped;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var t = new System.Threading.Thread(() => this.monitor?.Dispose());
            t.SetApartmentState(System.Threading.ApartmentState.MTA);
            t.Start();
        }

        [STAThread]
        private void Monitor_OnStopped(object sender, EventArgs e)
        {
            using var sessionManager = SessionManager.GetDefaultAudioSessionManager2(DataFlow.Render);
            using var sessionEnumerator = sessionManager.GetSessionEnumerator();
            foreach (var session in sessionEnumerator)
            {
                using var simpleVolume = session.QueryInterface<SimpleAudioVolume>();
                using var sessionControl = session.QueryInterface<AudioSessionControl2>();
                simpleVolume.IsMuted = false;
            }
        }

        private void Mon_OnWindowChanged(Process proc)
        {
            this.Dispatcher.Invoke(() => this.lbl_ActiveWindow.Content = proc.ProcessName);

            using var sessionManager = SessionManager.GetDefaultAudioSessionManager2(DataFlow.Render);
            using var sessionEnumerator = sessionManager.GetSessionEnumerator();
            foreach (var session in sessionEnumerator)
            {
                using var simpleVolume = session.QueryInterface<SimpleAudioVolume>();
                using var sessionControl = session.QueryInterface<AudioSessionControl2>();
                if (exceptions.Contains(sessionControl.Process.ProcessName))
                {
                    continue;
                }

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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var config = new ExceptionConfigurationWindow();
            config.ShowDialog();
        }
    }
}
