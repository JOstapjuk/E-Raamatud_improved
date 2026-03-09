using E_Raamatud.Pages;
using E_Raamatud.ViewModel;

namespace E_Raamatud.View;

public partial class AdminPage : ContentPage
{
    AdminViewModel vm;

    public AdminPage()
    {
        InitializeComponent();
        vm = new AdminViewModel();
        BindingContext = vm;

        vm.Navigation = Navigation;
        _ = vm.LoadDataAsync();
    }

    private async void OnAcceptanceButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AcceptancePage());
    }
}
