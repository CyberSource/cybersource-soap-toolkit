using System.IO;
using System.ServiceModel.Channels;
using System.Xml;

namespace CSharpSoapToolkit
{
    public class SoapEnvelopeUtility
    {
        public static void AddSecurityElements(ref Message request)
        {
            // (i) Import Request into XmlDocument
            XmlDocument xmlDoc = new XmlDocument { PreserveWhitespace = true };
            using (var tempMemoryStream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(tempMemoryStream))
                {
                    request.WriteMessage(writer);
                    writer.Flush();
                    tempMemoryStream.Position = 0;
                    xmlDoc.Load(tempMemoryStream);
                }
            }

            // (ii) Create Namespace Manager to handle different namespaces
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
            namespaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            namespaceManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

            // (iii) Retrieve SOAP Body and SOAP Header
            XmlNode soapBody = xmlDoc.SelectSingleNode("//s:Envelope/s:Body", namespaceManager);
            XmlNode soapHeader = xmlDoc.SelectSingleNode("//s:Envelope/s:Header", namespaceManager);

            if (soapHeader == null)
            {
                soapHeader = xmlDoc.CreateElement("s", "Header", "http://schemas.xmlsoap.org/soap/envelope/");
                xmlDoc.DocumentElement.InsertBefore(soapHeader, soapBody);
            }

            if (soapBody != null)
            {
                XmlAttribute bodyIdAttr = xmlDoc.CreateAttribute("wsu", "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                bodyIdAttr.Value = "Body";
                soapBody.Attributes.Append(bodyIdAttr);
            }

            // (iv) Add Binary Security Token envelope
            XmlElement tokenElement = xmlDoc.CreateElement("wsse", "BinarySecurityToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            tokenElement.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");
            tokenElement.SetAttribute("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");

            XmlAttribute tokenElementAttribute = xmlDoc.CreateAttribute("wsu", "Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            tokenElementAttribute.Value = "X509Token";
            tokenElement.Attributes.Append(tokenElementAttribute);
            tokenElement.InnerXml = SecurityUtility.GenerateBinarySecurityToken();

            // (v) Add Security envelope
            XmlElement securityElement = xmlDoc.CreateElement("wsse", "Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            securityElement.SetAttribute("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            securityElement.AppendChild(tokenElement);
            soapHeader.AppendChild(securityElement);

            // (vi) Add Security Token Reference element
            XmlElement securityTokenReferenceElement = xmlDoc.CreateElement("wsse", "SecurityTokenReference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            XmlElement referenceElement = xmlDoc.CreateElement("wsse", "Reference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            referenceElement.SetAttribute("URI", "#X509Token");
            securityTokenReferenceElement.AppendChild(referenceElement);

            // (vii) Combine Binary Security Token with Signature
            SecurityUtility.CreateDetachedSignature(ref xmlDoc, SecurityUtility.GetKeyFromCertificate(), securityTokenReferenceElement);

            // (viii) Export back to Request object
            var memoryStream = new MemoryStream(); // This has to remain open. Do NOT use `using` statement.
            xmlDoc.Save(memoryStream);
            memoryStream.Position = 0;

            var readerSettings = new XmlReaderSettings
            {
                CloseInput = true
            };

            var xmlReader = XmlReader.Create(memoryStream, readerSettings);
            request = Message.CreateMessage(xmlReader, int.MaxValue, request.Version);
        }
    }
}