using System;
using System.Data;
using System.IO;
using System.Xml.Serialization;

namespace A_Common_Library.XML
{
    public class XMLUtility : IDisposable
    {
        public void XMLFile_SaveFromObject<T>(T an_object, string xml_filepath)
        {
            XmlSerializer ser = new XmlSerializer(an_object.GetType());

            using (StreamWriter writer = new StreamWriter(xml_filepath))
            {
                ser.Serialize(writer, an_object);
            }
        }

        public T XMLFile_LoadToObject<T>(string xml_filepath) where T : class
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));

            using (StreamReader sr = new StreamReader(xml_filepath))
            {
                return (T)ser.Deserialize(sr);
            }
        }

        public string XML_FromObject(object an_object)
        {
            XmlSerializer ser = new XmlSerializer(an_object.GetType());

            using (StringWriter writer = new StringWriter())
            {
                ser.Serialize(writer, an_object);

                return writer.ToString();
            }
        }

        public T XML_ToObject<T>(string xml_string)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));

            using (StringReader reader = new StringReader(this.CleanXML(xml_string)))
            {
                return (T)ser.Deserialize(reader);
            }
        }

        public DataSet XML_ToDataSet(string xml_string)
        {
            StringReader reader = new StringReader(this.CleanXML(xml_string));

            DataSet ds = new DataSet();
            ds.ReadXml(reader);

            return ds;
        }

        public DataTable XML_ToDataTable(string xml_string)
        {
            DataSet ds = XML_ToDataSet(xml_string);

            return ds.Tables[0];
        }

        private string CleanXML(string xml_string)
        {
            while (xml_string.Contains("<field_values><row><label/><value/><selected/></row></field_values>"))
            {
                xml_string = xml_string.Replace("<field_values><row><label/><value/><selected/></row></field_values>", "<field_values/>");
            }

            while (xml_string.Contains("<field_values><row><label/><value>0.00</value><selected/></row></field_values>"))
            {
                xml_string = xml_string.Replace("<field_values><row><label/><value>0.00</value><selected/></row></field_values>", "<field_values/>");
            }

            return xml_string;
        }

        #region IDisposable Implementation
        private bool disposed = false;

        ~XMLUtility()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                //Console.WriteLine("This is the first call to Dispose. Necessary clean-up will be done!");

                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    //Console.WriteLine("Explicit call: Dispose is called by the user.");
                }
                else
                {
                    //Console.WriteLine("Implicit call: Dispose is called through finalization.");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.

                // TODO: set large fields to null.

                disposed = true;
            }
            else
            {
                //Console.WriteLine("Dispose is called more than one time. No need to clean up!");
            }
        }
        #endregion
    }
}