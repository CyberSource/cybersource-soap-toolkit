Imports System.Net
Imports System.Web.Services.Protocols
Imports Microsoft.Web.Services3.Security.Tokens
Imports myapp.CyberSource


Module Module1


    ' // Before using this example, replace the generic values with you own merchant ID and password. 
    Private Const MERCHANT_ID As String = "your_merchant_id"
    Private Const TRANSACTION_KEY As String = "your_transaction_key"
    Private Const LIB_VERSION As String = "3.0" ' WSE version
    Private Const POLICY_NAME As String = "CyberSource"

    Sub Main()
        Dim request As RequestMessage = New RequestMessage

        ' Replace this assignment statement as appropriate.
        request.merchantID = MERCHANT_ID
		
		' Replace the generic value with your reference number for the current transaction.
        request.merchantReferenceCode = "your_merchant_reference_code"

        ' To help us troubleshoot any problems that you may encounter,
        ' please include the following information about your PHP application.
        request.clientLibrary = ".NET VB WSE"
        request.clientLibraryVersion = LIB_VERSION
        request.clientEnvironment = _
            Environment.OSVersion.Platform.ToString() & _
            Environment.OSVersion.Version.ToString() & "-CLR" & _
            Environment.Version.ToString()

        ' This section contains a sample transaction request for the authorization 
        ' service with complete billing, payment card, and purchase (two items) information.
        request.ccAuthService = New CCAuthService
        request.ccAuthService.run = "true"

        Dim billTo As BillTo = New BillTo
        billTo.firstName = "John"
        billTo.lastName = "Doe"
        billTo.street1 = "1295 Charleston Road"
        billTo.city = "Mountain View"
        billTo.state = "CA"
        billTo.postalCode = "94043"
        billTo.country = "US"
        billTo.email = "null@cybersource.com"
        billTo.ipAddress = "10.7.111.111"
        request.billTo = billTo

        Dim card As Card = New Card
        card.accountNumber = "4111111111111111"
        card.expirationMonth = "12"
        card.expirationYear = "2020"
        request.card = card

        Dim purchaseTotals As PurchaseTotals = New PurchaseTotals
        purchaseTotals.currency = "USD"
        request.purchaseTotals = purchaseTotals

        request.item = New Item(2) {}

        Dim item As Item = New Item
        item.id = "0"
        item.unitPrice = "12.34"
        request.item(0) = item

        item = New Item
        item.id = "1"
        item.unitPrice = "56.78"
        request.item(1) = item

        Try
            Dim proc As TransactionProcessorWse = New TransactionProcessorWse
            proc.SetPolicy(POLICY_NAME)
            proc.SetClientCredential(Of UsernameToken)(New UsernameToken(request.merchantID, TRANSACTION_KEY, PasswordOption.SendPlainText))

            Dim reply As ReplyMessage = proc.runTransaction(request)

            ' To retrieve individual reply fields, follow these examples.
            Console.WriteLine("decision = " & reply.decision)
            Console.WriteLine("reasonCode = " & reply.reasonCode)
            Console.WriteLine("requestID = " & reply.requestID)
            Console.WriteLine("requestToken = " & reply.requestToken)
            Console.WriteLine("ccAuthReply.reasonCode = " & reply.ccAuthReply.reasonCode)
        Catch e As SoapHeaderException
            Console.WriteLine("SoapHeaderException: " & e.Message & vbCrLf & e.StackTrace)
        Catch e As SoapException
            Console.WriteLine("SoapException: " & e.Message & vbCrLf & e.StackTrace)
        Catch e As WebException
            Console.WriteLine("WebException: " & e.Message & vbCrLf & e.StackTrace)
        End Try

    End Sub

End Module
