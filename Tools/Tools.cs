﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace 工具合集
{
    internal class Tools
    {
        public static bool IsValidRegex(string pattern)
        {
            try
            {
                Regex.Match("", pattern);
                return true; // 正则表达式有效
            }
            catch (ArgumentException)
            {
                return false; // 正则表达式无效
            }
        }
    }
}
