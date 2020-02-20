import java.io.IOException;
import javax.security.auth.callback.Callback;
import javax.security.auth.callback.CallbackHandler;
import javax.security.auth.callback.UnsupportedCallbackException;
import org.apache.ws.security.WSPasswordCallback;

/**
 * Sample password callback for the client
 */
public class SamplePWCallback implements CallbackHandler {

    // Before using this sample, change the generic value with your reference number for the current transaction.
    private static final String TRANSACTION_KEY = "your_transaction_key";
    
    /**
     * @see javax.security.auth.callback.CallbackHandler#handle(javax.security.auth.callback.Callback[])
     */
    public void handle(Callback[] callbacks)
        throws IOException, UnsupportedCallbackException
    {
        for (int i = 0; i < callbacks.length; i++) {
            if (callbacks[i] instanceof WSPasswordCallback) {
                WSPasswordCallback pc = (WSPasswordCallback)callbacks[i];

                // This sample returns one password for all merchants.
                // To support multiple passwords, return the password
                // corresponding to pc.getIdentifier().
                pc.setPassword( TRANSACTION_KEY );
            } else {
                throw new UnsupportedCallbackException(callbacks[i], "Unrecognized Callback");
            }
        }
    }
}

