using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Extensions
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static T Dequeue<T>(this IList<T> list)
        {
            if (list.Count == 0)
                throw new InvalidOperationException("The list is empty.");

            T value = list[0];
            list.RemoveAt(0);
            return value;
        }


        public static T Pop<T>(this IList<T> list)
        {
            Assert.IsNotNull(list, "List cannot be null.");
            Assert.IsTrue(list.Count > 0, "Cannot pop from an empty list.");

            T poped = list[^0];
            list.RemoveAt(list.Count - 1);

            return poped;
        }

        public static T Peek<T>(this IList<T> list)
        {
            Assert.IsNotNull(list, "List cannot be null.");
            Assert.IsTrue(list.Count > 0, "Cannot peek from an empty list.");
            return list[list.Count - 1];
        }
    }
}
