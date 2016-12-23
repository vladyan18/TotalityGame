﻿using System;
using Totality.CommonClasses;
using Totality.Handlers.News;
using Totality.Model;
using Totality.Model.Interfaces;

namespace Totality.Handlers.Main
{
    public class MinInnerHandler : AbstractHandler, IMinisteryHandler
    {
        private enum Orders { SuppressRiot, Repressions, EndRepressions, LvlUp }

        public MinInnerHandler(NewsHandler newsHandler, IDataLayer dataLayer, ILogger logger) : base(newsHandler, dataLayer, logger)
        {
        }

        public bool ProcessOrder(Order order)
        {
            switch (order.OrderNum)
            {
                case (int)Orders.SuppressRiot: return SuppressRiot(order);

                case (int)Orders.Repressions: return Repressions(order);

                case (int)Orders.EndRepressions: return EndRepressions(order);

                case (int)Orders.LvlUp: return LvlUp(order);

                default: throw new ArgumentException("Order " + order + " not found in " + typeof(MinInnerHandler));
            }
        }

        private bool SuppressRiot(Order order)
        {
             Random _randomizer = new Random((DateTime.Today - new DateTime(1995, 1, 1)).Milliseconds);
            var c = _dataLayer.GetCountry(order.CountryName);
            if (c.Money < 500000)
            {
                _newsHandler.AddNews(order.CountryName, new Model.News(true) { text = "Не хватает денег на попытку подавления бунта." });
                return false;
            }
            c.Money -= 500000;
            _dataLayer.UpdateCountry(c);


            var mood = (double)_dataLayer.GetProperty(order.CountryName, "Mood");

            if (_randomizer.NextDouble() * 100 <= mood)
            {
                _dataLayer.SetProperty(order.CountryName, "IsRiot", false);
                _newsHandler.AddNews(order.CountryName, new Model.News(true) { text = "Бунт успешно подавлен!" });
                return true;
            }
            else
            {
                _newsHandler.AddNews(order.CountryName, new Model.News(true) { text = "Попытка подавления бунта провалилась." });
                return false;
            }
        }

        private bool Repressions(Order order)
        {
            _dataLayer.SetProperty(order.CountryName, "IsRepressed", true);
            _newsHandler.AddNews(order.CountryName, new Model.News(true) { text = "Начаты репрессии." });
            return true;
        }

        private bool EndRepressions(Order order)
        {
            _dataLayer.SetProperty(order.CountryName, "IsRepressed", false);
            _newsHandler.AddNews(order.CountryName, new Model.News(true) { text = "Прекращены репрессии." });
            return true;
        }

        private bool LvlUp(Order order)
        {
            var money = (long)_dataLayer.GetProperty(order.CountryName, "Money");
            var innerLvlUpCost = (long)_dataLayer.GetProperty(order.CountryName, "InnerLvlUpCost");

            if (money < innerLvlUpCost)
                return false;

            money -= innerLvlUpCost;
            _dataLayer.SetProperty(order.CountryName, "Money", money);
            innerLvlUpCost = (long)(innerLvlUpCost * Constants.InnerLvlUpCostRatio);
            _dataLayer.SetProperty(order.CountryName, "InnerLvlUpCost", innerLvlUpCost);

            var lvl = (int)_dataLayer.GetProperty(order.CountryName, "InnerLvl") + 1;
            _dataLayer.SetProperty(order.CountryName, "InnerLvl", lvl);

            _newsHandler.AddNews(order.CountryName, new Model.News(true) { text = "Повышена квалификация МВД." });
            return true;
        }
    }
}
