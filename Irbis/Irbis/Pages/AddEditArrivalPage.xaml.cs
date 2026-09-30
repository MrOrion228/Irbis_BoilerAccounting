using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;


namespace Irbis.Pages
{
    public partial class AddEditArrivalPage : Page
    {
        private Arrivals _current;
        private bool chekNew;

        public AddEditArrivalPage(Arrivals c)
        {
            InitializeComponent();
            LoadComboBoxes();
            if (c == null)
            {
                c = new Arrivals();
                chekNew = true;
            }
            else
            {
                chekNew = false;
            }
            DataContext = _current = c;

            if (!chekNew)
            {
                dpDate.SelectedDate = _current.Date;
                cmbNomenclature.SelectedValue = _current.NomenclatureID;
                txtQuantity.Text = _current.Quantity.ToString();
                txtPurchasePrice.Text = _current.PurchasePrice.ToString();
                cmbSupplier.SelectedValue = _current.SupplierID;
            }
        }

        private void LoadComboBoxes()
        {
            var rawData = (from n in Connect.context.Nomenclatures
                           join m in Connect.context.Models on n.ModelID equals m.ID
                           join at in Connect.context.AutomationTypes on n.AutomationTypeID equals at.ID
                           select new
                           {
                               n.ID,
                               ModelName = m.Name,
                               AutoName = at.Name
                           }).ToList(); 
            var nomenclatures = rawData.Select(x => new
            {
                x.ID,
                DisplayName = $"{x.ModelName} ({x.AutoName})"
            }).ToList();

            cmbNomenclature.DisplayMemberPath = "DisplayName";
            cmbNomenclature.SelectedValuePath = "ID";
            cmbNomenclature.ItemsSource = nomenclatures;

            cmbSupplier.ItemsSource = Connect.context.Suppliers.ToList();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            _current.Date = dpDate.SelectedDate ?? DateTime.Today;
            _current.NomenclatureID = (int)cmbNomenclature.SelectedValue;
            _current.Quantity = int.Parse(txtQuantity.Text);
            _current.PurchasePrice = decimal.Parse(txtPurchasePrice.Text);
            _current.SupplierID = (int)cmbSupplier.SelectedValue;

            if (chekNew)
                Connect.context.Arrivals.Add(_current);

            Connect.context.SaveChanges(); 
            MessageBox.Show("Сохранено.");
            Nav.MainFrame.GoBack();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e) => Nav.MainFrame.GoBack();
    }
}