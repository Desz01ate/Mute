using CSCore.CoreAudioAPI;
using Mute.Helpers;
using Mute.Sessions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mute
{
    /// <summary>
    /// Interaction logic for ExceptionConfigurationWindow.xaml
    /// </summary>
    public partial class ExceptionConfigurationWindow : Window
    {
        public ExceptionConfigurationWindow()
        {
            InitializeComponent();

            using var cacheFs = CacheFileManager.GetFileCache("exclude.json");
            using var streamReader = new StreamReader(cacheFs);
            var exceptions = JsonConvert.DeserializeObject<List<string>>(streamReader.ReadToEnd());

            var stackPanel = new StackPanel();
            stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
            stackPanel.VerticalAlignment = VerticalAlignment.Top;

            foreach (var session in GetSessionNames())
            {
                var checkBox = new CheckBox();
                checkBox.Content = session;
                if (exceptions.Contains(session))
                {
                    checkBox.IsChecked = true;
                }
                checkBox.Click += (s, e) =>
                {
                    if (checkBox.IsChecked.Value)
                    {
                        exceptions.Add(session);
                    }
                    else
                    {
                        exceptions.Remove(session);
                    }
                };

                stackPanel.Children.Add(checkBox);
            }

            this.ScrollViewer.Content = stackPanel;
            this.Closing += (s, e) =>
            {
                MessageBox.Show($"Exclude list has been updated, restart the program to take effect.");
                CacheFileManager.SaveCache("exclude.json", JsonConvert.SerializeObject(exceptions));
            };
        }

        private List<string> GetSessionNames()
        {
            var res = new List<string>();
            var thread = new Thread(() =>
            {
                using var sessionManager = SessionManager.GetDefaultAudioSessionManager2(DataFlow.Render);
                using var sessionEnumerator = sessionManager.GetSessionEnumerator();
                foreach (var session in sessionEnumerator)
                {
                    using var simpleVolume = session.QueryInterface<SimpleAudioVolume>();
                    using var sessionControl = session.QueryInterface<AudioSessionControl2>();
                    res.Add(sessionControl.Process.ProcessName);
                }
            });
            thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();
            thread.Join();
            return res;
        }
    }
}
