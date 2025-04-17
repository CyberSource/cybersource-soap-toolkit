using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace CSharpSoapToolkit
{
    public class InspectorBehavior : IEndpointBehavior
    {
        private ISecureCertificateStore _secureCertificateStore;

        public InspectorBehavior(ISecureCertificateStore secureCertificateStore)
        {
            // not calling the base implementation
            _secureCertificateStore = secureCertificateStore;
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // not calling the base implementation
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // not calling the base implementation
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // not calling the base implementation
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.ClientMessageInspectors.Add(new ClientInspector(_secureCertificateStore));
        }
    }

    public class ClientInspector : IClientMessageInspector
    {
        public MessageHeader[] Headers { get; set; }
        private ISecureCertificateStore _secureCertificateStore;

        public ClientInspector(ISecureCertificateStore secureCertificateStore, params MessageHeader[] headers)
        {
            _secureCertificateStore = secureCertificateStore;
            Headers = headers;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            MessageBuffer buffer = request.CreateBufferedCopy(int.MaxValue);
            Message copy = buffer.CreateMessage();

            if (Headers != null)
            {
                for (int i = Headers.Length - 1; i >= 0; i--)
                    request.Headers.Insert(0, Headers[i]);
            }

            SoapEnvelopeUtility.AddSecurityElements(ref copy, _secureCertificateStore);

            request = copy;

            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            // not calling the base implementation
        }
    }
}
