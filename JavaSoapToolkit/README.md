# IMPORTANT : SOAP Toolkit Update

**Products Included**: SOAP Toolkit for Java

**Region/Country**: Global

As part of ongoing Security Enhancements, we are planning to upgrade the SOAP API Authentication to P12 Authentication. You can upgrade to P12 Authentication in your SOAP toolkit by doing the following:

- Create a P12 certificate.
- Update the files in your project directory.
- Add your certificate information to a `toolkit.properties` file in your project directory.
- Update your `pom.xml` file.

You must upgrade the SOAP Authentication to use P12 by August 2025.

> **IMPORTANT** : This update is currently available only for the C# and Java SOAP Toolkit.
> 
> The following updated toolkits are available here on GitHub:
> 
> - Java SOAP toolkit
> - [C# SOAP toolkit](../CSharpSoapToolkit/README.md)
> - [PHP SOAP toolkit](../PHPSoapToolkit/README.md)
> - [C++ SOAP toolkit](../CPlusPlusSoapToolkit/README.md)
> 
> This Java SOAP Toolkit update only works with WSDL or XSD version 1.219 or earlier.

## Prerequisites

You must create a P12 certificate. See the [REST Getting Started Developer Guide](https://developer.cybersource.com/docs/cybs/en-us/platform/developer/all/rest/rest-getting-started/restgs-jwt-message-intro/restgs-security-p12-intro.html).

With this change to use a P12 certificate in your Java SOAP toolkit configuration, the new requirements for your application will be:

- Java 9 or higher
- Jakarta XML Web Services API
- JAX-WS Runtime
- Jakarta XML Web Services Distribution
- Bouncy Castle Cryptography APIs for JDK 1.5 to JDK 1.8
- Apache XML Security
- WSDL v1.219 or earlier

## Java Migration Steps

Follow these steps to upgrade your existing Java code:

1. Add these dependencies to the `pom.xml` file:

   ```xml
   <dependencies>
      <dependency>
         <groupId>jakarta.xml.ws</groupId>
         <artifactId>jakarta.xml.ws-api</artifactId>
         <version>4.0.2</version>
      </dependency>
      <dependency>
         <groupId>com.sun.xml.ws</groupId>
         <artifactId>jaxws-rt</artifactId>
         <version>4.0.3</version>
         <scope>runtime</scope>
      </dependency>
      <dependency>
         <groupId>com.sun.xml.ws</groupId>
         <artifactId>jaxws-ri</artifactId>
         <version>4.0.3</version>
         <type>pom</type>
      </dependency>
      <dependency>
         <groupId>org.bouncycastle</groupId>
         <artifactId>bcprov-jdk15to18</artifactId>
         <version>1.78</version>
      </dependency>
      <dependency>
         <groupId>org.apache.santuario</groupId>
         <artifactId>xmlsec</artifactId>
         <version>4.0.3</version>
      </dependency>
   </dependencies>
   ```

2. Add this plugin to the `pom.xml` file:

   ```xml
   <build>
      <plugins>
         <plugin>
             <groupId>com.sun.xml.ws</groupId>
             <artifactId>jaxws-maven-plugin</artifactId>
             <version>4.0.3</version>
             <configuration>
                 <wsdlUrls>
                     <wsdlUrl>https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor/CyberSourceTransaction_1.219.wsdl</wsdlUrl>
                 </wsdlUrls>
                 <keep>true</keep>
                 <packageName>com.cybersource.stub</packageName>
                 <sourceDestDir>src/main/java</sourceDestDir>
             </configuration>
         </plugin>
      </plugins>
   </build>
   ```

3. Check the value that is set in the `wsdlUrl` tag and update the version if necessary.

   > **IMPORTANT** : The highest version of the WSDL that can be supported is v1.219.

4. Run this command in your terminal:

   ```bash
    mvn clean jaxws:wsimport
    ```
   
5. Locate these lines in your existing code:

   ```java
   TransactionProcessorLocator service
        = new TransactionProcessorLocator();

   URL endpoint = new URL( SERVER_URL );
   
   ITransactionProcessorStub stub
           = (ITransactionProcessorStub) service.getportXML( endpoint );
                
   stub._setProperty( WSHandlerConstants.USER, request.getMerchantID() );
   ```

   and replace them with these lines:

   ```java
   TransactionProcessor service = new TransactionProcessor();

    service.setHandlerResolver( portInfo -> {
        List<Handler> handlerList = new ArrayList<>();
        handlerList.add(new BinarySecurityTokenHandler());
        return handlerList;
    });

    ITransactionProcessor stub = service.getPortXML();
   ```

6. Copy these files to your project directory:

   - [`BinarySecurityTokenHandler.java`](src/main/java/com/cybersource/BinarySecurityTokenHandler.java)
   - [`PropertiesUtil.java`](src/main/java/com/cybersource/PropertiesUtil.java)
   - [`SecurityUtils.java`](src/main/java/com/cybersource/SecurityUtils.java)

7. Add a toolkit.properties file in the src/main/resources folder in your project. The toolkit.properties must contain this content:

   ```properties
   MERCHANT_ID = <your_merchant_id>
   LIB_VERSION = 4.0.3
   KEY_ALIAS = <your_certificate_key_alias>
   KEY_FILE = <your_certificate_file>
   KEY_PASS = <your_certificate_password>
   KEY_DIRECTORY = src/main/resources
   ```

   If you want to use your own properties file, you can make these changes in the [`PropertiesUtil.java`](src/main/java/com/cybersource/stub/PropertiesUtil.java) file.

8. Add your P12 certificate to the `KEY_DIRECTORY`.

9. Run these commands in your terminal:

   ```bash
    mvn clean install
    ```

   ```bash
    java -jar target\JavaSoapToolkit.jar
    ```

You can confirm that your configuration is updated successfully by sending a test request. A successful configuration is indicated when the request log shows that the request was authenticated using a Bearer token.
