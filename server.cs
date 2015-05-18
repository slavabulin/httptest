//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using System.ServiceModel.Dispatcher;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Cryptography;


namespace Microsoft.Samples.Http
{
    // Define a service contract.
    [ServiceContract(Namespace="http://Microsoft.Samples.Http"),
    XmlSerializerFormat,
    SecurityContractBehavior
    ]
    public interface ICalculator
    {
        [OperationContract(Action = "*", ReplyAction = "*"),
        SecurityOperationBehavoirAttribute("Add", "http://Microsoft.Samples.Http")]
        double Add(double n1, double n2);
        [OperationContract(ReplyAction = "*"),
        SecurityOperationBehavoirAttribute("Subtract", "http://Microsoft.Samples.Http")]
        double Subtract(double n1, double n2);
        [OperationContract(ReplyAction = "*"),
        SecurityOperationBehavoirAttribute("Multiply", "http://Microsoft.Samples.Http")]
        double Multiply(double n1, double n2);
        [OperationContract]
        double Divide(double n1, double n2);
        [OperationContract(ReplyAction = "*"),
        SecurityOperationBehavoirAttribute("GetScopes", "http://www.onvif.org/ver10/device/wsdl")]
        double GetScopes(double n1, double n2);
    }

    // Service class which implements the service contract.
    public class CalculatorService : ICalculator
    {
        public double Add(double n1, double n2)
        {
            return n1 + n2;
        }

        public double Subtract(double n1, double n2)
        {
            return n1 - n2;
        }

        public double Multiply(double n1, double n2)
        {
            return n1 * n2;
        }

        public double Divide(double n1, double n2)
        {
            return n1 / n2;
        }

        public double GetScopes(double n1, double n2)
        {
            //return n1 + n2;
            return 10;
        }
    }

    
    public class Service
    {
        public static void Main()
        {
            Uri baseAddress = new Uri("http://192.168.1.170:80");
            using (ServiceHost serviceHost = new ServiceHost(typeof(CalculatorService), baseAddress))
            {
                WSHttpBinding binding = new WSHttpBinding(SecurityMode.None);                
                
                ServiceEndpoint DeviceServiceEndpoint = serviceHost.AddServiceEndpoint(typeof(ICalculator),
                    binding, 
                    "/onvif/device_service"
                    );

                //DeviceServiceEndpoint.Behaviors.Add(new MyTestBehavior());
                serviceHost.Open();
                // The service can now be accessed.
                Console.WriteLine("Service started at {0}", serviceHost.BaseAddresses[0]);
                Console.WriteLine("Press ENTER to terminate service.");
                Console.ReadLine();
            }            
        }
    }

    //public class SecurityMessageInspector : IClientMessageInspector, IDispatchMessageInspector
    //{
    //    public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
    //    {
    //        Console.WriteLine("IClientMessageInspector.AfterReceiveReply called.");
    //        Console.WriteLine("Message: {0}", reply.ToString());
    //    }

    //    public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
    //    {
    //        Console.WriteLine("IClientMessageInspector.BeforeSendRequest called.");
    //        return null;
    //    }

    //    public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
    //    {
    //        foreach (MessageHeader hdr in request.Headers)
    //        {
    //            if (hdr.Name == "Security")
    //            {
    //                Console.WriteLine(hdr.ToString());
    //            }
    //        }
    //        Console.WriteLine("IDispatchMessageInspector.AfterReceiveRequest called.");
    //        return null;
    //    }

    //    public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
    //    {
    //        Console.WriteLine("IDispatchMessageInspector.BeforeSendReply called.");
    //    }

    //}

    //public class MyTestBehavior : IEndpointBehavior
    //{
    //    public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
    //    {
    //    }

    //    public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
    //    {
    //        SecurityMessageInspector inspector = new SecurityMessageInspector();
    //        clientRuntime.MessageInspectors.Add(inspector);
    //    }

    //    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
    //    {
    //        endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new SecurityMessageInspector());
    //    }

    //    public void Validate(ServiceEndpoint endpoint)
    //    {
    //    }
    //}

    public class SecurityOperationBehavoirAttribute : Attribute, IOperationBehavior
    {

        XmlQualifiedName qname;
        
        public SecurityOperationBehavoirAttribute(string name)
        {
            qname = new XmlQualifiedName(name);
        }

        public SecurityOperationBehavoirAttribute(string name, string ns)
        {
            qname = new XmlQualifiedName(name, ns);
        }

