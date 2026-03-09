using Microsoft.Maui.Controls;

namespace E_Raamatud.View
{
    public partial class BookReaderPage : ContentPage
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public BookReaderPage(string title, string content)
        {
            InitializeComponent();
            Title = title;
            Content = content;
            BindingContext = this;
        }
    }
}
