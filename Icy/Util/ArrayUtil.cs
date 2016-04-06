using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Util
{
    public static class ArrayUtil
    {
        public static T[] sort<T>(T[] first)
        {
            T[] c = copy(first);
            Array.Sort(c);
            return c;
        }

        public static T[] concat<T>(T[] first, T[] second)
        {
            T[] result = new T[first.Length + second.Length];

            Array.Copy(first, result, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length); 

            return result;
        }

        public static T[] push<T>(T[] current, params T[] args)
        {
            return concat<T>(current, args);
        }

        public static T[] filter<T>(T[] current, Func<object, bool> matcher)
        {
            // TODO: is a list really necessary? Should consider Array.Resize?
            List<T> result = new List<T>();
            T[] fresult;

            for (var i = 0; i < current.Length; i++)
            {
                if (matcher(current[i])) result.Add(current[i]);
            }

            fresult = new T[result.Count];
            for (var i = 0; i < result.Count; i++)
            {
                fresult[i] = result[i];
            }

            return fresult;
        }

        public static T[] copy<T>(T[] current)
        {
            T[] result = new T[current.Length];

            Array.Copy(current, result, current.Length);

            return result;
        }

        public static R[] map<T, R>(T[] current, Func<T, R> callback)
        {
            R[] result = new R[current.Length];

            for (var i = 0; i < current.Length; i++)
            {
                result[i] = callback(current[i]);
            }

            return result;
        }

        public static T[] repeat<T>(T value, int times)
        {
            T[] result = new T[times];
            for (var i = 0; i < times; i++)
            {
                result[i] = value;
            }
            return result;
        }

        public static int indexOf<T>(T[] current, T value) where T : IComparable<T>
        {
            for (var i = 0; i < current.Length; i++)
            {
                if (current[i].CompareTo(value) == 0) return i;
            }
            return -1;
        }

    }
}
