using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboPiedade
{
    public static class MyExtensions
    {
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static bool isListNullOrEmpty(this List<string> list)
        {
            if (list == null)
                return true;
            return false;
        }
    }
}