        internal XmlQualifiedName QName
        {
            get { return qname; }
            set { qname = value; }
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    sealed class SecurityContractBehaviorAttribute : Attribute, IContractBehavior
    {
        #region IContractBehavior Members

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            // no binding parameters need to be set here
            return;
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            // this is a dispatch-side behavior which doesn't require
            // any action on the client
            return;
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
            // We iterate over the operation descriptions in the contract and
            // try to locate an DispatchBodyElementAttribute behaviors on each 
            // operation. If found, we add the operation, keyed by QName of the body element 
            // that selects which calls shall be dispatched to this operation to a 
            // dictionary. 

            Dictionary<XmlQualifiedName, string> dispatchDictionary = new Dictionary<XmlQualifiedName, string>();
            foreach (OperationDescription operationDescription in contractDescription.Operations)
            {
                SecurityOperationBehavoirAttribute securityOperationBehavoirAttribute =
                    operationDescription.Behaviors.Find<SecurityOperationBehavoirAttribute>();
                if (securityOperationBehavoirAttribute != null)
                {
                    dispatchDictionary.Add(securityOperationBehavoirAttribute.QName, operationDescription.Name);
                }
            }

            // Lastly, we create and assign and instance of our operation selector that
            // gets the dispatch dictionary we've just created.

            dispatchRuntime.OperationSelector =
                new SecurityOperationSelector(
                   dispatchDictionary,
                   dispatchRuntime.UnhandledDispatchOperation.Name);
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }

        #endregion
    }

    class SecurityOperationSelector : IDispatchOperationSelector
	{
        Dictionary<XmlQualifiedName, string> dispatchDictionary;
        string defaultOperationName;

        public SecurityOperationSelector(Dictionary<XmlQualifiedName,string> dispatchDictionary, string defaultOperationName)
        {
            this.dispatchDictionary = dispatchDictionary;
            this.defaultOperationName = defaultOperationName;
        }

        #region
        //private Message CreateMessageCopy(Message message, XmlDictionaryReader body)
        //{
        //    Message copy = Message.CreateMessage(message.Version,message.Headers.Action,body);
        //    copy.Headers.CopyHeaderFrom(message,0);
        //    copy.Properties.CopyProperties(message.Properties);
        //    return message;
        //}
        #endregion

        //private Message CreateMessageCopy(Message message, XmlDictionaryReader body)
        //{
        //    Message copy;
        //    //MessageBuffer msgbuf = message.CreateBufferedCopy(System.Int32.MaxValue);
        //    //copy = msgbuf.CreateMessage();
        //    try
        //    {
        //        copy = Message.CreateMessage(message.Version, message.Headers.Action, body);
        //        copy.Headers.CopyHeaderFrom(message, 0);
        //        copy.Properties.CopyProperties(message.Properties);
        //        //copy = Message.CreateMessage(message.Version, null, body);//19.08
        //    }
        //    catch (ArgumentNullException e)
        //    {
        //        throw e;
        //    }
        //    if (message.Headers.Action == null)
        //    {
        //        message.Headers.Action = body.LocalName;
        //        copy.Headers.CopyHeaderFrom(message, 0);///!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //        copy.Headers.Action = body.NamespaceURI + "/" + body.LocalName;

        //        copy.Properties.CopyProperties(message.Properties);
        //    }
        //    //return copy;
        //    return message;
        //}


