using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;

namespace Irbis.Pages
{
    public partial class StockBalancePage : Page
    {
        public StockBalancePage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var nomenclatures = (from n in Connect.context.Nomenclatures
                                 join m in Connect.context.Models on n.ModelID equals m.ID
                                 join at in Connect.context.AutomationTypes on n.AutomationTypeID equals at.ID
                                 select new
                                 {
                                     n.ID,
                                     ModelName = m.Name,
                                     AutomationName = at.Name
                                 }).ToList();
            var balanceList = new List<StockItem>();

            foreach (var item in nomenclatures)
            {
                int totalArrivals = Connect.context.Arrivals
                                        .Where(a => a.NomenclatureID == item.ID)
                                        .Sum(a => (int?)a.Quantity) ?? 0;

                int totalSales = Connect.context.Sales
                                     .Where(s => s.NomenclatureID == item.ID)
                                     .Sum(s => (int?)s.Quantity) ?? 0;

                int balance = totalArrivals - totalSales;

                balanceList.Add(new StockItem
                {
                    ModelName = item.ModelName,
                    AutomationType = item.AutomationName,
                    Balance = balance
                });
            }
            dgBalance.ItemsSource = balanceList;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Nav.MainFrame.GoBack();
        }
    }
    public class StockItem
    {
        public string ModelName { get; set; }
        public string AutomationType { get; set; }
        public int Balance { get; set; }
    }
}