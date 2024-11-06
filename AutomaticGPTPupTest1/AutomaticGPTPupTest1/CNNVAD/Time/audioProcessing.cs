//
//  Copyright © 2023 Ho Minh Quan. All rights reserved.
//
using PetterPet.CNNVAD.Frequency;
using PetterPet.CNNVAD.Ultilities;

namespace PetterPet.CNNVAD.Time
{
    using static FIRFilter;
    using static MelSpectr;
    using static Transforms;

    public class Variables : IDisposable
    {

        public FIR downsampleFilter;
        public Transform fft;
        public DownsamplerFD downsampler;
        public MelSpectrogram melSpectrogram;

        public float[] inputBuffer;
        public float[] downsampled;
        public float[] decimated;
        public float[] frame;

        public int samplingFrequency;
        public int stepSize;
        public int decimatedStepSize;
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (downsampler != null)
                        downsampler.Dispose();
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
    public static class audioProcessing
    {
        public static readonly float SHORT2FLOAT = 1 / 32768.0f;
        static readonly float FLOAT2SHORT = 32768.0f;
        static readonly int NFILT = 40;
        static readonly int FREQLOW = 300;
        static readonly int FREQHIGH = 8000;
        static readonly float EPS = 1.0e-7f;
        static readonly float S2F = 3.051757812500000e-05f;
        static readonly float F2S = 32768;
        static readonly int NCOEFFS = 81;

        public static Variables initialize(int frequency, int stepsize, int maxFFTLen)
        {
            Variables inParam = new Variables();

            double decimationFactor = frequency / (double)(FREQHIGH * 2);

            inParam.stepSize = stepsize;
            inParam.decimatedStepSize = (int)(stepsize / decimationFactor);
            inParam.samplingFrequency = FREQHIGH * 2;
            //Debug.WriteLine("Sampling frequency: " + inParam.samplingFrequency);

            //Debug.WriteLine("decimatedStepSize: " + inParam.decimatedStepSize);

            inParam.inputBuffer = new float[stepsize];
            inParam.downsampled = new float[stepsize];
            inParam.decimated = new float[2 * inParam.decimatedStepSize];

            inParam.fft = newTransform(2 * inParam.decimatedStepSize, maxFFTLen);
            inParam.downsampler = new DownsamplerFD(inParam.stepSize, decimationFactor);
            inParam.melSpectrogram = initMelSpectrogram(NFILT, FREQLOW, FREQHIGH, 2 * inParam.decimatedStepSize, inParam.samplingFrequency, inParam.fft.points);

            var normFilCoffs = FIRFilterBuilder.initLowFIRCoeff(NCOEFFS, frequency, inParam.samplingFrequency);
            inParam.downsampleFilter = initFIR(stepsize, normFilCoffs);

            return inParam;
        }

        public static void compute(ref Variables memoryPointer, float[] input)
        {
            Variables inParam = memoryPointer;

            int i, j;

            for (i = 0; i < inParam.stepSize; i++)
            {
                inParam.inputBuffer[i] = input[i];
            }
            // Downsample the audio
            processFIRFilter(inParam.downsampleFilter, inParam.inputBuffer, inParam.downsampled);

            // Decimate the audio
            float[] decimatedBatch = inParam.downsampler.Downsample(inParam.downsampled);
            Array.Copy(inParam.decimated, inParam.decimatedStepSize, inParam.decimated, 0, inParam.decimatedStepSize);
            Array.Copy(decimatedBatch, 0, inParam.decimated, inParam.decimatedStepSize, inParam.decimatedStepSize);
            //for (i = 0; i < inParam.decimatedStepSize; i++)
            //{
            //    inParam.decimated[i] = inParam.decimated[i + inParam.decimatedStepSize];
            //    inParam.decimated[i + inParam.decimatedStepSize] = decimatedBatch[i];
            //}

            ForwardFFT(inParam.fft, inParam.decimated);
            //Console.WriteLine(inParam.fft.power.Length);
            updateImage(inParam.melSpectrogram, inParam.fft.power);
        }

        public static void getMelImage(Variables memoryPointer, float[,] melImage)
        {
            Variables inParam = memoryPointer;
            for (int i = 0; i < NFILT; i++)
            {
                for (int j = 0; j < NFILT; j++)
                {
                    melImage[i, j] = inParam.melSpectrogram.melSpectrogramImage[i, j];
                }
            }
        }
    }
}
