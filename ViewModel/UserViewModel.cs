using E_Raamatud.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace E_Raamatud.ViewModel
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private SQLiteAsyncConnection _database = null!;
        public ObservableCollection<UserRole> Roles { get; set; }
        public Func<Task> OnLoginSuccess { get; set; }
        private string _username = string.Empty;
        private string _password = string.Empty;
        private UserRole _selectedRole;
        private string _loginStatus = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public UserRole SelectedRole
        {
            get => _selectedRole;
            set { _selectedRole = value; OnPropertyChanged(nameof(SelectedRole)); }
        }

        public string LoginStatus
        {
            get => _loginStatus;
            set { _loginStatus = value; OnPropertyChanged(nameof(LoginStatus)); }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public UserViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            RegisterCommand = new Command(async () => await RegisterAsync());

            SelectedRole = UserRole.Kasutaja;

            Roles = new ObservableCollection<UserRole>(
                Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Where(role => role != UserRole.Admin));

            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Books.db");
            Console.WriteLine($"Database Path: {dbPath}");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<User>();

            var admin = await _database.Table<User>().FirstOrDefaultAsync(u => u.Role == UserRole.Admin);
            if (admin == null)
            {
                await _database.InsertAsync(new User
                {
                    Username = "admin",
                    Password = "admin123",
                    Role = UserRole.Admin
                });
            }

            var users = await _database.Table<User>().ToListAsync();
            foreach (var u in users)
            {
                Console.WriteLine($"User ID: {u.Id}, Username: {u.Username} - {u.Password} - {u.Role}");
            }
        }

        public async Task<User?> LoginAsync()
        {
            await InitAsync();

            var user = await _database.Table<User>()
                .FirstOrDefaultAsync(u => u.Username == Username && u.Password == Password);

            if (user != null)
            {
                if (user.Role == UserRole.Avaldaja && !user.IsApproved)
                {
                    LoginStatus = "Sinu konto ootab administraatori kinnitust.";
                    return null;
                }

                SessionService.SetCurrentUser(user);
                LoginStatus = $"Sisselogimise edu kui {user.Role}";
                Console.WriteLine($"Login successful - User ID: {user.Id}, Role: {user.Role}");

                if (OnLoginSuccess != null)
                    await OnLoginSuccess();

                return user;
            }
            else
            {
                LoginStatus = "Vale kasutajanimi või salasõna";
                return null;
            }
        }


        private async Task RegisterAsync()
        {
            await InitAsync();

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                LoginStatus = "Palun täida kõik väljad.";
                return;
            }

            var existing = await _database.Table<User>()
                .FirstOrDefaultAsync(u => u.Username == Username);

            if (existing != null)
            {
                LoginStatus = "Kasutajanimi on juba võetud.";
                return;
            }

            var user = new User
            {
                Username = Username,
                Password = Password,
                Role = SelectedRole,
                IsApproved = SelectedRole != UserRole.Avaldaja
            };

            await _database.InsertAsync(user);

            LoginStatus = SelectedRole == UserRole.Avaldaja
                ? "Avaldaja konto ootab kinnitamist."
                : "Registreerimine õnnestus!";
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}