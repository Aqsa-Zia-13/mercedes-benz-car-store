using System.Windows;
using System.Windows.Controls;
using Car.Database;
using Car.Models;
using Car.Views;

namespace Car
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            string username = TxtUsername.Text.Trim();
            string password = TxtPassword.Password;
            string role = (CmbRole.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";

            // --- Validations ---
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Please enter your username.");
                TxtUsername.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Please enter your password.");
                TxtPassword.Focus();
                return;
            }
            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters.");
                TxtPassword.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(role))
            {
                ShowError("Please select your role.");
                return;
            }

            // --- Attempt login ---
            try
            {
                User? user = UserRepository.Login(username, password, role);

                if (user == null)
                {
                    ShowError("Incorrect username, password, or role. Please try again.");
                    TxtPassword.Clear();
                    TxtPassword.Focus();
                    return;
                }

                // --- Route to correct dashboard ---
                Window dashboard = user.UserRole switch
                {
                    "SuperAdmin" => new SuperAdminDashboard(user),
                    "Admin" => new AdminDashboard(user),
                    "Manager" => new ManagerDashboard(user),
                    "Customer" => new CustomerDashboard(user),
                    _ => throw new Exception("Unknown role.")
                };

                dashboard.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Connection error: {ex.Message}");
            }
        }

        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            var signUpWindow = new SignUpWindow();
            signUpWindow.ShowDialog();
        }

        private void ShowError(string message)
        {
            TxtError.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            TxtError.Text = "";
            ErrorBorder.Visibility = Visibility.Collapsed;
        }
    }
}