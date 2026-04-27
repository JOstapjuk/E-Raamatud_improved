using E_Raamatud.View;

namespace E_Raamatud.Views;

public partial class AvaldajaPage : ContentPage
{
    public AvaldajaPage()
    {
        InitializeComponent();
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
            UsernameLabel.Text = user.Username ?? "Avaldaja";
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

        int childCount = Math.Min(QuickActionsGrid.Children.Count, 2);

        if (width >= 700)
        {
            QuickActionsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
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

    private async void OnAddBookTapped(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new AddBookPage());
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

    private async void OnSettingsTapped(object sender, EventArgs e)
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

    private async void OnViewReportsTapped(object sender, EventArgs e)
    {
        try
        {
            if (SessionService.CurrentUser != null)
            {
                int avaldajaId = SessionService.CurrentUser.Id;
                await Navigation.PushAsync(new AvaldajaReportPage(avaldajaId));
            }
            else
            {
                await DisplayAlert("Viga", "Kasutaja ei ole sisse logitud.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Viga", $"{ex.GetType().Name}: {ex.Message}", "OK");
        }
    }

    private async void OnLogoutTapped(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logi valja",
            "Kas soovite kindlasti valja logida?", "Jah", "Ei");

        if (!confirm) return;

        SessionService.Clear();
        Application.Current.MainPage = new NavigationPage(new E_Raamatud.LoginPage());
    }
}