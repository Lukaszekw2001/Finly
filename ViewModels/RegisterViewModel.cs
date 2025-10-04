using System.Windows;
using System.Windows.Input;
using Aplikacja_do_sledzenia_wydatkow.Helpers;
using Aplikacja_do_�ledzenia_wydatk�w.Services;

namespace Aplikacja_do_sledzenia_wydatkow.ViewModels
{
    public class RegisterViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public ICommand RegisterCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(Register);
        }

        private void Register()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Login i has�o nie mog� by� puste.");
                return;
            }

            bool success = UserService.Register(Username, Password);
            if (success)
            {
                MessageBox.Show("Rejestracja udana!");
            }
            else
            {
                MessageBox.Show("Rejestracja nieudana. U�ytkownik mo�e ju� istnieje.");
            }
        }
    }
}