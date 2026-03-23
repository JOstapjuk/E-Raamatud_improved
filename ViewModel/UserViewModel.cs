using E_Raamatud.Model;
using E_Raamatud.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;

namespace E_Raamatud.ViewModel
{
    public class UserViewModel : INotifyPropertyChanged
    {
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

            _ = EnsureAdminExists();
        }

        private async Task EnsureAdminExists()
        {
            var users = await DatabaseService.Instance.GetUsersAsync();
            var admin = users.FirstOrDefault(u => u.Role == UserRole.Admin);
            if (admin == null)
            {
                await DatabaseService.Instance.InsertUserAsync(new User
                {
                    Username = "admin",
                    Password = "admin123",
                    Role = UserRole.Admin,
                    IsApproved = true
                });
            }
        }

        public async Task<User?> LoginAsync()
        {
            try
            {
                Debug.WriteLine($"=== LOGIN ATTEMPT ===");
                Debug.WriteLine($"Username entered: '{Username}'");
                Debug.WriteLine($"Password entered: '{Password}'");

                var allUsers = await DatabaseService.Instance.GetUsersAsync();

                Debug.WriteLine($"Total users in DB: {allUsers.Count}");

                foreach (var u in allUsers)
                {
                    Debug.WriteLine($"DB User: '{u.Username}' | Password: '{u.Password}' | Role: '{u.RoleString}' | IsApproved: {u.IsApproved}");
                }

                var user = allUsers.FirstOrDefault(u =>
                    u.Username.ToLower() == Username.ToLower() && u.Password == Password);

                Debug.WriteLine($"Match found: {user != null}");

                if (user != null)
                {
                    if (user.Role == UserRole.Avaldaja && !user.IsApproved)
                    {
                        LoginStatus = "Sinu konto ootab administraatori kinnitust.";
                        return null;
                    }

                    SessionService.SetCurrentUser(user);
                    LoginStatus = $"Tere tulemast, {user.Username}!";

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
            catch (Exception ex)
            {
                Debug.WriteLine($"LOGIN ERROR: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                LoginStatus = "Sisselogimisel tekkis viga.";
                return null;
            }
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                LoginStatus = "Palun täida kõik väljad.";
                return;
            }

            var users = await DatabaseService.Instance.GetUsersAsync();
            var existing = users.FirstOrDefault(u => u.Username == Username);

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

            await DatabaseService.Instance.InsertUserAsync(user);

            LoginStatus = SelectedRole == UserRole.Avaldaja
                ? "Avaldaja konto ootab kinnitamist."
                : "Registreerimine õnnestus!";
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}