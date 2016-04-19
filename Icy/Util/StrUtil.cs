using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Icy.Util
{
    public static class StrUtil
    {
        public static string snake(string input){
            return Regex.Replace(input, "([A-Z])", "_$0", RegexOptions.Compiled).Trim('_');
//            return string.Concat(input.Select((x,i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }

        public static string studly(string input)
        {
            var str = Regex.Replace(input, "([A-Z])", "_$0", RegexOptions.Compiled).Trim('_');
            return ucfirst(str);
        }

        public static string ucfirst(string input)
        {
            return input.Substring(0, 1).ToUpper() + input.Substring(1);
        }

        public static bool isNumeric(string input)
        {
            return Regex.IsMatch(input, @"^\d+$");
        }
    }
}
