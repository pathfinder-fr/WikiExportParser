// -----------------------------------------------------------------------
// <copyright file="DictionaryExtensions.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace WikiExportParser
{
    internal static class DictionaryExtensions
    {
        public static void Increment<TKey>(this IDictionary<TKey, int> @this, TKey key)
        {
            int value;
            if (!@this.TryGetValue(key, out value))
            {
                @this.Add(key, 1);
            }
            else
            {
                @this[key] = value + 1;
            }
        }

        public static TValue ValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                return defaultValue;
            }

            return value;
        }
    }
}