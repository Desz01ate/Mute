using CSCore.CoreAudioAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mute.Sessions
{
    public static class SessionManager
    {
        public static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia);
            return AudioSessionManager2.FromMMDevice(device);
        }
    }
}
