using System;
using System.Collections.Generic;
using System.Text;

namespace Icy.Util
{
    public static class StrUtil
    {
        public static string snake(string input){
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", "_$0", System.Text.RegularExpressions.RegexOptions.Compiled).Trim('_');
//            return string.Concat(input.Select((x,i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}
