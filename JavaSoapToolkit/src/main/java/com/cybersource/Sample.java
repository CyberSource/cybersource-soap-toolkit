package com.cybersource;

import com.cybersource.stub.*;
import jakarta.xml.ws.handler.Handler;

import java.io.IOException;
import java.math.BigInteger;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

public class Sample {
    public static void main(String[] args) throws IOException {
        Properties userDefinedProperties = PropertiesUtil.loadProperties();

        RequestMessage requestObj = new RequestMessage();

        requestObj.setMerchantID( userDefinedProperties.getProperty("MERCHANT_ID") );

        // Before using this example, replace the generic value with
        // your reference number for the current transaction.
        requestObj.setMerchantReferenceCode( "your_merchant_reference_code" );

        // To help us troubleshoot any problems that you may encounter,
        // please include the following information about your application.
        requestObj.setClientLibrary( "JAXWS" );
        requestObj.setClientLibraryVersion( userDefinedProperties.getProperty("LIB_VERSION") );
        requestObj.setClientEnvironment(
                System.getProperty( "os.name" ) + "/" +
                        System.getProperty( "os.version" ) + "/" +
                        System.getProperty( "java.vendor" ) + "/" +
                        System.getProperty( "java.version" ) );

        // This section contains a sample transaction requestObj for the authorization
        // service with complete billing, payment card, and purchase (two items) information.
        requestObj.setCcAuthService( new CCAuthService() );
        requestObj.getCcAuthService().setRun( "true" );

        BillTo billTo = new BillTo();
        billTo.setFirstName( "John" );
        billTo.setLastName( "Doe" );
        billTo.setStreet1( "1295 Charleston Road" );
        billTo.setCity( "Mountain View" );
        billTo.setState( "CA" );
        billTo.setPostalCode( "94043" );
        billTo.setCountry( "US" );
        billTo.setEmail( "null@cybersource.com" );
        billTo.setIpAddress( "10.7.111.111" );
        requestObj.setBillTo( billTo );

        Card card = new Card();
        card.setAccountNumber( "4111111111111111" );
        card.setExpirationMonth( new BigInteger("12") );
        card.setExpirationYear( new BigInteger("2035") );
        requestObj.setCard( card );

        PurchaseTotals purchaseTotals = new PurchaseTotals();
        purchaseTotals.setCurrency( "USD" );
        requestObj.setPurchaseTotals( purchaseTotals );

        Item[] items = new Item[2];

        Item item = new Item();
        item.setId( new BigInteger( "0" ) );
        item.setUnitPrice( "12.34" );
        item.setQuantity(String.valueOf(new BigInteger( "2" )));
        items[0] = item;

        item = new Item();
        item.setId( new BigInteger( "1" ) );
        item.setUnitPrice( "56.78" );
        items[1] = item;

        requestObj.getItem().add(items[0]);
        requestObj.getItem().add(items[1]);

        try {
            TransactionProcessor service = new TransactionProcessor();

            service.setHandlerResolver( portInfo -> {
                List<Handler> handlerList = new ArrayList<>();
                handlerList.add(new BinarySecurityTokenHandler());
                return handlerList;
            });

            ITransactionProcessor stub = service.getPortXML();

            ReplyMessage reply = stub.runTransaction( requestObj );

            // To retrieve individual reply fields, follow these examples.
            System.out.println("decision = " + reply.getDecision());
            System.out.println("reasonCode = " + reply.getReasonCode());
            System.out.println("requestID = " + reply.getRequestID());
            System.out.println("requestToken = " + reply.getRequestToken());
            System.out.println("ccAuthReply.reasonCode = " + reply.getCcAuthReply().getReasonCode());

        } catch (Exception e) {
            System.out.println( "RemoteException: " + e );
        }
    }
}