<?php

require_once("ExtendedClientWithToken.php");

// Before using this example, replace the generic values with your merchant ID.
define( 'MERCHANT_ID', 'YOUR MERCHANT ID' );
define( 'WSDL_URL', 'https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor/CyberSourceTransaction_1.219.wsdl' );

try {
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

    /*
    *  To see the functions and types that the SOAP extension can automatically generate from the WSDL file, uncomment this section:
    */
    // $functions = $soapClient->__getFunctions();
    // print_r($functions);
    // $types = $soapClient->__getTypes();
    // print_r($types);

    $request = new stdClass();

    $request->merchantID = MERCHANT_ID;

    // Before using this example, replace the generic value with your own.
    $request->merchantReferenceCode = "your_merchant_reference_code";

    // To help us troubleshoot any problems that you may encounter,
    // please include the following information about your PHP application.
    $request->clientLibrary = "PHP";
    $request->clientLibraryVersion = phpversion();
    $request->clientEnvironment = php_uname();

    // This section contains a sample transaction request for the authorization 
    // service with complete billing, payment card, and purchase (two items) information.    
    $ccAuthService = new stdClass();
    $ccAuthService->run = "true";
    $request->ccAuthService = $ccAuthService;

    $billTo = new stdClass();
    $billTo->firstName = "John";
    $billTo->lastName = "Doe";
    $billTo->street1 = "1295 Charleston Road";
    $billTo->city = "Mountain View";
    $billTo->state = "CA";
    $billTo->postalCode = "94043";
    $billTo->country = "US";
    $billTo->email = "null@cybersource.com";
    $billTo->ipAddress = "10.7.111.111";
    $request->billTo = $billTo;

    $card = new stdClass();
    $card->accountNumber = "4111111111111111";
    $card->expirationMonth = "12";
    $card->expirationYear = "2035";
    $request->card = $card;

    $purchaseTotals = new stdClass();
    $purchaseTotals->currency = "USD";
    $request->purchaseTotals = $purchaseTotals;

    $item0 = new stdClass();
    $item0->unitPrice = "12.34";
    $item0->quantity = "2";
    $item0->id = "0";

    $item1 = new stdClass();
    $item1->unitPrice = "56.78";
    $item1->id = "1";

    $request->item = array($item0, $item1);

    $reply = $soapClient->runTransaction($request);

    // This section will show all the reply fields.
    // var_dump($reply);

    // To retrieve individual reply fields, follow these examples.
    printf("DECISION                    : $reply->decision\n");
    printf("REASON CODE                 : $reply->reasonCode\n");
    printf("REQUEST ID                  : $reply->requestID\n");
    printf("REQUEST TOKEN               : $reply->requestToken\n");
    printf("CCAUTHREPLY -> REASON CODE  : " . $reply->ccAuthReply->reasonCode . "\n");
} catch (SoapFault $exception) {
    var_dump(get_class($exception));
    var_dump($exception);
}
