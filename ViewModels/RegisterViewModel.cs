using System.Linq;
using System.Windows;
using System.Windows.Input;
using Finly.Helpers;
using Finly.Services;

namespace Finly.ViewModels
{
    public class RegisterViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // >>> DODANE: potwierdzenie has�a (ustawiane przez RepeatPasswordBox_PasswordChanged)
        public string ConfirmPassword { get; set; } = string.Empty;

        public ICommand RegisterCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(Register);
        }

        private void Register()
        {
            // Walidacja u�ytkownika
            var normalized = (Username ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                MessageBox.Show("Login nie mo�e by� pusty.", "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Walidacja has�a
            if (!ValidatePassword(Password, ConfirmPassword, out string error))
            {
                MessageBox.Show(error, "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Rejestracja
            bool success = UserService.Register(normalized, Password);
            if (success)
            {
                MessageBox.Show("Rejestracja udana!", "Rejestracja", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Rejestracja nieudana. Taki u�ytkownik mo�e ju� istnie�.", "Rejestracja",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Prosta walidacja zgodna z checklist�:
        // - >= 8 znak�w, - ma�a litera, - DU�A litera, - cyfra, - znak specjalny, - bez spacji, - zgodno�� z powt�rzeniem
        private static bool ValidatePassword(string password, string confirm, out string error)
        {
            error = string.Empty;
            password ??= string.Empty;
            confirm ??= string.Empty;

            if (password.Length < 8)
            {
                error = "Has�o musi mie� co najmniej 8 znak�w.";
                return false;
            }
            if (!password.Any(char.IsLower))
            {
                error = "Has�o musi zawiera� ma�� liter�.";
                return false;
            }
            if (!password.Any(char.IsUpper))
            {
                error = "Has�o musi zawiera� wielk� liter�.";
                return false;
            }
            if (!password.Any(char.IsDigit))
            {
                error = "Has�o musi zawiera� cyfr�.";
                return false;
            }
            if (!password.Any(ch => char.IsPunctuation(ch) || char.IsSymbol(ch)))
            {
                error = "Has�o musi zawiera� znak specjalny (np. !@#$%).";
                return false;
            }
            if (password.Any(char.IsWhiteSpace))
            {
                error = "Has�o nie mo�e zawiera� spacji.";
                return false;
            }
            if (password != confirm)
            {
                error = "Powt�rzone has�o musi by� identyczne.";
                return false;
            }
            return true;
        }
    }
}
