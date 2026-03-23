using E_Raamatud.Model;
using E_Raamatud.Services;
using System.Collections.ObjectModel;

namespace E_Raamatud.Pages;

public partial class AcceptancePage : ContentPage
{
    public ObservableCollection<User> PendingUsers { get; set; } = new();

    public AcceptancePage()
    {
        InitializeComponent();
        BindingContext = this;
        _ = LoadPendingUsersAsync();
    }

    private async Task LoadPendingUsersAsync()
    {
        var users = await DatabaseService.Instance.GetUsersAsync();
        var pending = users.Where(u => u.Role == UserRole.Avaldaja && !u.IsApproved).ToList();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            PendingUsers.Clear();
            foreach (var user in pending)
                PendingUsers.Add(user);
        });
    }

    private async void OnAcceptClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is User user)
        {
            user.IsApproved = true;
            await DatabaseService.Instance.UpdateUserAsync(user);
            await LoadPendingUsersAsync();
        }
    }

    private async void OnDeclineClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is User user)
        {
            await DatabaseService.Instance.DeleteUserAsync(user.Id);
            await LoadPendingUsersAsync();
        }
    }
}