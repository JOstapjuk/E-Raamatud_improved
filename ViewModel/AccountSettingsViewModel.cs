using E_Raamatud.Model;
using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace E_Raamatud.ViewModel
{
    public class AccountSettingsViewModel : INotifyPropertyChanged
    {
        private SQLiteAsyncConnection _database;

        private string _username;
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;

        public string Username
        {
            get => _username;
            set { if (_username != value) { _username = value; OnPropertyChanged(); } }
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set { if (_currentPassword != value) { _currentPassword = value; OnPropertyChanged(); } }
        }

        public string NewPassword
        {
            get => _newPassword;
            set { if (_newPassword != value) { _newPassword = value; OnPropertyChanged(); } }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { if (_confirmPassword != value) { _confirmPassword = value; OnPropertyChanged(); } }
        }

        public ICommand SaveCommand { get; }

        public AccountSettingsViewModel()
        {
            // Initialize DB connection
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Books.db");
            _database = new SQLiteAsyncConnection(dbPath);

            // Load current user data
            if (SessionService.CurrentUser != null)
            {
                Username = SessionService.CurrentUser.Username;
            }

            SaveCommand = new Command(async () => await SaveChanges());
        }

        private async Task SaveChanges()
        {
            if (SessionService.CurrentUser == null)
                return;

            // Validate username is not empty
            if (string.IsNullOrWhiteSpace(Username))
            {
                await Application.Current.MainPage.DisplayAlert("Viga", "Kasutajanimi ei tohi olla tühi.", "OK");
                return;
            }

            // If password fields are filled, validate and change password
            if (!string.IsNullOrEmpty(CurrentPassword) ||
                !string.IsNullOrEmpty(NewPassword) ||
                !string.IsNullOrEmpty(ConfirmPassword))
            {
                // Check current password matches
                if (SessionService.CurrentUser.Password != CurrentPassword)
                {
                    await Application.Current.MainPage.DisplayAlert("Viga", "Praegune parool on vale.", "OK");
                    return;
                }

                // Validate new password and confirmation match
                if (NewPassword != ConfirmPassword)
                {
                    await Application.Current.MainPage.DisplayAlert("Viga", "Uued paroolid ei kattu.", "OK");
                    return;
                }

                // Optional: add password strength validation here

                SessionService.CurrentUser.Password = NewPassword;
            }

            // Update username
            SessionService.CurrentUser.Username = Username;

            try
            {
                // Update user record in database
                await _database.UpdateAsync(SessionService.CurrentUser);

                await Application.Current.MainPage.DisplayAlert("Seaded", "Muudatused on salvestatud.", "OK");

                // Clear password fields after successful change
                CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Viga", "Salvestamisel tekkis probleem: " + ex.Message, "OK");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
