using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NvAfxDotNet
{
    /// <summary>
    /// https://github.com/NVIDIA/MAXINE-AFX-SDK/blob/v1.1.0/nvafx/include/nvAudioEffects.h
    ///     As of https://github.com/NVIDIA/MAXINE-AFX-SDK/blob/9085c245031e4971fe7273246807c9ec14530bcd/nvafx/include/nvAudioEffects.h
    ///     
    /// https://docs.nvidia.com/deeplearning/maxine/audio-effects-sdk/index.html#use-sdk-in-apps
    /// https://docs.nvidia.com/deeplearning/maxine/audio-effects-sdk/index.html#api-ref
    /// </summary>
    public class NVAudioEffectsDLL
    {
        #region Supporting Methods/Classes

        public static string NVAFX_SDK_DIR { get { return Environment.GetEnvironmentVariable("NVAFX_SDK_DIR"); } }

        public static string NVAFX_SDK_DIR_models { get { return Path.Combine(NVAFX_SDK_DIR, "models"); } }

        public static string MakeModelPath(string filename)
        {
            return Path.Combine(NVAFX_SDK_DIR_models, filename);
        }

        /// <summary>
        /// Make it easier on the caller of NvAFX_GetEffectList by auto marshaling its [code documentation] "statically allocated" returned string list.
        /// </summary>
        private class MultiStringMarshaler : ICustomMarshaler
        {
            private static MultiStringMarshaler instance = new MultiStringMarshaler();

            static ICustomMarshaler GetInstance(string cookie) { return instance; }

            private MultiStringMarshaler() { }

            public void CleanUpManagedData(object ManagedObj) { } // unused

            public void CleanUpNativeData(IntPtr pNativeData) { } // unused

            public int GetNativeDataSize() { return -1; } // unused

            public IntPtr MarshalManagedToNative(object ManagedObj) { return IntPtr.Zero; } // unused

            public object MarshalNativeToManaged(IntPtr pNativeData)
            {
                var list = new List<string>();
                if (pNativeData != IntPtr.Zero)
                {
                    var i = 0;
                    var pszEffect = Marshal.ReadIntPtr(pNativeData);
                    while (pszEffect != IntPtr.Zero)
                    {
                        //Console.WriteLine($"MultiStringMarshaler: MarshalNativeToManaged: i={i}, pszEffect=0x{pszEffect.ToString("X8")}");
                        var szEffect = Marshal.PtrToStringAnsi(pszEffect);
                        //Console.WriteLine($"MultiStringMarshaler: MarshalNativeToManaged: i={i}, szEffect='{szEffect}'");
                        if (szEffect == String.Empty)
                        {
                            break;
                        }
                        list.Add(szEffect);
                        pszEffect = Marshal.ReadIntPtr(pNativeData, IntPtr.Size * ++i);
                    }
                }
                return list.ToArray();
            }
        }

        public class BufferWrapper : IDisposable
        {
            private static Queue<BufferWrapper> pool = new Queue<BufferWrapper>();

            public static BufferWrapper Get(float[] buffer, int offset, int count)
            {
                BufferWrapper bufferWrapper;
                if (pool.Count == 0)
                {
                    bufferWrapper = new BufferWrapper(buffer, offset, count);
                }
                else
                {
                    bufferWrapper = pool.Dequeue();
                    bufferWrapper.Init(buffer, offset, count);
                }
                return bufferWrapper;
            }

            public static void Return(BufferWrapper bufferWrapper)
            {
                pool.Enqueue(bufferWrapper);
            }

            public static void Clear()
            {
                while (pool.Count > 0)
                {
                    var bufferWrapper = pool.Dequeue();
                    bufferWrapper.Dispose();
                }
            }

            private BufferWrapper(float[] buffer, int offset, int count)
            {
                Init(buffer, offset, count);
            }

            public void Dispose()
            {
                hFinal.Free();
                hBuffers[0]?.Free();
                hBuffers[0] = null;
            }

            private float[] buffer;
            private int offset;
            private int count;

            private float[][] buffers = new float[1][];
            private GCHandle?[] hBuffers = new GCHandle?[1];
            private IntPtr[] pBuffers = new IntPtr[1];
            private GCHandle hFinal;
            private IntPtr pFinal = IntPtr.Zero;

            public void Init(float[] buffer, int offset, int count)
            {
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;

                var buffer0 = buffers[0];
                if (buffer0 == null || buffer0.Length < count)
                {
                    buffers[0] = new float[count];
                    hBuffers[0]?.Free();
                    var hBuffer = GCHandle.Alloc(buffers[0], GCHandleType.Pinned);
                    hBuffers[0] = hBuffer;
                    pBuffers[0] = hBuffer.AddrOfPinnedObject();
                    hFinal = GCHandle.Alloc(pBuffers, GCHandleType.Pinned);
                    pFinal = hFinal.AddrOfPinnedObject();
                }
            }

            internal IntPtr CopyManagedBufferToNativeBufferAndGetPointer()
            {
                Marshal.Copy(buffer, offset, pBuffers[0], count);
                return pFinal;
            }

            public void CopyNativeBufferToManagedBuffer()
            {
                Marshal.Copy(pBuffers[0], buffer, offset, count);
            }
        }

        private class BufferWrapperMarshaler : ICustomMarshaler
        {
            private static BufferWrapperMarshaler instance = new BufferWrapperMarshaler();

            static ICustomMarshaler GetInstance(string cookie) { return instance; }

            public void CleanUpManagedData(object ManagedObj)
            {
            }

            public void CleanUpNativeData(IntPtr pNativeData)
            {
            }

            public int GetNativeDataSize()
            {
                return -1;
            }

            public IntPtr MarshalManagedToNative(object ManagedObj)
            {
                if (ManagedObj is BufferWrapper)
                {
                    return ((BufferWrapper)ManagedObj).CopyManagedBufferToNativeBufferAndGetPointer();
                }
                throw new ArgumentException("ManagedObj must be of type BufferWrapper");
            }

            public object MarshalNativeToManaged(IntPtr pNativeData)
            {
                return null;
            }
        }

        #endregion Supporting Methods/Classes

        /// <summary>
        /// NOTE: NVAudioEffects.dll needs to be in your path.
        /// Installing `NVIDIA Audio Effects SDK` defines %NVAFX_SDK_DIR%.
        /// `set PATH=%PATH%;%NVAFX_SDK_DIR%`
        /// 
        /// TL;DR
        /// I implemented a version of this library that used kernel32::LoadLibrary to dynamically directly load NVAudioEffects.dll.
        /// That worked until I used it in an environment that did not have NVAFX_SDK_DIR added to the PATH.
        /// This leads me to conclude that NVAudioEffects.dll requires itself to be in the PATH.
        /// Also, adding a required delegate for each function made the API ugly. :/
        /// </summary>
        public const string _NVAudioEffectsDLL = "NVAudioEffects.dll";

        //
        // Port of nvAudioEffects.h to C#...
        //

        /*

        #ifndef __NVAUDIOEFFECTS_H__
        #define __NVAUDIOEFFECTS_H__

        #ifdef WIN32
        #if defined NVAFX_API_EXPORT
        #define NVAFX_API __declspec(dllexport)
        #else
        #define NVAFX_API __declspec(dllimport)
        #endif
        #else
                // Exports are controlled by version map for Linux, hence this is not needed
        #define NVAFX_API
        #endif

        #if defined(__cplusplus)
        extern "C" {
        #endif

        # include <stdbool.h>
        # include <stddef.h>
        # include <stdint.h>

        */

        /// <summary>
        /// The above lack of __stdcall tells me the CallingConvention is the default of __cdecl.
        /// Furthermore, apparently 64-bit binaries effectively ignore calling convention entirely,
        /// so I am not really sure if this is important:
        /// https://docs.microsoft.com/en-us/cpp/cpp/cdecl?view=msvc-170
        /// `dumpbin /headers nvaudioeffects.dll` confirms the dll is PE 0x8664 AMD64.
        /// I tried both CallingConvention.Cdecl and CallingConvention.StdCall and both seemed to work fine.
        /// Defining CallingConvention.Cdecl here anyway, just to be safe.
        /// </summary>
        public const CallingConvention NVAFX_CALL = CallingConvention.Cdecl;

        /// <summary>
        /// All DLL method strings are "char *", so I am assuming an Ansi charset for now.
        /// </summary>
        public const CharSet NXAFX_CHARSET = CharSet.Ansi;

        /** API return values */
        public enum NvAFX_Status
        {
            /** Success */
            NVAFX_STATUS_SUCCESS = 0,
            /** Failure */
            NVAFX_STATUS_FAILED = 1,
            /** Handle invalid */
            NVAFX_STATUS_INVALID_HANDLE = 2,
            /** Parameter value invalid */
            NVAFX_STATUS_INVALID_PARAM = 3,
            /** Parameter value immutable */
            NVAFX_STATUS_IMMUTABLE_PARAM = 4,
            /** Insufficient data to process */
            NVAFX_STATUS_INSUFFICIENT_DATA = 5,
            /** Effect not supported */
            NVAFX_STATUS_EFFECT_NOT_AVAILABLE = 6,
            /** Given buffer length too small to hold requested data */
            NVAFX_STATUS_OUTPUT_BUFFER_TOO_SMALL = 7,
            /** Model file could not be loaded */
            NVAFX_STATUS_MODEL_LOAD_FAILED = 8,

            /** (32 bit SDK only) COM server was not registered, please see user manual for details */
            NVAFX_STATUS_32_SERVER_NOT_REGISTERED = 9,
            /** (32 bit SDK only) COM operation failed */
            NVAFX_STATUS_32_COM_ERROR = 10,
            /** The selected GPU is not supported. The SDK requires Turing and above GPU with Tensor cores */
            NVAFX_STATUS_GPU_UNSUPPORTED = 11
        }

        /** Bool type (stdbool is available only with C99) */
        public const int NVAFX_TRUE = 1;
        public const int NVAFX_FALSE = 0;

        //typedef char NvAFX_Bool; // C# char

        /** We use strings as effect selectors */
        //typedef const char* NvAFX_EffectSelector; // C# string

        /** We use strings as parameter selectors. */
        //typedef const char* NvAFX_ParameterSelector; // C# string

        /** Each effect instantiation is associated with an opaque handle. */
        //typedef void* NvAFX_Handle; // C# IntPtr

        /** @brief Get a list of audio effects supported
         *
         * @param[out] num_effects Number of effects returned in effects array
         * @param[out] effects A list of effects returned by the API. This list is
         *                     statically allocated by the API implementation. Caller
         *                     does not need to allocate.
         *
         * @return Status values as enumerated in @ref NvAFX_Status
         */
        //NvAFX_Status NVAFX_API NvAFX_GetEffectList(int* num_effects, NvAFX_EffectSelector* effects[]);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_GetEffectList(out int num_effects, [Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(MultiStringMarshaler))] out string[] effects); // C# Verified

        /** @brief Create a new instance of an audio effect.
         *
         * @param[in] code   The selector code for the desired audio Effect.
         * @param[out] effect   A handle to the Audio Effect instantiation.
         *
         * @return Status values as enumerated in @ref NvAFX_Status
         */
        //NvAFX_Status NVAFX_API NvAFX_CreateEffect(NvAFX_EffectSelector code, NvAFX_Handle* effect);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_CreateEffect(string code, out IntPtr effect); // C# Verified

        /** @brief Create a new instance of an audio effect.
         *
         * @param[in] code   The selector code for the desired chained audio Effect.
         * @param[out] effect   A handle to the Audio Effect instantiation.
         *
         * @return Status values as enumerated in @ref NvAFX_Status
         */
        //NvAFX_Status NVAFX_API NvAFX_CreateChainedEffect(NvAFX_EffectSelector code, NvAFX_Handle* effect);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_CreateChainedEffect(string code, out IntPtr effect); // C# Verified

        /** @brief Delete a previously instantiated audio Effect.
         *
         * @param[in]  effect A handle to the audio Effect to be deleted.
         *
         * @return Status values as enumerated in @ref NvAFX_Status
         */
        //NvAFX_Status NVAFX_API NvAFX_DestroyEffect(NvAFX_Handle effect);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_DestroyEffect(IntPtr effect); // C# Verified

        /** Set the value of the selected parameter (unsigned int, char*)
         *
         * @param[in]  effect      The effect to configure.
         * @param[in]  param_name   The selector of the effect parameter to configure.
         * @param[in]  val         The value to be assigned to the selected effect parameter.
         *
         * @return Status values as enumerated in @ref NvAFX_Status
         */
        //NvAFX_Status NVAFX_API NvAFX_SetU32(NvAFX_Handle effect, NvAFX_ParameterSelector param_name, unsigned int val);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_SetU32(IntPtr effect, string param_name, uint val); // C# Verified

        //NvAFX_Status NVAFX_API NvAFX_SetU32List(NvAFX_Handle effect, NvAFX_ParameterSelector param_name, unsigned int* val, unsigned int size);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
#warning Not yet C# Verified
        public static extern NvAFX_Status NvAFX_SetU32List(IntPtr effect, string param_name, ref uint val, uint size);

        //NvAFX_Status NVAFX_API NvAFX_SetString(NvAFX_Handle effect, NvAFX_ParameterSelector param_name, const char* val);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_SetString(IntPtr effect, string param_name, string val); // C# Verified

        //NvAFX_Status NVAFX_API NvAFX_SetStringList(NvAFX_Handle effect, NvAFX_ParameterSelector param_name, const char** val, unsigned int size);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
#warning Not yet C# Verified
        public static extern NvAFX_Status NvAFX_SetStringList(IntPtr effect, string param_name, ref string[] val, uint size);

        //NvAFX_Status NVAFX_API NvAFX_SetFloat(NvAFX_Handle effect, NvAFX_ParameterSelector param_name, float val);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_SetFloat(IntPtr effect, string param_name, float val); // C# Verified

        //NvAFX_Status NVAFX_API NvAFX_SetFloatList(NvAFX_Handle effect, NvAFX_ParameterSelector param_name, float* val, unsigned int size);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
#warning Not yet C# Verified
        public static extern NvAFX_Status NvAFX_SetFloatList(IntPtr effect, string param_name, ref float[] val, uint size);

        /** Get the value of the selected parameter (unsigned int, char*)
        *
        * @param[in]  effect      The effect handle.
        * @param[in]  param_name   The selector of the effect parameter to read.
        * @param[out]  val         Buffer in which the parameter value will be assigned.
        * @param[in]  max_length  The length in bytes of the buffer provided.
        *
        * @return Status values as enumerated in @ref NvAFX_Status
        */
        //NvAFX_Status NVAFX_API NvAFX_GetU32(NvAFX_Handle effect, NvAFX_ParameterSelector param_name, unsigned int* val);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_GetU32(IntPtr effect, string param_name, out uint val); // C# Verified

        //NvAFX_Status NVAFX_API NvAFX_GetString(NvAFX_Handle effect, NvAFX_ParameterSelector param_name,
        //                                       char* val, int max_length);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_GetString(IntPtr effect, string param_name,
                                                      StringBuilder val, int max_length); // C# Verified

        //NvAFX_Status NVAFX_API NvAFX_GetStringList(NvAFX_Handle effect, NvAFX_ParameterSelector param_name,
        //                                       char** val, int* max_length, unsigned int size); 
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
#warning Not yet C# Verified
        public static extern NvAFX_Status NvAFX_GetStringList(IntPtr effect, string param_name,
                                                      out string[] val, int[] max_length, uint size);

        //NvAFX_Status NVAFX_API NvAFX_GetFloat(NvAFX_Handle effect, NvAFX_ParameterSelector param_name, float* val);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
#warning Not yet C# Verified
        public static extern NvAFX_Status NvAFX_GetFloat(IntPtr effect, string param_name, out float val);

        //NvAFX_Status NVAFX_API NvAFX_GetFloatList(NvAFX_Handle effect, NvAFX_ParameterSelector param_name, float* val, unsigned int size);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
#warning Not yet C# Verified
        public static extern NvAFX_Status NvAFX_GetFloatList(IntPtr effect, string param_name, out float val, uint size);

        /** Load the Effect based on the set params.
         *
         * @param[in]  effect     The effect object handle.
         *
         * @return Status values as enumerated in @ref NvAFX_Status
         */
        //NvAFX_Status NVAFX_API NvAFX_Load(NvAFX_Handle effect);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_Load(IntPtr effect); // C# Verified

        /** Get the devices supported by the model.
         *
         * @note This method must be called after setting model path.
         *
         * @param[in]      effect     The effect object handle.
         * @param[in,out]  num        The size of the input array. This value will be set by the function if call succeeds.
         * @param[in,out]  devices    Array of size num. The function will fill the array with CUDA device indices of devices
                                      supported by the model, in descending order of preference (first = most preferred device)
         * @return Status values as enumerated in @ref NvAFX_Status
         */
        //NvAFX_Status NVAFX_API NvAFX_GetSupportedDevices(NvAFX_Handle effect, int* num, int* devices);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
#warning Not yet C# Verified
        public static extern NvAFX_Status NvAFX_GetSupportedDevices(IntPtr effect, out int num, out int[] devices);

        /** Process the input buffer as per the effect selected. e.g. denoising
         *
         * @note The input float data is expected to be standard 32-bit float type with values in range [-1.0, +1.0]
         *
         * @param[in]  effect               The effect handle.
         * @param[in]  input                Input float buffer array. It points to an array of buffers where each buffer holds
         *                                   audio data for a single channel. Array size should be same as number of
         *                                   input channels expected by the effect. Also ensure sampling rate is same as
         *                                   expected by the Effect.
         *                                   For e.g. for denoiser it should be equal to the value returned by NvAFX_GetU32()
         *                                   returned value for NVAFX_FIXED_PARAM_DENOISER_SAMPLE_RATE parameter.
         * @param[out]  output               Output float buffer array. The layout is same as input. It points to an an array of
         *                                   buffers where buffer has audio data corresponding to that channel. The buffers have
         *                                   to be preallocated by caller. Size of each buffer (i.e. channel) is same as that of
         *                                   input. However, number of channels may differ (can be queried by calling
         *                                   NvAFX_GetU32() with NVAFX_PARAM_NUM_OUTPUT_CHANNELS as parameter).
         * @param[in]  num_input_samples     The number of samples in the input buffer. After this call returns output
         *                                   can be determined by calling NvAFX_GetU32() with 
         *                                   NVAFX_PARAM_NUM_OUTPUT_SAMPLES_PER_FRAME as parameter
         * @param[in]  num_input_channels    The number of channels in the input buffer. The @a input should point
         *                                   to @ num_input_channels number of buffers for input, which can be determined by
         *                                   calling NvAFX_GetU32() with NVAFX_PARAM_NUM_INPUT_CHANNELS as parameter.
         *
         * @return Status values as enumerated in @ref NvAFX_Status
         */
        //NvAFX_Status NVAFX_API NvAFX_Run(NvAFX_Handle effect, const float** input, float** output, unsigned num_input_samples, unsigned num_input_channels);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_Run(IntPtr effect,
            [In, Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] input,
            [In, Out, MarshalAs(UnmanagedType.LPArray)] IntPtr[] output,
            uint num_input_samples,
            uint num_input_channels); // C# Verified

        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_Run(IntPtr effect,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(BufferWrapperMarshaler))] BufferWrapper input,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(BufferWrapperMarshaler))] BufferWrapper output,
            uint num_input_samples,
            uint num_input_channels); // C# Verified

        /** Reset effect state
         *
         * @note Allows the state of an effect to be reset. This operation will reset the state of selected in the next
         *       NvAFX_Run call
         *
         * @param[in]  effect        The effect handle.
         *
         * @return Status values as enumerated in @ref NvAFX_Status
         */
        //NvAFX_Status NVAFX_API NvAFX_Reset(NvAFX_Handle effect);
        [DllImport(_NVAudioEffectsDLL, CharSet = NXAFX_CHARSET, CallingConvention = NVAFX_CALL)]
        public static extern NvAFX_Status NvAFX_Reset(IntPtr effect); // C# Verified

        /** Effect selectors. @ref NvAFX_EffectSelector */

        /** Denoiser Effect */
        public const string NVAFX_EFFECT_DENOISER = "denoiser";
        /** Dereverb Effect */
        public const string NVAFX_EFFECT_DEREVERB = "dereverb";
        /** Dereverb Denoiser Effect */
        public const string NVAFX_EFFECT_DEREVERB_DENOISER = "dereverb_denoiser";
        /** Acoustic Echo Cancellation Effect */
        public const string NVAFX_EFFECT_AEC = "aec";
        /** Super-resolution Effect */
        public const string NVAFX_EFFECT_SUPERRES = "superres";

        public const string NVAFX_CHAINED_EFFECT_DENOISER_16k_SUPERRES_16k_TO_48k = "denoiser16k_superres16kto48k";
        public const string NVAFX_CHAINED_EFFECT_DEREVERB_16k_SUPERRES_16k_TO_48k = "dereverb16k_superres16kto48k";
        public const string NVAFX_CHAINED_EFFECT_DEREVERB_DENOISER_16k_SUPERRES_16k_TO_48k = "dereverb_denoiser16k_superres16kto48k";
        public const string NVAFX_CHAINED_EFFECT_SUPERRES_8k_TO_16k_DENOISER_16k = "superres8kto16k_denoiser16k";
        public const string NVAFX_CHAINED_EFFECT_SUPERRES_8k_TO_16k_DEREVERB_16k = "superres8kto16k_dereverb16k";
        public const string NVAFX_CHAINED_EFFECT_SUPERRES_8k_TO_16k_DEREVERB_DENOISER_16k = "superres8kto16k_dereverb_denoiser16k";

        /** Parameter selectors */

        /** Common Effect parameters. */
        /** Number of audio streams in I/O (unsigned int). */
        public const string NVAFX_PARAM_NUM_STREAMS = "num_streams";
        /** To set if SDK should select the default GPU to run the effects in a Multi-GPU setup(unsigned int).
            Default value is 0. Please see user manual for details.*/
        public const string NVAFX_PARAM_USE_DEFAULT_GPU = "use_default_gpu";
        /** To be set to '1' if SDK user wants to create and manage own CUDA context. Other users can simply
            ignore this parameter. Once set to '1' this cannot be unset for that session (unsigned int) rw param 
            Note: NVAFX_PARAM_USE_DEFAULT_GPU and NVAFX_PARAM_USER_CUDA_CONTEXT cannot be used at the same time */
        public const string NVAFX_PARAM_USER_CUDA_CONTEXT = "user_cuda_context";
        /** To be set to '1' if SDK user wants to disable cuda graphs. Other users can simply ignore this parameter.
        Using Cuda Graphs helps to reduce the inference between GPU and CPU which makes operations quicker.*/
        public const string NVAFX_PARAM_DISABLE_CUDA_GRAPH = "disable_cuda_graph";
        /** To be set to '1' if SDK user wants to enable VAD */
        public const string NVAFX_PARAM_ENABLE_VAD = "enable_vad";
        /** Effect parameters. @ref NvAFX_ParameterSelector */
        /** Model path (char*) */
        public const string NVAFX_PARAM_MODEL_PATH = "model_path";
        /** Input Sample rate (unsigned int). Currently supported sample rate(s): 48000, 16000, 8000 */
        public const string NVAFX_PARAM_INPUT_SAMPLE_RATE = "input_sample_rate";
        /** Output Sample rate (unsigned int). Currently supported sample rate(s): 48000, 16000 */
        public const string NVAFX_PARAM_OUTPUT_SAMPLE_RATE = "output_sample_rate";
        /** Number of input samples per frame (unsigned int). This is immutable parameter */
        public const string NVAFX_PARAM_NUM_INPUT_SAMPLES_PER_FRAME = "num_input_samples_per_frame";
        /** Number of output samples per frame (unsigned int). This is immutable parameter */
        public const string NVAFX_PARAM_NUM_OUTPUT_SAMPLES_PER_FRAME = "num_output_samples_per_frame";
        /** Number of input audio channels */
        public const string NVAFX_PARAM_NUM_INPUT_CHANNELS = "num_input_channels";
        /** Number of output audio channels */
        public const string NVAFX_PARAM_NUM_OUTPUT_CHANNELS = "num_output_channels";
        /** Effect intensity factor (float) */
        public const string NVAFX_PARAM_INTENSITY_RATIO = "intensity_ratio";

        /** Deprecated parameters */
        [Obsolete("Use NVAFX_PARAM_MODEL_PATH", true)]
        public const string NVAFX_PARAM_DENOISER_MODEL_PATH = NVAFX_PARAM_MODEL_PATH;
        [Obsolete("Use NVAFX_PARAM_SAMPLE_RATE", true)]
        public const string NVAFX_PARAM_DENOISER_SAMPLE_RATE = NVAFX_PARAM_SAMPLE_RATE;
        [Obsolete("Use NVAFX_PARAM_NUM_SAMPLES_PER_FRAME", true)]
        public const string NVAFX_PARAM_DENOISER_NUM_SAMPLES_PER_FRAME = NVAFX_PARAM_NUM_SAMPLES_PER_FRAME;
        [Obsolete("Use NVAFX_PARAM_NUM_CHANNELS", true)]
        public const string NVAFX_PARAM_DENOISER_NUM_CHANNELS = NVAFX_PARAM_NUM_CHANNELS;
        [Obsolete("Use NVAFX_PARAM_INTENSITY_RATIO", true)]
        public const string NVAFX_PARAM_DENOISER_INTENSITY_RATIO = NVAFX_PARAM_INTENSITY_RATIO;

        /** Number of audio channels **/
        [Obsolete("", true)]
        public const string NVAFX_PARAM_NUM_CHANNELS = "num_channels";
        /** Sample rate (unsigned int). Currently supported sample rate(s): 48000, 16000 */
        [Obsolete("", true)]
        public const string NVAFX_PARAM_SAMPLE_RATE = "sample_rate";
        /** Number of samples per frame (unsigned int). This is immutable parameter */
        [Obsolete("", true)]
        public const string NVAFX_PARAM_NUM_SAMPLES_PER_FRAME = "num_samples_per_frame";

        /*

        #if defined(__cplusplus)
        }
        #endif

        #endif  // __NVAUDIOEFFECTS_H__

        */
    }
}
