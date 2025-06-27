using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using WaveTracker.Midi.Interop.Linux;

namespace WaveTracker.Midi {
    /// <summary>
    /// Info about a MIDI device.
    /// </summary>
    public class MidiInDeviceID {
        /// <summary>
        /// Name of the device
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Device number (Windows only)
        /// </summary>
        [SupportedOSPlatform("Windows")]
        public int DeviceNumber { get; internal set; }

        /// <summary>
        /// Client number (Linux only)
        /// </summary>
        [SupportedOSPlatform("Linux")]
        public int Client { get; internal set; }

        /// <summary>
        /// Port number (Linux only)
        /// </summary>
        [SupportedOSPlatform("Linux")]
        public int Port { get; internal set; }
    }

    /// <summary>
    /// Class to fetch MIDI In devices.
    /// </summary>
    public static class MidiInDevices {
        /// <summary>
        /// Get a list of all the devices
        /// </summary>
        public static List<MidiInDeviceID> GetDeviceIDs() {
            List<MidiInDeviceID> devices = new();

            unsafe {
                if (OperatingSystem.IsWindows()) {
                    for (int i = 0; i < MidiInWindows.NumberOfDevices; i++) {
                        devices.Add(new MidiInDeviceID() {
                            Name = MidiInWindows.DeviceInfo(i).ProductName,
                            DeviceNumber = i,
                        });
                    }
                }
                else if (OperatingSystem.IsLinux()) {
                    nint seqHandle;
                    Alsa.snd_seq_open(&seqHandle, "default", Alsa.SND_SEQ_OPEN_INPUT, 0);
                    Alsa.snd_seq_set_client_name(seqHandle, "WaveTracker MIDI Device Iterator");

                    nint clientInfo;
                    Alsa.snd_seq_client_info_malloc(&clientInfo);

                    nint portInfo;
                    Alsa.snd_seq_port_info_malloc(&portInfo);

                    Alsa.snd_seq_client_info_set_client(clientInfo, -1);
                    while (Alsa.snd_seq_query_next_client(seqHandle, clientInfo) >= 0) {
                        int client = Alsa.snd_seq_client_info_get_client(clientInfo);

                        Alsa.snd_seq_port_info_set_client(portInfo, client);
                        Alsa.snd_seq_port_info_set_port(portInfo, -1);
                        while (Alsa.snd_seq_query_next_port(seqHandle, portInfo) >= 0) {
                            // only list ports we can read from
                            uint capability = Alsa.snd_seq_port_info_get_capability(portInfo);
                            if ((capability & Alsa.SND_SEQ_PORT_CAP_READ) == 0)
                                continue;

                            string clientName = new string(Alsa.snd_seq_client_info_get_name(clientInfo));

                            int port = Alsa.snd_seq_port_info_get_port(portInfo);
                            string portName = new string(Alsa.snd_seq_port_info_get_name(portInfo));

                            devices.Add(new MidiInDeviceID() {
                                Name = $"{clientName} ({port}: {portName})",
                                Client = client,
                                Port = port,
                            });
                        }
                    }

                    Alsa.snd_seq_port_info_free(portInfo);
                    Alsa.snd_seq_client_info_free(clientInfo);
                    Alsa.snd_seq_close(seqHandle);
                }
            }

            return devices;
        }

        /// <summary>
        /// Get MIDI input device corresponding to this device ID
        /// </summary>
        public static IMidiIn GetDevice(MidiInDeviceID id) {
            unsafe {
                if (OperatingSystem.IsWindows()) {
                    return new MidiInWindows(id.DeviceNumber);
                }
                else if (OperatingSystem.IsLinux()) {
                    return new MidiInLinux(id.Client, id.Port);
                }
                else {
                    return null;
                }
            }
        }
    }
}