using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;

namespace Irbis.Pages
{
    public partial class AddEditCustomersPage : Page
    {
        private Customers _currentCustomer;
        private bool _isNew;

        public AddEditCustomersPage(Customers customer)
        {
            InitializeComponent();
            if (customer == null)
            {
                _currentCustomer = new Customers();
                _isNew = true;
            }
            else
            {
                _currentCustomer = customer;
                _isNew = false;
                txtName.Text = customer.Name;
                txtContact.Text = customer.Contact;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование покупателя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentCustomer.Name = txtName.Text.Trim();
            _currentCustomer.Contact = string.IsNullOrWhiteSpace(txtContact.Text) ? null : txtContact.Text.Trim();

            try
            {
                if (_isNew)
                    Connect.context.Customers.Add(_currentCustomer);
                Connect.context.SaveChanges();
                MessageBox.Show("Сохранено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Nav.MainFrame.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Nav.MainFrame.GoBack();
        }
    }
}