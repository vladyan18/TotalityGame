﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Totality.Model;
using Totality.Model.Interfaces;
using Totality.Model.Diplomatical;

namespace Totality.TransmitterService
{
    [ServiceContract(CallbackContract = typeof(ICallbackService))]
    public interface ITransmitterService : ITransmitter
    {
        [OperationContract]
        bool Register(string myName);

        [OperationContract]
        bool AddOrders(List<Order> orders, string name);

        [OperationContract]
        bool ShootDownNuke(string defender, Guid rocketId);

        [OperationContract]
        bool DipMsg(DipMsg msg);

        [OperationContract]
        Country GetCountryData(string name);

        [OperationContract]
        Dictionary<string, long> GetCurrencyStock();

        [OperationContract]
        Dictionary<string, long> GetCurrencyDemands();
    }

    public interface ICallbackService
    {
        [OperationContract(IsOneWay = true)]
        void InitializeNukeDialog();

        [OperationContract(IsOneWay = true)]
        void UpdateNukeDialog(List<NukeRocket> rockets);

        [OperationContract(IsOneWay = true)]
        void SendNews(List<News> newsList);

        [OperationContract(IsOneWay = true)]
        void UpdateClient(Country country);

        [OperationContract(IsOneWay = true)]
        void SendDip(DipMsg msg);

        [OperationContract(IsOneWay = true)]
        void SendContracts(List<DipContract> contracts);
    }
}
