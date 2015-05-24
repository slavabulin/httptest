//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Microsoft.Samples.Http
{
    // Define a service contract.
    [ServiceContract(Namespace="http://Microsoft.Samples.Http")]
    public interface ICalculator
    {
        [OperationContract]
        double Add(double n1, double n2);
        [OperationContract]
        double Subtract(double n1, double n2);
        [OperationContract]
        double Multiply(double n1, double n2);
        [OperationContract]
        double Divide(double n1, double n2);
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
    }

    
    public class Service
    {
        public static void Main()
        {
            Uri baseAddress = new Uri("http://192.168.1.170:8000");
            using (ServiceHost serviceHost = new ServiceHost(typeof(CalculatorService), baseAddress))
            {
                SecurityBindingElement sbe;
                sbe.AllowInsecureTransport = true;
                WSHttpBinding binding = new WSHttpBinding(SecurityMode.Message);
                binding.Security.Message.ClientCredentialType =  MessageCredentialType.UserName;
                
                
                ServiceEndpoint DeviceServiceEndpoint = serviceHost.AddServiceEndpoint(typeof(ICalculator),
                    binding, 
                    "ServiceModelSamples/service.svc"
                    );
                //serviceHost.Credentials.UserNameAuthentication.UserNamePasswordValidationMode();

                ServiceAuthenticationManager serviceAuthenticationManager = new ServiceAuthenticationManager();
                //serviceAuthenticationManager.Authenticate();
                serviceHost.Authentication.ServiceAuthenticationManager = serviceAuthenticationManager;


                serviceHost.Open();
                // The service can now be accessed.
                Console.WriteLine("Service started at {0}", serviceHost.BaseAddresses[0]);
                Console.WriteLine("Press ENTER to terminate service.");
                Console.ReadLine();
            }

            
        }
    }
}

