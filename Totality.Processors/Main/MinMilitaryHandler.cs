﻿using System;
using System.Collections.Generic;
using Totality.CommonClasses;
using Totality.Handlers.Nuke;
using Totality.Model;
using Totality.Model.Interfaces;

namespace Totality.Handlers.Main
{
    public class MinMilitaryHandler : AbstractHandler, IMinisteryHandler
    {
        private enum Orders { GeneralMobilization, Demobilization, IncreaseUranium, MakeNukes, MakeMissiles, NukeStrike, StartWar }
        private NukeHandler _nukeHandler;

        public MinMilitaryHandler(IDataLayer dataLayer, NukeHandler nukeHandler, ILogger logger) : base(dataLayer, logger)
        {
            _nukeHandler = nukeHandler;
        }

        public bool ProcessOrder(Order order)
        {
            switch (order.OrderNum)
            {
                case (int)Orders.GeneralMobilization: return Mobilize(order);

                case (int)Orders.Demobilization: return Demobilize(order);

                case (int)Orders.IncreaseUranium: return IncreaseUranium(order);

                case (int)Orders.MakeNukes: return MakeNukes(order);

                case (int)Orders.MakeMissiles: return MakeMissiles(order);

                case (int)Orders.NukeStrike: return NukeStrike(order);

                case (int)Orders.StartWar: return StartWar(order);

                default: throw new ArgumentException("Order " + order + " not found in " + typeof(MinMilitaryHandler));
            }
        }

        private bool Mobilize(Order order)
        {
            _dataLayer.SetProperty(order.CountryName, "IsMobilized", true);
            return true;
        }

        private bool Demobilize(Order order)
        {
            _dataLayer.SetProperty(order.CountryName, "IsMobilized", false);
            return true;
        }

        private bool IncreaseUranium(Order order)
        {
            var money = (long)_dataLayer.GetProperty(order.CountryName, "Money");
            var upgradeCost = (long)_dataLayer.GetProperty(order.CountryName, "ProductionUpgradeCost");

            if (money < upgradeCost)
                return false;

            money -= upgradeCost;
            _dataLayer.SetProperty(order.CountryName, "Money", money);
            upgradeCost = (long)(Constants.UpgradeCostRate*upgradeCost);
            _dataLayer.SetProperty(order.CountryName, "ProductionUpgradeCost", upgradeCost);

            var uraniumProduction = (double)_dataLayer.GetProperty(order.CountryName, "ProdUranus");
            uraniumProduction += Constants.ProductionUpgrade;
            _dataLayer.SetProperty(order.CountryName, "ProdUranus", uraniumProduction);
            return true;
        }

        private bool MakeNukes(Order order)
        {
            var money = (long)_dataLayer.GetProperty(order.CountryName, "Money");

            if ( money < Constants.NukeCost * order.Count)
                return false;

            money -= Constants.NukeCost * order.Count;
            _dataLayer.SetProperty(order.CountryName, "Money", money);


            var nukes = (int)_dataLayer.GetProperty(order.CountryName, "NukesCount");
            nukes += (int)order.Count;
            _dataLayer.SetProperty(order.CountryName, "NukesCount", nukes);
            return true;
        }

        private bool MakeMissiles(Order order)
        {
            var money = (long)_dataLayer.GetProperty(order.CountryName, "Money");

            if (money < Constants.MissileCost * order.Count)
                return false;

            money -= Constants.MissileCost * order.Count;
            _dataLayer.SetProperty(order.CountryName, "Money", money);


            var missiles = (int)_dataLayer.GetProperty(order.CountryName, "MissilesCount");
            missiles += (int)order.Count;
            _dataLayer.SetProperty(order.CountryName, "MissilesCount", missiles);
            return true;
        }

        private bool NukeStrike(Order order)
        {
            var nukes = (int)_dataLayer.GetProperty(order.CountryName, "NukesCount");

            if (nukes < order.Count)
                return false;

            nukes -= (int)order.Count;
            _dataLayer.SetProperty(order.CountryName, "NukesCount", nukes);

            _nukeHandler.AddRocket(new NukeRocket(order.CountryName, order.TargetCountryName, (int)order.Count));

            return true;
        }

        private bool StartWar(Order order)
        {
            var warList = (List<string>)_dataLayer.GetProperty(order.CountryName, "WarList");
            warList.Add(order.TargetCountryName);
            _dataLayer.SetProperty(order.CountryName, "WarList", warList);

            var targetWarList = (List<string>)_dataLayer.GetProperty(order.TargetCountryName, "WarList");
            warList.Add(order.CountryName);
            _dataLayer.SetProperty(order.TargetCountryName, "WarList", warList);

            return true;
        }

    }
}