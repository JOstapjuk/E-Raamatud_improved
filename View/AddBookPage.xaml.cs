namespace E_Raamatud.View;
using E_Raamatud.ViewModel;

public partial class AddBookPage : ContentPage
{
	public AddBookPage()
	{
		InitializeComponent();
	}
    private async void OnPickImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Vali pildifail"
            });

            if (result != null)
            {
                if (BindingContext is AvaldajaViewModel vm)
                {
                    vm.Pilt = result.FullPath;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Viga", $"Pildi valimisel tekkis viga: {ex.Message}", "OK");
        }
    }

    private async void OnPickTextFileClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "text/plain" } }
                }),
                PickerTitle = "Vali tekstifail"
            });

            if (result != null)
            {
                if (BindingContext is AvaldajaViewModel vm)
                {
                    vm.Tekstifail = result.FullPath;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Viga", $"Tekstifaili valimisel tekkis viga: {ex.Message}", "OK");
        }
    }
}