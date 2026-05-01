namespace E_Raamatud.View;

public partial class UpdatesPage : ContentPage
{
    public UpdatesPage()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
