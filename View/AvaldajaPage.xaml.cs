using E_Raamatud.View;
using E_Raamatud.ViewModel;

namespace E_Raamatud.Views;

public partial class AvaldajaPage : ContentPage
{
    public AvaldajaPage()
    {
        InitializeComponent();
    }

    private async void OnAddBookClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddBookPage());
    }

    private async void OnUpdatesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UpdatesPage());
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AccountSettingsPage());
    }

    private async void OnViewReportsClicked(object sender, EventArgs e)
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

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logi välja", "Kas soovite kindlasti välja logida?", "Jah", "Ei");
        if (!confirm) return;

        SessionService.Clear();
        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }
}
