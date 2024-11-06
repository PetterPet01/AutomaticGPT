//
//  Copyright © 2023 Ho Minh Quan. All rights reserved.
//

namespace PetterPet.CNNVAD.Time
{
    public class MovingAverageBuffer
    {

        private float[] queue;
        private int period;
        public int count;
        public float movingAverage;
        public float cumulativeAverage;

        public MovingAverageBuffer(int givenperiod = 5)
        {

            period = givenperiod;
            count = 0;
            movingAverage = 0;
            cumulativeAverage = 0;
            queue = new float[period];

        }

        public void addDatum(float datum)
        {
            float removed = queue[0];

            for (int i = 1; i < period; i++)
            {
                queue[i - 1] = queue[i];
            }

            queue[period - 1] = datum;

            movingAverage = movingAverage - (removed / period) + (datum / period);
            //count++;
            //System.Console.WriteLine(count);
            cumulativeAverage = cumulativeAverage + (datum - cumulativeAverage) / ++count;
        }
    }
}
