using E_Raamatud.Resources.Localization;

namespace E_Raamatud;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
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
        HeroTitle.Text         = AppResources.RegisterTitle;
        HeroSubtitle.Text      = AppResources.RegisterSubtitle;
        UsernameTitleLabel.Text = AppResources.UsernameLabel;
        UsernameEntry.Placeholder = AppResources.UsernamePlaceholder;
        PasswordTitleLabel.Text = AppResources.PasswordLabel;
        PasswordEntry.Placeholder = AppResources.PasswordPlaceholder;
        RoleTitleLabel.Text    = AppResources.RoleLabel;
        RegisterBtn.Text       = AppResources.RegisterButton;
        BackBtn.Text           = AppResources.BackButton;
        HasAccountLabel.Text   = AppResources.HasAccount;
        LoginLabel.Text        = AppResources.LoginLink;
    }

    private async void GoBack_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
