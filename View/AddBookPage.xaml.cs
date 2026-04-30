namespace E_Raamatud.View;
using E_Raamatud.Model;
using E_Raamatud.Services;
using E_Raamatud.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class AddBookPage : ContentPage
{
    public AddBookPage(Raamat existingBook = null)
    {
        InitializeComponent();
        BindingContext = new AvaldajaViewModel(existingBook);
        PageTitleLabel.Text = existingBook != null ? "Muuda raamatut" : "Lisa raamat";
        SubmitButton.Text = existingBook != null ? "Salvesta muudatused" : "Lisa fail kataloogi";

        if (existingBook != null && existingBook.Zanr_ID.HasValue)
            _ = PreFillGenreLabelAsync(existingBook.Zanr_ID.Value);
    }

    private async Task PreFillGenreLabelAsync(int zanrId)
    {
        var genres = await DatabaseService.Instance.GetGenresAsync();
        var genre = genres.FirstOrDefault(g => g.Zanr_ID == zanrId);
        if (genre != null)
        {
            GenreLabel.Text = genre.Nimetus;
            GenreLabel.TextColor = Color.FromArgb("#1f2e28");
        }
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
                { DevicePlatform.Android, new[] { "application/epub+zip", "application/pdf", "text/plain" } },
                { DevicePlatform.iOS, new[] { "org.idpf.epub-container", "com.adobe.pdf", "public.plain-text" } },
                { DevicePlatform.WinUI, new[] { ".epub", ".pdf", ".txt" } },
                { DevicePlatform.MacCatalyst, new[] { "org.idpf.epub-container", "com.adobe.pdf", "public.plain-text" } }
            }),
                PickerTitle = "Vali raamatu fail (EPUB, PDF, TXT)"
            });

            if (result != null && BindingContext is AvaldajaViewModel vm)
                vm.Tekstifail = result.FullPath;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Viga", $"Faili valimisel tekkis viga: {ex.Message}", "OK");
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

    private async void OnPickGenreTapped(object sender, EventArgs e)
    {
        if (BindingContext is not AvaldajaViewModel vm) return;

        var genreNames = vm.Genres.Select(g => g.Nimetus).ToArray();
        string picked = await DisplayActionSheet("Vali žanr", "Tühista", null, genreNames);

        if (picked == null || picked == "Tühista") return;

        var selected = vm.Genres.FirstOrDefault(g => g.Nimetus == picked);
        if (selected != null)
        {
            vm.SelectedGenre = selected;
            GenreLabel.Text = selected.Nimetus;
            GenreLabel.TextColor = Color.FromArgb("#1f2e28");
        }
    }
}