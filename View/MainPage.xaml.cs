using E_Raamatud.Model;
using E_Raamatud.Resources.Localization;
using E_Raamatud.Services;
using E_Raamatud.View;
using E_Raamatud.ViewModel;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Windows.Input;

namespace E_Raamatud
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (SessionService.CurrentUser == null)
            {
                await Navigation.PushAsync(new LoginPage());
                return;
            }

            WelcomeLabel.Text = $"{AppResources.WelcomeGreeting}, {SessionService.CurrentUser.Username}!";
            ApplyLocalization();
            BindingContext = new BookViewModel();
        }

        private void ApplyLocalization()
        {
            HeaderTitle.Text          = AppResources.WelcomeGreeting + "!";
            HeaderSubtitle.Text       = "Avasta uusi raamatuid täna"; // decorative, keep as-is or add resource key
            WelcomeSubLabel.Text      = AppResources.BooksTitle;
            BooksSectionTitle.Text    = AppResources.BooksTitle;
            BooksSectionSubtitle.Text = "Vali endale meelepärane";
            SearchEntry.Placeholder   = AppResources.SearchPlaceholder;
            SearchBtnLabel.Text       = "Otsi";
            FiltersLabel.Text         = "FILTRID";
            FileTypeAllLabel.Text     = "Kõik failid";
        }

        private async void OnBookSelected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
                    return;

                var selectedBook = e.CurrentSelection[0] as BookWithGenre;
                if (selectedBook == null) return;

                var raamat = new Raamat
                {
                    Raamat_ID  = selectedBook.Raamat_ID,
                    Pealkiri   = selectedBook.Pealkiri,
                    Kirjeldus  = selectedBook.Kirjeldus,
                    Pilt       = selectedBook.Pilt,
                    Tekstifail = selectedBook.Tekstifail,
                    Audiofail  = selectedBook.Audiofail
                };

                await Navigation.PushAsync(new BookDetailPage(raamat, selectedBook.Zanr_Nimi));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnBookSelected: {ex.Message}");
                await DisplayAlert(AppResources.Error, "Raamatu laadimisel tekkis probleem.", AppResources.OK);
            }
        }

        private async void OnUploadTapped(object sender, EventArgs e)
            => await Navigation.PushAsync(new AddBookPage());

        private async void OnLibraryTapped(object sender, EventArgs e)
            => await Navigation.PushAsync(new LibraryPage());

        private async void OnUserTapped(object sender, EventArgs e)
            => await Navigation.PushAsync(new UserProfilePage());

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            if (BooksGridLayout == null) return;
            double width = this.Width;
            if (width <= 0) return;

            BooksGridLayout.Span = width switch
            {
                < 500  => 2,
                < 800  => 3,
                < 1100 => 4,
                < 1400 => 5,
                _      => 6
            };
        }

        private void OnGenreChipTapped(object sender, TappedEventArgs e)
        {
            if (sender is not Border tapped) return;

            // Reset all genre chips
            foreach (var child in GenresLayout.Children)
            {
                if (child is Border b)
                {
                    b.BackgroundColor = Color.FromArgb("#FFFFFF");
                    b.Stroke = Color.FromArgb("#d5dfd8");
                    if (b.Content is Label lbl)
                        lbl.TextColor = Color.FromArgb("#3a5c52");
                }
            }

            // Highlight selected
            tapped.BackgroundColor = Color.FromArgb("#2d6e68");
            tapped.Stroke = Color.FromArgb("#2d6e68");
            if (tapped.Content is Label tappedLbl)
                tappedLbl.TextColor = Color.FromArgb("#FFFFFF");

            // Fire command with Genre object as parameter
            if (BindingContext is BookViewModel vm && tapped.BindingContext is Genre genre)
            {
                if (vm.SelectGenreCommand is ICommand cmd && cmd.CanExecute(genre))
                    cmd.Execute(genre);
            }
        }

        private void OnFileTypeChipTapped(object sender, TappedEventArgs e)
        {
            if (sender is not Border tapped) return;

            foreach (var child in FileTypeLayout.Children)
            {
                if (child is Border b)
                {
                    b.BackgroundColor = Color.FromArgb("#FFFFFF");
                    b.Stroke = Color.FromArgb("#d5dfd8");
                    if (b.Content is Label lbl)
                        lbl.TextColor = Color.FromArgb("#3a5c52");
                }
            }

            tapped.BackgroundColor = Color.FromArgb("#2d6e68");
            tapped.Stroke = Color.FromArgb("#2d6e68");
            if (tapped.Content is Label tappedLbl2)
                tappedLbl2.TextColor = Color.FromArgb("#FFFFFF");
        }
    }
}
