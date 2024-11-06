namespace AutomaticGPTTest1
{
    internal class OneEuroFilter
    {
        public float SmoothingFactor(float tE, float cutoff)
        {
            double r = 2 * Math.PI * cutoff * tE;
            return (float)(r / (r + 1));
        }

        public float ExponentialSmoothing(float a, float x, float xPrev)
        {
            return a * x + (1 - a) * xPrev;
        }

        float minCutoff, beta, dCutoff, xPrev, dxPrev, tPrev;


        public OneEuroFilter(float t0, float x0, float dx0 = 0.0f, float minCutoff = 1.0f, float beta = 0.0f, float dCutoff = 1.0f)
        {
            this.minCutoff = minCutoff;
            this.beta = beta;
            this.dCutoff = dCutoff;

            this.xPrev = x0;
            this.dxPrev = dx0;
            this.tPrev = t0;
        }

        public float Call(float t, float x)
        {
            float tE = t - tPrev;

            float aD = SmoothingFactor(tE, dCutoff);
            float dx = (x - xPrev) / tE;
            float dxHat = ExponentialSmoothing(aD, dx, dxPrev);

            float cutoff = minCutoff + beta * Math.Abs(dxHat);
            float a = SmoothingFactor(tE, cutoff);
            float xHat = ExponentialSmoothing(a, x, xPrev);

            xPrev = xHat;
            dxPrev = dxHat;
            tPrev = t;

            return xHat;
        }
    }
}
