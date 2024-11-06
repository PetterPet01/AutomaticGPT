//
//  Copyright © 2023 Ho Minh Quan. All rights reserved.
//

using PetterPet.FFTSSharp;
using static PetterPet.CNNVAD.Ultilities.Ultilities;

namespace PetterPet.CNNVAD.Time
{
    public static class Transforms
    {
        static readonly double P_REF = -93.9794;

        public struct Transform
        {
            public FFTS ffts;
            public int points;
            public int windowSize;
            public float[] buffer;
            public float[] window;
            public float[] power;
            public float totalPower;
            //public int framesPerSecond;
        }

        public static Transform newTransform(int window, int maxFFTLen)
        {
            Transform newTransform = new Transform();

            newTransform.windowSize = window;
            //newTransform.framesPerSecond = framesPerSecond;

            int pow2Size;
            int nearestPow2 = Nearest2Pow(window);
            if (nearestPow2 <= maxFFTLen)
                pow2Size = nearestPow2;
            else
            {
                int[] answer = optimalPower2Iteration(window, 7);
                pow2Size = answer[0] * answer[1];
            }
            //Debug.WriteLine("POW2SIZE: " + pow2Size);
            newTransform.points = pow2Size;

            newTransform.ffts = FFTS.Complex(FFTS.Forward, pow2Size);
            newTransform.buffer = new float[pow2Size * 2];
            newTransform.window = new float[pow2Size];
            newTransform.power = new float[pow2Size];
            for (int i = 0; i < window; i++)
            {
                //Hanning
                newTransform.window[i] = (float)((1.0 - Math.Cos(2.0 * Math.PI * (i + 1) / (window + 1))) * 0.5);
            }

            for (int i = window; i < pow2Size; i++)
            {
                newTransform.window[i] = 0;
            }

            return newTransform;
        }

        public static void ForwardFFT(Transform fft, float[] realInput)
        {
            float[] result = new float[fft.points];
            for (int i = 0; i < realInput.Length; i++)
                fft.buffer[i * 2] = realInput[i] * fft.window[i];
            fft.ffts.Execute(fft.buffer, fft.buffer);

            for (int i = 0; i < fft.points; i++)
            {
                fft.power[i] = (float)Math.Sqrt(fft.buffer[i * 2] * fft.buffer[i * 2] + fft.buffer[i * 2 + 1] * fft.buffer[i * 2 + 1]);
                fft.buffer[i * 2] = 0;
                fft.buffer[i * 2 + 1] = 0;
                fft.totalPower += fft.power[i] / fft.points;
            }
        }

        //public static void ForwardFFT(Transform fft, float[] realInput)
        //{
        //	int i, j, k, L, m, n, o, p, q;
        //	float tempReal, tempImaginary, cos, sin, xt, yt, temp;
        //	k = fft.points;
        //	Console.WriteLine(k);
        //	fft.totalPower = 0;

        //	for (i = 0; i < k; i++)
        //	{
        //		fft.real[i] = 0;
        //		fft.imaginary[i] = 0;
        //	}

        //	for (i = 0; i < fft.windowSize; i++)
        //	{
        //		Windowing
        //		fft.real[i] = realInput[i] * fft.window[i];
        //	}

        //	j = 0;
        //	m = k / 2;

        //	bit reversal
        //	for (i = 1; i < (k - 1); i++)
        //	{
        //		L = m;

        //		while (j >= L)
        //		{
        //			j = j - L;
        //			L = L / 2;
        //		}

        //		j = j + L;

        //		if (i < j)
        //		{
        //			tempReal = fft.real[i];
        //			tempImaginary = fft.imaginary[i];
        //			fft.real[i] = fft.real[j];
        //			fft.imaginary[i] = fft.imaginary[j];
        //			fft.real[j] = tempReal;
        //			fft.imaginary[j] = tempImaginary;
        //		}
        //	}

        //	L = 0;
        //	m = 1;
        //	n = k / 2;

        //	computation
        //	for (i = k; i > 1; i = (i >> 1))
        //	{
        //		L = m;
        //		m = 2 * m;
        //		o = 0;

        //		for (j = 0; j < L; j++)
        //		{
        //			cos = fft.cosine[o];
        //			sin = fft.sine[o];
        //			o = o + n;

        //			for (p = j; p < k; p = p + m)
        //			{
        //				q = p + L;

        //				xt = cos * fft.real[q] - sin * fft.imaginary[q];
        //				yt = sin * fft.real[q] + cos * fft.imaginary[q];
        //				fft.real[q] = (fft.real[p] - xt);
        //				fft.real[p] = (fft.real[p] + xt);
        //				fft.imaginary[q] = (fft.imaginary[p] - yt);
        //				fft.imaginary[p] = (fft.imaginary[p] + yt);
        //			}
        //		}
        //		n = n >> 1;
        //	}

        //	for (i = 0; i < k; i++)
        //	{
        //		fft.power[i] = (float)Math.Sqrt(fft.real[i] * fft.real[i] + fft.imaginary[i] * fft.imaginary[i]);
        //		fft.totalPower += fft.power[i] / k;
        //	}
        //}
    }
}
