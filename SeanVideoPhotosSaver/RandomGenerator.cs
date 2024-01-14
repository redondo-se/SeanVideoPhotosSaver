using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace SeanVideoPhotosSaver
{
    public class RandomGenerator
    {
        private static RandomGenerator _theInstance = new RandomGenerator();
        //private Random _random = new Random(DateTime.Now.Millisecond);
        RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();
        private byte[] _uint32Buffer = new byte[4];

        private RandomGenerator() { }

        public static RandomGenerator Instance { get { return _theInstance; } }
        //public int Next(int min, int max)
        //{
        //    return _random.Next(min, max);
        //}

        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
                throw new ArgumentOutOfRangeException("minValue");
            if (minValue == maxValue) return minValue;
            Int64 diff = maxValue - minValue;
            while (true)
            {
                _rng.GetBytes(_uint32Buffer);
                UInt32 rand = BitConverter.ToUInt32(_uint32Buffer, 0);

                Int64 max = (1 + (Int64)UInt32.MaxValue);
                Int64 remainder = max % diff;
                if (rand < max - remainder)
                {
                    return (Int32)(minValue + (rand % diff));
                }
            }

        }
    }
}