        public string SelectOperation(ref System.ServiceModel.Channels.Message message)
        {

            MessageBuffer buffer = message.CreateBufferedCopy(Int32.MaxValue);
            Message message1 = buffer.CreateMessage();
            Message message2 = buffer.CreateMessage();
            XmlDictionaryReader bodyReader = message2.GetReaderAtBodyContents();
            XmlQualifiedName lookupQName = new XmlQualifiedName(bodyReader.LocalName, bodyReader.NamespaceURI);

            //message = CreateMessageCopy(message2,bodyReader);           
            message = message2;
            //---------------------------------------
            foreach (MessageHeaderInfo mheadinfo in message.Headers)
            {
                if (mheadinfo.Name == "Security" || mheadinfo.Name == "security")
                {
                    Console.WriteLine("Security Header found!");

                    // - check if method needs security
                    //message.Action
                    //         else select operation
                    // - cut sec header
                    // - deserialize sec header
                    // - check if credentials are valid
                    // - select operation 

                    String msg = message.ToString();
                    int startindex = msg.IndexOf("<UsernameToken");// and if in lower case?
                    int endindex = msg.IndexOf("</UsernameToken");
                    String securityheaderstring = msg.Substring(startindex, (endindex - startindex + 16));
                    if (!securityheaderstring.EndsWith(">"))
                        securityheaderstring.Insert(securityheaderstring.Length, ">");

                    Security secheader = new Security();
                    secheader.Token = new UsernameToken();
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(UsernameToken));
                    securityheaderstring = "<?xml version='1.0' encoding='utf-8' ?>" + securityheaderstring;
                    int strtcutindexcreate;
                    if (securityheaderstring.Contains("Created"))
                    {
                        strtcutindexcreate = securityheaderstring.IndexOf("<Created");
                    }
                    else
                    {
                        strtcutindexcreate = securityheaderstring.IndexOf("<created");
                    }
                    int endcutindexcreate = securityheaderstring.IndexOf(">", strtcutindexcreate);
                    securityheaderstring = securityheaderstring.Remove(strtcutindexcreate + 9, (endcutindexcreate - strtcutindexcreate - 9));
                    using (Stream ms = new MemoryStream(Encoding.UTF8.GetBytes(securityheaderstring)))
                    {
                        try
                        {
                            secheader.Token = (UsernameToken)xmlSerializer.Deserialize(ms);
                            CheckPasswordDigest checkpass = new CheckPasswordDigest();
                            checkpass.Created = secheader.Token.Created;
                            checkpass.Name = secheader.Token.Username;
                            checkpass.Nonce = secheader.Token.Nonce;
                            checkpass.Pass = secheader.Token.Password;
                            if (checkpass.CheckPassword())
                            {
                                Console.WriteLine("Pass is valid!");
                                //choose operation
                                //get user type
                                //check if type allows to call desired method

                                if (dispatchDictionary.ContainsKey(lookupQName))
                                {
                                    //string tr = message.Headers.Action.ToString().Remove(0, message.Headers.Action.ToString().LastIndexOf("/") + 1);
                                    // call method if true
                                    message = message1;
                                    return dispatchDictionary[lookupQName];
                                    //return defaultOperationName;
                                }
                                else
                                {
                                    message = message1;
                                    //call default method if false
                                    return defaultOperationName;
                                }
                            }
                            else
                            {
                                message = message1;
                                return defaultOperationName;
                            }
                        }
                        catch (SerializationException g)
                        {
                            Console.WriteLine("Не могу десериализовать файл конфигурации; " + g.Message);
                            return null;
                        }
                        finally
                        {
                            ms.Close();
                        }
                    }
                }
            }
            return defaultOperationName;
        }      
    }

    public class Security
    {
        private UsernameToken _token;

        public UsernameToken Token
        {
            get
            {
                return this._token;
            }
            set
            {
                this._token = value;
            }
        }

    }

    public class UsernameToken
    {
        private string _name;
        private string _pass;
        private string _nonce;
        private string _created;// = "1111111111111111";//"2015-05-08T10:41:15.074Z";

        public string Username
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public string Password
        {
            get
            {
                return this._pass;
            }
            set
            {
                this._pass = value;
            }
        }

        public string Nonce
        {
            get
            {
                return this._nonce;
            }
            set
            {
                this._nonce = value;
            }
        }

        public string Created
        {
            get
            {
                return this._created;
            }
            set
            {
                this._created = value;
            }
        }
    }

    public class CheckPasswordDigest
    {
        public string Pass;// = "admin";
        public string Name;// = "admin";
        public string Nonce;// = "aN6VJLJZfkCkkjpXT6GbX1UAAAAAAA==";
        public string Created;// = "2015-05-13T13:44:47.631Z";
        public string KnownPass = "admin";

        public bool CheckPassword()
        {
            byte[] bytearrnonce = System.Convert.FromBase64String(Nonce);
            byte[] bytearrcreated = Encoding.UTF8.GetBytes(Created);
            byte[] bytearrpass = Encoding.UTF8.GetBytes(KnownPass);

            byte[] barr = new byte[bytearrnonce.Length + bytearrcreated.Length + bytearrpass.Length];
            for (int r = 0; r < bytearrnonce.Length; r++)
            {
                barr[r] = bytearrnonce[r];
            }
            for (int t = 0; t < bytearrcreated.Length; t++)
            {
                barr[bytearrnonce.Length + t] = bytearrcreated[t];
            }
            for (int y = 0; y < bytearrpass.Length; y++)
            {
                barr[bytearrnonce.Length + bytearrcreated.Length + y] = bytearrpass[y];
            }

            string tmpstring = System.Convert.ToBase64String(SHA1.Create().ComputeHash(barr));
            Console.WriteLine(tmpstring);
            if (tmpstring == Pass)
                return true;
            else return false;
            //return tmpstring;
            //wRHUFc1slwB8LaMm7QJnLaiD4cI=
        }
    }

}

