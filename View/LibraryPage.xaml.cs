using E_Raamatud.Model;
using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace E_Raamatud.View
{
    public partial class LibraryPage : ContentPage
    {
        public LibraryPage()
        {
            InitializeComponent();
            BindingContext = new ViewModel.LibraryViewModel();
        }

        private async void OnItemTapped(object sender, EventArgs e)
        {
            var frame = sender as Frame;
            var raamat = frame?.BindingContext as Raamat;

            if (raamat == null || string.IsNullOrWhiteSpace(raamat.Tekstifail))
                return;

            try
            {
                string content;

                if (Path.IsPathRooted(raamat.Tekstifail) && File.Exists(raamat.Tekstifail))
                {
                    // File saved in local storage (runtime)
                    content = await File.ReadAllTextAsync(raamat.Tekstifail);
                }
                else
                {
                    // File embedded in Resources/Raw (build-time)
                    using var stream = await FileSystem.OpenAppPackageFileAsync(raamat.Tekstifail);
                    using var reader = new StreamReader(stream);
                    content = await reader.ReadToEndAsync();
                }

                await Navigation.PushAsync(new BookReaderPage(raamat.Pealkiri, content));
            }
            catch (FileNotFoundException)
            {
                await DisplayAlert("Viga", "Raamatu tekstifaili ei leitud.", "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Viga raamatu avamisel: {ex.Message}");
                await DisplayAlert("Viga", "Raamatu avamisel tekkis probleem.", "OK");
            }
        }
    }
}

