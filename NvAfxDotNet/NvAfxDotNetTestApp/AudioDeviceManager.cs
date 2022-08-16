using CSCore.CoreAudioAPI;
using CSCore.Win32;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using static NvAfxDotNetTestApp.AudioDeviceManager.DeviceUpdatedEventArgs;

namespace NvAfxDotNetTestApp
{
    public class AudioDeviceManager
    {
        private static readonly string TAG = "AudioDeviceManager";

        public static readonly MMDeviceEnumerator DeviceEnumerator = new MMDeviceEnumerator();

        public class DeviceUpdatedEventArgs : EventArgs
        {
            public enum DeviceUpdateKind
            {
                Added,
                Removed,
                Changed
            }

            public readonly DeviceUpdateKind Kind;
            public readonly string DeviceId;

            public DeviceUpdatedEventArgs(DeviceUpdateKind kind, string deviceId)
            {
                Kind = kind;
                DeviceId = deviceId;
            }

            /// <summary>
            /// Tries the get device associated with the <see cref="DeviceId"/>.
            /// </summary>
            /// <returns><c>true</c> if the associated device be successfully retrieved; false otherwise.</returns>
            public MMDevice TryGetDevice()
            {
                try
                {
                    using (var deviceEnumerator = new MMDeviceEnumerator())
                    {
                        return deviceEnumerator.GetDevice(DeviceId);
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public override string ToString()
            {
                return $"{{ Kind={Kind}, DeviceId={DeviceId} }}";
            }

        }

        public event EventHandler<DeviceUpdatedEventArgs> OnAudioDeviceUpdated;

        public AudioDeviceManager()
        {
            // TODO:(pv) Figure out why DeviceAdded/DeviceRemoved ain't firing!!!!!
            // TODO:(pv) Consider rolling my own MMDeviceEnumerator like I did in NightVsKnight.ps1 Audio.cs?
            //  NightVsKnight\desktop\scripts\NightVsKnight.ps1 USES WMI!
            //  NightVsKnight\desktop\scripts\Audio.cs USES IMMDeviceEnumerator (but does not register for event)
            //  https://docs.microsoft.com/en-us/windows/desktop/CoreAudio/device-events
            //  https://www.codeproject.com/Articles/14500/Detecting-Hardware-Insertion-and-or-Removal
            //  https://www.codeguru.com/columns/dotnet/detecting-usb-devices-using-c.html
            DeviceEnumerator.DeviceStateChanged += DeviceEnumerator_DeviceStateChanged;
            DeviceEnumerator.DeviceAdded += DeviceEnumerator_DeviceAdded; // not firing?!?!?!
            DeviceEnumerator.DeviceRemoved += DeviceEnumerator_DeviceRemoved; // not firing?!?!?!
        }

        public MMDevice GetDefaultAudioEndpoint(DataFlow dataFlow, Role role)
        {
            return DeviceEnumerator.GetDefaultAudioEndpoint(dataFlow, role);
        }

        public MMDeviceCollection EnumAudioEndpoints(DataFlow dataFlow, DeviceState stateMask)
        {
            return DeviceEnumerator.EnumAudioEndpoints(dataFlow, stateMask);
        }

        private void DeviceEnumerator_DeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
        {
            // https://docs.microsoft.com/en-us/windows/win32/coreaudio/device-state-xxx-constants
            //var s = $"e={{ DeviceState={e.DeviceState}, DeviceId={e.DeviceId} }}";
            //Console.WriteLine($"{TAG} Info OnDeviceStateChanged({sender}, {s})");
            if ((e.DeviceState & DeviceState.Active) == DeviceState.Active)
            {
                OnAudioDeviceUpdated?.Invoke(this, new DeviceUpdatedEventArgs(DeviceUpdateKind.Added, e.DeviceId));
            }
            if (((e.DeviceState & DeviceState.NotPresent) == DeviceState.NotPresent) || ((e.DeviceState & DeviceState.NotPresent) == DeviceState.Disabled))
            {
                OnAudioDeviceUpdated?.Invoke(this, new DeviceUpdatedEventArgs(DeviceUpdateKind.Removed, e.DeviceId));
            }
        }

        /// <summary>
        /// TODO: Why doesn't this actually ever fire on add?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceEnumerator_DeviceAdded(object sender, DeviceNotificationEventArgs e)
        {
            var s = $"e={{ DeviceId={e.DeviceId} }}";
            Console.WriteLine($"{TAG} Info OnDeviceAdded({sender}, {e})");
            OnAudioDeviceUpdated?.Invoke(this, new DeviceUpdatedEventArgs(DeviceUpdateKind.Added, e.DeviceId));
        }

        /// <summary>
        /// TODO: Why doesn't this actually ever fire on remove?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceEnumerator_DeviceRemoved(object sender, DeviceNotificationEventArgs e)
        {
            var s = $"e={{ DeviceId={e.DeviceId} }}";
            Console.WriteLine($"{TAG} Info OnDeviceRemoved({sender}, {s})");
            OnAudioDeviceUpdated?.Invoke(this, new DeviceUpdatedEventArgs(DeviceUpdateKind.Removed, e.DeviceId));
        }
    }
}
