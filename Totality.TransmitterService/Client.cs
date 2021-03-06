﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Totality.TransmitterService
{
    public class Client
    {
        public string Name { get; }
        public ICallbackService Transmitter { get; }
        public delegate void faultHandler(Client sender);
        public event faultHandler Fault;

        public Client(ICallbackService channel, string name)
        {
            Name = name;
            Transmitter = channel;
            IContextChannel channelHandle = OperationContext.Current.Channel;
            channelHandle.Faulted += ChannelHandle_Faulted;
        }

        private void ChannelHandle_Faulted(object sender, EventArgs e)
        {
            Fault.Invoke(this);
        }
    }
}
