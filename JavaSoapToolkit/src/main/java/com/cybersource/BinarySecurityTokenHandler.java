package com.cybersource;

import jakarta.xml.soap.*;
import jakarta.xml.ws.handler.MessageContext;
import jakarta.xml.ws.handler.soap.SOAPHandler;
import jakarta.xml.ws.handler.soap.SOAPMessageContext;

import javax.xml.namespace.QName;
import java.util.Set;

public class BinarySecurityTokenHandler implements SOAPHandler<SOAPMessageContext> {
    @Override
    public Set<QName> getHeaders() {
        return Set.of();
    }

    @Override
    public boolean handleMessage(SOAPMessageContext soapMessageContext) {
        Boolean outboundProperty = (Boolean) soapMessageContext.get(MessageContext.MESSAGE_OUTBOUND_PROPERTY);
        if (outboundProperty) {
            SOAPMessage soapMessage = soapMessageContext.getMessage();

            soapMessage = processSoapMessage(soapMessage);
        }
        return true;
    }

    public SOAPMessage processSoapMessage(SOAPMessage soapMessage) {
        try {
            // (i) Fetch SOAP envelope
            SOAPEnvelope soapEnvelope = soapMessage.getSOAPPart().getEnvelope();
            SOAPBody soapBody = soapMessage.getSOAPBody();
            soapBody.addAttribute(soapEnvelope.createName("Id", "wsu",
                    "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"), "Body");
            SOAPHeader soapHeader = soapEnvelope.getHeader();
            if (soapHeader == null) {
                soapHeader = soapEnvelope.addHeader();
            }

            // (ii) Add security envelope
            SOAPElement securityElement = soapHeader.addChildElement("Security", "wsse",
                    "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            securityElement.addNamespaceDeclaration("wsu",
                    "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

            // (iii) Add Binary Security Token envelope
            SOAPElement tokenElement = securityElement.addChildElement("BinarySecurityToken", "wsse");
            tokenElement.setAttribute("ValueType",
                    "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3");
            tokenElement.setAttribute("EncodingType",
                    "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
            tokenElement.setAttribute("wsu:Id", "X509Token");
            tokenElement.addTextNode(SecurityUtils.generateBinarySecurityToken());

            // (iv) Combine Binary Security Token with Signature
            SOAPElement securityTokenReferenceElement = securityElement.addChildElement("SecurityTokenReference", "wsse");
            SOAPElement referenceElement = securityTokenReferenceElement.addChildElement("Reference", "wsse");
            referenceElement.setAttribute("URI", "#X509Token");

            SecurityUtils.createDetachedSignature(securityElement, SecurityUtils.getKeyFromCertificate(), securityTokenReferenceElement);
            return soapMessage;
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }

    @Override
    public boolean handleFault(SOAPMessageContext soapMessageContext) {
        return false;
    }

    @Override
    public void close(MessageContext messageContext) {

    }
}
