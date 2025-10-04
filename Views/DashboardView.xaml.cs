using Aplikacja_do_sledzenia_wydatkow.Models;
using Aplikacja_do_sledzenia_wydatkow.Models;
using Aplikacja_do_sledzenia_wydatkow.Services;
using Aplikacja_do_sledzenia_wydatkow.Services;
using Aplikacja_do_�ledzenia_wydatk�w.Services;
using Aplikacja_do_sledzenia_wydatkow.ViewModels;
using Aplikacja_do_sledzenia_wydatkow.Views;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Aplikacja_do_sledzenia_wydatkow.Views
{
    public partial class DashboardView : Window
    {
        private readonly int _userId;
        private List<ExpenseDisplayModel> _expenses = new();

        public DashboardView(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadExpenses();
            LoadCategories();
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            var ask = MessageBox.Show(
                "Na pewno chcesz trwale usun�� konto wraz ze wszystkimi wydatkami i kategoriami?\n" +
                "Tej operacji nie mo�na cofn��.",
                "Usu� konto",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (ask != MessageBoxResult.Yes) return;

            try
            {
                var ok = UserService.DeleteAccount(_userId);
                if (!ok)
                {
                    MessageBox.Show("Nie uda�o si� usun�� konta.", "B��d", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Powr�t do okna logowania z banerem �konto usuni�te�
                var auth = new AuthWindow();
                var vm = (AuthViewModel)auth.DataContext;
                vm.ShowAccountDeletedInfo();

                Application.Current.MainWindow = auth;
                auth.Show();

                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Wyst�pi� b��d podczas usuwania konta:\n" + ex.Message,
                                "B��d", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ShowChart_Click(object sender, RoutedEventArgs e)
        {
            var chart = new ChartView(_userId);
            chart.ShowDialog(); // dzia�a tylko je�li ChartView dziedziczy po Window
        }

        private void LoadExpenses()
        {
            var expenses = new List<ExpenseDisplayModel>();

            using (var connection = new SQLiteConnection(DatabaseService.ConnectionString))
            {
                connection.Open();
                SchemaService.Ensure(connection);


                string query = @"
        SELECT e.Id, e.Amount, e.Date, e.Description, c.Name 
        FROM Expenses e
        LEFT JOIN Categories c ON e.CategoryId = c.Id
        WHERE e.UserId = @userId";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", _userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            expenses.Add(new ExpenseDisplayModel
                            {
                                Id = reader.GetInt32(0),
                                Amount = reader.GetDouble(1),
                                Date = DateTime.Parse(reader.GetString(2)), // upewnij si� �e w bazie jest tekst!
                                Description = reader.GetString(3),
                                Category = reader.IsDBNull(4) ? "Brak kategorii" : reader.GetString(4)
                            });
                        }
                    }
                }
                _expenses = expenses;
                ExpenseListView.ItemsSource = _expenses;

                TotalAmountText.Text = _expenses.Sum(e => e.Amount).ToString("0.00") + " z�";
                EntryCountText.Text = _expenses.Count.ToString();

                if (_expenses.Any())
                {
                    var days = (_expenses.Max(e => e.Date) - _expenses.Min(e => e.Date)).TotalDays + 1;
                    var average = _expenses.Sum(e => e.Amount) / days;
                    DailyAverageText.Text = $"{average:0.00} z�";
                }
                else
                {
                    DailyAverageText.Text = "0 z�";
                }
            }

            _expenses = expenses;
            ExpenseListView.ItemsSource = _expenses;
        }

        private void LoadCategories()
        {
            var categories = _expenses
                .Select(e => e.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            CategoryFilterComboBox.ItemsSource = categories;
        }

        private void AddExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            var addView = new AddExpenseView(_userId);
            addView.ShowDialog();
            LoadExpenses();
            LoadCategories();
        }

        private void ExpenseListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ExpenseListView.SelectedItem is ExpenseDisplayModel selectedExpense)
            {
                var fullExpense = DatabaseService.GetExpenseById(selectedExpense.Id);
                var editView = new EditExpenseView(fullExpense, _userId);
                editView.ShowDialog();
                LoadExpenses();
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedCategory = CategoryFilterComboBox.Text?.Trim();
            DateTime? from = FromDatePicker.SelectedDate;
            DateTime? to = ToDatePicker.SelectedDate;

            var filtered = _expenses.Where(exp =>
                (string.IsNullOrWhiteSpace(selectedCategory) || exp.Category == selectedCategory) &&
                (!from.HasValue || exp.Date >= from.Value) &&
                (!to.HasValue || exp.Date <= to.Value)).ToList();

            ExpenseListView.ItemsSource = filtered;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddExpenseView(_userId);
            addWindow.ShowDialog();
            LoadExpenses();
            LoadCategories();
        }

        private void DeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            if (ExpenseListView.SelectedItem is ExpenseDisplayModel expenseToDelete)
            {
                if (MessageBox.Show("Czy na pewno chcesz usun�� ten wydatek?", "Potwierdzenie", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DatabaseService.DeleteExpense(expenseToDelete.Id);
                    LoadExpenses();
                    LoadCategories();
                }
            }
            else
            {
                MessageBox.Show("Nie wybrano �adnego wydatku do usuni�cia.", "B��d", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // otw�rz z powrotem okno logowania/rejestracji
            var auth = new Views.AuthWindow();
            var vm = (ViewModels.AuthViewModel)auth.DataContext;
            vm.ShowLogoutInfo();

            // KLUCZOWE: przestaw g��wne okno na AuthWindow
            Application.Current.MainWindow = auth;
            auth.Show();

            // zamknij Dashboard
            this.Close();
        }

    }
}
