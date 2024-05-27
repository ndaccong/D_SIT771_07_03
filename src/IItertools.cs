using System;
using System.Collections.Generic;

namespace CSharpItertools.Interfaces
{
    public interface IItertools
    {
        IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> iterable, int r);
    }

    public static class Extensions
    {
        public static T Next<T>(this T src) where T : struct 
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());

            int j = (Array.IndexOf<T>(Arr, src) + 1) % Arr.Length; // <- Modulo % Arr.Length added

            return Arr[j];
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }
    }
}