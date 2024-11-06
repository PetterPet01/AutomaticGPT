namespace PetterPet.CNNVAD.Ultilities
{
    public enum WindowType
    {
        Rectangular,
        Triangular,
        Welch,
        Sine,
        Hann,
        Hanning,
        Hamming,
        Blackman,
        Nuttall,
        BlackmanNuttall,
        BlackmanHarris,
        FlatTop
    }
    public static class FIRFilterBuilder
    {
        /// <summary>
        /// Computes the coefficients of a low pass FIR filter
        /// </summary>
        /// <param name="num_taps">The number of coefficients desired</param>
        /// <param name="Fs">The sampling rate of the signal. This IS 2 * [the Nyquist frequency]</param>
        /// <param name="Fx">The targeted Nyquist frequency</param>
        /// <returns>The output low pass FIR coefficients</returns>
        public static double[] initLowFIRCoeff(int num_taps, double Fs, double Fx)
        {
            double[] m_taps = new double[num_taps];
            int n;
            double mm;
            double m_lambda = Math.PI * Fx / (Fs * 0.5);
            for (n = 0; n < num_taps; n++)
            {
                mm = n - (num_taps - 1.0) * 0.5;
                if (mm == 0.0) m_taps[n] = m_lambda / Math.PI;
                else m_taps[n] = Math.Sin(mm * m_lambda) / (mm * Math.PI);
            }
            return m_taps;
        }
        /// <summary>
        /// Computes the coefficients of a low pass FIR filter
        /// </summary>
        /// <param name="num_taps">The number of coefficients desired</param>
        /// <param name="Fs">The sampling rate of the signal. This IS 2 * [the Nyquist frequency]</param>
        /// <param name="Fx">The targeted Nyquist frequency</param>
        /// <returns>The output low pass FIR coefficients</returns>
        public static float[] initLowFIRCoeffF(int num_taps, double Fs, double Fx)
        {
            float[] m_taps = new float[num_taps];
            int n;
            double mm;
            double m_lambda = Math.PI * Fx / (Fs * 0.5);
            for (n = 0; n < num_taps; n++)
            {
                mm = n - (num_taps - 1.0) * 0.5;
                if (mm == 0.0) m_taps[n] = (float)(m_lambda / Math.PI);
                else m_taps[n] = (float)(Math.Sin(mm * m_lambda) / (mm * Math.PI));
            }
            return m_taps;
        }
        public static double[] ComputeResponses(int num_taps, int num_shift, double Fs, double Fx)
        {
            double SAMPLE_TIME_S = 1 / Fs;
            var impulseResponse = new double[num_taps];

            for (int n = 0; n < num_taps; n++)
            {
                if (n != num_shift)
                {
                    impulseResponse[n] = Math.Sin(2.0 * Math.PI * Fx * SAMPLE_TIME_S * (n - num_shift)) / (Math.PI * SAMPLE_TIME_S * (n - num_shift));
                }
                else /* Avoid divide-by-zero, limit is 2*fc */
                {
                    impulseResponse[n] = 2.0 * Fx;
                }

            }

            /* Normalise by DC gain to achieve 0dB gain at DC and then compute step response */
            for (int n = 0; n < num_taps; n++)
                impulseResponse[n] *= SAMPLE_TIME_S;
            return impulseResponse;
        }
        /// <summary>
        /// Compute windowing coefficients of various windowing types
        /// </summary>
        /// <param name="length">The windowing length</param>
        /// <param name="winType">The windowing type</param>
        /// <returns>Windowing coefficients in doubles</returns>
        public static double[] ComputeWindowD(int length, WindowType winType)
        {
            var window = new double[length];

            for (int n = 0; n < length; n++)
            {
                switch (winType)
                {
                    case WindowType.Rectangular:
                        window[n] = 1.0;
                        break;

                    case WindowType.Triangular:
                        window[n] = 1.0 - Math.Abs((n - 0.5 * length) / (0.5 * length));
                        break;

                    case WindowType.Welch:
                        window[n] = 1.0 - Math.Pow((n - 0.5 * length) / (0.5 * length), 2.0);
                        break;

                    case WindowType.Sine:
                        window[n] = Math.Sin(Math.PI * n / ((double)length));
                        break;

                    case WindowType.Hann:
                        window[n] = 0.5 * (1 - Math.Cos(2.0 * Math.PI * n / ((double)length)));
                        break;

                    case WindowType.Hanning:
                        window[n] = 0.5 - 0.5 * Math.Cos(2 * Math.PI * n / ((double)length - 1));
                        break;

                    case WindowType.Hamming:
                        window[n] = (25.0 / 46.0) - (21.0 / 46.0) * Math.Cos(2.0 * Math.PI * n / ((double)length));
                        break;

                    case WindowType.Blackman:
                        window[n] = 0.42 - 0.5 * Math.Cos(2.0 * Math.PI * n / ((double)length)) + 0.08 * Math.Cos(4.0 * Math.PI * n / ((double)length));
                        break;

                    case WindowType.Nuttall:
                        window[n] = 0.355768 - 0.487396 * Math.Cos(2.0 * Math.PI * n / ((double)length)) + 0.144232 * Math.Cos(4.0 * Math.PI * n / ((double)length)) - 0.012604 * Math.Cos(6.0 * Math.PI * n / ((double)length));
                        break;

                    case WindowType.BlackmanNuttall:
                        window[n] = 0.3635819 - 0.4891775 * Math.Cos(2.0 * Math.PI * n / ((double)length)) + 0.1365995 * Math.Cos(4.0 * Math.PI * n / ((double)length)) - 0.0106411 * Math.Cos(6.0 * Math.PI * n / ((double)length));
                        break;

                    case WindowType.BlackmanHarris:
                        window[n] = 0.35875 - 0.48829 * Math.Cos(2.0 * Math.PI * n / ((double)length)) + 0.14128 * Math.Cos(4.0 * Math.PI * n / ((double)length)) - 0.01168 * Math.Cos(6.0 * Math.PI * n / ((double)length));
                        break;

                    case WindowType.FlatTop:
                        window[n] = 0.21557895 - 0.41663158 * Math.Cos(2.0 * Math.PI * n / ((double)length)) + 0.277263158 * Math.Cos(4.0 * Math.PI * n / ((double)length)) - 0.083578947 * Math.Cos(6.0 * Math.PI * n / ((double)length)) + 0.006947368 * Math.Cos(8.0 * Math.PI * n / ((double)length));
                        break;

                    default:
                        window[n] = 1.0;
                        break;
                }
            }
            return window;
        }
        /// <summary>
        /// Compute windowing coefficients of various windowing types
        /// </summary>
        /// <param name="length">The windowing length</param>
        /// <param name="winType">The windowing type</param>
        /// <returns>Windowing coefficients in floats</returns>
        public static float[] ComputeWindowF(int length, WindowType winType)
        {
            var window = new float[length];

            for (int n = 0; n < length; n++)
            {
                switch (winType)
                {
                    case WindowType.Rectangular:
                        window[n] = 1.0f;
                        break;

                    case WindowType.Triangular:
                        window[n] = (float)(1.0 - Math.Abs((n - 0.5 * length) / (0.5 * length)));
                        break;

                    case WindowType.Welch:
                        window[n] = (float)(1.0 - Math.Pow((n - 0.5 * length) / (0.5 * length), 2.0));
                        break;

                    case WindowType.Sine:
                        window[n] = (float)(Math.Sin(Math.PI * n / ((double)length)));
                        break;

                    case WindowType.Hann:
                        window[n] = (float)(0.5 * (1 - Math.Cos(2.0 * Math.PI * n / ((double)length))));
                        break;

                    case WindowType.Hanning:
                        window[n] = (float)(0.5 - 0.5 * Math.Cos(2 * Math.PI * n / ((double)length - 1)));
                        break;

                    case WindowType.Hamming:
                        window[n] = (float)((25.0 / 46.0) - (21.0 / 46.0) * Math.Cos(2.0 * Math.PI * n / ((double)length)));
                        break;

                    case WindowType.Blackman:
                        window[n] = (float)(0.42 - 0.5 * Math.Cos(2.0 * Math.PI * n / ((double)length)) + 0.08 * Math.Cos(4.0 * Math.PI * n / ((double)length)));
                        break;

                    case WindowType.Nuttall:
                        window[n] = (float)(0.355768 - 0.487396 * Math.Cos(2.0 * Math.PI * n / ((double)length)) + 0.144232 * Math.Cos(4.0 * Math.PI * n / ((double)length)) - 0.012604 * Math.Cos(6.0 * Math.PI * n / ((double)length)));
                        break;

                    case WindowType.BlackmanNuttall:
                        window[n] = (float)(0.3635819 - 0.4891775 * Math.Cos(2.0 * Math.PI * n / ((double)length)) + 0.1365995 * Math.Cos(4.0 * Math.PI * n / ((double)length)) - 0.0106411 * Math.Cos(6.0 * Math.PI * n / ((double)length)));
                        break;

                    case WindowType.BlackmanHarris:
                        window[n] = (float)(0.35875 - 0.48829 * Math.Cos(2.0 * Math.PI * n / ((double)length)) + 0.14128 * Math.Cos(4.0 * Math.PI * n / ((double)length)) - 0.01168 * Math.Cos(6.0 * Math.PI * n / ((double)length)));
                        break;

                    case WindowType.FlatTop:
                        window[n] = (float)(0.21557895 - 0.41663158 * Math.Cos(2.0 * Math.PI * n / ((double)length)) + 0.277263158 * Math.Cos(4.0 * Math.PI * n / ((double)length)) - 0.083578947 * Math.Cos(6.0 * Math.PI * n / ((double)length)) + 0.006947368 * Math.Cos(8.0 * Math.PI * n / ((double)length)));
                        break;

                    default:
                        window[n] = 1.0f;
                        break;
                }
            }
            return window;
        }
        public static double[] ComputeWindowedResponses(int num_taps, double[] impulseResponse, double[] window)
        {
            var windowedImpulseResponse = new double[num_taps];
            var windowedStepResponse = new double[num_taps];


            for (int n = 0; n < num_taps; n++)
            {
                windowedImpulseResponse[n] = impulseResponse[n] * window[n];


                if (n == 0)
                {
                    windowedStepResponse[n] = 0.5 * windowedStepResponse[n];
                }
                else
                {
                    windowedStepResponse[n] = windowedStepResponse[n - 1] + 0.5 * (windowedImpulseResponse[n] + windowedImpulseResponse[n - 1]);
                }
            }
            return windowedStepResponse;
        }
        public static float[] ComputeWindowedResponses(int num_taps, float[] impulseResponse, float[] window)
        {
            var windowedImpulseResponse = new float[num_taps];
            var windowedStepResponse = new float[num_taps];


            for (int n = 0; n < num_taps; n++)
            {
                windowedImpulseResponse[n] = impulseResponse[n] * window[n];


                if (n == 0)
                {
                    windowedStepResponse[n] = 0.5f * windowedStepResponse[n];
                }
                else
                {
                    windowedStepResponse[n] = windowedStepResponse[n - 1] + 0.5f * (windowedImpulseResponse[n] + windowedImpulseResponse[n - 1]);
                }
            }
            return windowedStepResponse;
        }

    }
}
