using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace CSharpSoapToolkit
{
    public class PropertiesUtility
    {
        private static readonly Dictionary<string, string> properties = new Dictionary<string, string>();

        public static string GetKeyFilePath()
        {
            string keyFile = properties["KEY_FILE"];
            if (keyFile == null)
            {
                throw new ConfigurationErrorsException("Key File is missing in properties file");
            }

            string keyDirectory = properties["KEY_DIRECTORY"];

            string filePath;

            if (!keyFile.EndsWith(".p12", StringComparison.OrdinalIgnoreCase))
            {
                filePath = Path.Combine(keyDirectory, keyFile + ".p12");
            }
            else
            {
                filePath = Path.Combine(keyDirectory, keyFile);
            }

            if (!File.Exists(filePath))
            {
                throw new ConfigurationErrorsException($"The file \"{filePath}\" is missing or is not a file.");
            }

            if ((new FileInfo(filePath)).IsReadOnly)
            {
                throw new ConfigurationErrorsException($"This application does not have permission to read the file \"{filePath}\".");
            }

            return filePath;
        }

        public static IDictionary<string, string> LoadProperties()
        {
            if (ConfigurationManager.GetSection("toolkitProperties") is System.Collections.Specialized.NameValueCollection customProperties)
            {
                foreach (var key in customProperties.AllKeys)
                {
                    properties[key] = customProperties[key];
                }
            }

            return properties;
        }
    }
}