using E_Raamatud.ViewModel;
using System;
using System.Windows.Input;

namespace E_Raamatud.View;

public partial class AdminPage : ContentPage
{
    AdminViewModel vm;

    public AdminPage()
    {
        InitializeComponent();
        vm = new AdminViewModel();
        BindingContext = vm;

        vm.Navigation = Navigation;
        _ = vm.LoadDataAsync();
    }

    private void OnLogoutTapped(object sender, EventArgs e)
    {
        if (vm.LogoutCommand?.CanExecute(null) == true)
            vm.LogoutCommand.Execute(null);
    }

    // ===== Удаление через рефлексию-безопасный подход =====
    private void OnDeleteUserTapped(object sender, TappedEventArgs e)
    {
        ExecuteCommand(vm.DeleteUserCommand, e.Parameter ?? GetParam(sender));
    }

    private void OnDeleteGenreTapped(object sender, TappedEventArgs e)
    {
        ExecuteCommand(vm.DeleteGenreCommand, e.Parameter ?? GetParam(sender));
    }

    private void OnDeleteBookTapped(object sender, TappedEventArgs e)
    {
        ExecuteCommand(vm.DeleteBookCommand, e.Parameter ?? GetParam(sender));
    }

    private static object GetParam(object sender)
    {
        if (sender is Border b && b.GestureRecognizers != null)
        {
            foreach (var g in b.GestureRecognizers)
            {
                if (g is TapGestureRecognizer tap)
                    return tap.CommandParameter;
            }
        }
        return null;
    }

    private void ExecuteCommand(ICommand cmd, object parameter)
    {
        if (cmd != null && cmd.CanExecute(parameter))
            cmd.Execute(parameter);
    }
}