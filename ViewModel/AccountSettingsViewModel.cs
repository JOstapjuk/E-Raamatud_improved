using E_Raamatud.Model;
using E_Raamatud.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace E_Raamatud.ViewModel
{
    public class AccountSettingsViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _currentPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string? _profilePictureUrl;

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

        public string? ProfilePictureUrl
        {
            get => _profilePictureUrl;
            set { if (_profilePictureUrl != value) { _profilePictureUrl = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasProfilePicture)); } }
        }

        public bool HasProfilePicture => !string.IsNullOrEmpty(ProfilePictureUrl);

        public ICommand PickProfilePictureCommand { get; }
        public ICommand SaveCommand { get; }

        public AccountSettingsViewModel()
        {
            if (SessionService.CurrentUser != null)
            {
                Username = SessionService.CurrentUser.Username;
                ProfilePictureUrl = SessionService.CurrentUser.ProfilePicture;
            }

            SaveCommand = new Command(async () => await SaveChanges());
            PickProfilePictureCommand = new Command(async () => await PickProfilePicture());
        }

        private async Task PickProfilePicture()
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Vali profiilipilt"
            });

            if (result == null) return;

            using var stream = await result.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var user = SessionService.CurrentUser;
            if (user == null) return;

            var fileName = $"avatar_{DateTime.UtcNow.Ticks}.jpg";
            var url = await DatabaseService.Instance.UploadProfilePictureAsync(user.Id, bytes, fileName);

            if (url != null)
            {
                user.ProfilePicture = url;
                ProfilePictureUrl = url;
                await DatabaseService.Instance.UpdateUserAsync(user);
                await Application.Current.MainPage.DisplayAlert("Valmis", "Profiilipilt on uuendatud.", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Viga", "Pildi üleslaadimine ebaõnnestus.", "OK");
            }
        }

        private async Task SaveChanges()
        {
            if (SessionService.CurrentUser == null) return;

            if (string.IsNullOrWhiteSpace(Username))
            {
                await Application.Current.MainPage.DisplayAlert("Viga", "Kasutajanimi ei tohi olla tühi.", "OK");
                return;
            }

            if (!string.IsNullOrEmpty(CurrentPassword) || !string.IsNullOrEmpty(NewPassword) || !string.IsNullOrEmpty(ConfirmPassword))
            {
                if (SessionService.CurrentUser.Password != CurrentPassword)
                {
                    await Application.Current.MainPage.DisplayAlert("Viga", "Praegune parool on vale.", "OK");
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    await Application.Current.MainPage.DisplayAlert("Viga", "Uued paroolid ei kattu.", "OK");
                    return;
                }

                SessionService.CurrentUser.Password = NewPassword;
            }

            SessionService.CurrentUser.Username = Username;

            try
            {
                await DatabaseService.Instance.UpdateUserAsync(SessionService.CurrentUser);
                await Application.Current.MainPage.DisplayAlert("Seaded", "Muudatused on salvestatud.", "OK");
                CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Viga", "Salvestamisel tekkis probleem: " + ex.Message, "OK");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}