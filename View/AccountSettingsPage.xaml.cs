using E_Raamatud.Resources.Localization;
using E_Raamatud.Services;
using E_Raamatud.ViewModel;

namespace E_Raamatud.View;

public partial class AccountSettingsPage : ContentPage
{
    public AccountSettingsPage()
    {
        InitializeComponent();

        var vm = new AccountSettingsViewModel();
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(AccountSettingsViewModel.ProfilePictureUrl))
                UpdateProfilePreview();
        };
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateProfilePreview();
        ApplyResponsiveLayout(this.Width);
        // No ApplyLocalization() needed — all static text is handled by {x:Static} in XAML
    }

    private void UpdateProfilePreview()
    {
        var user = SessionService.CurrentUser;
        if (user == null) return;

        if (AvatarInitial != null)
            AvatarInitial.Text = !string.IsNullOrWhiteSpace(user.Username)
                ? user.Username.Substring(0, 1).ToUpper()
                : "?";

        if (ProfileName != null)
            ProfileName.Text = user.Username ?? "";

        if (!string.IsNullOrEmpty(user.ProfilePicture))
        {
            ProfileImage.Source = ImageSource.FromUri(new Uri(user.ProfilePicture));
            AvatarImageBorder.IsVisible = true;
            AvatarBorder.IsVisible = false;
        }
        else
        {
            AvatarImageBorder.IsVisible = false;
            AvatarBorder.IsVisible = true;
        }
    }

    private void OnChangeAvatarTapped(object sender, EventArgs e)
    {
        if (BindingContext is AccountSettingsViewModel vm)
            vm.PickProfilePictureCommand.Execute(null);
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
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(320)));
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Star));

            Grid.SetColumn(ProfileCard, 0); Grid.SetRow(ProfileCard, 0);
            Grid.SetColumn(FormPanel, 1);   Grid.SetRow(FormPanel, 0);
        }
        else
        {
            ContentRoot.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            ContentRoot.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

            Grid.SetColumn(ProfileCard, 0); Grid.SetRow(ProfileCard, 0);
            Grid.SetColumn(FormPanel, 0);   Grid.SetRow(FormPanel, 1);
        }
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
