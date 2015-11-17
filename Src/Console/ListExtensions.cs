using System;
using System.Collections.Generic;
using System.Linq;

namespace WikiExportParser
{
    public static class ListExtensions
    {
        public static T GetOrAdd<T>(this IList<T> @this, Func<T, bool> predicate, Func<T> factory)
        {
            var item = @this.FirstOrDefault(predicate);
            if (object.Equals(item, default(T)))
            {
                item = factory();
                @this.Add(item);
            }
            return item;
        }
    }
}
