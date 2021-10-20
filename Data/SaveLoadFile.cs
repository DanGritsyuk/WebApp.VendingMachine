using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace WebApp.VendingMachine
{
    internal static class SaveLoadFile
    {
        internal static void SerializeObject<T>(T serializableObject, string filePath)
        {
            if (serializableObject == null || String.IsNullOrWhiteSpace(filePath)) { throw new ArgumentException(); }

            try
            {
                var ser = new DataContractSerializer(typeof(T));

                using (XmlWriter xw = XmlWriter.Create(filePath))
                {
                    ser.WriteObject(xw, serializableObject);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static T DeSerializeObject<T>(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) { return default(T); }

            T objectOut = default(T);

            try
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(T));
                FileStream fs = new FileStream(filePath, FileMode.Open);
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas());

                objectOut = (T)dcs.ReadObject(reader);
                reader.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return objectOut;
        }

        public static T DeSerializeObjectFromXmlText<T>(string fileText)
        {
            if (string.IsNullOrWhiteSpace(fileText)) { return default(T); }

            T objectOut = default(T);

            using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(fileText)))
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(T));

                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());

                objectOut = (T)dcs.ReadObject(reader);
                reader.Close();
            }

            return objectOut;
        }
    }
}
