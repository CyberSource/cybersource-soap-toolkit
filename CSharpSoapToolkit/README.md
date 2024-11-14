# IMPORTANT : SOAP Toolkit Update

**Products Included**: SOAP Toolkit for C#

**Region/Country**: Global

As part of ongoing Security Enhancements, we are planning to upgrade the SOAP API Authentication to P12 Authentication. You can upgrade to P12 Authentication in your SOAP toolkit by doing the following:

- Create a P12 certificate.
- Update the files in your project directory.
- Add your certificate information to the `app.config` file in your project directory.
- Update your `app.config` file.

You must upgrade the SOAP Authentication to use P12 by February 13, 2025.

> **IMPORTANT** : This update is currently available only for the C# and Java SOAP Toolkit.
> 
> The following updated SDKs are available here on GitHub:
> 
> - Java SOAP toolkit
> - C# SOAP toolkit
> 
> Other toolkits will be available in January 2025.

## Prerequisites

You must create a P12 certificate. See the [REST Getting Started Developer Guide](https://developer.cybersource.com/docs/cybs/en-us/platform/developer/all/rest/rest-getting-started/restgs-jwt-message-intro/restgs-security-p12-intro.html).

With this change to use a P12 certificate in your C# SOAP toolkit configuration, the new requirements for your application will be:

- .NET Framework 4.7.2 and later Redistributable Package
- [NuGet Command-Line Interface](https://learn.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference?tabs=windows)
- Portable.BouncyCastle

## C# Migration Steps

Follow these steps to upgrade your existing C# code:

### Getting the service reference classes

1. Add the following service URL as a service reference to your project:

   ```text
   https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor/CyberSourceTransaction_N.NNN.wsdl
   ```

   where *N.NNN* is the latest server API version.

   This will generate a "Connected Services" section in your project. It will also generate an `app.config` file for your project.

### Modifying `app.config`

1. Add the following sections to the top of your `app.config` file:

   ```xml
   <configuration>
      <configSections>
         <section name="toolkitProperties" type="System.Configuration.NameValueSectionHandler"/>
      </configSections>

      <toolkitProperties>
         <add key="MERCHANT_ID" value="<your_merchant_id>"/>
         <add key="KEY_ALIAS" value="<your_certificate_key_alias>"/>
         <add key="KEY_FILE" value="<your_certificate_file>"/>
         <add key="KEY_PASS" value="<your_certificate_password>"/>
         <add key="KEY_DIRECTORY" value="<path/to/certificate/file>"/>
      </toolkitProperties>
   </configuration>
   ```

   > > **NOTE** : The `configSections` tag **MUST** be the first section inside `configurations`.

2. In the generated `app.config` file, leave the `<binding>` section as it is.

   It must look like follows:

   ```xml
   <bindings>
      <basicHttpBinding>
            <binding name="ITransactionProcessor">
            <security mode="Transport"/>
            </binding>
      </basicHttpBinding>
   </bindings>
   ```

### Adding new dependency

1. Add this dependency to the `packages.config` file:

   ```xml
   <packages>
      <package id="Portable.BouncyCastle" version="1.9.0" targetFramework="net472" />
   </packages>
   ```

2. Install the dependency:
   ```cmd
   nuget install packages.config -OutputDirectory packages
   ```

3. Add this package reference to your `.csproj` file:
   ```xml
   <Reference Include="BouncyCastle.Crypto, Version=1.9.0.0, Culture=neutral, PublicKeyToken=0e99375e54769942, processorArchitecture=MSIL">
      <HintPath>packages\Portable.BouncyCastle.1.9.0\lib\net40\BouncyCastle.Crypto.dll</HintPath>
   </Reference>
   ```

> **NOTE** : These steps can also be done through Visual Studio Package Manager.

### Adding new files to project

1. Add your P12 certificate to the `KEY_DIRECTORY`.

   This `KEY_DIRECTORY` location must be accessible by your code. Ensure that your code has permissions to read this location.

2. Copy these files to your project directory:
   - [CertificateCacheUtility.cs](CSharpSoapToolkit\CertificateCacheUtility.cs)
   - [InspectorBehavior.cs](CSharpSoapToolkit\InspectorBehavior.cs)
   - [PropertiesUtility.cs](CSharpSoapToolkit\PropertiesUtility.cs)
   - [SecurityUtility.cs](CSharpSoapToolkit\SecurityUtility.cs)
   - [SoapEnvelopeUtility.cs](CSharpSoapToolkit\SoapEnvelopeUtility.cs)

3. Import the files above to your project.

### Modifying existing code

1. Locate these lines in your existing code:

   ```csharp
   TransactionProcessorClient proc = new TransactionProcessorClient();

   proc.ChannelFactory.Credentials.UserName.UserName =  request.merchantID;
   proc.ChannelFactory.Credentials.UserName.Password =  TRANSACTION_KEY;

   ReplyMessage reply = proc.runTransaction(request);
   ```

   and replace them with these lines:

   ```csharp
   TransactionProcessorClient proc = new TransactionProcessorClient();

   proc.Endpoint.EndpointBehaviors.Add(new InspectorBehavior());

   ReplyMessage reply = proc.runTransaction(request);
   ```

### Compiling project

1.  Locate your installation of .NET Framework.

   Usually this is at `C:\Windows\Microsoft.NET\Framework\v4.0.30319` (32-bit) or `C:\Windows\Microsoft.NET\Framework64\v4.0.30319` (64-bit).

2.  Use `msBuild.exe` to compile your project.

   ```cmd
   <path_to_framework>\msBuild.exe <name_of_project>.csproj
   ```

### Running project

1. Run the project executable:

   ```cmd
   bin\<configuration>\<project_name>.exe
   ```

You can confirm that your configuration is updated successfully by sending a test request. A successful configuration is indicated when the request log shows that the request was authenticated using a Bearer token.
