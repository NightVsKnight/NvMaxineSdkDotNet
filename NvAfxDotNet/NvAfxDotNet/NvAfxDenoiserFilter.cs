using CSCore;
using System;

namespace NvAfxDotNet
{
    /// <summary>
    /// CSCore effect that wraps <see cref="NvAfxDenoiser"/>.
    /// </summary>
    public class NvAfxDenoiserFilter : SampleAggregatorBase
    {
        private const string TAG = "NvAfxDenoiserFilter";

        private readonly NvAfxDenoiser denoiser = new NvAfxDenoiser();

        public event EventHandler OnEnabledChanged;

        public bool Enable
        {
            set
            {
                if (value != IsEnabled)
                {
                    denoiser.Enable = value;
                    OnEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsEnabled { get { return denoiser.IsEnabled; } }

        public float IntensityRatio { get { return denoiser.IntensityRatio; } }

        public bool IntensityRatioSet(float value)
        {
            return denoiser.IntensityRatioSet(value);
        }

        public NvAfxDenoiserFilter(ISampleSource source) : base(source) { }

        protected override void Dispose(bool disposing)
        {
            Enable = false;
            base.Dispose(disposing);
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            if (read > 0 && denoiser.IsEnabled)
            {
                if (!denoiser.Run(buffer, offset, read))
                {
                    Console.WriteLine($"{TAG} Error NvAfxDenoiserFilter.Read: denoiser.Run failed, error");
                    Enable = false;
                }
            }
            return read;
        }
    }
}
