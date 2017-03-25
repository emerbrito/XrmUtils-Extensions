using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XrmUtils.Extensions.Resources;

namespace XrmUtils.Extensions
{
    /// <summary>
    /// Extension methods for objects.
    /// </summary>
    public static class ObjectExtensions
    {

        /// <summary>
        /// Ensures the target objecy is not null. The assertion fails and an exception is thrown if the object is null. The default exception is <see cref="ArgumentNullException"/> which can be overwriten by the exceptionType parameter.
        /// </summary>
        /// <param name="object">The object instance.</param>
        /// <param name="paramName">The name of the parameter which will cause the exception (used by the argument null exception).</param>
        public static void AssertIsNotNull(this Object @object, string paramName)
        {
            AssertIsNotNull(@object, paramName, (string) null);
        }

        /// <summary>
        /// Ensures the target objecy is not null. The assertion fails and an exception is thrown if the object is null. The default exception is <see cref="ArgumentNullException"/> which can be overwriten by the exceptionType parameter.
        /// </summary>
        /// <param name="object">The object instance.</param>
        /// <param name="paramName">The name of the parameter which will cause the exception (used by the argument null exception).</param>
        /// <param name="message">A message that described the error.</param>
        public static void AssertIsNotNull(this Object @object, string paramName, string message)
        {

            if (string.IsNullOrWhiteSpace(paramName))
            {
                throw new ArgumentNullException(nameof(paramName), string.Format(Messages.ArgumentNull, nameof(paramName)));
            }                

            if (@object == null)
            {
                if(string.IsNullOrWhiteSpace(message))
                {
                    throw new ArgumentNullException(paramName, string.Format(Messages.ArgumentNull, paramName));
                }
                else
                {
                    throw new ArgumentNullException(paramName, message);
                }                
            }

        }

        /// <summary>
        /// Ensures the target objecy is not null. The assertion fails and an exception is thrown if the object is null. The default exception is <see cref="ArgumentNullException"/> which can be overwriten by the exceptionType parameter.
        /// </summary>
        /// <param name="object">The object instance.</param>
        /// <param name="exceptionType">When specified, will throw this exception instead of the default <see cref="ArgumentNullException"/>.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of exception's constructor. If args is an empty array or null, the constructor that takes no parameters (the default constructor) is invoked.</param>
        public static void AssertIsNotNull(this Object @object, Type exceptionType, params object[] args)
        {

            if (exceptionType == null)
                throw new ArgumentNullException(nameof(exceptionType));

            if (@object == null)
            {
                var ex = (Exception)Activator.CreateInstance(exceptionType,args);
                throw ex;
            }
        }

        /// <summary>
        /// Determines whether the specified object is numeric.
        /// </summary>
        /// <param name="value">The instance to evaluate.</param>
        /// <returns></returns>
        internal static Boolean IsNumericType(this Object value)
        {
            return value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal;
        }

    }
}
