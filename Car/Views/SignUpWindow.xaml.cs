using System.Text.RegularExpressions;
using System.Windows;
using Car.Database;

namespace Car.Views
{
    public partial class SignUpWindow : Window
    {
        public SignUpWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            HideMessages();

            // --- Read inputs ---
            string fullName = TxtFullName.Text.Trim();
            string username = TxtUsername.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string phone = TxtPhone.Text.Trim();
            string address = TxtAddress.Text.Trim();
            string password = TxtPassword.Password;
            string confirmPass = TxtConfirmPassword.Password;

            // --- Validations ---
            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowError("Full name is required.");
                TxtFullName.Focus();
                return;
            }
            if (fullName.Length < 3)
            {
                ShowError("Full name must be at least 3 characters.");
                TxtFullName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Username is required.");
                TxtUsername.Focus();
                return;
            }
            if (username.Length < 3)
            {
                ShowError("Username must be at least 3 characters.");
                TxtUsername.Focus();
                return;
            }
            if (username.Contains(" "))
            {
                ShowError("Username cannot contain spaces.");
                TxtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Email address is required.");
                TxtEmail.Focus();
                return;
            }
            if (!IsValidEmail(email))
            {
                ShowError("Please enter a valid email address.");
                TxtEmail.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(phone) && !IsValidPhone(phone))
            {
                ShowError("Please enter a valid phone number (e.g. 0312-1234567).");
                TxtPhone.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError("Password is required.");
                TxtPassword.Focus();
                return;
            }
            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters.");
                TxtPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(confirmPass))
            {
                ShowError("Please confirm your password.");
                TxtConfirmPassword.Focus();
                return;
            }
            if (password != confirmPass)
            {
                ShowError("Passwords do not match.");
                TxtConfirmPassword.Focus();
                return;
            }

            // --- Save to database ---
            try
            {
                bool usernameExists = UserRepository.UsernameExists(username);
                if (usernameExists)
                {
                    ShowError("This username is already taken. Please choose another.");
                    TxtUsername.Focus();
                    return;
                }

                bool emailExists = UserRepository.EmailExists(email);
                if (emailExists)
                {
                    ShowError("An account with this email already exists.");
                    TxtEmail.Focus();
                    return;
                }

                UserRepository.RegisterCustomer(
                    fullName, username, email, phone, address, password);

                ShowSuccess("Account created successfully! You can now log in.");
                ClearForm();
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // --- Helpers ---
        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        private static bool IsValidPhone(string phone)
        {
            return Regex.IsMatch(phone, @"^[\d\-\+\(\)\s]{7,20}$");
        }

        private void ShowError(string msg)
        {
            TxtError.Text = msg;
            ErrorBorder.Visibility = Visibility.Visible;
            SuccessBorder.Visibility = Visibility.Collapsed;
        }

        private void ShowSuccess(string msg)
        {
            TxtSuccess.Text = msg;
            SuccessBorder.Visibility = Visibility.Visible;
            ErrorBorder.Visibility = Visibility.Collapsed;
        }

        private void HideMessages()
        {
            ErrorBorder.Visibility = Visibility.Collapsed;
            SuccessBorder.Visibility = Visibility.Collapsed;
        }

        private void ClearForm()
        {
            TxtFullName.Clear();
            TxtUsername.Clear();
            TxtEmail.Clear();
            TxtPhone.Clear();
            TxtAddress.Clear();
            TxtPassword.Clear();
            TxtConfirmPassword.Clear();
        }
    }
}