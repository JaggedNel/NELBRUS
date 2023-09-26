using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptBuilderUtil.Helpers {

    /// <summary>
    /// Serializer modul
    /// </summary>
    internal class MySerializer {

        /// <summary>
        /// Object serializer
        /// </summary>
        /// <typeparam name="T">Any type of deserializable object.</typeparam>
        /// <param name="someClass">Serializable object instance.</param>
        /// <param name="filePath">Directory where serialized object will placed.</param>
        /// <param name="error">Null or captured exception.</param>
        public static void SerializeObject<T>(T someClass, string filePath, out Exception error) {
            error = null;
            try {
                System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(someClass.GetType());

                using (FileStream textWriter = new FileStream(filePath, FileMode.Create)) {
                    xmlSerializer.Serialize(textWriter, someClass);
                }
            } catch (Exception e) {
                error = e;
            }

        }

        /// <summary>
        /// Object deserializer.
        /// </summary>
        /// <typeparam name="T">Any type of deserializable object.</typeparam>
        /// <param name="filePath">Path to deserializable file.</param>
        /// <param name="error">Null or captured exception.</param>
        /// <returns>Serialized T object or its default value.</returns>
        public static T DeserializeObject<T>(string filePath, out Exception error) {
            error = null;
            if (File.Exists(filePath))
                try {
                    System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(T));

                    using (FileStream textWriter = new FileStream(filePath, FileMode.Open)) {
                        return (T)xmlSerializer.Deserialize(textWriter);
                    }
                } catch (Exception e) {
                    error = e;
                    return default;
                }
            else {
                error = new DirectoryNotFoundException($"File path \"{filePath}\" not exists.");
                return default;
            }
        }

    }
}
