using System.Net.Http;
using System.Text;
using E_Raamatud.Services;
using E_Raamatud.View;

namespace E_Raamatud.View;

public partial class PdfReaderPage : ContentPage
{
    private string _localPath;
    private string _title;

    public PdfReaderPage(string title, string pdfUrl)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        _title = title;
        _ = LoadPdfAsync(pdfUrl);
    }

    private async Task LoadPdfAsync(string pdfUrl)
    {
        try
        {
            if (pdfUrl.StartsWith("http"))
            {
                using var httpClient = new HttpClient();
                var bytes = await httpClient.GetByteArrayAsync(pdfUrl);
                _localPath = Path.Combine(FileSystem.CacheDirectory, "current.pdf");
                await File.WriteAllBytesAsync(_localPath, bytes);
            }
            else
            {
                _localPath = pdfUrl;
            }

            PdfViewer.Uri = _localPath;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            PdfViewer.IsVisible = true;
        }
        catch (Exception ex)
        {
            LoadingIndicator.IsVisible = false;
            await DisplayAlert("Viga", $"PDF avamisel tekkis probleem: {ex.Message}", "OK");
        }
    }

    private async void OnSummarize(object sender, EventArgs e)
    {
        SummarizeButton.IsEnabled = false;

        try
        {
            if (string.IsNullOrWhiteSpace(_localPath) || !File.Exists(_localPath))
            {
                await DisplayAlert("Viga", "PDF pole veel laetud.", "OK");
                return;
            }

            var sb = new StringBuilder();
            var bytes = await File.ReadAllBytesAsync(_localPath);

            using var pdfDoc = UglyToad.PdfPig.PdfDocument.Open(bytes);
            foreach (var page in pdfDoc.GetPages().Take(10))
            {
                foreach (var word in page.GetWords())
                    sb.Append(word.Text + " ");
                sb.AppendLine();
                if (sb.Length > 6000) break;
            }

            var rawText = sb.ToString();

            var service = new SummarizationService();
            string summary = await service.SummarizeAsync(
                title: _title,
                description: null,
                rawHtml: null,
                rawText: rawText);

            var popup = new SummaryPopupPage(_title, summary);
            await Navigation.PushModalAsync(popup, animated: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Summarize error: {ex.Message}");
            await DisplayAlert("Viga", "Kokkuvõtte loomine ebaõnnestus.", "OK");
        }
        finally
        {
            SummarizeButton.IsEnabled = true;
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}