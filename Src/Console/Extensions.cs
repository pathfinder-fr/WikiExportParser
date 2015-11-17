using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WikiExportParser
{
    internal static class Extensions
    {
        public static string SafeSubstring(this string @this, int index, int length)
        {
            if (@this.Length > index + length)
            {
                return @this.Substring(index, length);
            }
            else
            {
                return @this.Substring(index);
            }
        }
    }
}
