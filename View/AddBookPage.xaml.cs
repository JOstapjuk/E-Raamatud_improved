namespace E_Raamatud.View;

using E_Raamatud.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AddBookPage : ContentPage
{
    public AddBookPage()
    {
        InitializeComponent();
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
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Vali pildifail"
            });

            if (result != null && BindingContext is AvaldajaViewModel vm)
                vm.Pilt = result.FullPath;
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
                    { DevicePlatform.Android, new[] { "application/epub+zip" } },
                    { DevicePlatform.iOS, new[] { "org.idpf.epub-container" } },
                    { DevicePlatform.WinUI, new[] { ".epub" } },
                    { DevicePlatform.MacCatalyst, new[] { "org.idpf.epub-container" } }
                }),
                PickerTitle = "Vali EPUB fail"
            });

            if (result != null && BindingContext is AvaldajaViewModel vm)
                vm.Tekstifail = result.FullPath;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Viga", $"EPUB faili valimisel tekkis viga: {ex.Message}", "OK");
        }
    }

    private async void OnPickAudioFilesClicked(object sender, EventArgs e)
    {
        try
        {
            var audioTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/mpeg", "audio/mp4", "audio/ogg", "audio/wav" } },
                { DevicePlatform.iOS, new[] { "public.audio" } },
                { DevicePlatform.WinUI, new[] { ".mp3", ".m4a", ".wav", ".ogg", ".aac", ".flac" } },
                { DevicePlatform.MacCatalyst, new[] { "public.audio" } }
            });

            var results = await FilePicker.PickMultipleAsync(new PickOptions
            {
                FileTypes = audioTypes,
                PickerTitle = "Vali audiofailid (saad valida mitu korraga)"
            });

            if (results != null && BindingContext is AvaldajaViewModel vm)
            {
                var paths = results.Select(r => r.FullPath);
                vm.AddAudioFiles(paths);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Viga", $"Audiofailide valimisel tekkis viga: {ex.Message}", "OK");
        }
    }
}