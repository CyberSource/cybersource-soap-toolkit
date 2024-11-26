<?php

require_once 'PropertiesUtility.php';
require_once 'SecurityUtility.php';

class ExtendedClientWithToken extends SoapClient
{
    // namespaces defined by standard
    const WSU_NS    = 'http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd';
    const WSSE_NS   = 'http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd';
    const SOAP_NS   = 'http://schemas.xmlsoap.org/soap/envelope/';
    const DS_NS     = 'http://www.w3.org/2000/09/xmldsig#';

    protected $_ssl_options     = array();
    protected $_timeout         = 6000;

    private $propertiesUtility;
    private $securityUtility;

    function __construct($wsdl, $options = array())
    {
        $this->propertiesUtility = new PropertiesUtility();
        $this->securityUtility = new SecurityUtility();

        if (isset($options['SSL'])) {
            $this->_ssl_options = $options['SSL'];
            if (isset($this->_ssl_options['KEY_FILE']))
            {
                if ($this->propertiesUtility->isValidFilePath($this->_ssl_options))
                {
                    $certificateInfo = pathinfo($this->propertiesUtility->getFilePath($this->_ssl_options));
                    if (in_array(strtolower($certificateInfo['extension']), array('p12', 'pfx')))
                    {
                        $this->_ssl_options['certificate_type'] = 'P12';
                    }
                }
            }
        }
        else
        {
            throw new InvalidArgumentException("SSL Options are missing.");
        }

        if (isset($options['CONNECTION_TIMEOUT']) && intval($options['CONNECTION_TIMEOUT']))
        {
            $this->_timeout = intval($options['CONNECTION_TIMEOUT']);
        }

        return parent::__construct($wsdl, $options);
    }

    /**
     * Replace generic request with our own signed HTTPS request
     *
     * @param string $request
     * @param string $location
     * @param string $action
     * @param int $version
     * @param bool $oneWay
     * @return string
     */
    #[\ReturnTypeWillChange]
    function __doRequest($request, $location, $action, $version, $oneWay = false)
    {
        // Load request and add security headers
        $requestDom = new DOMDocument('1.0', 'utf-8');
        $requestDom->loadXML($request);

        $domXPath = new DOMXPath($requestDom);
        $domXPath->registerNamespace('SOAP-ENV', self::SOAP_NS);

        // Mark SOAP-ENV:Body with wsu:Id for signing
        $bodyNode = $domXPath->query('/SOAP-ENV:Envelope/SOAP-ENV:Body')->item(0);
        $bodyNode->setAttributeNS(self::WSU_NS, 'wsu:Id', 'Body');

        // Extract or Create SoapHeader
        $headerNode = $domXPath->query('/SOAP-ENV:Envelope/SOAP-ENV:Header')->item(0);
        if (!$headerNode)
        {
            $headerNode = $requestDom->documentElement->insertBefore($requestDom->createElementNS(self::SOAP_NS, 'SOAP-ENV:Header'), $bodyNode);
        }

        // Prepare Security element
        $securityElement = $headerNode->appendChild($requestDom->createElementNS(self::WSSE_NS, 'wsse:Security'));

        $privateKeyId = '';

        // Update with token data
        $securityElement->appendChild($this->securityUtility->generateSecurityToken($requestDom,
                                            $this->propertiesUtility->getFilePath($this->_ssl_options),
                                            $this->propertiesUtility->getCertificatePassword($this->_ssl_options),
                                            $privateKeyId)
                                        );

        // Create Signature element and build SignedInfo for elements with provided ids
        $signatureElement = $securityElement->appendChild($requestDom->createElementNS(self::DS_NS, 'ds:Signature'));
        $signInfo = $signatureElement->appendChild($this->securityUtility->buildSignedInfo($requestDom, array('Body')));

        // Combine Binary Security Token with Signature element
        openssl_sign($this->securityUtility->canonicalizeNode($signInfo), $signature, $privateKeyId, OPENSSL_ALGO_SHA256);

        $signatureElement->appendChild($requestDom->createElementNS(self::DS_NS, 'ds:SignatureValue', base64_encode($signature)));
        $keyInfo = $signatureElement->appendChild($requestDom->createElementNS(self::DS_NS, 'ds:KeyInfo'));
        $securityTokenReferenceElement = $keyInfo->appendChild($requestDom->createElementNS(self::WSSE_NS, 'wsse:SecurityTokenReference'));
        $keyReference = $securityTokenReferenceElement->appendChild($requestDom->createElementNS(self::WSSE_NS, 'wsse:Reference'));
        $keyReference->setAttribute('URI', "#X509Token");

        // Convert Document to String
        $request = $requestDom->saveXML();

        return parent::__doRequest($request, $location, $action, $version, $oneWay);
    }
}

?>