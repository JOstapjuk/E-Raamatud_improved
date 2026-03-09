using E_Raamatud.ViewModel;

namespace E_Raamatud.View;

public partial class AccountSettingsPage : ContentPage
{
    public AccountSettingsPage()
    {
        InitializeComponent();
        BindingContext = new AccountSettingsViewModel();
    }
}
