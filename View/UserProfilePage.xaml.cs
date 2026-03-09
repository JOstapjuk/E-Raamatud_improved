using E_Raamatud.ViewModel;
using E_Raamatud.View;
namespace E_Raamatud;

public partial class UserProfilePage : ContentPage
{
    public UserProfilePage()
    {
        InitializeComponent();
        BindingContext = new UserProfileViewModel();
    }

    private async void OnUpdatesClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UpdatesPage());
    }

    private async void OnBooksClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LibraryPage());
    }
    private async void OnAccountSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AccountSettingsPage());
    }

    private async void OnPurchaseHistoryClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new PurchaseHistoryPage());
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logi välja", "Kas soovite kindlasti välja logida?", "Jah", "Ei");
        if (!confirm) return;

        SessionService.Clear();

        Application.Current.MainPage = new NavigationPage(new LoginPage());
    }
}
