//
//  Copyright © 2023 Ho Minh Quan. All rights reserved.
//

namespace PetterPet.CNNVAD.Time
{
    public struct FIR
    {
        public int N;
        public double[] filterCoefficients;
        public float[] inputBuffer;
    }
    public static class FIRFilter
    {
        static readonly int NCOEFFS = 81;
        static readonly float[] filterCoefficients = new float[] { -0.000159f, -0.000250f, -0.000188f, 0.000124f,
            0.000525f, 0.000641f, 0.000187f, -0.000668f, -0.001240f, -0.000812f, 0.000627f, 0.002062f,
            0.002071f, 0.000097f, -0.002663f, -0.003814f, -0.001708f, 0.002704f, 0.005952f, 0.004603f,
            -0.001444f, -0.007877f, -0.008781f, -0.001761f, 0.008842f, 0.014153f, 0.007787f, -0.007572f,
            -0.020182f, -0.017558f, 0.002323f, 0.026206f, 0.033056f, 0.010520f, -0.031329f, -0.060927f,
            -0.043355f, 0.034802f, 0.151141f, 0.255497f, 0.297319f, 0.255497f, 0.151141f, 0.034802f, -0.043355f,
            -0.060927f, -0.031329f, 0.010520f, 0.033056f, 0.026206f, 0.002323f, -0.017558f, -0.020182f,
            -0.007572f, 0.007787f, 0.014153f, 0.008842f, -0.001761f, -0.008781f, -0.007877f, -0.001444f,
            0.004603f, 0.005952f, 0.002704f, -0.001708f, -0.003814f, -0.002663f, 0.000097f, 0.002071f,
            0.002062f, 0.000627f, -0.000812f, -0.001240f, -0.000668f, 0.000187f, 0.000641f, 0.000525f,
            0.000124f, -0.000188f, -0.000250f, -0.000159f };
        static float checkRange(float input)
        {
            float output;
            if (input > 1.0)
            {
                output = 1.0f;
            }
            else if (input < -1.0)
            {
                output = -1.0f;
            }
            else
            {
                output = input;
            }

            return output;
        }

        public static FIR initFIR(int stepSize, double[] coeffs)
        {

            FIR fir = new FIR();

            fir.N = stepSize;

            fir.filterCoefficients = coeffs;

            fir.inputBuffer = new float[2 * stepSize];

            return fir;

        }

        public static void processFIRFilter(FIR fir, float[] input, float[] output)
        {

            int i, j, idx;
            float temp;

            for (i = 0; i < fir.N; i++)
            {
                fir.inputBuffer[i] = fir.inputBuffer[fir.N + i];
                fir.inputBuffer[fir.N + i] = input[i];
            }

            for (i = 0; i < fir.N; i++)
            {
                temp = 0;

                for (j = 0; j < NCOEFFS; j++)
                {
                    idx = fir.N + (i - j);
                    temp += (float)(fir.inputBuffer[idx] * fir.filterCoefficients[j]);
                }
                output[i] = checkRange(temp);
            }
        }
    }
}
