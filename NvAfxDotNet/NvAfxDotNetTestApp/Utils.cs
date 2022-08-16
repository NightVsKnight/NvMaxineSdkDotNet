using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NvAfxDotNetTestApp
{
    internal class Utils
    {
        public static string Quote(object value)
        {
            if (value == null) return "null";
            if (value is string) return $"\"{value}\"";
            return value.ToString();
        }
    }
}
