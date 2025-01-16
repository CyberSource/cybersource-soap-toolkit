#include "soapITransactionProcessorProxy.h" 
#include "ITransactionProcessor.nsmap" 
#ifdef WIN32
#include "gsoapWinInet.h"
#endif
#include "wsseapi.h"
#include "BinarySecurityTokenHandler.h"


#ifndef WIN32
#include <signal.h>
void sigpipe_handle(int x) {}
#endif

// Before using this example, replace the generic values with your merchant ID and password. 
const char *MERCHANT_ID = "your_merchant_id";
const char *TRANSACTION_KEY = "your_transaction_key";
const char *SERVER_URL = "https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor";
const char *LIB_VERSION = "2.8.135"; // gSOAP version
const char *ENVIRONMENT = "Microsoft Windows 11";
#ifndef WIN32
const char *CACERTS_FILE = "../gsoap/samples/ssl/cacerts.pem";
#endif


void handleFault(struct soap *soap)
{
	// This section dumps information to stdout.
	soap_print_fault( soap, stdout );
	SOAP_ENV__Fault *fault = soap->fault;
	printf("faultcode = %s\n", fault->faultcode);
	printf("faultstring = %s\n", fault->faultstring);
	SOAP_ENV__Detail *detail  = fault->detail;
	if (detail != NULL) {
		printf("detail = %s\n", detail->__any);
	}
}

int main(int argc, char* argv[])
{

#ifndef WIN32
	// one-time initializations
	signal(SIGPIPE, sigpipe_handle); 
	soap_ssl_init();
#endif

//	ITransactionProcessor service;
//	service.endpoint = SERVER_URL;
	ITransactionProcessorProxy service;
	service.soap_endpoint = SERVER_URL;

#ifdef WIN32
	soap_register_plugin( service.soap, wininet_plugin );
#else
	if (soap_ssl_client_context(
		service.soap, SOAP_SSL_DEFAULT, NULL, NULL, CACERTS_FILE, NULL, NULL )) 
	{ 
	   handleFault(service.soap);
	   exit(1); 
	} 
#endif

	//upgrading from previous versions: all ns2__ lines were originally ns__3, e.g.:
	//ns3__RequestMessage request;
	ns2__RequestMessage request;
	
	request.merchantID = (char *) MERCHANT_ID;
	
	// Before using this example, replace the generic values with your
	// reference number for the current transaction. 
	request.merchantReferenceCode = (char *) "your_merchant_reference_code";

	// To help us troubleshoot any problems that you may encounter,
    // please include the following information about your application.
	request.clientLibrary = (char *) "gSOAP";
	request.clientLibraryVersion = (char *) LIB_VERSION;
	request.clientEnvironment = (char *) ENVIRONMENT;


	// This section contains a sample transaction request for the authorization service 
    // with complete billing, payment card, and purchase (two items) information.
	ns2__CCAuthService ccAuthService;
	ccAuthService.run = (char *) "true";
	request.ccAuthService = &ccAuthService;

	ns2__BillTo billTo;
	billTo.firstName = (char *) "John";
	billTo.lastName = (char *) "Doe";
	billTo.street1 = (char *) "1295 Charleston Road";
	billTo.city = (char *) "Mountain View";
	billTo.state = (char *) "CA";
	billTo.postalCode = (char *) "94043";
	billTo.country = (char *) "US";
	billTo.email = (char *) "null@cybersource.com";
	billTo.ipAddress = (char *) "10.7.111.111";
	request.billTo = &billTo;

	ns2__Card card;
	card.accountNumber = (char *) "4111111111111111";

	//upgrading from previous versions: numeric values were int* before, now they're char*
	//due to type mapping changes in newer versions of gsoap/WS/WS-typemap.dat
	//int expmo = 12;
	//int expyr = 2020;
	//card.expirationMonth = &expmo;
	//card.expirationYear = &expyr;
	card.expirationMonth = (char *) "12";
	card.expirationYear = (char *) "2030";
	request.card = &card;

	ns2__PurchaseTotals purchaseTotals;
	purchaseTotals.currency = (char *) "USD";
	request.purchaseTotals = &purchaseTotals;

	ns2__Item item0;
	item0.id = (char *) "0";
	item0.unitPrice = (char *) "12.34";
	item0.quantity = (char *) "2";

	ns2__Item item1;
	item1.id = (char *) "1";
	item1.unitPrice = (char *) "1.99";

	// Note that request.item expects an array of ns2__Item pointers,
	// which is different from an array of contiguous ns2__Item's.
	// Also, this sample uses a static array. In your code, you are more likely
	// to need a dynamic heap-allocated array as the number of line items may vary.
	// In any case, you must set request.__sizeitem to the size of the array.
	
	ns2__Item *items[2] = { &item0, &item1 };
	request.__sizeitem = 2;
	request.item = items;

	ns2__ReplyMessage reply;

	//upgrading from previous versions: remove old security code
	//soap_wsse_add_UsernameTokenText(
	//	service.soap, NULL, request.merchantID, TRANSACTION_KEY );

	//upgrading from previous versions: add security fields to SOAP header
	addSecurityTokenAndSignature(service.soap);

	int ret = service.runTransaction(&request, reply);

	switch (ret)
	{
		case SOAP_OK:
			// display selected reply fields 
			printf("decision = %s\n", reply.decision);
			printf("reasonCode = %s\n", reply.reasonCode);
			printf("requestID = %s\n", reply.requestID);
			printf("requestToken = %s\n", reply.requestToken);
			printf("ccAuthReply.reasonCode = %s\n",
			       reply.ccAuthReply->reasonCode);
			break;

		case SOAP_FAULT:
			handleFault(service.soap);
			break;

		default:
			// soap_print_fault works even with non-fault error codes. 
			soap_print_fault(service.soap, stdout);
			printf( "Error code: %d\nPlease consult the gSOAP documentation.\n", ret );
	}

	//upgrading from previous versions: cleanup security call
	cleanupSecurityTokenAndSignature();

	return 0;
}
