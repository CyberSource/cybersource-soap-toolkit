import java.math.BigInteger;
import java.net.MalformedURLException;
import java.rmi.RemoteException;
import java.net.URL;
import javax.xml.rpc.ServiceException;
import org.apache.axis.AxisFault;
import org.apache.ws.security.WSConstants;
import org.apache.ws.security.handler.WSHandlerConstants;
import com.cybersource.stub.*;

public class Sample
{
    //  Before using this example, replace the generic value with your merchant ID. 
    private static final String MERCHANT_ID = "your_merchant_id";
    private static final String LIB_VERSION = "1.4/1.5.1"; // Axis Version / WSS4J Version
    // Remember to also change the TRANSACTION_KEY in SamplePWCallback.java

    private static final String SERVER_URL = "https://ics2wstest.ic3.com/commerce/1.x/transactionProcessor";
    
    public static void main( String[] args )
    {
        RequestMessage request = new RequestMessage();
        
        request.setMerchantID( MERCHANT_ID );
       
	    // Before using this example, replace the generic value with
		// your reference number for the current transaction. 
	    request.setMerchantReferenceCode( "your_merchant_reference_code" );
        
        // To help us troubleshoot any problems that you may encounter,
        // please include the following information about your application.
        request.setClientLibrary( "Java Axis WSS4J" );
        request.setClientLibraryVersion( LIB_VERSION );
        request.setClientEnvironment(
                  System.getProperty( "os.name" ) + "/" +
                  System.getProperty( "os.version" ) + "/" +
                  System.getProperty( "java.vendor" ) + "/" +
                  System.getProperty( "java.version" ) );
    
	   // This section contains a sample transaction request for the authorization 
       // service with complete billing, payment card, and purchase (two items) information.	
	    request.setCcAuthService( new CCAuthService() );
        request.getCcAuthService().setRun( "true" );

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
        request.setBillTo( billTo );

        Card card = new Card();
        card.setAccountNumber( "4111111111111111" );
        card.setExpirationMonth( new BigInteger("12") );
        card.setExpirationYear( new BigInteger("2020") );
        request.setCard( card );

        PurchaseTotals purchaseTotals = new PurchaseTotals();
        purchaseTotals.setCurrency( "USD" );
        request.setPurchaseTotals( purchaseTotals );

        Item[] items = new Item[2];

        Item item = new Item();
        item.setId( new BigInteger( "0" ) );
        item.setUnitPrice( "12.34" );
        item.setQuantity( new BigInteger( "2" ) );
        items[0] = item;

        item = new Item();
        item.setId( new BigInteger( "1" ) );
        item.setUnitPrice( "56.78" );
        items[1] = item;
        
        request.setItem( items );

        try {
            TransactionProcessorLocator service
                = new TransactionProcessorLocator();
                
            URL endpoint = new URL( SERVER_URL );

            ITransactionProcessorStub stub
                = (ITransactionProcessorStub) service.getportXML( endpoint );
                
            stub._setProperty( WSHandlerConstants.USER, request.getMerchantID() );
            
            ReplyMessage reply = stub.runTransaction( request );
            
            // To retrieve individual reply fields, follow these examples.
            System.out.println("decision = " + reply.getDecision());
            System.out.println("reasonCode = " + reply.getReasonCode());
            System.out.println("requestID = " + reply.getRequestID());
            System.out.println("requestToken = " + reply.getRequestToken());
            System.out.println("ccAuthReply.reasonCode = " + reply.getCcAuthReply().getReasonCode());
            
        } catch(AxisFault e) {
            System.out.println( "AxisFault: " + e );
        } catch (MalformedURLException e) {
            System.out.println( "MalformedURLException: " + e );
        } catch (RemoteException e) {
            System.out.println( "RemoteException: " + e );
        } catch (ServiceException e) {
            System.out.println( "ServiceException: " + e );
        }
    }    
    
} 

