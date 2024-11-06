using PetterPet.FFTSSharp;

namespace PetterPet.CNNVAD.Frequency
{
    /// <summary>
    /// A transform specialized for filter convolution and resampling using FFT
    /// </summary>
    public class SpecializedTransform : IDisposable
    {
        //The FFT class for performing transformations
        FFTS ffts1;
        FFTS ffts2;
        /*The placeholder for the fixed filter coefficients, with the values aggregated
         * https://dsp.stackexchange.com/questions/70603/frequency-domain-equivalent-to-zero-phase-filtering-filtfilt
         * The signal's length being less than the filter's will work, though will yield unexpected results.
         */
        float[] filterAgg;
        int filterLength;
        int signalLength;
        int fftLength;
        //Math.Max between filter and fft size
        int maxLength;
        int len, n, truncatedLen;

        #region Dispose Implementation
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (ffts1 != null)
                        ffts1.Dispose();
                    if (ffts2 != null)
                        ffts2.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SpecializedTransform()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>
        /// Returns a new, length specific instance of SpecializedTransform
        /// </summary>
        /// <param name="pow2"></param>
        /// <param name="iteration"></param>
        /// <param name="filter"></param>
        /// <param name="filterLength"></param>
        public SpecializedTransform(int size, float[] filter, int filterLength, double ratio)
        {
            this.signalLength = size;
            this.filterLength = filterLength;
            maxLength = Math.Max(this.signalLength, this.filterLength);
            fftLength = maxLength + 2;
            var filterCopy = new float[fftLength];
            Buffer.BlockCopy(filter, 0, filterCopy, 0, filter.Length * sizeof(float));

            ffts1 = FFTS.Real(FFTS.Forward, maxLength);
            ffts1.Execute(filterCopy, filterCopy);

            len = maxLength;
            n = (int)(len * (ratio - 1) / 2 / ratio);
            truncatedLen = len - n * 2;
            ffts2 = FFTS.Complex(FFTS.Backward, truncatedLen);
            float[] filterCopyAgg = new float[fftLength];
            for (int i = 0; i < fftLength; i += 2)
            {
                filterCopyAgg[i] = filterCopy[i];
                filterCopyAgg[i + 1] = -filterCopy[i + 1];
            }
            filterAgg = new float[fftLength];
            MultiplyRealFFT(filterCopy, filterCopyAgg, ref filterAgg);
        }
        public static void MultiplyRealFFT(float[] x, float[] y, ref float[] result)
        {
            if (x.Length != y.Length) throw new Exception("Must be of equal sizes");
            for (int i = 0; i < x.Length; i += 2)
            {
                result[i] = (x[i] * y[i]) - (x[i + 1] * y[i + 1]);
                result[i + 1] = (x[i + 1] * y[i]) + x[i] * y[i + 1];
            }
        }
        public void FFTConvolution(float[] input, ref float[] result, float[] window = null)
        {
            if ((result.Length != fftLength) || (input.Length != signalLength) ||
                (window == null && input.Length != window.Length))
                throw new Exception("At least one of the arrays has an invalid size");
            float[] inputC = new float[fftLength];

            if (window != null)
            {
                for (int i = 0; i < maxLength; i++)
                    inputC[i] = input[i] * window[i];
                ffts1.Execute(inputC, inputC);
            }
            else
                ffts1.Execute(input, inputC);
            MultiplyRealFFT(inputC, filterAgg, ref result);
        }
        /// <summary>
        /// Modify the sample's frequency to the target frequency
        /// </summary>
        /// <param name="input">Filtered signal data in frequency domain</param>
        /// <param name="sampleFreq">Sample frequency of the ORIGINAL signal</param>
        /// <param name="targetFreq">Target frequency to manipulate to</param>
        /// <returns>Resampled signal in frequency domain</returns>
        public float[] ManipulateSampleFrequency(int targetLen, float[] input,
            bool manipulateLength = false)
        {
            int len = targetLen;
            if (len % 2 != 0) len++;
            float[] result = new float[manipulateLength ? len : input.Length];
            Buffer.BlockCopy(input, 0, result, 0, len * sizeof(float));
            return result;
        }
        /// <summary>
        /// Modify the sample's frequency to the target frequency
        /// </summary>
        /// <param name="input">Orignal signal data in time domain</param>
        /// <param name="sampleFreq">Sample frequency of the ORIGINAL signal</param>
        /// <param name="targetFreq">Target frequency to manipulate to</param>
        /// <returns>Resampled signal in frequency domain</returns>
        public float[] ManipulateSampleFrequency(float[] input,
            float[] window = null)
        {
            float[] result = new float[input.Length + 2];
            FFTConvolution(input, ref result, window);
            return result;
        }
        public float[] DecimateFFTToTimeDomain(float[] fft)
        {
            float[] expanded = new float[len * 2];
            Array.Copy(fft, 0, expanded, 0, fft.Length);
            for (int i = 2; i < len; i += 2)
            {
                expanded[len * 2 - i] = fft[i];
                expanded[len * 2 - i + 1] = fft[i + 1];
            }
            float[] a1Truncated = new float[truncatedLen * 2];
            Array.Copy(expanded, 0, a1Truncated, 0, truncatedLen);
            Array.Copy(expanded, expanded.Length - truncatedLen, a1Truncated, truncatedLen, truncatedLen);

            ffts2.Execute(a1Truncated, a1Truncated);
            float[] result = new float[truncatedLen];
            for (int i = 0; i < truncatedLen; i++)
                result[i] = a1Truncated[i * 2] / len;

            return result;
        }

        public static float[] GetPowerSpectrum(float[] fftValues)
        {
            int length = fftValues.Length;
            float[] power = new float[length / 2];
            //power[0] = fftValues[0] * fftValues[0];
            float lenRatio = 1f / power.Length;
            for (int i = 0; i < length; i += 2)
            {
                power[i / 2] = /*lenRatio **/ (float)/*Math.Sqrt*/(fftValues[i] * fftValues[i] + fftValues[i + 1] * fftValues[i + 1]);
            }
            //power[length / 2 - 1] = fftValues[length] * fftValues[length];
            return power;
        }
    }
}
