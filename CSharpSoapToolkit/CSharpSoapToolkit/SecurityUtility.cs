using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Xml;
using System;
using System.Security.Cryptography.Xml;

namespace CSharpSoapToolkit
{
    public class SecurityUtility
    {
        private static readonly IDictionary<string, string> _userDefinedProperties;

        static SecurityUtility()
        {
            // Load Properties
            try
            {
                _userDefinedProperties = PropertiesUtility.LoadProperties();
            }
            catch (IOException e)
            {
                throw new Exception(e.Message);
            }
        }

        public static string GenerateBinarySecurityToken()
        {
            var certificate = ExtractMerchantCertificateFromFile();
            var certificateBytes = certificate.GetRawCertData();
            return Convert.ToBase64String(certificateBytes);
        }

        private static X509Certificate2 ExtractMerchantCertificateFromFile()
        {
            // (i) Get certificate
            return CertificateCacheUtility.FetchCachedCertificate(PropertiesUtility.GetKeyFilePath(), _userDefinedProperties["KEY_PASS"]);
        }

        public static void CreateDetachedSignature(ref XmlDocument xmlDoc, RSA privateKey, XmlElement securityTokenReference)
        {
            // (i) Import XmlDocument into SignedXmlWithId
            var signedXml = new SignedXmlWithId(xmlDoc)
            {
                SigningKey = privateKey
            };

            // (ii) Create Reference
            var reference = new Reference
            {
                Uri = "#Body"
            };

            // (iii) Create Transform
            var envTransform = new XmlDsigExcC14NTransform();
            reference.AddTransform(envTransform);

            signedXml.AddReference(reference);

            // (iv) Create Signed Info
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoNode(securityTokenReference));

            signedXml.KeyInfo = keyInfo;

            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

            // (v) Sign Context to create detached signature
            signedXml.ComputeSignature();

            var xmlDigitalSignature = signedXml.GetXml();

            // (vi) Create Namespace Manager to handle different namespaces
            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("s", "http://schemas.xmlsoap.org/soap/envelope/");
            namespaceManager.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            namespaceManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            namespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");

            // (vii) Add prefix to Signature Element
            XmlNode signatureElement = xmlDoc.SelectSingleNode("//s:Envelope/s:Header/wsse:Security", namespaceManager);
            signatureElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

            xmlDigitalSignature = signedXml.GetXml();
            SetPrefix("ds", xmlDigitalSignature);

            signedXml.LoadXml(xmlDigitalSignature);
            signedXml.SignedInfo.References.Clear();

            // (viii) Recompute detached signature and add to XML
            signedXml.ComputeSignature();
            string recomputedSignature = Convert.ToBase64String(signedXml.SignatureValue);

            ReplaceSignature(xmlDigitalSignature, recomputedSignature);

            signatureElement.RemoveChild(signatureElement.ChildNodes[1]);
            signatureElement.AppendChild(xmlDigitalSignature);
        }

        // Used to add prefix to all elements inside Signature child elements
        private static void ReplaceSignature(XmlElement signature, string newValue)
        {
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            if (signature.OwnerDocument == null) throw new ArgumentException("No owner document", nameof(signature));

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(signature.OwnerDocument.NameTable);
            namespaceManager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);

            XmlNode signatureValue = signature.SelectSingleNode("ds:SignatureValue", namespaceManager);
            if (signatureValue == null)
                throw new Exception("Signature does not contain 'ds:SignatureValue'");

            signatureValue.InnerXml = newValue;
        }

        private static void SetPrefix(string prefix, XmlNode node)
        {
            if (string.IsNullOrEmpty(node.Prefix))
            {
                node.Prefix = prefix;
            }
            foreach (XmlNode n in node.ChildNodes)
            {
                SetPrefix(prefix, n);
            }
        }

        public static RSA GetKeyFromCertificate()
        {
            var certificate = CertificateCacheUtility.FetchCachedCertificate(PropertiesUtility.GetKeyFilePath(), _userDefinedProperties["KEY_PASS"]);

            // Get the private key
            var privateKey = certificate.GetRSAPrivateKey();

            if (privateKey == null)
            {
                throw new InvalidOperationException("No RSA private key found in the certificate.");
            }

            return privateKey;
        }
    }

    // This class is needed because the SignedXml class cannot process elements with Id attribute
    public class SignedXmlWithId : SignedXml
    {
        public SignedXmlWithId(XmlDocument xml) : base(xml)
        {
        }

        public SignedXmlWithId(XmlElement xmlElement)
            : base(xmlElement)
        {
        }

        public override XmlElement GetIdElement(XmlDocument doc, string id)
        {
            XmlElement elementById = doc.GetElementById(id);

            if (elementById != null)
            {
                return elementById;
            }

            // check to see if it's a standard ID reference
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

            XmlElement idElement = doc.SelectSingleNode("//*[@wsu:Id=\"" + id + "\"]", nsManager) as XmlElement;

            return idElement;
        }
    }
}