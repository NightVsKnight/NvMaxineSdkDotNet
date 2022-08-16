using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Text;
using static NvAfxDotNet.NVAudioEffectsDLL;

namespace NvAfxDotNetUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IntPtr handle = IntPtr.Zero;
            NvAFX_Status result;

            //
            // Test GetEffectList
            //

            int num_effects;
            string[] effects;
            result = NvAFX_GetEffectList(out num_effects, out effects);
            if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
            {
                Console.WriteLine("NvAFX_GetEffectList() failed");
                goto CLEANUP;
            }
            Console.WriteLine($"Total Effects supported: {num_effects}");
            for (int i = 0; i < num_effects; ++i)
            {
                Console.WriteLine($"({i + 1}) {effects[i]}");
            }

            //
            // Test Create Effect
            //

            result = NvAFX_CreateEffect(NVAFX_EFFECT_DENOISER, out handle);
            if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
            {
                Console.WriteLine("NvAFX_CreateEffect() failed");
                goto CLEANUP;
            }
            Console.WriteLine($"Main: result={result}");
            Console.WriteLine($"Main: handle=0x{handle.ToString("X8")}");

            //
            // Test Create Chained Effect
            //

            //...

            //
            // Test Getters & Setters
            //

            if (false)
            {
                uint expected = 1;
                result = NvAFX_SetU32(handle, NVAFX_PARAM_NUM_INPUT_CHANNELS, expected);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_SetU32() failed");
                    goto CLEANUP;
                }
                uint actual;
                result = NvAFX_GetU32(handle, NVAFX_PARAM_NUM_INPUT_CHANNELS, out actual);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_GetU32() failed");
                    goto CLEANUP;
                }
                Debug.Assert(expected == actual, "expected != actual");
            }

            if (true)
            {
                // NvAFX_SetString/NvAFX_GetString
                string expected = MakeModelPath("denoiser_48k.trtpkg");
                result = NvAFX_SetString(handle, NVAFX_PARAM_MODEL_PATH, expected);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_SetString() failed");
                    goto CLEANUP;
                }
                StringBuilder actual = new StringBuilder(1024);
                result = NvAFX_GetString(handle, NVAFX_PARAM_MODEL_PATH, actual, actual.Capacity);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_GetString() failed");
                    goto CLEANUP;
                }
                Debug.Assert(expected == actual.ToString(), "expected != actual");
            }

            if (true)
            {
#if false

effect denoiser
model C:\Program Files\NVIDIA Corporation\NVIDIA Audio Effects SDK/models/denoiser_48k.trtpkg
input_wav denoiser_input_48k.wav
output_wav denoiser_output_48k.wav
real_time 0 
intensity_ratio 1.0
enable_vad 0 

#endif

                //uint num_samples_per_frame, num_channels, sample_rate;
                //result = NvAFX_GetU32(handle, NVAFX_PARAM_NUM_INPUT_SAMPLES_PER_FRAME, out num_samples_per_frame);
                //result = NvAFX_GetU32(handle, NVAFX_PARAM_NUM_INPUT_CHANNELS, out num_channels);
                //result = NvAFX_GetU32(handle, NVAFX_PARAM_OUTPUT_SAMPLE_RATE, out sample_rate);

                //result = NvAFX_SetU32(handle, NVAFX_PARAM_USE_DEFAULT_GPU, 1);
                //if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                //{
                //    Console.WriteLine("NvAFX_SetU32() failed");
                //    goto CLEANUP;
                //}

                string modelPath = MakeModelPath("denoiser_48k.trtpkg");
                result = NvAFX_SetString(handle, NVAFX_PARAM_MODEL_PATH, modelPath);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_SetString() failed");
                    goto CLEANUP;
                }

                result = NvAFX_SetU32(handle, NVAFX_PARAM_INPUT_SAMPLE_RATE, 48000);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_SetU32() failed");
                    goto CLEANUP;
                }

                result = NvAFX_SetU32(handle, NVAFX_PARAM_NUM_STREAMS, 1);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_SetU32() failed");
                    goto CLEANUP;
                }

                result = NvAFX_SetFloat(handle, NVAFX_PARAM_INTENSITY_RATIO, 1.0f);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_SetU32() failed");
                    goto CLEANUP;
                }

                result = NvAFX_SetU32(handle, NVAFX_PARAM_ENABLE_VAD, 0);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_SetU32() failed");
                    goto CLEANUP;
                }

                result = NvAFX_Load(handle);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_Load() failed");
                    goto CLEANUP;
                }

                // ...
            }

        CLEANUP:
            if (handle != IntPtr.Zero)
            {
                //
                // Test Destroy Effect
                //
                result = NvAFX_DestroyEffect(handle);
                if (result != NvAFX_Status.NVAFX_STATUS_SUCCESS)
                {
                    Console.WriteLine("NvAFX_DestroyEffect() failed");
                }
                handle = IntPtr.Zero;
            }
        }
    }
}
