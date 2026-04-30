using E_Raamatud.Model;
using E_Raamatud.Resources.Localization;
using E_Raamatud.View;
using E_Raamatud.ViewModel;

namespace E_Raamatud;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        HeroTitle.Text    = AppResources.LoginTitle;
        HeroSubtitle.Text = AppResources.LoginSubtitle;
        AccountSectionLabel.Text    = AppResources.AccountLabel;
        UsernameTitleLabel.Text     = AppResources.UsernameLabel;
        UsernameEntry.Placeholder   = AppResources.UsernamePlaceholder;
        PasswordTitleLabel.Text     = AppResources.PasswordLabel;
        PasswordEntry.Placeholder   = AppResources.PasswordPlaceholder;
        LoginBtn.Text               = AppResources.LoginButton;
        OrLabel.Text                = AppResources.OrDivider;
        NoAccountLabel.Text         = AppResources.NoAccount;
        RegisterLabel.Text          = AppResources.RegisterLink;
    }

    private async void Login_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is UserViewModel vm)
        {
            var user = await vm.LoginAsync();

            if (user != null)
            {
                SessionService.SetCurrentUser(user);

                if (user.Role == UserRole.Kasutaja)
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                else
                    Application.Current.MainPage = new NavigationPage(new AdminPage());
            }
            else
            {
                await DisplayAlert("Login", vm.LoginStatus, "OK");
            }
        }
    }

    private async void GoToRegister_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}
