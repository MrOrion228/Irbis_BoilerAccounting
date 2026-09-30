using Irbis.AppData;
using System;
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

namespace Irbis.Pages
{
    /// <summary>
    /// Логика взаимодействия для SuppliersPage.xaml
    /// </summary>
    public partial class SuppliersPage : Page
    {
        private List<Suppliers> _suppliers;
        private int _currentIndex = 0;

        public SuppliersPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            _suppliers = Connect.context.Suppliers.OrderBy(s => s.ID).ToList();
            UpdateUI();
        }

        private void UpdateUI()
        {
            dgSuppliers.ItemsSource = _suppliers;
            if (_suppliers.Count == 0) return;
            txtTotalRecords.Text = _suppliers.Count.ToString();
            txtCurrentRecord.Text = (_currentIndex + 1).ToString();
            dgSuppliers.SelectedItem = _suppliers[_currentIndex];
            dgSuppliers.ScrollIntoView(_suppliers[_currentIndex]);
        }

        // Навигация
        private void First_Click(object sender, RoutedEventArgs e) { _currentIndex = 0; UpdateUI(); }
        private void Prev_Click(object sender, RoutedEventArgs e) { if (_currentIndex > 0) _currentIndex--; UpdateUI(); }
        private void Next_Click(object sender, RoutedEventArgs e) { if (_currentIndex < _suppliers.Count - 1) _currentIndex++; UpdateUI(); }
        private void Last_Click(object sender, RoutedEventArgs e) { _currentIndex = _suppliers.Count - 1; UpdateUI(); }

        // CRUD – теперь по выделению
        private void AddBtn_Click(object sender, RoutedEventArgs e) => Nav.MainFrame.Navigate(new AddEditSupplierPage(null));
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgSuppliers.SelectedItem as Suppliers;
            if (selected == null) { MessageBox.Show("Выберите запись."); return; }
            Nav.MainFrame.Navigate(new AddEditSupplierPage(selected));
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgSuppliers.SelectedItem as Suppliers;
            if (selected == null) { MessageBox.Show("Выберите запись."); return; }
            if (MessageBox.Show($"Удалить поставщика '{selected.Name}'?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Connect.context.Suppliers.Remove(selected);
                Connect.context.SaveChanges();
                LoadData();
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
                dgSuppliers.IsReadOnly = true;
                // Если нужно скрыть столбец с кнопками, можно сделать так:
                var editColumn = dgSuppliers.Columns.LastOrDefault();
                if (editColumn != null)
                    editColumn.Visibility = Visibility.Collapsed;
            }
            LoadData();   // вызов должен быть внутри метода
        }
    }
}