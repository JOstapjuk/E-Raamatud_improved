using E_Raamatud.Model;
using E_Raamatud.ViewModel;

namespace E_Raamatud;

public partial class BookDetailPage : ContentPage
{
    public BookDetailPage(Raamat selectedBook, string zanrNimi)
    {
        InitializeComponent();

        if (selectedBook == null)
            throw new ArgumentNullException(nameof(selectedBook));

        BindingContext = new BookDetailViewModel(selectedBook, zanrNimi);
    }
}
