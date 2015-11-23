// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" organization="Pathfinder-Fr">
// Copyright (c) Pathfinder-fr. Tous droits reserves.
// </copyright>
// -----------------------------------------------------------------------

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
            return @this.Substring(index);
        }
    }
}