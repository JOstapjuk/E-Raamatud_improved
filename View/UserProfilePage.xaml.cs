using E_Raamatud.Model;
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
    }

    private void LoadUserData()
    {
        var user = SessionService.CurrentUser;
        if (user == null) return;

        if (UsernameLabel != null)
            UsernameLabel.Text = user.Username ?? "kasutaja";

        if (EmailLabel != null)
            EmailLabel.Text = "";

        if (AvatarInitial != null)
        {
            if (!string.IsNullOrWhiteSpace(user.Username))
                AvatarInitial.Text = user.Username.Substring(0, 1).ToUpper();
            else
                AvatarInitial.Text = "?";
        }

        if (RoleLabel != null)
        {
            RoleLabel.Text = user.Role switch
            {
                UserRole.Admin => "ADMIN",
                UserRole.Avaldaja => "AVALDAJA",
                _ => "KASUTAJA"
            };
        }
    }

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

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnLibraryTapped(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new LibraryPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Viga", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnPurchaseHistoryTapped(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new E_Raamatud.PurchaseHistoryPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Viga", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnUpdatesTapped(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new UpdatesPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Viga", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnAccountSettingsTapped(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new AccountSettingsPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Viga", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnRedeemGiftCardTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Kingikaart", "Funktsioon tulekul!", "OK");
    }

    private async void OnAddAccountTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Lisa konto", "Funktsioon tulekul!", "OK");
    }

    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logi välja",
            "Kas soovite kindlasti välja logida?", "Jah", "Ei");

        if (!confirm) return;

        SessionService.Clear();
        Application.Current.MainPage = new NavigationPage(new E_Raamatud.LoginPage());
    }
}