using E_Raamatud.Services;

namespace E_Raamatud;

public partial class App : Application
{
    public App()
    {
        LanguageService.Initialize();

        InitializeComponent();
        MainPage = new NavigationPage(new LoginPage());

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await DatabaseService.Instance.InitializeAsync();
    }
}