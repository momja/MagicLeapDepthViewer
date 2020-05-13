using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities {
    public static class DictionaryExtentions
    {
        public static T2 TryGetValue<T1, T2>(this Dictionary<T1, T2> dict, T1 key)
        {
            T2 value;
            if (dict.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return default;
            }
        }

    }

}
