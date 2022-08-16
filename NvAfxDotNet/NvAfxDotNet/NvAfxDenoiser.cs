using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvAfxDotNet
{
    /// <summary>
    /// "Simple" class that starts and stops NVAFX_EFFECT_DENOISER
    /// </summary>
    public class NvAfxDenoiser : IDisposable
    {
        private const string TAG = "NvAfxDenoiser";

        //
        // nvafx constants from https://github.com/obsproject/obs-studio/blob/master/plugins/obs-filters/noise-suppress-filter.c#L70
        //
        public const uint NVAFX_SAMPLE_RATE = 48000;
        public const uint NVAFX_FRAME_SIZE = 480; // the sdk does not explicitly set this as a constant though it relies on it
        public const uint NVAFX_NUM_CHANNELS = 1;

        public IntPtr Handle { get; private set; }

        public bool Enable
        {
            set
            {
                if (value)
                {
                    CreateEffect();
                }
                else
                {
                    DestroyEffect();
                }
            }
        }

        public bool IsEnabled { get { return Handle != IntPtr.Zero; } }

        public int NumChannels { get { return (int)NVAFX_NUM_CHANNELS; } }
        public int NumSamplesPerFrame { get { return (int)NVAFX_FRAME_SIZE; } }
        public int SampleRate { get { return (int)NVAFX_SAMPLE_RATE; } }

        private float _intensityRatio = 1.0f;
        public float IntensityRatio
        {
            get
            {
                if (IsEnabled)
                {
                    NVAudioEffectsDLL.NvAFX_Status result = NVAudioEffectsDLL.NvAFX_GetFloat(Handle, NVAudioEffectsDLL.NVAFX_PARAM_INTENSITY_RATIO, out _intensityRatio);
                    if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                    {
                        Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_GetFloat(Intensity Ratio) failed, error {result}");
                    }
                }
                return _intensityRatio;
            }
        }

        public bool IntensityRatioSet(float value)
        {
            _intensityRatio = value;
            if (IsEnabled)
            {
                NVAudioEffectsDLL.NvAFX_Status result = NVAudioEffectsDLL.NvAFX_SetFloat(Handle, NVAudioEffectsDLL.NVAFX_PARAM_INTENSITY_RATIO, _intensityRatio);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_SetFloat(Intensity Ratio: {_intensityRatio}) failed, error {result}");
                    return false;
                }
            }
            return true;
        }

        private bool CreateEffect()
        {
            Console.WriteLine($"{TAG} Info CreateEffect()");

            if (IsEnabled) return true;

            NVAudioEffectsDLL.NvAFX_Status result = NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_FAILED;

            try
            {
                //
                // Code inspired from:
                //  https://github.com/obsproject/obs-studio/blob/master/plugins/obs-filters/noise-suppress-filter.c
                //  https://github.com/NVIDIA/MAXINE-AFX-SDK/blob/v1.1.0/samples/effects_demo/effects_demo.cpp
                //

                IntPtr _handle = IntPtr.Zero;

                result = NVAudioEffectsDLL.NvAFX_CreateEffect(NVAudioEffectsDLL.NVAFX_EFFECT_DENOISER, out _handle);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_CreateEffect failed, error {result}");
                    goto FAILURE;
                }

                // Set AI models path
                string modelPath = NVAudioEffectsDLL.MakeModelPath("denoiser_48k.trtpkg");
                result = NVAudioEffectsDLL.NvAFX_SetString(_handle, NVAudioEffectsDLL.NVAFX_PARAM_MODEL_PATH, modelPath);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_SetString(Model Path: {modelPath}) failed, error {result}");
                    goto FAILURE;
                }

                // Set sample rate of FX
                uint sampleRate = (uint)SampleRate;
                result = NVAudioEffectsDLL.NvAFX_SetU32(_handle, NVAudioEffectsDLL.NVAFX_PARAM_INPUT_SAMPLE_RATE, sampleRate);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_SetU32(Sample Rate: {sampleRate}) failed, error {result}");
                    goto FAILURE;
                }

                // Set intensity of FX
                float intensity_ratio = IntensityRatio;
                result = NVAudioEffectsDLL.NvAFX_SetFloat(_handle, NVAudioEffectsDLL.NVAFX_PARAM_INTENSITY_RATIO, intensity_ratio);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_SetFloat(Intensity Ratio: {intensity_ratio}) failed, error {result}");
                    goto FAILURE;
                }

