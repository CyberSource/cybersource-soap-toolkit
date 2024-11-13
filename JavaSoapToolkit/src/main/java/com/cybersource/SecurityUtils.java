package com.cybersource;

import jakarta.xml.soap.SOAPElement;
import org.bouncycastle.jce.provider.BouncyCastleProvider;

import javax.naming.ConfigurationException;
import javax.xml.crypto.dom.DOMStructure;
import javax.xml.crypto.dsig.*;
import javax.xml.crypto.dsig.dom.DOMSignContext;
import javax.xml.crypto.dsig.keyinfo.KeyInfo;
import javax.xml.crypto.dsig.keyinfo.KeyInfoFactory;
import javax.xml.crypto.dsig.spec.C14NMethodParameterSpec;
import javax.xml.crypto.dsig.spec.TransformParameterSpec;
import java.io.FileInputStream;
import java.io.IOException;
import java.security.*;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.*;

public class SecurityUtils {
    static Properties userDefinedProperties;
    static {
        // Load Properties
        try {
            userDefinedProperties = PropertiesUtil.loadProperties();
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }

    public static String generateBinarySecurityToken()
            throws ConfigurationException, CertificateException, IOException, KeyStoreException, NoSuchAlgorithmException {
        X509Certificate certificate = extractMerchantCertificateFromFile();
        byte[] certificateBytes = certificate.getEncoded();
        return Base64.getEncoder().encodeToString(certificateBytes);
    }

    private static X509Certificate extractMerchantCertificateFromFile()
            throws IOException, ConfigurationException, KeyStoreException, CertificateException, NoSuchAlgorithmException {
        // (i) Load merchant certificate into keystore
        KeyStore merchantKeyStore = loadCertificateIntoKeyStore();

        // (ii) Get corresponding certificate alias
        String merchantKeyAlias = extractMerchantKeyAlias(merchantKeyStore);

        // (iii) Get certificate
        try {
            KeyStore.PrivateKeyEntry e = (KeyStore.PrivateKeyEntry) merchantKeyStore.getEntry(merchantKeyAlias,
                    new KeyStore.PasswordProtection(userDefinedProperties.getProperty("KEY_PASS").toCharArray()));
            return (X509Certificate) e.getCertificate();
        } catch (UnrecoverableEntryException var5) {
            return null;
        }
    }

    public static PrivateKey getKeyFromCertificate()
            throws IOException, NoSuchAlgorithmException, KeyStoreException, CertificateException, ConfigurationException {
        // (i) Load merchant certificate into keystore
        KeyStore merchantKeyStore = loadCertificateIntoKeyStore();

        // (ii) Get corresponding certificate alias
        String merchantKeyAlias = extractMerchantKeyAlias(merchantKeyStore);

        // (iii) Extract Private Key
        try {
            KeyStore.PrivateKeyEntry e = (KeyStore.PrivateKeyEntry) merchantKeyStore.getEntry(merchantKeyAlias,
                    new KeyStore.PasswordProtection(userDefinedProperties.getProperty("KEY_PASS").toCharArray()));
            return e.getPrivateKey();
        } catch (UnrecoverableEntryException var5) {
            return null;
        }
    }

    private static String extractMerchantKeyAlias(KeyStore merchantKeyStore) throws KeyStoreException {
        Enumeration<String> enumKeyStore = merchantKeyStore.aliases();
        ArrayList<String> array = new ArrayList<>();

        while (enumKeyStore.hasMoreElements()) {
            String internalMerchantKeyAlias = enumKeyStore.nextElement();
            array.add(internalMerchantKeyAlias);
        }

        return keyAliasValidator(array, userDefinedProperties.getProperty("KEY_ALIAS"));
    }

    public static void createDetachedSignature(SOAPElement signatureElement, PrivateKey privateKey,
                                               SOAPElement securityTokenReference) throws Exception {
        Security.addProvider(new BouncyCastleProvider());
        XMLSignatureFactory xmlSignatureFactory = XMLSignatureFactory.getInstance("DOM");

        // (i) Digest method
        DigestMethod digestMethod = xmlSignatureFactory.newDigestMethod("http://www.w3.org/2001/04/xmlenc#sha256", null);
        ArrayList<Transform> transformList = new ArrayList<>();

        // (ii) Transform
        Transform envTransform = xmlSignatureFactory.newTransform("http://www.w3.org/2001/10/xml-exc-c14n#", (TransformParameterSpec) null);
        transformList.add(envTransform);

        // (iii) References
        ArrayList<Reference> refList = new ArrayList<>();
        Reference refBody = xmlSignatureFactory.newReference("#Body", digestMethod, transformList, null, null);
        refList.add(refBody);

        // (iv) Signed Info
        CanonicalizationMethod cm = xmlSignatureFactory.newCanonicalizationMethod("http://www.w3.org/2001/10/xml-exc-c14n#",
                (C14NMethodParameterSpec) null);

        SignatureMethod sm = xmlSignatureFactory.newSignatureMethod("http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", null);
        SignedInfo signedInfo = xmlSignatureFactory.newSignedInfo(cm, sm, refList);

        // (v) Sign Context to finally create detached signature
        DOMSignContext signContext = new DOMSignContext(privateKey, signatureElement);
        signContext.setDefaultNamespacePrefix("ds");
        signContext.putNamespacePrefix("http://www.w3.org/2000/09/xmldsig#", "ds");

        KeyInfoFactory keyFactory = KeyInfoFactory.getInstance();
        DOMStructure domKeyInfo = new DOMStructure(securityTokenReference);
        KeyInfo keyInfo = keyFactory.newKeyInfo(Collections.singletonList(domKeyInfo));
        XMLSignature signature = xmlSignatureFactory.newXMLSignature(signedInfo, keyInfo);
        signContext.setBaseURI("");

        signature.sign(signContext);

    }

    private static KeyStore loadCertificateIntoKeyStore()
            throws IOException, ConfigurationException, KeyStoreException, CertificateException, NoSuchAlgorithmException {
        if (userDefinedProperties.getProperty("KEY_ALIAS") == null) {
            throw new ConfigurationException("Key Alias is missing in properties file.");
        }

        String keyFilePath = PropertiesUtil.getKeyFilePath();

        // Get certificate private key.
        // (i) Get password
        String password = userDefinedProperties.getProperty("KEY_PASS");

        // (ii) Open the cert using KeyStore
        KeyStore merchantKeyStore = KeyStore.getInstance("PKCS12", new BouncyCastleProvider());
        merchantKeyStore.load(new FileInputStream(keyFilePath), password.toCharArray());

        return merchantKeyStore;
    }

    private static String keyAliasValidator(ArrayList<String> array, String merchantID) {
        String tempKeyAlias, merchantKeyAlias, result;
        StringTokenizer str;
        for (String s : array) {
            merchantKeyAlias = s;
            str = new StringTokenizer(merchantKeyAlias, ",");
            while (str.hasMoreTokens()) {
                tempKeyAlias = str.nextToken();
                if (tempKeyAlias.contains("CN")) {
                    str = new StringTokenizer(tempKeyAlias, "=");
                    while (str.hasMoreElements()) {
                        result = str.nextToken();
                        if (result.equalsIgnoreCase(merchantID)) {
                            return merchantKeyAlias;
                        }
                    }
                }
            }
        }
        return null;
    }
}
