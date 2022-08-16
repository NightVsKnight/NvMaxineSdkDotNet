using CSCore.CoreAudioAPI;
using System;
using System.Windows.Forms;

namespace NvAfxDotNetTestApp
{
    public class DeviceWrapper
    {
        public MMDevice Device { get; private set; }

        public DeviceWrapper(MMDevice device)
        {
            this.Device = device;
        }

        public override string ToString()
        {
            return Device.FriendlyName;
        }

        public static void AudioDevicesUpdate(MMDeviceCollection devices, ComboBox comboBox, MMDevice selectedDevice)
        {
            if (devices == null)
            {
                throw new ArgumentNullException("devices");
            }
            if (comboBox == null)
            {
                throw new ArgumentNullException("comboBox");
            }

            comboBox.BeginUpdate();
            comboBox.Items.Clear();
            foreach (var device in devices)
            {
                var wrapper = new DeviceWrapper(device);
                comboBox.Items.Add(wrapper);
                if (selectedDevice != null && device.DeviceID == selectedDevice.DeviceID)
                {
                    comboBox.SelectedItem = wrapper;
                }
            }
            comboBox.EndUpdate();
        }
    }
}
