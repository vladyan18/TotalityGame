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

namespace Totality.Client.ClientComponents.Dialogs.Finance
{
    /// <summary>
    /// Логика взаимодействия для NukeStrikeDialog.xaml
    /// </summary>
    public partial class TaxesDialog : AbstractDialog, Dialog
    {
        private enum Orders { ChangeTaxes, PurchaseCurrency, SellCurrency, CurrencyInfusion };
        public delegate void ReceiveOrder(object sender, Order order, string text, long price);
        ReceiveOrder _receiveOrder;

        public TaxesDialog(ReceiveOrder receiveOrder)
        {
            _receiveOrder = receiveOrder;
            InitializeComponent();
            doubleUpDown.Value = CountryData.TaxesLvl / 100.0;
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            Order order = new Order(CountryData.Name);
            order.Ministery = (short)Mins.Finance;
            order.OrderNum = (short)Orders.ChangeTaxes;
            order.Value = (short)(doubleUpDown.Value*100);
            _receiveOrder(this, order, "Изменение уровня налогов: " + order.Value, order.Count);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            _receiveOrder(this, null, null, 0);
        }
    }
}
