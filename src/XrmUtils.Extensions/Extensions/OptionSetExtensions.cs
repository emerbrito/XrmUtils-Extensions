using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XrmUtils.Extensions
{
    public static class OptionSetExtensions
    {


        /// <summary>
        /// Returns an enum representation of the option set value.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="optionSet">The option set value instance.</param>
        /// <param name="defaultValue">The enum default value to return if <see cref="OptionSetValue"/> is null.</param>
        /// <returns></returns>
        public static TEnum? ToEnum<TEnum>(this OptionSetValue optionSet)
            where TEnum : struct
        {
            return ToEnum<TEnum>(optionSet, default(TEnum));
        }

        /// <summary>
        /// Returns an enum representation of the option set value.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="optionSet">The option set value instance.</param>
        /// <param name="defaultValue">The enum default value to return if <see cref="OptionSetValue"/> is null.</param>
        /// <returns></returns>
        public static TEnum? ToEnum<TEnum>(this OptionSetValue optionSet, TEnum defaultValue)
            where TEnum : struct
        {
            if (optionSet == null)
                return defaultValue;

            return (TEnum) Enum.ToObject(typeof(TEnum), optionSet.Value);
        }

    }
}
