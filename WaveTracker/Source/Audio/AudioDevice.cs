using System;
using System.Runtime.Versioning;

namespace WaveTracker.Audio {
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
            Name = "default",
            ID = "default",
        };

        [SupportedOSPlatform("Windows")]
        internal static AudioDevice DefaultWindowsOutputDevice => new AudioDevice {
            Name = "default",
            DeviceNumber = 0xffffffff, // WAVE_MAPPER
        };

        [SupportedOSPlatform("Macos")]
        internal static AudioDevice DefaultMacosOutputDevice => throw new NotImplementedException();

        public string Name;

        [SupportedOSPlatform("Linux")]
        internal string ID;

        [SupportedOSPlatform("Windows")]
        internal uint DeviceNumber;
    }
}
