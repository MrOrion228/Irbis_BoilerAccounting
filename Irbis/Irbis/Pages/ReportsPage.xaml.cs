using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;
using Excel = Microsoft.Office.Interop.Excel;

namespace Irbis.Pages
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Nav.MainFrame.GoBack();
        }

        private void ReportArrivals_Click(object sender, RoutedEventArgs e)
        {
            var data = (from a in Connect.context.Arrivals
                        join n in Connect.context.Nomenclatures on a.NomenclatureID equals n.ID
                        join m in Connect.context.Models on n.ModelID equals m.ID
                        join at in Connect.context.AutomationTypes on n.AutomationTypeID equals at.ID
                        join s in Connect.context.Suppliers on a.SupplierID equals s.ID
                        select new
                        {
                            a.Date,
                            Boiler = m.Name + " (" + at.Name + ")",
                            a.Quantity,
                            a.PurchasePrice,
                            Supplier = s.Name,
                            a.Total
                        }).ToList();

            ExportToExcel(data, "Отчёт по приходу");
        }

        private void ReportSales_Click(object sender, RoutedEventArgs e)
        {
            var data = (from s in Connect.context.Sales
                        join n in Connect.context.Nomenclatures on s.NomenclatureID equals n.ID
                        join m in Connect.context.Models on n.ModelID equals m.ID
                        join at in Connect.context.AutomationTypes on n.AutomationTypeID equals at.ID
                        join c in Connect.context.Customers on s.CustomerID equals c.ID
                        select new
                        {
                            s.Date,
                            Boiler = m.Name + " (" + at.Name + ")",
                            s.Quantity,
                            s.SalePrice,
                            Customer = c.Name,
                            s.Total
                        }).ToList();

            ExportToExcel(data, "Отчёт по продаже");
        }

        private void ReportStock_Click(object sender, RoutedEventArgs e)
        {            var nomenclatures = (from n in Connect.context.Nomenclatures
                                 join m in Connect.context.Models on n.ModelID equals m.ID
                                 join at in Connect.context.AutomationTypes on n.AutomationTypeID equals at.ID
                                 select new
                                 {
                                     n.ID,
                                     ModelName = m.Name,
                                     AutoName = at.Name
                                 }).ToList();

            var stockData = nomenclatures.Select(item =>
            {
                int totalArrivals = Connect.context.Arrivals
                                    .Where(a => a.NomenclatureID == item.ID)
                                    .Sum(a => (int?)a.Quantity) ?? 0;
                int totalSales = Connect.context.Sales
                                 .Where(s => s.NomenclatureID == item.ID)
                                 .Sum(s => (int?)s.Quantity) ?? 0;
                int balance = totalArrivals - totalSales;
                return new
                {
                    item.ModelName,
                    item.AutoName,
                    Balance = balance
                };
            }).Where(x => x.Balance != 0) 
              .ToList();

            ExportToExcel(stockData, "Остатки котлов");
        }

        private void ExportToExcel<T>(System.Collections.Generic.IEnumerable<T> data, string sheetName)
        {
            if (data == null || !data.Any())
            {
                MessageBox.Show("Нет данных для экспорта.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var excelApp = new Excel.Application();
            excelApp.Visible = true;             
            excelApp.SheetsInNewWorkbook = 1;

            Excel.Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
            worksheet.Name = sheetName;

            var props = typeof(T).GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                worksheet.Cells[1, i + 1] = props[i].Name;
                ((Excel.Range)worksheet.Cells[1, i + 1]).Font.Bold = true;
            }

            int row = 2;
            foreach (var item in data)
            {
                for (int i = 0; i < props.Length; i++)
                {
                    var value = props[i].GetValue(item);
                    worksheet.Cells[row, i + 1] = value?.ToString();
                }
                row++;
            }

            worksheet.Columns.AutoFit();
        }
    }
}