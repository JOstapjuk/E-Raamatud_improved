using CommunityToolkit.Mvvm.ComponentModel;
using E_Raamatud.Model;

namespace E_Raamatud.ViewModel
{
    public partial class UserProfileViewModel : ObservableObject
    {
        [ObservableProperty]
        private User? currentUser;

        public UserProfileViewModel()
        {
            CurrentUser = SessionService.CurrentUser;
        }
    }
}
