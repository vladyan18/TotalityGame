﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Totality.CommonClasses;
using Totality.Model;

namespace Totality.Client.ClientComponents.Dialogs.Military
{
    /// <summary>
    /// Логика взаимодействия для MobilizeDialog.xaml
    /// </summary>
    public partial class MobilizeDialog : AbstractDialog, Dialog
    {
        private enum Orders { GeneralMobilization, Demobilization, IncreaseUranium, MakeNukes, MakeMissiles, NukeStrike, StartWar }
        public delegate void ReceiveOrder(object sender, Order order, string text, long price);
        private ReceiveOrder _receiveOrder;

        public MobilizeDialog(ReceiveOrder receiveOrder)
        {
            _receiveOrder = receiveOrder;
            InitializeComponent();

            if (CountryData.IsMobilized)
            {
                label.Content = "Отменить мобилизацию?";
            }
            else
            {
                label.Content = "Мобилизовать страну?";
            }
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            Order order = new Order(CountryData.Name);
            order.Ministery = (short)Mins.Military;
            if (CountryData.IsMobilized)
            {
                order.OrderNum = (short)Orders.Demobilization;
                _receiveOrder(this, order, "Демобилизация", 0);
            }
            else
            {
                order.OrderNum = (short)Orders.GeneralMobilization;
                _receiveOrder(this, order, "Всеобщая мобилизация", 0);
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            _receiveOrder(this, null, null, 0);
        }
    }
}
