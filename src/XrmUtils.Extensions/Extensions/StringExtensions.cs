using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmUtils.Extensions
{
    public static class StringExtensions
    {

        /// <summary>
        /// Determines whether the specified object is numeric.
        /// </summary>
        /// <param name="value">The instance to evaluate.</param>
        /// <returns></returns>
        public static Boolean IsNumber(this string value)
        {

            decimal nvalue;
            return decimal.TryParse(value, out nvalue);

        }

    }
}
