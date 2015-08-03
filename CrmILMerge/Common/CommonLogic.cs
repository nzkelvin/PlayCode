using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class CommonLogic
    {
        public static string RemoveSpaces(this string originStr){
            return originStr.Replace(" ", "");
        }
    }
}
