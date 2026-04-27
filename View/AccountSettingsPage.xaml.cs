using E_Raamatud.Services;
using E_Raamatud.ViewModel;

namespace E_Raamatud.View;

public partial class AccountSettingsPage : ContentPage
{
    public AccountSettingsPage()
    {
        InitializeComponent();
        BindingContext = new AccountSettingsViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateProfilePreview();
        ApplyResponsiveLayout(this.Width);
    }

    private void UpdateProfilePreview()
    {
        if (AvatarInitial == null) return;

        var user = SessionService.CurrentUser;
        if (user != null && !string.IsNullOrWhiteSpace(user.Username))
        {
            AvatarInitial.Text = user.Username.Substring(0, 1).ToUpper();
        }
        else
        {
            AvatarInitial.Text = "?";
        }
    }

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        ApplyResponsiveLayout(this.Width);
    }

    private void ApplyResponsiveLayout(double width)
    {
        if (ContentRoot == null || ProfileCard == null || FormPanel == null) return;
        if (width <= 0) return;

        ContentRoot.RowDefinitions.Clear();
        ContentRoot.ColumnDefinitions.Clear();

        if (width >= 900)
        {
            // ===== ДЕСКТОП: профиль слева, форма справа =====
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(320)));
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            Grid.SetColumn(ProfileCard, 0);
            Grid.SetRow(ProfileCard, 0);
            Grid.SetColumn(FormPanel, 1);
            Grid.SetRow(FormPanel, 0);
        }
        else
        {
            // ===== МОБИЛЬНЫЙ: одна колонка, стопкой =====
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(ProfileCard, 0);
            Grid.SetRow(ProfileCard, 0);
            Grid.SetColumn(FormPanel, 0);
            Grid.SetRow(FormPanel, 1);
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}