using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSoapToolkit
{
    /// <summary>
    /// Ensure any custom implentations are secure
    /// </summary>
    public interface ISecureCertificateStore
    {
        X509Certificate2 MerchantCertificate { get; }
    }
}
