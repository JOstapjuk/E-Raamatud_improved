using E_Raamatud.Model;
using E_Raamatud.Services;
using E_Raamatud.ViewModel;
using System.Diagnostics;

namespace E_Raamatud;

public partial class BookDetailPage : ContentPage
{
    private readonly Raamat _book;
    private bool _isWishlisted = false;

    public BookDetailPage(Raamat selectedBook, string zanrNimi)
    {
        InitializeComponent();

        if (selectedBook == null)
            throw new ArgumentNullException(nameof(selectedBook));

        _book = selectedBook;
        BindingContext = new BookDetailViewModel(selectedBook, zanrNimi);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyResponsiveLayout(this.Width);
    }

    // ===== Адаптивный layout =====
    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        ApplyResponsiveLayout(this.Width);
    }

    private void ApplyResponsiveLayout(double width)
    {
        if (ContentRoot == null || CoverBorder == null || InfoPanel == null) return;
        if (width <= 0) return;

        ContentRoot.RowDefinitions.Clear();
        ContentRoot.ColumnDefinitions.Clear();

        if (width >= 900)
        {
            // ===== ДЕСКТОП =====
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            Grid.SetColumn(CoverBorder, 0);
            Grid.SetRow(CoverBorder, 0);
            Grid.SetColumn(InfoPanel, 1);
            Grid.SetRow(InfoPanel, 0);

            CoverBorder.WidthRequest = 320;
            CoverBorder.HeightRequest = 460;
            CoverBorder.HorizontalOptions = LayoutOptions.Start;

            PriceActionGrid.ColumnDefinitions.Clear();
            PriceActionGrid.RowDefinitions.Clear();
            PriceActionGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            PriceActionGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            PriceActionGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(ActionButtons, 1);
            Grid.SetRow(ActionButtons, 0);
            ActionButtons.HorizontalOptions = LayoutOptions.End;

            SetInfoBlocksColumns(3);
        }
        else
        {
            // ===== МОБИЛЬНЫЙ / УЗКИЙ =====
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(CoverBorder, 0);
            Grid.SetRow(CoverBorder, 0);
            Grid.SetColumn(InfoPanel, 0);
            Grid.SetRow(InfoPanel, 1);

            double coverWidth = Math.Min(260, width - 80);
            CoverBorder.WidthRequest = coverWidth;
            CoverBorder.HeightRequest = coverWidth * 1.45;
            CoverBorder.HorizontalOptions = LayoutOptions.Center;

            PriceActionGrid.ColumnDefinitions.Clear();
            PriceActionGrid.RowDefinitions.Clear();
            PriceActionGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            PriceActionGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            PriceActionGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(ActionButtons, 0);
            Grid.SetRow(ActionButtons, 1);
            ActionButtons.HorizontalOptions = LayoutOptions.Fill;

            if (width < 500)
                SetInfoBlocksColumns(1);
            else
                SetInfoBlocksColumns(3);
        }
    }

    private void SetInfoBlocksColumns(int columns)
    {
        if (InfoBlocksGrid == null) return;
        if (InfoBlocksGrid.Children.Count < 3) return;

        InfoBlocksGrid.ColumnDefinitions.Clear();
        InfoBlocksGrid.RowDefinitions.Clear();

        for (int i = 0; i < columns; i++)
            InfoBlocksGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        int rows = (int)Math.Ceiling(3.0 / columns);
        for (int i = 0; i < rows; i++)
            InfoBlocksGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        for (int i = 0; i < InfoBlocksGrid.Children.Count && i < 3; i++)
        {
            var child = (Microsoft.Maui.Controls.View)InfoBlocksGrid.Children[i];
            Grid.SetColumn(child, i % columns);
            Grid.SetRow(child, i / columns);
        }
    }

    // ===== Обработчики =====
    private async void OnBackTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnCartTapped(object sender, TappedEventArgs e)
    {
        if (SessionService.CurrentUser == null)
        {
            await DisplayAlert("Logi sisse", "Ostukorvi kasutamiseks pead olema sisse logitud.", "OK");
            return;
        }
        await Navigation.PushAsync(new CartPage(SessionService.CurrentUser.Id));
    }

    // ===== Сердечко: тоггл =====
    private void OnWishlistTapped(object sender, TappedEventArgs e)
    {
        _isWishlisted = !_isWishlisted;

        if (_isWishlisted)
        {
            HeartIcon.Fill = new SolidColorBrush(Color.FromArgb("#d4537e"));
            HeartIcon.Stroke = new SolidColorBrush(Color.FromArgb("#d4537e"));
            WishlistBorder.BackgroundColor = Color.FromArgb("#fbeaf0");
            WishlistBorder.Stroke = new SolidColorBrush(Color.FromArgb("#d4537e"));
        }
        else
        {
            HeartIcon.Fill = new SolidColorBrush(Colors.Transparent);
            HeartIcon.Stroke = new SolidColorBrush(Color.FromArgb("#2d6e68"));
            WishlistBorder.BackgroundColor = Colors.White;
            WishlistBorder.Stroke = new SolidColorBrush(Color.FromArgb("#d5dfd8"));
        }
    }
}