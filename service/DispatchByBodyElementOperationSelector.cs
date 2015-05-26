//  Copyright (c) Microsoft Corporation.  All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace Microsoft.Samples.Http
{
    class DispatchByBodyElementOperationSelector : IDispatchOperationSelector
    {
        Dictionary<XmlQualifiedName, string> dispatchDictionary;
        string defaultOperationName;

        public DispatchByBodyElementOperationSelector(Dictionary<XmlQualifiedName, string> dispatchDictionary, string defaultOperationName)
        {
            this.dispatchDictionary = dispatchDictionary;
            this.defaultOperationName = defaultOperationName;
        }

        #region IDispatchOperationSelector Members

        private Message CreateMessageCopy(Message message, XmlDictionaryReader body)
        {
            Message copy;

            MessageBuffer buffer = message.CreateBufferedCopy(Int32.MaxValue);
            
            Message message1 = buffer.CreateMessage();// using
            Message message2 = buffer.CreateMessage();// using

            if (message1.Headers.Action == null)
            {
                message1.Headers.Action = body.NamespaceURI + "/" + body.LocalName;
            }
            MessageBuffer buffer1 = message1.CreateBufferedCopy(Int32.MaxValue);
            copy = buffer1.CreateMessage();
            //try
            //{
            //    copy = Message.CreateMessage(message.Version, message.Headers.Action, body);
            //    //copy = Message.CreateMessage(message.Version, null, body);//19.08
            //}
            //catch (ArgumentNullException e)
            //{
            //    throw e;
            //}
            //if (message.Headers.Action == null)
            //{
            //    message.Headers.Action = body.LocalName;
            //    copy.Headers.CopyHeaderFrom(message, 0);///!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                
            //    copy.Headers.Action = body.NamespaceURI + "/" + body.LocalName;

            //    copy.Properties.CopyProperties(message.Properties);
            //}
            message = copy;
            return copy;
        }



        public string SelectOperation(ref System.ServiceModel.Channels.Message message)
        {
            //XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents();
            //XmlQualifiedName lookupQName = new XmlQualifiedName(bodyReader.LocalName, bodyReader.NamespaceURI);
            
            MessageBuffer msgbuffer = message.CreateBufferedCopy(Int32.MaxValue);
            Message messagecopy1 = msgbuffer.CreateMessage();
            Message messagecopy2 = msgbuffer.CreateMessage();
            XmlDictionaryReader bodyReader = messagecopy2.GetReaderAtBodyContents();
            XmlQualifiedName lookupQName = new XmlQualifiedName(bodyReader.LocalName, bodyReader.NamespaceURI);

            //message = CreateMessageCopy(message, bodyReader);
            message = CreateMessageCopy(messagecopy1, bodyReader);
            if (dispatchDictionary.ContainsKey(lookupQName))
            {
                return dispatchDictionary[lookupQName];
            }
            else
            {
                return defaultOperationName;
            }
        }

        #endregion
    }
}