﻿using System;
using System.Collections.Generic;
using Totality.CommonClasses;
using Totality.Handlers.News;
using Totality.Model;
using Totality.Model.Interfaces;

namespace Totality.Handlers.Main
{
    public class MinFinanceHandler : AbstractHandler, IMinisteryHandler
    {
        private enum Orders { ChangeTaxes , PurchaseCurrency, SellCurrency, CurrencyInfusion }

        public MinFinanceHandler(NewsHandler newsHandler, IDataLayer dataLayer, ILogger logger) : base(newsHandler, dataLayer, logger)
        {
        }

        public OrderResult ProcessOrder(Order order)
        {
            switch (order.OrderNum)
            {
                case (int)Orders.ChangeTaxes: return ChangeTaxes(order);

                case (int)Orders.PurchaseCurrency: return PurchaseCurrency(order);

                case (int)Orders.SellCurrency: return SellCurrency(order);

                case (int)Orders.CurrencyInfusion: return CurrencyInfusion(order);

                default: throw new ArgumentException("Order " + order + " not found in " + typeof(MinPremierHandler));
            }
        }

        private OrderResult ChangeTaxes(Order order)
        {
            if (order.Value > 100 || order.Value < 0)
                return new OrderResult(order.CountryName, "Изменение уровня налогов", false, 0);

            _dataLayer.SetProperty(order.CountryName, "TaxesLvl", order.Value);
            _newsHandler.AddNews(order.CountryName, new Model.News(true) { text = "Изменен уровень налогов: " + order.Value + "%" });
            return new OrderResult(order.CountryName, "Изменение уровня налогов: " + order.Value + "%", true, 0); ;
        }

        private OrderResult PurchaseCurrency(Order order)
        {
            var ourDemand = (long)_dataLayer.GetProperty(order.CountryName, "NationalCurrencyDemand");
            var theirDemand = (long)_dataLayer.GetProperty(order.TargetCountryName, "NationalCurrencyDemand");

            var ourQuontityOnStock = (long)_dataLayer.GetCurrencyOnStock(order.CountryName);
            var theirQuontityOnStock = (long)_dataLayer.GetCurrencyOnStock(order.TargetCountryName);

            var ourIndPower = (double)_dataLayer.GetProperty(order.CountryName, "FinalHeavyIndustry") + (double)_dataLayer.GetProperty(order.CountryName, "FinalLightIndustry");
            var theirIndPower = (double)_dataLayer.GetProperty(order.TargetCountryName, "FinalHeavyIndustry") + (double)_dataLayer.GetProperty(order.TargetCountryName, "FinalLightIndustry");

            var money = (long)_dataLayer.GetProperty(order.CountryName, "Money");
            var exchangeCost = FinancialTools.GetExchangeCostHighAcc(order.Count,
                ourDemand, theirDemand,
                ourQuontityOnStock, theirQuontityOnStock,
                ourIndPower, theirIndPower);

            if (money < exchangeCost)
                return new OrderResult(order.CountryName, "Покупка валюты " + order.TargetCountryName, false, exchangeCost);

            money -= exchangeCost;
            _dataLayer.SetProperty(order.CountryName, "Money", money);

            ourQuontityOnStock += exchangeCost;
            _dataLayer.SetCurrencyOnStock(order.CountryName, ourQuontityOnStock);

            theirQuontityOnStock -= order.Count;
            _dataLayer.SetCurrencyOnStock(order.TargetCountryName, theirQuontityOnStock);

            var ourAccounts = (Dictionary<string, long>)_dataLayer.GetProperty(order.CountryName, "CurrencyAccounts");
            if (!ourAccounts.ContainsKey(order.TargetCountryName))
                ourAccounts.Add(order.TargetCountryName, order.Count);
            else
                ourAccounts[order.TargetCountryName] += order.Count;
            _dataLayer.SetProperty(order.CountryName, "CurrencyAccounts", ourAccounts);

            _newsHandler.AddNews(order.CountryName, new Model.News(true) { text = "Закуплена валюта страны " + order.TargetCountryName  + " в размере " + String.Format("{0:0,0}",order.Count) + "."});

            return new OrderResult(order.CountryName, "Покупка валюты " + order.TargetCountryName, true, exchangeCost);
        }

