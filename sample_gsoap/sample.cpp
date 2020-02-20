#include "soapITransactionProcessorProxy.h" 
#include "ITransactionProcessor.nsmap" 
#ifdef WIN32
#include "gsoapWinInet.h"
#endif
#include "wsseapi.h"

#ifndef WIN32
#include <signal.h>
void sigpipe_handle(int x) {}
#endif

// Before using this example, replace the generic values with your merchant ID and password. 
const char *MERCHANT_ID = "your_merchant_id";
const char *TRANSACTION_KEY = "your_transaction_key";
const char *SERVER_URL = "https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor";
const char *LIB_VERSION = "2.7.9d"; // gSOAP version
const char *ENVIRONMENT = "your_platform_info_e.g._Linux 2.6";
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

	ITransactionProcessor service;
	service.endpoint = SERVER_URL;

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

	ns3__RequestMessage request;
	
	request.merchantID = (char *) MERCHANT_ID;
	
	// Before using this example, replace the generic values with your
	// reference number for the current transaction. 
	request.merchantReferenceCode = "your_merchant_reference_code";

	// To help us troubleshoot any problems that you may encounter,
    // please include the following information about your application.
	request.clientLibrary = "gSOAP";
	request.clientLibraryVersion = (char *) LIB_VERSION;
	request.clientEnvironment = (char *) ENVIRONMENT;


	// This section contains a sample transaction request for the authorization service 
    // with complete billing, payment card, and purchase (two items) information.
	ns3__CCAuthService ccAuthService;
	ccAuthService.run = "true";
	request.ccAuthService = &ccAuthService;

	ns3__BillTo billTo;
	billTo.firstName = "John";
	billTo.lastName = "Doe";
	billTo.street1 = "1295 Charleston Road";
	billTo.city = "Mountain View";
	billTo.state = "CA";
	billTo.postalCode = "94043";
	billTo.country = "US";
	billTo.email = "null@cybersource.com";
	billTo.ipAddress = "10.7.111.111";
	request.billTo = &billTo;

	ns3__Card card;
	card.accountNumber = "4111111111111111";
	int expmo = 12;
	int expyr = 2020;
	card.expirationMonth = &expmo;
	card.expirationYear = &expyr;
	request.card = &card;

	ns3__PurchaseTotals purchaseTotals;
	purchaseTotals.currency = "USD";
	request.purchaseTotals = &purchaseTotals;

	ns3__Item item0;
	int id0 = 0;
	item0.id = &id0;
	item0.unitPrice = "12.34";
	int quantity = 2;
	item0.quantity = &quantity;

	ns3__Item item1;
	int id1 = 1;
	item1.id = &id1;
	item1.unitPrice = "12.34";

	// Note that request.item expects an array of ns3__Item pointers,
	// which is different from an array of contiguous ns3__Item's.
	// Also, this sample uses a static array. In your code, you are more likely
	// to need a dynamic heap-allocated array as the number of line items may vary.
	// In any case, you must set request.__sizeitem to the size of the array.
	
	ns3__Item *items[2] = { &item0, &item1 };
	request.__sizeitem = 2;
	request.item = items;

	ns3__ReplyMessage reply;

	soap_wsse_add_UsernameTokenText(
		service.soap, NULL, request.merchantID, TRANSACTION_KEY );

	int ret = service.__ns1__runTransaction( &request, &reply );
	switch (ret)
	{
		case SOAP_OK:
			// display selected reply fields 
			printf("decision = %s\n", reply.decision);
			printf("reasonCode = %d\n", reply.reasonCode);
			printf("requestID = %s\n", reply.requestID);
			printf("requestToken = %s\n", reply.requestToken);
			printf("ccAuthReply.reasonCode = %d\n",
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
	return 0;
}
