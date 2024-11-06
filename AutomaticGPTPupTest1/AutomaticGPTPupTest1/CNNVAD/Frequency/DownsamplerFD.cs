//
//  Copyright © 2023 Ho Minh Quan. All rights reserved.
//
using PetterPet.FFTSSharp;

namespace PetterPet.CNNVAD.Frequency
{
    public class DownsamplerFD : IDisposable
    {
        FFTS ffts1;
        FFTS ffts2;
        public int len, n, truncatedLen;
        float[] a1Transformed;
        float[] a1Truncated;
        float[] result;
        private bool disposedValue;

        public DownsamplerFD(int len, double ratio)
        {
            this.len = len;
            n = (int)(len * (ratio - 1) / 2 / ratio);
            truncatedLen = len - n * 2;
            ffts1 = FFTS.Complex(FFTS.Forward, len);
            //Debug.WriteLine(truncatedLen);
            ffts2 = FFTS.Complex(FFTS.Backward, truncatedLen);

            a1Transformed = new float[len * 2];
            a1Truncated = new float[truncatedLen * 2];
            result = new float[truncatedLen];
        }

        public float[] Downsample(float[] a1)
        {
            for (int i = 0; i < a1.Length; i++)
            {
                a1Transformed[i * 2] = a1[i];
                a1Transformed[i * 2 + 1] = a1[i];
            }
            ffts1.Execute(a1Transformed, a1Transformed);

            a1Truncated = new float[truncatedLen * 2];
            Array.Copy(a1Transformed, 0, a1Truncated, 0, truncatedLen);
            Array.Copy(a1Transformed, a1Transformed.Length - truncatedLen, a1Truncated, truncatedLen, truncatedLen);

            ffts2.Execute(a1Truncated, a1Truncated);
            result = new float[truncatedLen];
            for (int i = 0; i < truncatedLen; i++)
                result[i] = a1Truncated[i * 2] / len;

            for (int i = 0; i < a1Truncated.Length; i++)
                a1Truncated[i] = 0;
            return result;
        }

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

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
