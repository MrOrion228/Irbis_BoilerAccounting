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
    /// Логика взаимодействия для NomenclaturesPage.xaml
    /// </summary>
    public partial class NomenclaturesPage : Page
    {
        private List<NomenclatureDisplay> _displayList;
        private List<Nomenclatures> _nomenclatures;
        private int _currentIndex = 0;

        public NomenclaturesPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Загружаем номенклатуру с подстановкой имён модели и автоматики
            var query = from n in Connect.context.Nomenclatures
                        join m in Connect.context.Models on n.ModelID equals m.ID
                        join a in Connect.context.AutomationTypes on n.AutomationTypeID equals a.ID
                        select new NomenclatureDisplay
                        {
                            ID = n.ID,
                            ModelName = m.Name,
                            AutomationName = a.Name,
                            Price = n.Price
                        };
            _displayList = query.ToList();
            _nomenclatures = Connect.context.Nomenclatures.ToList();
            UpdateUI();
        }

        private void UpdateUI()
        {
            dgNomenclatures.ItemsSource = _displayList;
            if (_displayList.Count == 0)
            {
                txtCurrentRecord.Text = "0";
                txtTotalRecords.Text = "0";
                return;
            }
            txtTotalRecords.Text = _displayList.Count.ToString();
            txtCurrentRecord.Text = (_currentIndex + 1).ToString();
            dgNomenclatures.SelectedItem = _displayList[_currentIndex];
            dgNomenclatures.ScrollIntoView(_displayList[_currentIndex]);
        }

        // Навигация
        private void First_Click(object sender, RoutedEventArgs e)
        {
            if (_displayList.Count == 0) return;
            _currentIndex = 0;
            UpdateUI();
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (_displayList.Count == 0) return;
            if (_currentIndex > 0) _currentIndex--;
            UpdateUI();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (_displayList.Count == 0) return;
            if (_currentIndex < _displayList.Count - 1) _currentIndex++;
            UpdateUI();
        }

        private void Last_Click(object sender, RoutedEventArgs e)
        {
            if (_displayList.Count == 0) return;
            _currentIndex = _displayList.Count - 1;
            UpdateUI();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            Nav.MainFrame.Navigate(new AddEditNomenclaturePage(null));
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var display = button?.Tag as NomenclatureDisplay;
            if (display == null) return;
            var nomenclature = Connect.context.Nomenclatures.Find(display.ID);
            Nav.MainFrame.Navigate(new AddEditNomenclaturePage(nomenclature));
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранную строку из DataGrid (через SelectedItem)
            var selectedDisplay = dgNomenclatures.SelectedItem as NomenclatureDisplay;
            if (selectedDisplay == null)
            {
                MessageBox.Show("Выберите запись для удаления.");
                return;
            }

            // Находим сущность в контексте по ID
            var toDelete = Connect.context.Nomenclatures.Find(selectedDisplay.ID);
            if (toDelete == null)
            {
                MessageBox.Show("Запись не найдена.");
                return;
            }

            // Проверка связей
            bool inArrivals = Connect.context.Arrivals.Any(a => a.NomenclatureID == toDelete.ID);
            bool inSales = Connect.context.Sales.Any(s => s.NomenclatureID == toDelete.ID);
            if (inArrivals || inSales)
            {
                MessageBox.Show("Нельзя удалить номенклатуру, так как есть приход или продажа с ней.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Удалить номенклатуру?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Connect.context.Nomenclatures.Remove(toDelete);
                Connect.context.SaveChanges();
                LoadData(); // перезагружаем список
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
                dgNomenclatures.IsReadOnly = true;
                // Если нужно скрыть столбец с кнопками, можно сделать так:
                var editColumn = dgNomenclatures.Columns.LastOrDefault();
                if (editColumn != null)
                    editColumn.Visibility = Visibility.Collapsed;
            }
            LoadData();   // вызов должен быть внутри метода
        }
    }
    public class NomenclatureDisplay
    {
        public int ID { get; set; }
        public string ModelName { get; set; }
        public string AutomationName { get; set; }
        public decimal Price { get; set; }
    }
}
