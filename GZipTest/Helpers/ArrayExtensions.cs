using System;

namespace GZipTest.Helpers
{
    public static class ArrayExtensions
    {
        public static T[] Take<T>(this T[] array, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, index, result, 0, length);
            return result;
        }
        public static bool SequenceEqual(this byte[] array, int index, int length, byte[] other)
        {
            if (length != other.Length) return false;
            for (int i = 0; i < length; i++)
            {
                if (other[i] != array[i + index]) return false;
            }
            return true;
        }
        public static bool SequenceEqual(this byte[] array, byte[] other) => array.SequenceEqual(0, array.Length, other);
        public static int Find(this byte[] array, byte[] pattern, int index, int length)
        {
            for (int i = index; i < length; i++)
            {
                if (array[i] == pattern[0] && array.SequenceEqual(i, pattern.Length, pattern)) return i;
            }
            return -1;
        }
    }
}
