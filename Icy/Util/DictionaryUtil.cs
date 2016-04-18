using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Icy.Util
{
    public static class DictionaryUtil
    {
        public static V[] values<K, V>(Dictionary<K, V> dict)
        {
            List<V> result = new List<V>();

            foreach(var e in dict){
                result.Add(e.Value);
            }

            return result.ToArray();
        }

        public static Dictionary<K, V> copy<K, V>(Dictionary<K, V> current)
        {
            return new Dictionary<K,V>(current);
        }


        public static K[] keys<K, V>(Dictionary<K, V> dict)
        {
            List<K> result = new List<K>();

            foreach (var e in dict)
            {
                result.Add(e.Key);
            }

            return result.ToArray();
        }

        public static J[] map<K, V, J>(Dictionary<K, V> dict, Func<K, V, J> fn)
        {
            J[] result = new J[dict.Count];

            int i = 0;
            foreach (var e in dict)
            {
                result[i] = fn(e.Key, e.Value);
                i++;
            }

            return result;
        }

        public static Dictionary<K, V> merge<K, V>(Dictionary<K, V> obj, Dictionary<K, V> mer)
        {
            Dictionary<K, V> newConfig = new Dictionary<K, V>(obj);
            foreach (var e in mer)
            {
                newConfig[e.Key] = e.Value;
            }
            return newConfig;
        }

        public static Dictionary<K, V> normalize<K, V>(Dictionary<K, V> obj)
        {
            Dictionary<K, V> normalized = new Dictionary<K, V>();
            K[] ks = ArrayUtil.sort(keys(obj));
            foreach (var k in ks)
            {
                normalized[k] = obj[k];
            }
            return normalized;
        }

        public static void toDataTable<K, V>(Dictionary<K, V>[] data, ref DataTable dt)
        {
            foreach (var row in data)
            {
                DataRow dr = dt.NewRow();
                foreach (var e in row)
                {
                    if (dt.Columns.Contains(e.Key.ToString()))
                    {
                        dr[e.Key.ToString()] = e.Value;
                    }
                }
                dt.Rows.Add(dr);
                dr.AcceptChanges();
            }
        }
    }
}
