// -----------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace WikiExportParser.Writers
{
    internal static class EumerableExtensions
    {
        public static K SelectOrDefault<T, K>(this T @this, Func<T, K> selector, K defaultValue = default(K))
        {
            if (@this == null)
            {
                return defaultValue;
            }

            return selector(@this);
        }

        public static K FirstOrDefaultSelect<T, K>(this IEnumerable<T> @this, Func<T, bool> predicate, Func<T, K> selector, K defaultValue = default(K))
        {
            var item = @this.FirstOrDefault(predicate);

            if (item == null)
            {
                return defaultValue;
            }

            return selector(item);
        }
    }
}