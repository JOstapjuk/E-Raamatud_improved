using E_Raamatud.Model;
using E_Raamatud.Resources.Localization;
using E_Raamatud.Services;
using E_Raamatud.ViewModel;

namespace E_Raamatud.View;

public partial class UserProfilePage : ContentPage
{
    public UserProfilePage()
    {
        InitializeComponent();
        BindingContext = new UserProfileViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUserData();
        ApplyResponsiveLayout(this.Width);
        HighlightActiveLang();
    }

    // ── User data ─────────────────────────────────────────────────────────────

    private void LoadUserData()
    {
        var user = SessionService.CurrentUser;
        if (user == null) return;

        if (UsernameLabel != null)
            UsernameLabel.Text = user.Username ?? AppResources.UsernameLabel;

        if (EmailLabel != null)
            EmailLabel.Text = "";

        if (AvatarInitial != null)
            AvatarInitial.Text = !string.IsNullOrWhiteSpace(user.Username)
                ? user.Username.Substring(0, 1).ToUpper()
                : "?";

        if (RoleLabel != null)
        {
            RoleLabel.Text = user.Role switch
            {
                UserRole.Admin    => "ADMIN",
                UserRole.Avaldaja => "AVALDAJA",
                _                 => "KASUTAJA"
            };
        }

        if (!string.IsNullOrEmpty(user.ProfilePicture))
        {
            ProfileImage.Source = ImageSource.FromUri(new Uri(user.ProfilePicture));
            AvatarImageBorder.IsVisible = true;
            AvatarBorder.IsVisible = false;
        }
        else
        {
            AvatarImageBorder.IsVisible = false;
            AvatarBorder.IsVisible = true;
        }
    }

    // ── Layout ────────────────────────────────────────────────────────────────

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        ApplyResponsiveLayout(this.Width);
    }

    private void ApplyResponsiveLayout(double width)
    {
        if (QuickActionsGrid == null || width <= 0) return;

        QuickActionsGrid.ColumnDefinitions.Clear();
        QuickActionsGrid.RowDefinitions.Clear();

        int childCount = Math.Min(QuickActionsGrid.Children.Count, 3);

        if (width >= 700)
        {
            for (int i = 0; i < 3; i++)
                QuickActionsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            QuickActionsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            for (int i = 0; i < childCount; i++)
            {
                var child = (Microsoft.Maui.Controls.View)QuickActionsGrid.Children[i];
                Grid.SetColumn(child, i);
                Grid.SetRow(child, 0);
            }
        }
        else
        {
            QuickActionsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            for (int i = 0; i < childCount; i++)
                QuickActionsGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            for (int i = 0; i < childCount; i++)
            {
                var child = (Microsoft.Maui.Controls.View)QuickActionsGrid.Children[i];
                Grid.SetColumn(child, 0);
                Grid.SetRow(child, i);
            }
        }
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnUpdatesTapped(object sender, EventArgs e)
    {
        try { await Navigation.PushAsync(new UpdatesPage()); }
        catch (Exception ex) { await DisplayAlert(AppResources.Error, $"{ex.GetType().Name}: {ex.Message}", AppResources.OK); }
    }

    private async void OnAccountSettingsTapped(object sender, EventArgs e)
    {
        try { await Navigation.PushAsync(new AccountSettingsPage()); }
        catch (Exception ex) { await DisplayAlert(AppResources.Error, $"{ex.GetType().Name}: {ex.Message}", AppResources.OK); }
    }

    private async void OnRedeemGiftCardTapped(object sender, EventArgs e)
    {
        await DisplayAlert(AppResources.GiftCard, AppResources.FunctionComingSoon, AppResources.OK);
    }

    private async void OnAddAccountTapped(object sender, EventArgs e)
    {
        await DisplayAlert(AppResources.AddAccount, AppResources.FunctionComingSoon, AppResources.OK);
    }

    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            AppResources.Logout,
            AppResources.LogoutConfirm,
            AppResources.Yes,
            AppResources.No);

        if (!confirm) return;

        SessionService.Clear();
        Application.Current.MainPage = new NavigationPage(new E_Raamatud.LoginPage());
    }

    // ── Language picker ───────────────────────────────────────────────────────

    private void OnLangEtTapped(object sender, TappedEventArgs e) => ApplyLang("et");
    private void OnLangRuTapped(object sender, TappedEventArgs e) => ApplyLang("ru");
    private void OnLangEnTapped(object sender, TappedEventArgs e) => ApplyLang("en");

    private void ApplyLang(string code)
    {
        LanguageService.ChangeLanguage(code);
        Application.Current.MainPage = new NavigationPage(new E_Raamatud.LoginPage());
    }

    private void HighlightActiveLang()
    {
        var current = LanguageService.CurrentLanguage;
        SetLangActive(LangEtBorder, LangEtCheck, current == "et");
        SetLangActive(LangRuBorder, LangRuCheck, current == "ru");
        SetLangActive(LangEnBorder, LangEnCheck, current == "en");
    }

    private static void SetLangActive(Border border, Label check, bool active)
    {
        border.Stroke          = active ? Color.FromArgb("#2d6e68") : Color.FromArgb("#e0e6e2");
        border.BackgroundColor = active ? Color.FromArgb("#e8f4f2") : Color.FromArgb("#f5f7f5");
        check.IsVisible        = active;
    }
}
