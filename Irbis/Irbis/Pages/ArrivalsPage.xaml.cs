using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;
namespace Irbis.Pages
{
    public partial class ArrivalsPage : Page
    {
        private List<ArrivalDisplay> _fullList;    
        private List<ArrivalDisplay> _displayList;   
        private List<Arrivals> _arrivals;
        private int _currentIndex = 0;

        // Текущие значения фильтров (для дат и поставщика)
        private string _currentSearch = "";
        private int? _currentSupplierId = null;
        private DateTime? _currentStartDate = null;
        private DateTime? _currentEndDate = null;

        public ArrivalsPage()
        {
            InitializeComponent();
            LoadComboBoxes();
            LoadData();
        }

        private void LoadComboBoxes()
        {
            cmbSupplier.ItemsSource = Connect.context.Suppliers.ToList();
            cmbSupplier.DisplayMemberPath = "Name";
            cmbSupplier.SelectedValuePath = "ID";
        }

        private void LoadData()
        {
            // 1. Загружаем полные данные без форматирования строк
            var rawData = (from a in Connect.context.Arrivals
                           join n in Connect.context.Nomenclatures on a.NomenclatureID equals n.ID
                           join m in Connect.context.Models on n.ModelID equals m.ID
                           join at in Connect.context.AutomationTypes on n.AutomationTypeID equals at.ID
                           join s in Connect.context.Suppliers on a.SupplierID equals s.ID
                           select new
                           {
                               a.ID,
                               a.Date,
                               ModelName = m.Name,
                               AutomationName = at.Name,
                               a.Quantity,
                               a.PurchasePrice,
                               SupplierName = s.Name,
                               a.Total
                           }).ToList();

            // 2. Формируем _fullList с красивым названием котла
            _fullList = rawData.Select(x => new ArrivalDisplay
            {
                ID = x.ID,
                Date = x.Date,
                Boiler = $"{x.ModelName} ({x.AutomationName})",
                Quantity = x.Quantity,
                PurchasePrice = x.PurchasePrice,
                SupplierName = x.SupplierName,
                Total = x.Total
            }).ToList();

            _arrivals = Connect.context.Arrivals.OrderBy(a => a.ID).ToList();

            // 3. Сбрасываем фильтры
            ResetFilters();
        }

        // Применение всех фильтров (поиск, поставщик, даты) к _fullList
        private void ApplyFilters()
        {
            if (_fullList == null) return;

            var filtered = _fullList.AsEnumerable();

            // Поиск по тексту (мгновенный)
            if (!string.IsNullOrEmpty(_currentSearch))
                filtered = filtered.Where(x => x.Boiler.Contains(_currentSearch) || x.SupplierName.Contains(_currentSearch));

            // Фильтр по поставщику
            if (_currentSupplierId.HasValue)
            {
                var supplierName = Connect.context.Suppliers.Find(_currentSupplierId.Value)?.Name;
                if (!string.IsNullOrEmpty(supplierName))
                    filtered = filtered.Where(x => x.SupplierName == supplierName);
            }

            // Фильтр по датам
            if (_currentStartDate.HasValue)
                filtered = filtered.Where(x => x.Date >= _currentStartDate.Value);
            if (_currentEndDate.HasValue)
                filtered = filtered.Where(x => x.Date <= _currentEndDate.Value);

            _displayList = filtered.ToList();
            _currentIndex = 0;
            UpdateUI();
        }

        // Сброс всех фильтров (очистка переменных и элементов управления)
        private void ResetFilters()
        {
            _currentSearch = "";
            _currentSupplierId = null;
            _currentStartDate = null;
            _currentEndDate = null;

            // Очищаем UI
            txtSearch.Text = "";
            cmbSupplier.SelectedItem = null;
            dpStart.SelectedDate = null;
            dpEnd.SelectedDate = null;

            _displayList = _fullList?.ToList() ?? new List<ArrivalDisplay>();
            _currentIndex = 0;
            UpdateUI();
        }

        private void UpdateUI()
        {
            dgArrivals.ItemsSource = _displayList;
            if (_displayList.Count == 0)
            {
                txtCurrentRecord.Text = "0";
                txtTotalRecords.Text = "0";
                dgArrivals.SelectedItem = null;
                return;
            }
            if (_currentIndex >= _displayList.Count) _currentIndex = _displayList.Count - 1;
            txtTotalRecords.Text = _displayList.Count.ToString();
            txtCurrentRecord.Text = (_currentIndex + 1).ToString();
            dgArrivals.SelectedItem = _displayList[_currentIndex];
            dgArrivals.ScrollIntoView(_displayList[_currentIndex]);
        }

        // --- Обработчик мгновенного поиска ---
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSearch = txtSearch.Text.Trim();
            ApplyFilters();
        }

        // --- Обработчик кнопки "Применить" (для поставщика и дат) ---
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            _currentSupplierId = cmbSupplier.SelectedItem != null ? (int?)cmbSupplier.SelectedValue : null;
            _currentStartDate = dpStart.SelectedDate;
            _currentEndDate = dpEnd.SelectedDate;
            ApplyFilters();
        }

        // --- Обработчик кнопки "Сбросить" ---
        private void ResetFiltersBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetFilters();
        }

        // --- Навигация и CRUD (без изменений) ---
        private void First_Click(object sender, RoutedEventArgs e) { if (_displayList.Count > 0) _currentIndex = 0; UpdateUI(); }
        private void Prev_Click(object sender, RoutedEventArgs e) { if (_currentIndex > 0) _currentIndex--; UpdateUI(); }
        private void Next_Click(object sender, RoutedEventArgs e) { if (_currentIndex < _displayList.Count - 1) _currentIndex++; UpdateUI(); }
        private void Last_Click(object sender, RoutedEventArgs e) { if (_displayList.Count > 0) _currentIndex = _displayList.Count - 1; UpdateUI(); }

        private void AddBtn_Click(object sender, RoutedEventArgs e) => Nav.MainFrame.Navigate(new AddEditArrivalPage(null));
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgArrivals.SelectedItem as ArrivalDisplay;
            if (selected == null) { MessageBox.Show("Выберите запись."); return; }
            var arrival = Connect.context.Arrivals.Find(selected.ID);
            Nav.MainFrame.Navigate(new AddEditArrivalPage(arrival));
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgArrivals.SelectedItem as ArrivalDisplay;
            if (selected == null) { MessageBox.Show("Выберите запись."); return; }
            if (MessageBox.Show($"Удалить запись о приходе от {selected.Date.ToShortDateString()}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var toDelete = Connect.context.Arrivals.Find(selected.ID);
                if (toDelete != null)
                {
                    Connect.context.Arrivals.Remove(toDelete);
                    Connect.context.SaveChanges();
                    LoadData(); // перезагружаем все данные
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
                dgArrivals.IsReadOnly = true;
                // Если нужно скрыть столбец с кнопками, можно сделать так:
                var editColumn = dgArrivals.Columns.LastOrDefault();
                if (editColumn != null)
                    editColumn.Visibility = Visibility.Collapsed;
            }
            LoadData();   // вызов должен быть внутри метода
        }
    }

    public class ArrivalDisplay
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Boiler { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public string SupplierName { get; set; }
        public decimal? Total { get; set; }
    }
}