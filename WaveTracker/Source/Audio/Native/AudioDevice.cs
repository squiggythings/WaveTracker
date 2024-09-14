using System;
using System.Runtime.Versioning;

namespace WaveTracker.Audio.Native {
    public class AudioDevice {
        public static AudioDevice DefaultOutputDevice {
            get {
                if (OperatingSystem.IsWindows())
                    return DefaultWindowsOutputDevice;
                else if (OperatingSystem.IsLinux())
                    return DefaultLinuxOutputDevice;
                else if (OperatingSystem.IsMacOS())
                    return DefaultMacosOutputDevice;
                else
                    throw new PlatformNotSupportedException("This platform has no audio device");
            }
        }

        [SupportedOSPlatform("Linux")]
        internal static AudioDevice DefaultLinuxOutputDevice => new AudioDevice {
            ID = "default",
            Name = "default",
        };

        [SupportedOSPlatform("Windows")]
        internal static AudioDevice DefaultWindowsOutputDevice => throw new NotImplementedException();

        [SupportedOSPlatform("Macos")]
        internal static AudioDevice DefaultMacosOutputDevice => throw new NotImplementedException();

        public string Name;

        [SupportedOSPlatform("Linux")]
        internal string ID;
    }
}