        private OrderResult SellCurrency(Order order)
        {
            var ourDemand = (long)_dataLayer.GetProperty(order.CountryName, "NationalCurrencyDemand");
            var theirDemand = (long)_dataLayer.GetProperty(order.TargetCountryName, "NationalCurrencyDemand");

            var ourQuontityOnStock = (long)_dataLayer.GetCurrencyOnStock(order.CountryName);
            var theirQuontityOnStock = (long)_dataLayer.GetCurrencyOnStock(order.TargetCountryName);

            var ourIndPower = (double)_dataLayer.GetProperty(order.CountryName, "FinalHeavyIndustry") + (double)_dataLayer.GetProperty(order.CountryName, "FinalLightIndustry");
            var theirIndPower = (double)_dataLayer.GetProperty(order.TargetCountryName, "FinalHeavyIndustry") + (double)_dataLayer.GetProperty(order.TargetCountryName, "FinalLightIndustry");

            var exchangeCost = (long)FinancialTools.GetMaximumPurchaseHighAcc(order.Count,
                theirDemand, ourDemand,
                theirQuontityOnStock, ourQuontityOnStock,
                theirIndPower, ourIndPower);
            var ourAccounts = (Dictionary<string, long>)_dataLayer.GetProperty(order.CountryName, "CurrencyAccounts");

            if (!ourAccounts.ContainsKey(order.TargetCountryName) ||  ourAccounts[order.TargetCountryName] < order.Count)
                return new OrderResult(order.CountryName, "Продажа валюты " + order.TargetCountryName, false, exchangeCost);

            ourAccounts[order.TargetCountryName] -= order.Count;
            _dataLayer.SetProperty(order.CountryName, "CurrencyAccounts", ourAccounts);

            var money = (long)_dataLayer.GetProperty(order.CountryName, "Money");
            money += exchangeCost;
            _dataLayer.SetProperty(order.CountryName, "Money", money);

            ourQuontityOnStock -= exchangeCost;
            _dataLayer.SetCurrencyOnStock(order.CountryName, ourQuontityOnStock);

            theirQuontityOnStock += order.Count;
            _dataLayer.SetCurrencyOnStock(order.TargetCountryName, theirQuontityOnStock);

            _newsHandler.AddNews(order.CountryName, new Model.News(true) { text = "Продана валюта страны " + order.TargetCountryName + " в размере " + String.Format("{0:0,0}", order.Count) + "." });
            return new OrderResult(order.CountryName, "Продажа валюты " + order.TargetCountryName, true, exchangeCost);
        }

        private OrderResult CurrencyInfusion(Order order)
        {
            var money = (long)_dataLayer.GetProperty(order.CountryName, "Money");

            if (money < 500000)
                return new OrderResult(order.CountryName, "Эмиссия валюты в объеме: " + order.Count.ToString("N0"), false, 500000);

            money -= 500000;
            _dataLayer.SetProperty(order.CountryName, "Money", money);

            var ourCurrencyOnStock = _dataLayer.GetCurrencyOnStock(order.CountryName);
            ourCurrencyOnStock += order.Count;
            _dataLayer.SetCurrencyOnStock(order.CountryName, ourCurrencyOnStock);

            _newsHandler.AddNews(order.CountryName, new Model.News(true) { text = "Произведена эмиссия валюты в объеме "+ order.Count.ToString("N0") +"." });
            return new OrderResult(order.CountryName, "Эмиссия валюты в объеме: " + order.Count.ToString("N0"), true, 500000);
        }
    }
}
