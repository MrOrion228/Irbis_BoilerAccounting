using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;

namespace Irbis.Pages
{
    public partial class AddEditSalePage : Page
    {
        private Sales _current;
        private bool _isNew;

        public AddEditSalePage(Sales sale)
        {
            InitializeComponent();
            LoadComboBoxes();
            if (sale == null)
            {
                _current = new Sales();
                _isNew = true;
                dpDate.SelectedDate = DateTime.Today;
            }
            else
            {
                _current = sale;
                _isNew = false;
                dpDate.SelectedDate = _current.Date;
            }
            DataContext = _current;
        }

        private void LoadComboBoxes()
        {
            // 1. Загружаем данные без форматирования
            var rawData = (from n in Connect.context.Nomenclatures
                           join m in Connect.context.Models on n.ModelID equals m.ID
                           join at in Connect.context.AutomationTypes on n.AutomationTypeID equals at.ID
                           select new
                           {
                               n.ID,
                               ModelName = m.Name,
                               AutoName = at.Name
                           }).ToList();  // выполняется на сервере, затем в памяти

            // 2. Формируем отображаемые строки в памяти
            var nomenclatures = rawData.Select(x => new
            {
                x.ID,
                DisplayName = $"{x.ModelName} ({x.AutoName})"
            }).ToList();

            cmbNomenclature.DisplayMemberPath = "DisplayName";
            cmbNomenclature.SelectedValuePath = "ID";
            cmbNomenclature.ItemsSource = nomenclatures;

            // 3. Поставщики не требуют форматирования, можно напрямую
            cmbCustomer.ItemsSource = Connect.context.Suppliers.ToList();
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_current.NomenclatureID == 0) { MessageBox.Show("Выберите котёл."); return; }
            if (_current.Quantity <= 0) { MessageBox.Show("Количество должно быть положительным."); return; }
            if (_current.SalePrice <= 0) { MessageBox.Show("Цена продажи должна быть положительной."); return; }
            if (_current.CustomerID == 0) { MessageBox.Show("Выберите покупателя."); return; }

            _current.Date = dpDate.SelectedDate ?? DateTime.Today;

            try
            {
                if (_isNew)
                    Connect.context.Sales.Add(_current);
                // При редактировании объект уже отслеживается – ничего дополнительно не требуется
                Connect.context.SaveChanges();
                Nav.MainFrame.GoBack();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) => Nav.MainFrame.GoBack();
    }
}