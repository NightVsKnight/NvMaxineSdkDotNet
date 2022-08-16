using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Win32;
using System;
using System.Configuration;
using System.Windows.Forms;
using static NvAfxDotNet.NVAudioEffectsDLL;
using static NvAfxDotNetTestApp.AudioDeviceManager;

namespace NvAfxDotNetTestApp
{
    public partial class Form1 : Form, IDisposable, IPersistComponentSettings
    {
        private const string TAG = "Form1";

        private readonly Form1Settings settings;

        public Form1()
        {
            settings = new Form1Settings(this, SettingsKey);
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (SaveSettings)
                {
                    SaveComponentSettings();
                }
            }
            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode) return;
            NoiseSuppressionInit();
            AudioLoad();
            LoadComponentSettings();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (DesignMode) return;
            AudioUnload();
        }

        #region IPersistComponentSettings

        public bool SaveSettings { get; set; }

        public string SettingsKey
        {
            get
            {
                return this.Name;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void LoadComponentSettings()
        {
            if (DesignMode) return;
            Console.WriteLine($"{TAG} Info LoadComponentSettings()");
            AudioDevicesUpdate("LoadComponentSettings", false);

            // Should be last to only start capturing after all other settings are loaded
            IsAudioCaptureEnabled = settings.IsAudioCaptureEnabled;
        }

        public void SaveComponentSettings()
        {
            if (DesignMode) return;
            Console.WriteLine($"{TAG} Info SaveComponentSettings()");
            settings.IsAudioCaptureEnabled = IsAudioCaptureEnabled;
            settings.Save();
        }

        public void ResetComponentSettings()
        {
            if (DesignMode) return;
            Console.WriteLine($"{TAG} Info ResetComponentSettings()");
            settings.Reset();
            LoadComponentSettings();
        }

        #endregion

        #region Noise Suppression

        private void checkBoxNoiseSuppression_CheckedChanged(object sender, EventArgs e)
        {
            NoiseSuppressionUpdate();
        }

        private void trackBarNoiseSuppressionIntensity_ValueChanged(object sender, EventArgs e)
        {
            NoiseSuppressionUpdateIntensity();
        }

        private void NoiseSuppression_OnEnabledChanged(object sender, EventArgs e)
        {
            checkBoxNoiseSuppression.Checked = mNoiseSuppression != null ? mNoiseSuppression.IsEnabled : false;
        }

        private float NoiseSuppressionIntensityRatio
        {
            get
            {
                return trackBarNoiseSuppressionIntensity.Value / (float)trackBarNoiseSuppressionIntensity.Maximum;
            }
            set
            {
                value = Math.Max(0, Math.Min(value, 1.0f));
                trackBarNoiseSuppressionIntensity.Value = (int)Math.Round(value * trackBarNoiseSuppressionIntensity.Maximum);
            }
        }

        private void NoiseSuppressionInit()
        {
            trackBarNoiseSuppressionIntensity.Maximum = 20;
            trackBarNoiseSuppressionIntensity.TickFrequency = 2;

            NoiseSuppressionIntensityRatio = 1.0f;
        }

        private void NoiseSuppressionUpdate()
        {
            var isEnabled = checkBoxNoiseSuppression.Checked;
            Console.WriteLine($"{TAG} Info NoiseSuppressionUpdate: isEnabled={isEnabled}");
            if (mNoiseSuppression != null)
            {
                mNoiseSuppression.Enable = isEnabled;
                checkBoxNoiseSuppression.Checked = mNoiseSuppression.IsEnabled;
            }

            NoiseSuppressionUpdateIntensity();
        }

        private void NoiseSuppressionUpdateIntensity()
        {
            float intensity_ratio = NoiseSuppressionIntensityRatio;
            Console.WriteLine($"{TAG} Info NoiseSuppressionUpdateIntensity: intensity_ratio={intensity_ratio:0.00}");
            textBoxNoiseSuppressionIntensity.Text = String.Format("{0:0.00}", intensity_ratio);
            if (mNoiseSuppression != null)
            {
                if (mNoiseSuppression.IntensityRatioSet(intensity_ratio))
                {
                    Console.WriteLine($"{TAG} Info NoiseSuppressionUpdateIntensity: IntensityRatioSet({intensity_ratio:0.00}) success");
                }
                else
                {
                    Console.WriteLine($"{TAG} Info NoiseSuppressionUpdateIntensity: IntensityRatioSet({intensity_ratio:0.00}) failed");
                }
            }
        }

        #endregion

        #region Audio Capture/Playback

        private AudioDeviceManager audioDeviceManager = new AudioDeviceManager();

        private void AudioLoad()
        {
            audioDeviceManager.OnAudioDeviceUpdated += AudioDeviceManager_OnAudioDeviceUpdated;
        }

        private void AudioUnload()
        {
            audioDeviceManager.OnAudioDeviceUpdated -= AudioDeviceManager_OnAudioDeviceUpdated;
            AudioCaptureStop("OnHandleDestroyed");
            BufferWrapper.Clear();
        }

        private MMDevice TryGetAudioDevice(string deviceId, DataFlow dataFlow)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                MMDevice device;
                try
                {
                    device = AudioDeviceManager.DeviceEnumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia);
                }
                catch (CoreAudioAPIException)
                {
                    device = null;
                }
                deviceId = device?.DeviceID;
            }
            //Console.WriteLine($"{TAG} Info TryGetAudioDevice get deviceId={deviceId}");
            try
            {
                return deviceId != null ? AudioDeviceManager.DeviceEnumerator[deviceId] : null;
            }
            catch (CoreAudioAPIException)
            {
                return null;
            }
        }

        private string SelectedCaptureDeviceId
        {
            get
            {
                var deviceId = settings.SelectedCaptureDeviceId;
                Console.WriteLine($"{TAG} Info SelectedCaptureDeviceId get deviceId={deviceId}");
                return deviceId;
            }
            set
            {
                AudioCaptureStop("SelectedCaptureDeviceId set");

                var deviceId = value;
                Console.WriteLine($"{TAG} Info SelectedCaptureDeviceId set deviceId={deviceId}");
                settings.SelectedCaptureDeviceId = deviceId;
                settings.Save();

                AudioDevicesUpdate("SelectedCaptureDeviceId set");
            }
        }

        private MMDevice SelectedCaptureDevice
        {
            get
            {
                var deviceId = SelectedCaptureDeviceId;
                Console.WriteLine($"{TAG} Info SelectedCaptureDevice get deviceId={deviceId}");
                MMDevice device = TryGetAudioDevice(deviceId, DataFlow.Capture);
                return device;
            }
        }

        private string SelectedRenderDeviceId
        {
            get
            {
                var deviceId = settings.SelectedRenderDeviceId;
                Console.WriteLine($"{TAG} Info SelectedRenderDeviceId get deviceId={deviceId}");
                return deviceId;
            }
            set
            {
                AudioCaptureStop("SelectedRenderDeviceId set");

                var deviceId = value;
                Console.WriteLine($"{TAG} Info SelectedRenderDeviceId set deviceId={deviceId}");
                settings.SelectedRenderDeviceId = deviceId;
                settings.Save();

                AudioDevicesUpdate("SelectedRenderDeviceId set");
            }
        }

        private MMDevice SelectedRenderDevice
        {
            get
            {
                var deviceId = SelectedRenderDeviceId;
                Console.WriteLine($"{TAG} Info SelectedRenderDevice get deviceId={deviceId}");
                MMDevice device = TryGetAudioDevice(deviceId, DataFlow.Render);
                return device;
            }
        }

        public bool IsAudioCaptureEnabled
        {
            get
            {
                return settings.IsAudioCaptureEnabled;
            }
            private set
            {
                checkBoxCaptureRender.Checked = value;

                settings.IsAudioCaptureEnabled = value;
                settings.Save();

                if (IsAudioCaptureEnabled)
                {
                    AudioCaptureStart("IsAudioCaptureEnabled set");
                }
                else
                {
                    AudioCaptureStop("IsAudioCaptureEnabled set");
                }
            }
        }

        public bool IsActivelyCapturing { get { return mAudioCapture != null; } }

        public bool IsSelectedCaptureDeviceShowing { get { return comboBoxCapture.SelectedItem != null; } }

        #region UiEvents

        private void checkBoxCaptureRender_CheckedChanged(object sender, EventArgs e)
        {
            IsAudioCaptureEnabled = checkBoxCaptureRender.Checked;
        }

        private void labelCapture_DoubleClick(object sender, EventArgs e)
        {
            Console.WriteLine($"{TAG} Info labelCapture_DoubleClick");
            SelectedCaptureDeviceId = null;
            AudioDevicesUpdate("labelCapture_DoubleClick");
        }

        private void comboBoxCapture_SelectedValueChanged(object sender, EventArgs e)
        {
            if (audioDevicesUpdating) return;
            //Console.WriteLine($"{TAG} Info ComboBoxCapture_SelectedValueChanged(...)");
            var selectedDeviceWrapper = comboBoxCapture.SelectedItem as DeviceWrapper;
            var deviceId = selectedDeviceWrapper?.Device?.DeviceID;
            Console.WriteLine($"{TAG} Info comboBoxCapture_SelectedValueChanged: SelectedCaptureDeviceId={deviceId}");
            SelectedCaptureDeviceId = deviceId;
        }

        private void labelRender_DoubleClick(object sender, EventArgs e)
        {
            Console.WriteLine($"{TAG} Info labelRender_DoubleClick");
            SelectedRenderDeviceId = null;
            AudioDevicesUpdate("labelRender_DoubleClick");
        }

        private void comboBoxRender_SelectedValueChanged(object sender, EventArgs e)
        {
            if (audioDevicesUpdating) return;
            //Console.WriteLine($"{TAG} Info ComboBoxRender_SelectedValueChanged(...)");
            var selectedDeviceWrapper = comboBoxRender.SelectedItem as DeviceWrapper;
            var deviceId = selectedDeviceWrapper?.Device?.DeviceID;
            Console.WriteLine($"{TAG} Info comboBoxRender_SelectedValueChanged: SelectedCaptureDeviceId={deviceId}");
            SelectedRenderDeviceId = deviceId;
        }

        #endregion

        private WasapiCapture mAudioCapture;
        private IWaveSource mAudioCaptureSource;
        private WasapiOut mAudioRender;
        private NvAfxDotNet.NvAfxDenoiserFilter mNoiseSuppression;

        private void AudioDeviceManager_OnAudioDeviceUpdated(object sender, DeviceUpdatedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { AudioDeviceManager_OnAudioDeviceUpdated(sender, e); });
                return;
            }
            Console.WriteLine($"{TAG} Info AudioDeviceManager_OnAudioDeviceUpdated {e}");
            AudioDevicesUpdate("AudioDeviceManager_OnAudioDeviceUpdated");
        }

        /// <summary>
        /// Gate to prevent comboBoxCapture_SelectedValueChanged and comboBoxRender_SelectedValueChanged
        /// </summary>
        private bool audioDevicesUpdating;

        private void AudioDevicesUpdate(string caller, bool autoStart = true)
        {
            audioDevicesUpdating = true;

            Console.WriteLine($"{TAG} Info AudioDevicesUpdate({Utils.Quote(caller)})");

            DeviceWrapper.AudioDevicesUpdate(
                AudioDeviceManager.DeviceEnumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active),
                comboBoxCapture,
                SelectedCaptureDevice);

            DeviceWrapper.AudioDevicesUpdate(
                AudioDeviceManager.DeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active),
                comboBoxRender,
                SelectedRenderDevice);

            audioDevicesUpdating = false;

            if (autoStart && IsAudioCaptureEnabled && IsSelectedCaptureDeviceShowing)
            {
                if (!IsActivelyCapturing)
                {
                    Console.WriteLine($"{TAG} Info AudioDevicesUpdate: IsSelectedCaptureDeviceShowing && !IsActivelyCapturing; AudioCaptureStart();");
                    AudioCaptureStart("AudioDevicesUpdate");
                }
            }
            else
            {
                if (IsActivelyCapturing)
                {
                    Console.WriteLine($"{TAG} Info AudioDevicesUpdate: !IsSelectedCaptureDeviceShowing && IsActivelyCapturing; AudioCaptureStop();");
                    AudioCaptureStop("AudioDevicesUpdate");
                }
            }
        }

        public bool AudioCaptureStart(string caller)
        {
            Console.WriteLine($"{TAG} Info AudioCaptureStart({Utils.Quote(caller)})");
            try
            {
                if (!IsAudioCaptureEnabled || IsActivelyCapturing)
                {
                    return false;
                }

                var deviceCapture = SelectedCaptureDevice;
                var deviceRender = SelectedRenderDevice;
                if (deviceCapture == null || deviceRender == null)
                {
                    var msg = @"Voice Morphing 'Capture' *AND* 'Render' devices must *BOTH* not be null";
                    MessageBox.Show(msg);
                    return false;
                }

                var eventSync = false;
                var shareMode = AudioClientShareMode.Shared;
                var latency = 100;

                mAudioCapture = new WasapiCapture(eventSync, shareMode, latency)
                {
                    Device = deviceCapture
                };
                mAudioCapture.Initialize();

                mAudioCaptureSource = new SoundInSource(mAudioCapture) { FillWithZeros = true };

                mNoiseSuppression = new NvAfxDotNet.NvAfxDenoiserFilter(mAudioCaptureSource.ToSampleSource());
                mNoiseSuppression.OnEnabledChanged += NoiseSuppression_OnEnabledChanged;
                NoiseSuppressionUpdate();
                mAudioCaptureSource = mNoiseSuppression.ToWaveSource();

                mAudioRender = new WasapiOut(eventSync, shareMode, latency)
                {
                    Device = deviceRender
                };
                // 0x8889000A "Device In Use"
                mAudioRender.Initialize(mAudioCaptureSource);
                mAudioRender.Stopped += OnAudioRender_Stopped;
                mAudioRender.Play();

                mAudioCapture.Stopped += OnAudioCapture_Stopped;
                mAudioCapture.Start();

                //OnAudioCaptureStarted?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                AudioCaptureStop($"AudioCaptureStart({Utils.Quote(caller)})");
                if (false)
                {
                    var msg = $"Error in AudioCaptureStart: \r\n{ex.Message}";
                    MessageBox.Show(msg);
                    Console.WriteLine($"{TAG} Error {msg}");
                }
                return false;
            }

            return true;
        }

        private bool AudioCaptureStop(string caller)
        {
            Console.WriteLine($"{TAG} Info AudioCaptureStop({Utils.Quote(caller)})");
            if (!IsActivelyCapturing) return false;
            if (mAudioCapture != null)
            {
                mAudioCapture.Stop();
                mAudioCapture.Dispose();
                mAudioCapture = null;
            }
            if (mAudioCaptureSource != null)
            {
                mAudioCaptureSource.Dispose();
                mAudioCaptureSource = null;
            }
            if (mAudioRender != null)
            {
                mAudioRender.Stop();
                mAudioRender.Dispose();
                mAudioRender = null;
            }
            if (mNoiseSuppression != null)
            {
                mNoiseSuppression.OnEnabledChanged -= NoiseSuppression_OnEnabledChanged;
                mNoiseSuppression.Dispose();
                mNoiseSuppression = null;
            }
            //OnAudioCaptureStopped?.Invoke(this, null);
            return true;
        }

        private void OnAudioRender_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { OnAudioRender_Stopped(sender, e); });
                return;
            }
            if (e.HasError)
            {
                var exception = e.Exception;
                if (exception.HResult == (int)HResult.E_POINTER && exception.TargetSite.Name == "get_WaveFormat")
                {
                    // NullReferenceException: Object reference not set to an instance of an object.
                    //  at CSCore.SampleAggregatorBase.get_WaveFormat()
                    //  at CSCore.SampleAggregatorBase.Read(Single[] buffer, Int32 offset, Int32 count)
                    //  ...
                    if (IsAudioCaptureEnabled && IsActivelyCapturing)
                    {
                        Console.WriteLine($"{TAG} Info OnAudioRender_Stopped(...): Restarting capture as workaround for get_WaveFormat() E_POINTER error");
                        AudioCaptureStop("OnAudioRender_Stopped");
                        AudioCaptureStart("OnAudioRender_Stopped");
                    }
                }
                else
                {
                    Console.WriteLine($"{TAG} Error OnAudioRender_Stopped(...): e={e}");
                }
            }
        }

        private void OnAudioCapture_Stopped(object sender, RecordingStoppedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate { OnAudioCapture_Stopped(sender, e); });
                return;
            }

            if (e.HasError)
            {
                var exception = e.Exception;
                if (exception.HResult == (int)HResult.E_POINTER && exception.TargetSite.Name == "get_WaveFormat")
                {
                    // NullReferenceException: Object reference not set to an instance of an object.
                    //  at CSCore.SampleAggregatorBase.get_WaveFormat()
                    //  at CSCore.SampleAggregatorBase.Read(Single[] buffer, Int32 offset, Int32 count)
                    //  ...
                    if (IsAudioCaptureEnabled && IsActivelyCapturing)
                    {
                        Console.WriteLine($"{TAG} Info OnAudioCapture_Stopped(...): Restarting capture as workaround for get_WaveFormat() E_POINTER error");
                        AudioCaptureStop("OnAudioCapture_Stopped");
                        AudioCaptureStart("OnAudioCapture_Stopped");
                    }
                }
                else
                {
                    Console.WriteLine($"{TAG} Error OnAudioCapture_Stopped(...): e={e}");
                }
            }
        }

        #endregion
    }
}
