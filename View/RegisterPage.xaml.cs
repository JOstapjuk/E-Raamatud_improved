namespace E_Raamatud;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void GoBack_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}