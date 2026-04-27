using E_Raamatud.Model;
using SQLite;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using E_Raamatud.ViewModel;

namespace E_Raamatud
{
    public partial class CartPage : ContentPage
    {
        public CartPage(int userId)
        {
            InitializeComponent();
            BindingContext = new CartViewModel();

            // Подписываемся на изменения списка — чтобы переключать пустое состояние
            if (BindingContext is CartViewModel vm && vm.CartItems is INotifyCollectionChanged ncc)
            {
                ncc.CollectionChanged += (s, e) => UpdateEmptyState();
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateEmptyState();
            ApplyResponsiveLayout(this.Width);
        }

        private void UpdateEmptyState()
        {
            if (BindingContext is not CartViewModel vm) return;

            int count = vm.CartItems?.Count ?? 0;
            bool empty = count == 0;

            if (EmptyCartView != null) EmptyCartView.IsVisible = empty;
            if (MainContent != null) MainContent.IsVisible = !empty;

            string itemsText = count switch
            {
                0 => "0 raamatut",
                1 => "1 raamat",
                _ => $"{count} raamatut"
            };

            if (ItemCountLabel != null) ItemCountLabel.Text = itemsText;
            if (ItemCountInline != null) ItemCountInline.Text = itemsText;
        }

        private void OnPageSizeChanged(object sender, EventArgs e)
        {
            ApplyResponsiveLayout(this.Width);
        }

        private void ApplyResponsiveLayout(double width)
        {
            if (MainContent == null || ItemsPanel == null || SummaryPanel == null) return;
            if (width <= 0) return;

            MainContent.RowDefinitions.Clear();
            MainContent.ColumnDefinitions.Clear();

            if (width >= 900)
            {
                // ===== ДЕСКТОП: 2 колонки =====
                MainContent.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                MainContent.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(360)));
                MainContent.RowDefinitions.Add(new RowDefinition(GridLength.Star));

                Grid.SetColumn(ItemsPanel, 0);
                Grid.SetRow(ItemsPanel, 0);
                Grid.SetColumn(SummaryPanel, 1);
                Grid.SetRow(SummaryPanel, 0);
            }
            else
            {
                // ===== МОБИЛЬНЫЙ: одна колонка =====
                MainContent.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                MainContent.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                MainContent.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

                Grid.SetColumn(ItemsPanel, 0);
                Grid.SetRow(ItemsPanel, 0);
                Grid.SetColumn(SummaryPanel, 0);
                Grid.SetRow(SummaryPanel, 1);
            }
        }

        private async void OnBackTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}