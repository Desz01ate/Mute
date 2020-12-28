using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Mute.Engine.Events
{
    public static class ProcessMonitorEvent
    {
        public delegate void WindowChanged(Process proc);
    }
}
