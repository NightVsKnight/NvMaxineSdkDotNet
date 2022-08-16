//
// NvAfxTestConsole.cpp
// 
// To run this you need to set Project->Debugging->Environment to "PATH=%NVAFX_SDK_DIR%;%PATH%"
//

#include <iostream>
#include <nvAudioEffects.h>

int main()
{
    std::cout << "Hello!\n";

    NvAFX_Handle handle = 0;
    NvAFX_Status result;

    //
    // Test GetEffectList
    //

    int num_effects;
    NvAFX_EffectSelector* effects;
    result = NvAFX_GetEffectList(&num_effects, &effects);
    if (result != NVAFX_STATUS_SUCCESS) {
        std::cerr << "NvAFX_GetEffectList() failed" << std::endl;
        goto CLEANUP;
    }
    std::cout << "Total Effects supported: " << num_effects << std::endl;
    for (int i = 0; i < num_effects; ++i) {
        std::cout << "(" << i + 1 << ") " << effects[i] << std::endl;
    }

    //
    // Test Create Effect
    //

    result = NvAFX_CreateEffect(NVAFX_EFFECT_DENOISER, &handle);
    if (result != NVAFX_STATUS_SUCCESS)
    {
        std::cerr << "NvAFX_CreateEffect() failed" << std::endl;
        goto CLEANUP;
    }

    //...

    //
    // Test Getters & Setters
    //

    if (false)
    {
        // NvAFX U32
        uint32_t expected = 16000;
        result = NvAFX_SetU32(handle, NVAFX_PARAM_INPUT_SAMPLE_RATE, expected);
        if (result != NVAFX_STATUS_SUCCESS)
        {
            std::cerr << "NvAFX_SetU32() failed" << std::endl;
            goto CLEANUP;
        }

        uint32_t actual;
        result = NvAFX_GetU32(handle, NVAFX_PARAM_INPUT_SAMPLE_RATE, &actual);
        if (result != NVAFX_STATUS_SUCCESS)
        {
            std::cerr << "NvAFX_GetU32() failed" << std::endl;
            goto CLEANUP;
        }
        std::cout << "NvAFX U32: expected=" << expected << std::endl;
        std::cout << "NvAFX U32: actual=" << actual << std::endl;
        _ASSERT(expected == actual);
    }

    if (true)
    {
        // NvAFX String
        std::string expected = "C:\\Program Files\\NVIDIA Corporation\\NVIDIA Audio Effects SDK\\models\\denoiser_48k.trtpkg";
        result = NvAFX_SetString(handle, NVAFX_PARAM_MODEL_PATH, expected.c_str());
        if (result != NVAFX_STATUS_SUCCESS)
        {
            std::cerr << "NvAFX_SetString() failed" << std::endl;
            goto CLEANUP;
        }

        char actual[1024];
        result = NvAFX_GetString(handle, NVAFX_PARAM_MODEL_PATH, actual, sizeof(actual));
        if (result != NVAFX_STATUS_SUCCESS)
        {
            std::cerr << "NvAFX_GetString() failed" << std::endl;
            goto CLEANUP;
        }
        std::cout << "NvAFX NvAFX_GetString: actual=" << actual << std::endl;
    }

    if (false)
    {
        unsigned num_samples_per_frame, num_channels, sample_rate;
        result = NvAFX_GetU32(handle, NVAFX_PARAM_NUM_INPUT_SAMPLES_PER_FRAME, &num_samples_per_frame);
        result = NvAFX_GetU32(handle, NVAFX_PARAM_NUM_INPUT_CHANNELS, &num_channels);
        result = NvAFX_GetU32(handle, NVAFX_PARAM_OUTPUT_SAMPLE_RATE, &sample_rate);
        std::cout << "NvAFX NvAFX_GetU32: num_samples_per_frame=" << num_samples_per_frame << std::endl;
        std::cout << "NvAFX NvAFX_GetU32: num_channels=" << num_channels << std::endl;
        std::cout << "NvAFX NvAFX_GetU32: sample_rate=" << sample_rate << std::endl;
    }

    //...

CLEANUP:
    if (handle != 0)
    {
        result = NvAFX_DestroyEffect(handle);
        if (result != NVAFX_STATUS_SUCCESS)
        {
            std::cerr << "NvAFX_DestroyEffect() failed" << std::endl;
        }
        handle = 0;
    }

    std::cout << "Goodbye!\n";

    return result == NVAFX_STATUS_SUCCESS ? 0 : 1;
}
