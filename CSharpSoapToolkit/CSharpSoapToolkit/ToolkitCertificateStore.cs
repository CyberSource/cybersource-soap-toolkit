using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSoapToolkit
{
    /// <summary>
    /// Makes use of PropertiesUtility, CertificateCacheUtility and the Portable.BouncyCastle dependancy
    /// if you choose not to use ToolkitCertificateStore, then the above are optional
    /// </summary>
    public class ToolkitCertificateStore : ISecureCertificateStore
    {
        private static readonly IDictionary<string, string> _userDefinedProperties;

        static ToolkitCertificateStore()
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

        public  X509Certificate2 MerchantCertificate { 
            get 
            {
                return ExtractMerchantCertificateFromFile();
            } 
        }

        private static X509Certificate2 ExtractMerchantCertificateFromFile()
        {
            // (i) Get certificate
            return CertificateCacheUtility.FetchCachedCertificate(PropertiesUtility.GetKeyFilePath(), _userDefinedProperties["KEY_PASS"]);
        }
    }
}
