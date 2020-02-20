using System;
using System.Net;
using System.Web.Services.Protocols;
using System.Xml;

using myapp.CyberSource;

using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Design;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Tokens;
using System.Security.Cryptography.X509Certificates;

namespace myapp
{
    class Class1
    {
        // Before using this example, replace the generic values with you own merchant ID and password. 
        private const String MERCHANT_ID = "your_merchant_id";
        private const String TRANSACTION_KEY = "your_transaction_key";
        private const String LIB_VERSION = "3.0";  // WSE version
        private const String POLICY_NAME = "CyberSource";

        static void Main(string[] args)
        {
            RequestMessage request = new RequestMessage();

            request.merchantID = MERCHANT_ID;
           
		    // Replace the generic value with your reference number for the current transaction.
		    request.merchantReferenceCode = "your_merchant_reference_code";

            // To help us troubleshoot any problems that you may encounter,
            // please include the following information about your PHP application.
            request.clientLibrary = ".NET C# WSE";
            request.clientLibraryVersion = LIB_VERSION;
            request.clientEnvironment =
                Environment.OSVersion.Platform +
                Environment.OSVersion.Version.ToString() + "-CLR" +
                Environment.Version.ToString();

            // This section contains a sample transaction request for the authorization		 
            // service with complete billing, payment card, and purchase (two items) information.	
			request.ccAuthService = new CCAuthService();
            request.ccAuthService.run = "true";

            BillTo billTo = new BillTo();
            billTo.firstName = "John";
            billTo.lastName = "Doe";
            billTo.street1 = "1295 Charleston Road";
            billTo.city = "Mountain View";
            billTo.state = "CA";
            billTo.postalCode = "94043";
            billTo.country = "US";
            billTo.email = "null@cybersource.com";
            billTo.ipAddress = "10.7.111.111";
            request.billTo = billTo;

            Card card = new Card();
            card.accountNumber = "4111111111111111";
            card.expirationMonth = "12";
            card.expirationYear = "2020";
            request.card = card;

            PurchaseTotals purchaseTotals = new PurchaseTotals();
            purchaseTotals.currency = "USD";
            request.purchaseTotals = purchaseTotals;

            request.item = new Item[2];

            Item item = new Item();
            item.id = "0";
            item.unitPrice = "12.34";
            request.item[0] = item;

            item = new Item();
            item.id = "1";
            item.unitPrice = "56.78";
            request.item[1] = item;

            try
            {
                TransactionProcessorWse proc = new TransactionProcessorWse();
                proc.SetPolicy(POLICY_NAME);
                proc.SetClientCredential<UsernameToken>(new UsernameToken(request.merchantID, TRANSACTION_KEY, PasswordOption.SendPlainText));

                ReplyMessage reply = proc.runTransaction(request);

                // To retrieve individual reply fields, follow these examples.
                Console.WriteLine("decision = " + reply.decision);
                Console.WriteLine("reasonCode = " + reply.reasonCode);
                Console.WriteLine("requestID = " + reply.requestID);
                Console.WriteLine("requestToken = " + reply.requestToken);
                Console.WriteLine("ccAuthReply.reasonCode = " + reply.ccAuthReply.reasonCode);
            }
            catch (SoapHeaderException e)
            {
                Console.WriteLine("SoapHeaderException: " + e.Message + "\n" + e.StackTrace);
            }
            catch (SoapException e)
            {
                Console.WriteLine("SoapException: " + e.Message + "\n" + e.StackTrace);
            }
            catch (WebException e)
            {
                Console.WriteLine("WebException: " + e.Message + "\n" + e.StackTrace);
            }
        }
    }
}