#if false
  //Enabling VAD based on SDK user input!
  if (NvAFX_SetU32(handle, NVAFX_PARAM_ENABLE_VAD, vad_supported_) != NVAFX_STATUS_SUCCESS) {
    std::cerr << "Could not initialize VAD" << std::endl;
  }

    // Another option could be to use cudaGetDeviceCount for num
  int num_supported_devices = 0;
  if (NvAFX_GetSupportedDevices(handle, &num_supported_devices, nullptr) != NVAFX_STATUS_OUTPUT_BUFFER_TOO_SMALL) {
    std::cerr << "Could not get number of supported devices" << std::endl;
    return false;
  }

  std::cout << "Number of supported devices for this model: " << num_supported_devices << std::endl;

  std::vector<int> ret(num_supported_devices);
  if (NvAFX_GetSupportedDevices(handle, &num_supported_devices, ret.data()) != NVAFX_STATUS_SUCCESS) {
    std::cerr << "No supported devices found" << std::endl;
    return false;
  }

  std::cout << "Devices supported (sorted by preference)" << std::endl;
  for (int device : ret) {
    std::cout << "- " << device << std::endl;
  }
#endif

                // Load FX
                result = NVAudioEffectsDLL.NvAFX_Load(_handle);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_Load() failed, error {result}");
                    goto FAILURE;
                }
                Console.WriteLine($"{TAG} Info CreateEffect: NvAFX_Load() success!");

                //
                // Unnecessary but recommended verification of param values
                //

                result = NVAudioEffectsDLL.NvAFX_GetU32(_handle, NVAudioEffectsDLL.NVAFX_PARAM_INPUT_SAMPLE_RATE, out sampleRate);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_GetU32() failed, error {result}");
                    goto FAILURE;
                }
                if (sampleRate != NVAFX_SAMPLE_RATE)
                {
                    result = NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_FAILED;
                    Console.WriteLine($"{TAG} Error CreateEffect: The input sample rate {sampleRate} is not the expected {NVAFX_SAMPLE_RATE} ==> update the code");
                    goto FAILURE;
                }

                result = NVAudioEffectsDLL.NvAFX_GetU32(_handle, NVAudioEffectsDLL.NVAFX_PARAM_OUTPUT_SAMPLE_RATE, out sampleRate);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_GetU32() failed, error {result}");
                    goto FAILURE;
                }
                if (sampleRate != NVAFX_SAMPLE_RATE)
                {
                    result = NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_FAILED;
                    Console.WriteLine($"{TAG} Error CreateEffect: The output sample rate {sampleRate} is not the expected {NVAFX_SAMPLE_RATE} ==> update the code");
                    goto FAILURE;
                }

                uint numChannels;
                result = NVAudioEffectsDLL.NvAFX_GetU32(_handle, NVAudioEffectsDLL.NVAFX_PARAM_NUM_INPUT_CHANNELS, out numChannels);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_GetU32() failed, error {result}");
                    goto FAILURE;
                }
                if (numChannels != NVAFX_NUM_CHANNELS)
                {
                    result = NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_FAILED;
                    Console.WriteLine($"{TAG} Error CreateEffect: The input number of channels {numChannels} is not the expected {NVAFX_NUM_CHANNELS} ==> update the code");
                    goto FAILURE;
                }

                result = NVAudioEffectsDLL.NvAFX_GetU32(_handle, NVAudioEffectsDLL.NVAFX_PARAM_NUM_OUTPUT_CHANNELS, out numChannels);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    result = NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_FAILED;
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_GetU32() failed, error {result}");
                    goto FAILURE;
                }
                if (numChannels != NVAFX_NUM_CHANNELS)
                {
                    result = NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_FAILED;
                    Console.WriteLine($"{TAG} Error CreateEffect: The output number of channels {numChannels} is not the expected {NVAFX_NUM_CHANNELS} ==> update the code");
                    goto FAILURE;
                }

                uint numSamplesPerFrame;
                result = NVAudioEffectsDLL.NvAFX_GetU32(_handle, NVAudioEffectsDLL.NVAFX_PARAM_NUM_INPUT_SAMPLES_PER_FRAME, out numSamplesPerFrame);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_GetU32() failed, error {result}");
                    goto FAILURE;
                }
                if (numSamplesPerFrame != NVAFX_FRAME_SIZE)
                {
                    result = NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_FAILED;
                    Console.WriteLine($"{TAG} Error CreateEffect: The input samples per frame {numSamplesPerFrame} is not the expected {NVAFX_FRAME_SIZE} (= 10 ms) ==> update the code");
                    goto FAILURE;
                }

                result = NVAudioEffectsDLL.NvAFX_GetU32(_handle, NVAudioEffectsDLL.NVAFX_PARAM_NUM_OUTPUT_SAMPLES_PER_FRAME, out numSamplesPerFrame);
                if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine($"{TAG} Error CreateEffect: NvAFX_GetU32() failed, error {result}");
                    goto FAILURE;
                }
                if (numSamplesPerFrame != NVAFX_FRAME_SIZE)
                {
                    result = NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_FAILED;
                    Console.WriteLine($"{TAG} Error CreateEffect: The output samples per frame {numSamplesPerFrame} is not the expected {NVAFX_FRAME_SIZE} (= 10 ms) ==> update the code");
                    goto FAILURE;
                }

                Console.WriteLine($"{TAG} Info CreateEffect: NvAFX Initialized Successfully!");

                this.Handle = _handle;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{TAG} Info CreateEffect: NvAFX Initialization Failed, error {e}!");
                if (!(e is DllNotFoundException || e is BadImageFormatException || e is EntryPointNotFoundException))
                {
                    throw e;
                }
            }

        FAILURE:
            if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
            {
                DestroyEffect();
            }

            return IsEnabled;
        }

        private void DestroyEffect()
        {
            if (Handle != IntPtr.Zero)
            {
                Console.WriteLine($"{TAG} Info DestroyEffect()");
                try
                {
                    var result = NVAudioEffectsDLL.NvAFX_DestroyEffect(Handle);
                    if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                    {
                        Console.WriteLine($"{TAG} Error DestroyEffect: NvAFX_DestroyEffect failed, error {result}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{TAG} Error DestroyEffect: NvAFX_DestroyEffect Failed, error {e}!");
                    if (!(e is DllNotFoundException || e is BadImageFormatException || e is EntryPointNotFoundException))
                    {
                        throw e;
                    }
                }
                Handle = IntPtr.Zero;
                Console.WriteLine($"{TAG} Info DestroyEffect: NvAFX Uninitialized");
            }
        }

        public bool Run(float[] buffer, int offset, int count)
        {
            try
            {
                NVAudioEffectsDLL.BufferWrapper bufferWrapper;
                NVAudioEffectsDLL.NvAFX_Status result;
                int end = offset + count;
                while (offset < end)
                {
                    bufferWrapper = NVAudioEffectsDLL.BufferWrapper.Get(buffer, offset, NumSamplesPerFrame);
                    result = NVAudioEffectsDLL.NvAFX_Run(Handle, bufferWrapper, bufferWrapper, (uint)NumSamplesPerFrame, (uint)NumChannels);
                    if (result != NVAudioEffectsDLL.NvAFX_Status.NVAFX_STATUS_SUCCESS)
                    {
                        Console.WriteLine($"{TAG} Error Run: NvAFX_Run failed, error {result}");
                        return false;
                    }
                    bufferWrapper.CopyNativeBufferToManagedBuffer();
                    NVAudioEffectsDLL.BufferWrapper.Return(bufferWrapper);
                    offset += NumSamplesPerFrame;
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{TAG} Error Run: Exception {e}");
                return false;
            }
        }

        public void Dispose()
        {
            DestroyEffect();
        }
    }
}
