using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;
using System.Data.Entity;
using System.Collections.Generic;

namespace Irbis.Pages
{
    public partial class CustomersPage : Page
    {
        private List<Customers> _customers;
        private int _currentIndex = 0;

        public CustomersPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            _customers = Connect.context.Customers.OrderBy(c => c.ID).ToList();
            UpdateUI();
        }

        private void UpdateUI()
        {
            dgCustomers.ItemsSource = _customers;
            if (_customers.Count == 0)
            {
                txtCurrentRecord.Text = "0";
                txtTotalRecords.Text = "0";
                return;
            }
            txtTotalRecords.Text = _customers.Count.ToString();
            txtCurrentRecord.Text = (_currentIndex + 1).ToString();
            // Синхронизируем выделение с текущим индексом
            dgCustomers.SelectedItem = _customers[_currentIndex];
            dgCustomers.ScrollIntoView(_customers[_currentIndex]);
        }

        // Навигация (кнопки)
        private void First_Click(object sender, RoutedEventArgs e)
        {
            if (_customers.Count == 0) return;
            _currentIndex = 0;
            UpdateUI();
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (_customers.Count == 0) return;
            if (_currentIndex > 0) _currentIndex--;
            UpdateUI();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (_customers.Count == 0) return;
            if (_currentIndex < _customers.Count - 1) _currentIndex++;
            UpdateUI();
        }

        private void Last_Click(object sender, RoutedEventArgs e)
        {
            if (_customers.Count == 0) return;
            _currentIndex = _customers.Count - 1;
            UpdateUI();
        }

        // CRUD операции (используем выделенную строку)
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            Nav.MainFrame.Navigate(new AddEditCustomersPage(null));
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgCustomers.SelectedItem as Customers;
            if (selected == null)
            {
                MessageBox.Show("Выберите покупателя для редактирования.");
                return;
            }
            Nav.MainFrame.Navigate(new AddEditCustomersPage(selected));
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgCustomers.SelectedItem as Customers;
            if (selected == null)
            {
                MessageBox.Show("Выберите покупателя для удаления.");
                return;
            }

            // Проверка наличия связанных продаж
            bool hasSales = Connect.context.Sales.Any(s => s.CustomerID == selected.ID);
            if (hasSales)
            {
                MessageBox.Show("Нельзя удалить покупателя, так как есть продажи, связанные с ним.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить покупателя '{selected.Name}'?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // Находим запись в контексте (на всякий случай)
                var toDelete = Connect.context.Customers.Find(selected.ID);
                if (toDelete != null)
                {
                    Connect.context.Customers.Remove(toDelete);
                    Connect.context.SaveChanges();
                    LoadData(); // перезагрузка списка
                }
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Nav.MainFrame.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null && App.CurrentUser.Role != "Admin")
            {
                AddBtn.Visibility = Visibility.Collapsed;
                DeleteBtn.Visibility = Visibility.Collapsed;
                // Кнопка "Изменить" находится внутри DataGridTemplateColumn, её нужно скрыть через шаблон, но проще сделать DataGrid только для чтения
                dgCustomers.IsReadOnly = true;
                // Если нужно скрыть столбец с кнопками, можно сделать так:
                var editColumn = dgCustomers.Columns.LastOrDefault();
                if (editColumn != null)
                    editColumn.Visibility = Visibility.Collapsed;
            }
            LoadData();   // вызов должен быть внутри метода
        }
    }
}