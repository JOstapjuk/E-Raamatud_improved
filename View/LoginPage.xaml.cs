using E_Raamatud.Model;
using E_Raamatud.View;
using E_Raamatud.ViewModel;
using E_Raamatud.Views;

namespace E_Raamatud;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void Login_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is UserViewModel vm)
        {
            var user = await vm.LoginAsync();

            if (user != null)
            {
                SessionService.SetCurrentUser(user);

                if (user.Role == UserRole.Avaldaja)
                {
                    Application.Current.MainPage = new NavigationPage(new AvaldajaPage());
                }
                else if (user.Role == UserRole.Kasutaja)
                {
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
                else
                {
                    Application.Current.MainPage = new NavigationPage(new AdminPage());
                }
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