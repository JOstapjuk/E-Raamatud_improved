using E_Raamatud.Model;
using SQLite;
using System.Collections.ObjectModel;
using E_Raamatud.ViewModel;

namespace E_Raamatud
{
    public partial class CartPage : ContentPage
    {
        public CartPage(int userId)
        {
            InitializeComponent();
            BindingContext = new CartViewModel();
        }
    }
}