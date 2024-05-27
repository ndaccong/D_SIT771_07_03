using CSharpItertools.Collections;
using CSharpItertools.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpItertools
{
    public sealed class Itertools : IItertools
    {
        public IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> iterable, int r)
        {
            int n = iterable.Count();
            int i, breakWhile;

            if (r > n)
                yield break;

            int[] range = Enumerable.Range(0, r).ToArray();
            int[] reversedRange = range.Reverse().ToArray();

            IEnumerable<T> firstOutput, output;

            firstOutput = range.Select(x => iterable.ElementAt(x)).ToArray();

            yield return firstOutput;

            while (true)
            {
                i = 0;
                breakWhile = -1;

                for (; i < range.Length; i++)
                {
                    if (range.ElementAt(reversedRange.ElementAt(i)) != reversedRange.ElementAt(i) + n - r)
                    {
                        breakWhile++;
                        break;
                    }
                }

                if (breakWhile < 0) break;

                range[reversedRange.ElementAt(i)] += 1;

                foreach (int j in Enumerable.Range(reversedRange.ElementAt(i) + 1, r - (reversedRange.ElementAt(i) + 1)))
                    range[j] = range[j - 1] + 1;

                output = range.Select(x => iterable.ElementAt(x)).ToArray();

                yield return output;
            }
        }
    }
}