namespace PetterPet.CNNVAD.Ultilities
{
    public static class Ultilities
    {
        /// <summary>
        /// returns an integer array with [0] = optimal power two, [1] = optimalIteration
        /// </summary>
        /// <param name="targetNum"></param>
        /// <param name="iteration"></param>
        /// <returns></returns>
        public static int[] optimalPower2Iteration(int targetNum, int iteration)
        {
            double optimalIteration = targetNum;
            int pow2Size = 1;
            while (optimalIteration > iteration)
            {
                optimalIteration *= 0.5;
                pow2Size *= 2;
            }
            if (pow2Size * (int)Math.Round(optimalIteration) < targetNum)
                optimalIteration += 1;
            return new int[] { pow2Size, (int)Math.Round(optimalIteration) };
        }
        public static int Nearest2Pow(int num)
        {
            int pow2Size = 0x01;
            while (pow2Size < num)
                pow2Size = pow2Size << 1;
            return pow2Size;
        }
        public static float[] Concat(this float[] x, float[] y)
        {
            float[] z = new float[x.Length + y.Length];
            var byteIndex = x.Length * sizeof(float);
            Buffer.BlockCopy(x, 0, z, 0, byteIndex);
            Buffer.BlockCopy(y, 0, z, byteIndex, y.Length * sizeof(float));
            return z;
        }
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
