# IMPORTANT : SOAP Toolkit Update

**Products Included**: SOAP Toolkit for PHP

**Region/Country**: Global

As part of ongoing Security Enhancements, we are planning to upgrade the SOAP API Authentication to P12 Authentication. You can upgrade to P12 Authentication in your SOAP toolkit by doing the following:

- Create a P12 certificate.
- Update the files in your project directory.
- Add your certificate information to your code.

You must upgrade the SOAP Authentication to use P12 by February 13, 2025.

> **IMPORTANT** : This update is currently available only for the C#, Java and PHP SOAP Toolkit.
> 
> The following updated SDKs are available here on GitHub:
> 
> - Java SOAP toolkit
> - C# SOAP toolkit
> - PHP SOAP toolkit
> - C++ SOAP toolkit
> 
> Other toolkits will be available in January 2025.

## Prerequisites

You must create a P12 certificate. See the [REST Getting Started Developer Guide](https://developer.cybersource.com/docs/cybs/en-us/platform/developer/all/rest/rest-getting-started/restgs-jwt-message-intro/restgs-security-p12-intro.html).

With this change to use a P12 certificate in your PHP SOAP toolkit configuration, the new requirements for your application will be:

- PHP 5.6.x and higher
- PHP SOAP extension
- PHP OpenSSL extension

## PHP Migration Steps

Follow these steps to upgrade your existing PHP code:

1. Update the following service URL (`WSDL_URL`) in your code:

   ```text
   https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor/CyberSourceTransaction_N.NNN.wsdl
   ```

   where *N.NNN* is the latest server API version.

2. Copy these files to your project directory:
   - [ExtendedClientWithToken.php](ExtendedClientWithToken.php)
   - [PropertiesUtility.php](PropertiesUtility.php)
   - [SecurityUtility.php](SecurityUtility.php)

3. Locate these lines in your existing code:

   ```php
   $soapClient = new ExtendedClient(WSDL_URL, array());
   ```

   and replace them with these lines:

   ```php
   $soapClient = new ExtendedClientWithToken(
        WSDL_URL,
        array(
            'SSL' => array(
                    'KEY_ALIAS'     => 'YOUR KEY ALIAS',
                    'KEY_FILE'      => 'YOUR CERTIFICATE FILE',
                    'KEY_PASS'      => 'YOUR KEY PASS',
                    'KEY_DIRECTORY' => 'PATH TO CERTIFICATES'
                )
            )
    );
   ```

4. Update the necessary values for the following fields in your code:
   - `MERCHANT_ID`
   - `KEY_ALIAS`
   - `KEY_FILE`
   - `KEY_PASS`
   - `KEY_DIRECTORY`

5. Add your P12 certificate to the `KEY_DIRECTORY`.

   This `KEY_DIRECTORY` location must be accessible by your code. Ensure that your code has permissions to read this location.

6. Run the code:

   ```bash
   php <sample_PHP_file>
   ```

You can confirm that your configuration is updated successfully by sending a test request. A successful configuration is indicated when the request log shows that the request was authenticated using a Bearer token.
