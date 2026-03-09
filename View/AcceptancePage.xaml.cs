using E_Raamatud.Model;
using SQLite;
using System.Collections.ObjectModel;

namespace E_Raamatud.Pages;

public partial class AcceptancePage : ContentPage
{
    private SQLiteAsyncConnection _database;
    public ObservableCollection<User> PendingUsers { get; set; } = new();

    public AcceptancePage()
    {
        InitializeComponent();
        BindingContext = this;
        _ = LoadPendingUsersAsync();
    }

    private async Task InitDatabaseAsync()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Books.db");
        _database = new SQLiteAsyncConnection(dbPath);
        await _database.CreateTableAsync<User>();
    }

    private async Task LoadPendingUsersAsync()
    {
        await InitDatabaseAsync();

        var users = await _database.Table<User>()
            .Where(u => u.Role == UserRole.Avaldaja && !u.IsApproved)
            .ToListAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            PendingUsers.Clear();
            foreach (var user in users)
                PendingUsers.Add(user);
        });
    }

    private async void OnAcceptClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is User user)
        {
            user.IsApproved = true;
            await _database.UpdateAsync(user);
            await LoadPendingUsersAsync();
        }
    }

    private async void OnDeclineClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is User user)
        {
            await _database.DeleteAsync(user);
            await LoadPendingUsersAsync();
        }
    }
}
