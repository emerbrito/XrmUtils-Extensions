using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace XrmUtils
{

    /// <summary>
    /// Wrappers for Data Contract serializers. Serialized and deserializes an instance of a type into an XML or JSON string using a supplied data contract.
    /// </summary>
    public static class SerializationUtility
    {

        /// <summary>
        /// Serializes object to the JavaScript Object Notation (JSON). Target object must have <see cref="DataContractAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The type of the serializable object.</typeparam>
        /// <param name="serializableObject">The instance to serialize.</param>
        /// <returns></returns>
        public static string ToJson<T>(T serializableObject)
            where T : class, new()
        {
            return ToJson<T>(serializableObject, dateTimeFormat: "s", emitTypeInformation: System.Runtime.Serialization.EmitTypeInformation.Never);
        }

        /// <summary>
        /// Serializes object to the JavaScript Object Notation (JSON). Target object must have <see cref="DataContractAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The type of the serializable object.</typeparam>
        /// <param name="serializableObject">The instance to serialize.</param>
        /// <param name="dateTimeFormat">The DateTimeFormat that defines the culturally appropriate format of displaying dates and times.</param>
        /// <param name="emitTypeInformation">Sets the data contract JSON serializer settings to emit type information.</param>
        /// <returns></returns>
        public static string ToJson<T>(T serializableObject, string dateTimeFormat, System.Runtime.Serialization.EmitTypeInformation emitTypeInformation)
            where T : class, new()
        {

            string jsonString;

            using (MemoryStream stream = new MemoryStream())
            {

                var s = new DataContractJsonSerializerSettings()
                {
                    DateTimeFormat = new System.Runtime.Serialization.DateTimeFormat(dateTimeFormat),
                    EmitTypeInformation = emitTypeInformation
                };
                var ds = new DataContractJsonSerializer(typeof(T), s);

                ds.WriteObject(stream, serializableObject);
                jsonString = Encoding.UTF8.GetString(stream.ToArray());

            }

            return jsonString;

        }

    }
}
