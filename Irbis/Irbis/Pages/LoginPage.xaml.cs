using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Irbis.AppData;

namespace Irbis.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {

            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            var user = Connect.context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user != null)
            {
                App.CurrentUser = user;
                Nav.MainFrame.Navigate(new MainPage());
            }
            else
            {
                lblError.Text = "Неверный логин или пароль.";
            }
        }
    }
}