using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMS
{
    static class StringEx
    {
        public static string TakeUntil(this string me, string ex)
        {
            StringBuilder sb = new StringBuilder();
            string token = String.Empty;
            var j = 0;
            var b = false;
            for (var i = 0; i < me.Length; ++i)
            {
                token += me[i];

                if (token.Length != ex.Length)
                    continue;

                if (token == ex)
                {
                    b = true;
                    break;
                }

                sb.Append(token[0]);
                i = j++;
                token = String.Empty;
            }

            if (!b)
            {
                sb.Append(token);
            }

            return sb.ToString();
        }
    }
}
