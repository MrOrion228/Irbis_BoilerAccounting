using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;
using System.Data.Entity;

namespace Irbis.Pages
{
    public partial class AddEditNomenclaturePage : Page
    {
        private Nomenclatures _current;
        private bool _isNew;

        public AddEditNomenclaturePage(Nomenclatures item)
        {
            InitializeComponent();
            LoadComboBoxes(); // Загружаем списки и настраиваем ComboBox

            if (item == null)
            {
                _current = new Nomenclatures();
                _isNew = true;
                // Для новой записи можно установить значения по умолчанию, если нужно
            }
            else
            {
                _current = item;
                _isNew = false;
                // Заполняем форму после загрузки ComboBox
                FillForm();
            }
        }

        private void LoadComboBoxes()
        {
            // Загружаем данные
            var models = Connect.context.Models.ToList();
            var autoTypes = Connect.context.AutomationTypes.ToList();

            // Настраиваем первый ComboBox
            cmbModel.ItemsSource = models;
            cmbModel.DisplayMemberPath = "Name";
            cmbModel.SelectedValuePath = "ID";

            // Настраиваем второй ComboBox
            cmbAutomation.ItemsSource = autoTypes;
            cmbAutomation.DisplayMemberPath = "Name";
            cmbAutomation.SelectedValuePath = "ID";

            // Если данных нет, можно вывести предупреждение
            if (models.Count == 0) MessageBox.Show("Нет данных в таблице Models");
            if (autoTypes.Count == 0) MessageBox.Show("Нет данных в таблице AutomationTypes");
        }

        private void FillForm()
        {
            // Устанавливаем выбранные значения
            cmbModel.SelectedValue = _current.ModelID;
            cmbAutomation.SelectedValue = _current.AutomationTypeID;
            txtPrice.Text = _current.Price.ToString(CultureInfo.InvariantCulture);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Валидация выбора
            if (cmbModel.SelectedValue == null || cmbAutomation.SelectedValue == null)
            {
                MessageBox.Show("Выберите модель и автоматику.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Парсинг цены
            string priceText = txtPrice.Text.Trim();
            if (string.IsNullOrEmpty(priceText))
            {
                MessageBox.Show("Введите цену.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                if (!decimal.TryParse(priceText, out price))
                {
                    MessageBox.Show("Цена должна быть числом (используйте точку или запятую).", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (price < 0)
            {
                MessageBox.Show("Цена не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int modelId = (int)cmbModel.SelectedValue;
            int autoId = (int)cmbAutomation.SelectedValue;

            // Проверка уникальности (если новые значения отличаются)
            if (_isNew || _current.ModelID != modelId || _current.AutomationTypeID != autoId)
            {
                bool exists = Connect.context.Nomenclatures.Any(n => n.ModelID == modelId && n.AutomationTypeID == autoId);
                if (exists)
                {
                    MessageBox.Show("Такая комбинация модели и автоматики уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            // Обновляем объект
            _current.ModelID = modelId;
            _current.AutomationTypeID = autoId;
            _current.Price = price;

            // Сохраняем
            if (_isNew)
                Connect.context.Nomenclatures.Add(_current);
            Connect.context.SaveChanges();
            MessageBox.Show("Сохранено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Nav.MainFrame.GoBack();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) => Nav.MainFrame.GoBack();
    }
}