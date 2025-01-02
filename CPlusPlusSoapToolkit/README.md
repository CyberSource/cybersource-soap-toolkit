# IMPORTANT : SOAP Toolkit Update

**Products Included**: SOAP Toolkit for C++

**Region/Country**: Global

As part of ongoing Security Enhancements, we are planning to upgrade the SOAP API Authentication to P12 Authentication. You can upgrade to P12 Authentication in your SOAP toolkit by doing the following:

- Create a P12 certificate.
- Update the files in your project directory.
- Add your certificate information to a `toolkit.properties` file in your project directory.
- Update your Makefile.

You must upgrade the SOAP Authentication to use P12 by February 13, 2025.

> 
> The following updated SDKs are available here on GitHub:
> 
> - Java SOAP toolkit
> - C# SOAP toolkit
> - PHP SOAP toolkit
> - C++ SOAP toolkit

> Other toolkits will be available in January 2025.
> 

## Prerequisites

You must create a P12 certificate. See the [REST Getting Started Developer Guide](https://developer.cybersource.com/docs/cybs/en-us/platform/developer/all/rest/rest-getting-started/restgs-jwt-message-intro/restgs-security-p12-intro.html).

With this change to use a P12 certificate in your C++ SOAP toolkit configuration, the new requirements for your application will be:

- GSOAP 2.8.135 or higher: Developer version including header files.
- OpenSSL 3.4.0 or higher: Developer version including header files.
- G++ compiler

## C++ Migration Steps

Follow these steps to upgrade your existing C++ code:

1. Makefile

If you already have an existing Makefile from a previous version of C++ SOAP Toolkit, then update it as follows. Change cybsdemo target from: 
```bash
cybsdemo:	sample.cpp $(SOAPH) $(SOAPCPP) ../gsoap/dom.cpp wsseapi.o smdevp.o
		$(CPP) $(CFLAGS) -o cybsdemo sample.cpp soapC.cpp soapClient.cpp ../gsoap/dom.cpp $(SOAPCPP) wsseapi.o smdevp.o $(LIBS)
   ```
   To:
```bash
cybsdemo:	sample.cpp $(SOAPH) $(SOAPCPP) ../gsoap/dom.cpp wsseapi.o smdevp.o
		$(CPP) $(CFLAGS) -o cybsdemo sample.cpp soapC.cpp ../gsoap/dom.cpp stdsoap2.cpp ../gsoap/import/custom/struct_timeval.cpp ../gsoap/plugin/threads.c ../gsoap/plugin/mecevp.c ../gsoap/plugin/wsaapi.c wsseapi.o smdevp.o soapITransactionProcessorProxy.cpp ../gsoap/import/gsoapWinInet.cpp PropertiesUtil.cpp BinarySecurityTokenHandler.cpp $(LIBS)
   ```

Alternatively, or if you don't already have a Makefile, then choose one of the following provided:
- UnixBuildAllCommented.Makefile
- UnixQuickBuild.Makefile
- WindowsBuildAllCommented.Makefile
- WindowsQuickBuild.Makefile

The "BuildAllCommented" flavours are recommended for most use cases.
If you need to make changes to individual targets, you may find it quicker to use the "QuickBuild" flavours, as each target can be built individually.

2. WSDL version.

Copies of version 1.224 of CyberSourceTransaction wsdl and xsd are provided in the project for convenience. If you want a different version, download your preferred version to the project directory, and update the versions in the header target of your makefile. For example, change:

```bash
header:	CyberSourceTransaction_1.224.wsdl
		$(GWSDL) -t ../gsoap/WS/WS-typemap.dat -s -o cybersource.h CyberSourceTransaction_1.224.wsdl
   ```
To:
```bash
header:	CyberSourceTransaction_1.219.wsdl
		$(GWSDL) -t ../gsoap/WS/WS-typemap.dat -s -o cybersource.h CyberSourceTransaction_1.219.wsdl
   ```

3. GSOAP

Ensure that gsoap is on the compile path.

Either: 
- Copy the gsoap directory (including header files) to one level above the project directory; or
- Update all paths in the Makefile which reference 'gsoap' to point to where gsoap can be found

4. Sample.cpp

If you already have an existing sample.cpp from a previous version of C++ SOAP Toolkit, see the comments marked "upgrading from previous versions" to find out what has changed. The most significant is:

Replace the following line:

```C++
	soap_wsse_add_UsernameTokenText(
		service.soap, NULL, request.merchantID, TRANSACTION_KEY );
```
    
With:
```C++
	addSecurityTokenAndSignature(service.soap);
```
    
After the web service call to runTransaction(), add the following line:
```C++
    cleanupSecurityTokenAndSignature();
```
    
Alternatively, or if you don't already have an existing sample.cpp, then use the new version provided in the project.

Update the values of the following to match your test credentials:

```C++
const char *MERCHANT_ID = "your_merchant_id";
const char *TRANSACTION_KEY = "your_transaction_key";
```

5. Other source files

The following new source files are provided, these should not need modifications:
- BinarySecurityTokenHandler.cpp/.h
- PropertiesUtil.cpp/.h
- stdsoap2.cpp/.h

6. Configuration file

A configuration file, toolkit.properties has been added. At runtime it is read from the same location as the executable.

Edit it to update the following to match your test credentials:

MERCHANT_ID=<your merchant id>
KEY_ALIAS=<your key alias>
KEY_FILE=<p12 filename>
KEY_PASS=<p12 password>
KEY_DIRECTORY=<p12 directory>

**Important:** ensure there are no spaces at the start and end of the lines, or either side of '='

7. Run this command in your terminal:

```bash
    make header
```
This will generate the following file:
- cybersource.h
   
8. Update cybersource.h:

Edit cybersource.h and insert the following line in the Import section:
```C++
    #import "WS-Header.h"
```
    
9. Run this command in your terminal:

```bash
    make source
```
This will generate the following files:
- soapITransactionProcessorProxy.cpp
- soapITransactionProcessorProxy.h
- ITransactionProcessor.runTransaction.req.xml
- ITransactionProcessor.runTransaction.res.xml
- ITransactionProcessor.nsmap
- soapStub.h
- soapH.h
- soapC.cpp

10. Run these commands in your terminal:

```bash
    make wsseapi.o
    make smdevp.o
    make cybsdemo
```

The last target will generate the executable we want, e.g. cybsdemo.exe

11. Add your P12 certificate to the `KEY_DIRECTORY`.

12. Run the executable. For example, in your terminal:

```bash
    cybsdemo.exe
```

The console output should consist of the keyFile full path, followed by the response from the web service. You have been successful when there are no errors output. 

Example successful output:

```bash
keyFile full path: C:/keys/test_p12_file.p12
decision = APPROVED
reasonCode = 0
requestID = 6017349752184504643266
requestToken = Axxd6DCgJoj77wSTjm5pe7DwFPfjpNDMyadDIZ/u1Pfje7D9IGU1ogwAGkmXoDc3JoZmTToZDIAAvxSz
ccAuthReply.reasonCode = 0
```
