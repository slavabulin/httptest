
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
using System.Collections.ObjectModel;


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
        SecurityOperationBehavoirAttribute("Add", "http://Microsoft.Samples.Http", 3)]
        double Add(double n1, double n2);
        [OperationContract(ReplyAction = "*"),
        SecurityOperationBehavoirAttribute("Subtract", "http://Microsoft.Samples.Http", 3)]
        double Subtract(double n1, double n2);
        [OperationContract(ReplyAction = "*"),
        SecurityOperationBehavoirAttribute("Multiply", "http://Microsoft.Samples.Http", 1)]
        double Multiply(double n1, double n2);

        [OperationContract(ReplyAction = "*"),
        SecurityOperationBehavoirAttribute("Divide", "http://Microsoft.Samples.Http", 3)]
        double Divide(double n1, double n2);

        [OperationContract(ReplyAction = "*"),
        SecurityOperationBehavoirAttribute("GetScopes", "http://www.onvif.org/ver10/device/wsdl", 0)]
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
                serviceHost.Open();
                Console.WriteLine("Service started at {0}", serviceHost.BaseAddresses[0]);
                Console.WriteLine("Press ENTER to terminate service.");
                Console.ReadLine();
            }            
        }
    }

   
 
}

