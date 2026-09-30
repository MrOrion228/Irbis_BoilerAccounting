using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;

namespace Irbis.Pages
{
    public partial class SalesPage : Page
    {
        private List<SaleDisplay> _fullList;      // исходные данные (без фильтров)
        private List<SaleDisplay> _displayList;   // данные после фильтрации
        private List<Sales> _sales;
        private int _currentIndex = 0;

        // Переменные для текущих значений фильтров
        private string _currentSearch = "";
        private int? _currentCustomerId = null;
        private DateTime? _currentStartDate = null;
        private DateTime? _currentEndDate = null;

        public SalesPage()
        {
            InitializeComponent();
            LoadComboBoxes();
            LoadData();
        }

        private void LoadComboBoxes()
        {
            cmbCustomer.ItemsSource = Connect.context.Customers.ToList();
            cmbCustomer.DisplayMemberPath = "Name";
            cmbCustomer.SelectedValuePath = "ID";
        }

        private void LoadData()
        {
            // 1. Загружаем полные данные без форматирования строк
            var rawData = (from s in Connect.context.Sales
                           join n in Connect.context.Nomenclatures on s.NomenclatureID equals n.ID
                           join m in Connect.context.Models on n.ModelID equals m.ID
                           join at in Connect.context.AutomationTypes on n.AutomationTypeID equals at.ID
                           join c in Connect.context.Customers on s.CustomerID equals c.ID
                           select new
                           {
                               s.ID,
                               s.Date,
                               ModelName = m.Name,
                               AutomationName = at.Name,
                               s.Quantity,
                               s.SalePrice,
                               CustomerName = c.Name,
                               s.Total
                           }).ToList();

            // 2. Формируем _fullList с вычисляемым полем Boiler
            _fullList = rawData.Select(x => new SaleDisplay
            {
                ID = x.ID,
                Date = x.Date,
                Boiler = $"{x.ModelName} ({x.AutomationName})",
                Quantity = x.Quantity,
                SalePrice = x.SalePrice,
                CustomerName = x.CustomerName,
                Total = x.Total
            }).ToList();

            _sales = Connect.context.Sales.OrderBy(s => s.ID).ToList();
            ResetFilters();
        }

        private void ApplyFilters()
        {
            if (_fullList == null) return;

            var filtered = _fullList.AsEnumerable();

            if (!string.IsNullOrEmpty(_currentSearch))
                filtered = filtered.Where(x => x.Boiler.Contains(_currentSearch) || x.CustomerName.Contains(_currentSearch));

            if (_currentCustomerId.HasValue)
            {
                var customerName = Connect.context.Customers.Find(_currentCustomerId.Value)?.Name;
                if (!string.IsNullOrEmpty(customerName))
                    filtered = filtered.Where(x => x.CustomerName == customerName);
            }

            if (_currentStartDate.HasValue)
                filtered = filtered.Where(x => x.Date >= _currentStartDate.Value);
            if (_currentEndDate.HasValue)
                filtered = filtered.Where(x => x.Date <= _currentEndDate.Value);

            _displayList = filtered.ToList();
            _currentIndex = 0;
            UpdateUI();
        }

        private void ResetFilters()
        {
            _currentSearch = "";
            _currentCustomerId = null;
            _currentStartDate = null;
            _currentEndDate = null;

            txtSearch.Text = "";
            cmbCustomer.SelectedItem = null;
            dpStart.SelectedDate = null;
            dpEnd.SelectedDate = null;

            _displayList = _fullList?.ToList() ?? new List<SaleDisplay>();
            _currentIndex = 0;
            UpdateUI();
        }

        private void UpdateUI()
        {
            dgSales.ItemsSource = _displayList;
            if (_displayList.Count == 0)
            {
                txtCurrentRecord.Text = "0";
                txtTotalRecords.Text = "0";
                dgSales.SelectedItem = null;
                return;
            }
            if (_currentIndex >= _displayList.Count) _currentIndex = _displayList.Count - 1;
            txtTotalRecords.Text = _displayList.Count.ToString();
            txtCurrentRecord.Text = (_currentIndex + 1).ToString();
            dgSales.SelectedItem = _displayList[_currentIndex];
            dgSales.ScrollIntoView(_displayList[_currentIndex]);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = txtSearch.Text.Trim();
            ApplyFilters();
        }
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentCustomerId = cmbCustomer.SelectedItem != null ? (int?)cmbCustomer.SelectedValue : null;
            _currentStartDate = dpStart.SelectedDate;
            _currentEndDate = dpEnd.SelectedDate;
            ApplyFilters();
        }

        private void ResetFiltersBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetFilters();
        }

        private void First_Click(object sender, RoutedEventArgs e) { if (_displayList.Count > 0) _currentIndex = 0; UpdateUI(); }
        private void Prev_Click(object sender, RoutedEventArgs e) { if (_currentIndex > 0) _currentIndex--; UpdateUI(); }
        private void Next_Click(object sender, RoutedEventArgs e) { if (_currentIndex < _displayList.Count - 1) _currentIndex++; UpdateUI(); }
        private void Last_Click(object sender, RoutedEventArgs e) { if (_displayList.Count > 0) _currentIndex = _displayList.Count - 1; UpdateUI(); }
        private void AddBtn_Click(object sender, RoutedEventArgs e) => Nav.MainFrame.Navigate(new AddEditSalePage(null));
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgSales.SelectedItem as SaleDisplay;
            if (selected == null)
            {
                MessageBox.Show("Выберите запись для редактирования.");
                return;
            }
            var sale = Connect.context.Sales.Find(selected.ID);
            Nav.MainFrame.Navigate(new AddEditSalePage(sale));
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgSales.SelectedItem as SaleDisplay;
            if (selected == null)
            {
                MessageBox.Show("Выберите запись для удаления.");
                return;
            }
            if (MessageBox.Show($"Удалить запись о продаже от {selected.Date.ToShortDateString()}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var toDelete = Connect.context.Sales.Find(selected.ID);
                if (toDelete != null)
                {
                    Connect.context.Sales.Remove(toDelete);
                    Connect.context.SaveChanges();
                    LoadData();
                }
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) => Nav.MainFrame.GoBack();
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null && App.CurrentUser.Role != "Admin")
            {
                AddBtn.Visibility = Visibility.Collapsed;
                DeleteBtn.Visibility = Visibility.Collapsed;
                // Кнопка "Изменить" находится внутри DataGridTemplateColumn, её нужно скрыть через шаблон, но проще сделать DataGrid только для чтения
                dgSales.IsReadOnly = true;
                var editColumn = dgSales.Columns.LastOrDefault();
                if (editColumn != null)
                    editColumn.Visibility = Visibility.Collapsed;
            }
            LoadData();   
        }
    }

    public class SaleDisplay
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Boiler { get; set; }
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public string CustomerName { get; set; }
        public decimal? Total { get; set; }
    }
}