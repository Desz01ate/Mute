using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using static Mute.Engine.Events.ProcessMonitorEvent;

namespace Mute.Engine
{
    public sealed class ProcessMonitor : IDisposable
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        private IntPtr activeHandle = IntPtr.Zero;

        private readonly Timer worker;

        private bool disposed = false;

        public event WindowChanged OnWindowChanged;

        public event EventHandler OnStopped;

        private ProcessMonitor()
        {
            worker = new Timer();
            worker.Interval = 1000;
            worker.Elapsed += Worker_Elapsed;
            worker.Start();
        }

        private void Worker_Elapsed(object sender, ElapsedEventArgs e)
        {
            IntPtr handle = GetForegroundWindow();
            if (activeHandle == handle)
            {
                return;
            }

            try
            {
                var proc = Process.GetProcesses().Single(p => p.Id != 0 && p.MainWindowHandle == handle);
                OnWindowChanged?.Invoke(proc);
                activeHandle = handle;
            }
            catch (Exception ex)
            {
                // To be implement.
            }
        }

        private static ProcessMonitor instance;

        public static ProcessMonitor GetInstance()
        {
            return instance ?? (instance = new ProcessMonitor());
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing || disposed)
                return;

            this.OnStopped?.Invoke(this, null);
            disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
