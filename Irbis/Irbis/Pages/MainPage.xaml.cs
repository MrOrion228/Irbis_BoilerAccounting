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
using System.Windows.Shapes;

namespace Irbis.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
            public MainPage()
            {
                InitializeComponent();
            }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;
            if (App.CurrentUser.Role != "Admin")
            {
                NomenclaturesBtn.Visibility = Visibility.Collapsed;
                SuppliersBtn.Visibility = Visibility.Collapsed;
                CustomersBtn.Visibility = Visibility.Collapsed;
            }
        }
        private void NomenclaturesBtn_Click(object sender, RoutedEventArgs e)
            {
                Nav.MainFrame.Navigate(new NomenclaturesPage());
            }

            private void SuppliersBtn_Click(object sender, RoutedEventArgs e)
            {
                Nav.MainFrame.Navigate(new SuppliersPage());
            }

            private void CustomersBtn_Click(object sender, RoutedEventArgs e)
            {
                Nav.MainFrame.Navigate(new CustomersPage());
            }

            private void ArrivalsBtn_Click(object sender, RoutedEventArgs e)
            {
                Nav.MainFrame.Navigate(new ArrivalsPage());
            }

            private void SalesBtn_Click(object sender, RoutedEventArgs e)
            {
                Nav.MainFrame.Navigate(new SalesPage());
            }

            private void StockBalanceBtn_Click(object sender, RoutedEventArgs e)
            {
                Nav.MainFrame.Navigate(new StockBalancePage());
            }

            private void ReportsBtn_Click(object sender, RoutedEventArgs e)
            {
             Nav.MainFrame.Navigate(new ReportsPage());
            }

            private void ExitBtn_Click(object sender, RoutedEventArgs e)
            {
                Application.Current.Shutdown();
            }

        }

    }
