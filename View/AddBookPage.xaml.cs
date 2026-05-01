using E_Raamatud.Model;
using E_Raamatud.Resources.Localization;
using E_Raamatud.ViewModel;

namespace E_Raamatud.View;

public partial class AddBookPage : ContentPage
{
    public AddBookPage()
    {
        InitializeComponent();
        BindingContext = new AvaldajaViewModel();
    }

    public void LoadBook(Raamat book)
    {
        BindingContext = new AvaldajaViewModel(book);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnPickImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Vali kaanepilt",
                FileTypes = FilePickerFileType.Images
            });
            if (result != null && BindingContext is AvaldajaViewModel vm)
                vm.Pilt = result.FullPath;
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppResources.Error, ex.Message, AppResources.OK);
        }
    }

    private async void OnPickTextFileClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Vali raamatu fail",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/epub+zip", "application/pdf", "text/plain" } },
                    { DevicePlatform.iOS,     new[] { "public.epub", "com.adobe.pdf", "public.plain-text" } },
                    { DevicePlatform.WinUI,   new[] { ".epub", ".pdf", ".txt" } },
                    { DevicePlatform.macOS,   new[] { "epub", "pdf", "txt" } },
                })
            });
            if (result != null && BindingContext is AvaldajaViewModel vm)
                vm.Tekstifail = result.FullPath;
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppResources.Error, ex.Message, AppResources.OK);
        }
    }

    private async void OnPickAudioFilesClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Vali audiofailid",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "audio/mpeg", "audio/mp4", "audio/wav", "audio/ogg" } },
                    { DevicePlatform.iOS,     new[] { "public.audio" } },
                    { DevicePlatform.WinUI,   new[] { ".mp3", ".m4a", ".wav", ".ogg", ".aac", ".flac" } },
                    { DevicePlatform.macOS,   new[] { "mp3", "m4a", "wav", "ogg", "aac", "flac" } },
                })
            });
            if (result != null && BindingContext is AvaldajaViewModel vm)
                vm.AddAudioFiles(result.Select(r => r.FullPath));
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppResources.Error, ex.Message, AppResources.OK);
        }
    }
}