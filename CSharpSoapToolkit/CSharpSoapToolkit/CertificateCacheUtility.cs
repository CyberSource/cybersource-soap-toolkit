using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System;
using System.Runtime.Caching;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;

namespace CSharpSoapToolkit
{
    public static class CertificateCacheUtility
    {
        private static readonly object mutex = new object();

        private static readonly object mutexForPrivateKeyFromPEM = new object();

        private static readonly string regexForFileNameFromDirectory = "(^([a-z]|[A-Z]):(?=\\\\(?![\\0-\\37<>:\"/\\\\|?*])|\\/(?![\\0-\\37<>:\"/\\\\|?*])|$)|^\\\\(?=[\\\\\\/][^\\0-\\37<>:\"/\\\\|?*]+)|^(?=(\\\\|\\/)$)|^\\.(?=(\\\\|\\/)$)|^\\.\\.(?=(\\\\|\\/)$)|^(?=(\\\\|\\/)[^\\0-\\37<>:\"/\\\\|?*]+)|^\\.(?=(\\\\|\\/)[^\\0-\\37<>:\"/\\\\|?*]+)|^\\.\\.(?=(\\\\|\\/)[^\\0-\\37<>:\"/\\\\|?*]+))((\\\\|\\/)([^\\0-\\37<>:\"/\\\\|?*]+|(\\\\|\\/)$))*()$";

        public static X509Certificate2 FetchCachedCertificate(string p12FilePath, string keyPassword)
        {
            try
            {
                lock (mutex)
                {
                    ObjectCache cache = MemoryCache.Default;

                    var matches = Regex.Match(p12FilePath, regexForFileNameFromDirectory);
                    var certFile = matches.Groups[11].ToString();

                    // If certificate is not found in cache, add certificate to cache
                    if (!cache.Contains(certFile))
                    {
                        var policy = new CacheItemPolicy();
                        var filePaths = new List<string>();
                        var cachedFilePath = Path.GetFullPath(p12FilePath);
                        filePaths.Add(cachedFilePath);
                        policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));

                        var certificate = new X509Certificate2(p12FilePath, keyPassword);
                        cache.Set(certFile, certificate, policy);
                        return certificate;
                    }
                    else if (cache[certFile] is X509Certificate2 cachedCertificateFromP12File)
                    {
                        return cachedCertificateFromP12File;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (CryptographicException e)
            {
                if (e.Message.Equals("The specified network password is not correct.\r\n"))
                {
                    throw new Exception($"KeyPassword provided:{keyPassword} is incorrect");
                }

                throw e;
            }
        }

        public static RSAParameters FetchCachedRSAParameters()
        {
            lock (mutexForPrivateKeyFromPEM)
            {
                var pemFilePath = PropertiesUtility.GetKeyFilePath();
                ObjectCache cache = MemoryCache.Default;

                var matches = Regex.Match(PropertiesUtility.GetKeyFilePath(), regexForFileNameFromDirectory);
                var certFile = matches.Groups[11].ToString();

                // If certificate is not found in cache, add certificate to cache
                if (!cache.Contains(certFile))
                {
                    var policy = new CacheItemPolicy();
                    var filePaths = new List<string>();
                    var privateKey = File.ReadAllText(PropertiesUtility.GetKeyFilePath());
                    var cachedFilePath = Path.GetFullPath(pemFilePath);
                    filePaths.Add(cachedFilePath);
                    policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));

                    PemReader pemReader = new PemReader(new StringReader(privateKey));
                    AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                    RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);

                    cache.Set(certFile, rsaParams, policy);
                }
                return (RSAParameters)cache[certFile];
            }
        }
    }
}