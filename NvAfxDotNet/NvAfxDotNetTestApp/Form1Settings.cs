using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvAfxDotNetTestApp
{
    internal class Form1Settings : ApplicationSettingsBase
    {
        public Form1Settings(IComponent owner, string settingsKey) : base(owner, settingsKey) { }

        [UserScopedSetting()]
        public string SelectedCaptureDeviceId
        {
            get
            {
                return (string)this["SelectedCaptureDeviceId"];
            }
            set
            {
                this["SelectedCaptureDeviceId"] = value;
            }
        }

        [UserScopedSetting()]
        public string SelectedRenderDeviceId
        {
            get
            {
                return (string)this["SelectedRenderDeviceId"];
            }
            set
            {
                this["SelectedRenderDeviceId"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("true")]
        public bool IsAudioCaptureEnabled
        {
            get
            {
                return (bool)this["IsAudioCaptureEnabled"];
            }
            set
            {
                this["IsAudioCaptureEnabled"] = value;
            }
        }
    }
}
