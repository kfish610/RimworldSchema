using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RimworldSchema
{
    static class StringExtension
    {
        public static string ToCamelCase(this string str)
        {
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
