using System;
using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;

namespace Irbis.Pages
{
    public partial class AddEditSupplierPage : Page
    {
        private Suppliers _currentSupplier;
        private bool _isNew;

        public AddEditSupplierPage(Suppliers supplier)
        {
            InitializeComponent();
            if (supplier == null)
            {
                _currentSupplier = new Suppliers();
                _isNew = true;
            }
            else
            {
                _currentSupplier = supplier;
                _isNew = false;
                txtName.Text = supplier.Name;
                txtContact.Text = supplier.Contact;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование.");
                return;
            }
            _currentSupplier.Name = txtName.Text.Trim();
            _currentSupplier.Contact = txtContact.Text.Trim();

            if (_isNew)
                Connect.context.Suppliers.Add(_currentSupplier);
            Connect.context.SaveChanges();
            MessageBox.Show("Сохранено.");
            Nav.MainFrame.GoBack();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) => Nav.MainFrame.GoBack();
    }
}