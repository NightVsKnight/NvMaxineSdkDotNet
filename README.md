# NvMaxineSdkDotNet

**Shameless plug:** Seriously, one way you can really help out this project is to subscribe to NightVsKnight's
[YouTube](https://www.youtube.com/channel/UCn8Ds6jeUzjxCPkMApg_koA) and/or [Twitch](https://www.twitch.tv/nightvsknight) channels.
I will be showing off this project there from time to time, and getting new subscribers gives me a little morale boost to help me continue this project.

# NvAfxDotNet

A C#/.NET wrapper around Nvidia's Audio Effects (NV-AFX, NvAudioEffects) C++ SDK:
* https://github.com/NVIDIA/MAXINE-AFX-SDK
* https://github.com/NVIDIA/MAXINE-AFX-SDK/tree/v1.1.0/nvafx/include
* https://github.com/NVIDIA/MAXINE-AFX-SDK/blob/v1.1.0/nvafx/include/nvAudioEffects.h

[NvAfxDotNet](NvAfxDotNet/NvAfxDotNet): The actual class library project.  
[NvAfxDotNetTestApp](NvAfxDotNet/NvAfxDotNetTestApp): A C# demo WinForms app.  
[NvAfxDotNetUnitTest](NvAfxDotNet/NvAfxDotNetUnitTest): Incomplete unit tests for the class library.  
[NvAfxTestConsole](NvAfxDotNet/NvAfxTestConsole): A C++ Console app that I used as a working baseline.  
[cscore](https://github.com/NightVsKnight/cscore/): Used by the NvAfxDotNetTestApp; I don't use NAudio because I don't know how to write effects/filters for it.  
[scratch](NvAfxDotNet/scratch): Where snippets go to die.  

Currently only tested with NVAFX_EFFECT_DENOISER.

## Requirements
1. Windows >= 10 x64  
   There are "AllCPU" and "x86" versions in the Solution/Projects, but those don't work and should probably be removed.
2. You need to install the Nvidia Broadcast Audio Effects (aka: "Maxine") SDK runtime for the library and demo app to work:  
   https://www.nvidia.com/en-us/geforce/broadcasting/broadcast-sdk/resources/
3. NVAudioEffects.dll needs to be in your path.  
   Installing `NVIDIA Audio Effects SDK` adds `%NVAFX_SDK_DIR%` to your Environment.  
   So, add `%NVAFX_SDK_DIR%` to your `PATH`.  
   Example: `set PATH=%PATH%;%NVAFX_SDK_DIR%`  
   TL;DR: I initially implemented a dynamic LoadLibrary version of this, but adding a delegate for each function made the API ugly. :/

## TODO
1. Verify P/Invoke Marshalling of 7 remaining unverified SDK methods.
2. Support more than just NVAFX_EFFECT_DENOISER.
3. Expand this repo to support [MAXINE-VFX-SDK](https://github.com/NVIDIA/MAXINE-VFX-SDK) and/or [MAXINE-AR-SDK](https://github.com/NVIDIA/MAXINE-AR-SDK)

## Resources Used:
* https://developer.nvidia.com/blog/achieving-noise-free-audio-for-virtual-collaboration-and-content-creation-apps/
* https://www.youtube.com/watch?v=Rfdtx-Oj54g
  * https://youtu.be/Rfdtx-Oj54g?t=160
* https://docs.nvidia.com/deeplearning/maxine/audio-effects-sdk/index.html
  * https://docs.nvidia.com/deeplearning/maxine/audio-effects-sdk/index.html#use-sdk-in-apps
  * https://docs.nvidia.com/deeplearning/maxine/audio-effects-sdk/index.html#api-ref
* https://github.com/NVIDIA/MAXINE-AFX-SDK/blob/v1.1.0/samples/effects_demo/effects_demo.cpp
* OBS Studio:
  * https://github.com/obsproject/obs-studio/commit/4ac96e1352db1272f4cacc2e0eafcd7b24d527b6
  * https://github.com/obsproject/obs-studio/blob/master/plugins/obs-filters/nvafx-load.h
  * https://github.com/obsproject/obs-studio/blob/master/plugins/obs-filters/obs-filters.c
  * https://github.com/obsproject/obs-studio/blob/master/plugins/obs-filters/noise-suppress-filter.c
  * ...

