using E_Raamatud.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace E_Raamatud.ViewModel
{
    public class GenreViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Genre> Genres { get; set; }

        public GenreViewModel()
        {
            Genres = new ObservableCollection<Genre>
            {
                new Genre { Zanr_ID = 1, Nimetus = "romaan", Kirjeldus = "Romaanid ja ilukirjandus" },
                new Genre { Zanr_ID = 2, Nimetus = "fantaasia", Kirjeldus = "Fantaasiakirjandus" },
                new Genre { Zanr_ID = 3, Nimetus = "klassika", Kirjeldus = "Klassikalised teosed" },
                new Genre { Zanr_ID = 4, Nimetus = "ajalugu", Kirjeldus = "Ajaloolised raamatud" },
                new Genre { Zanr_ID = 5, Nimetus = "ulme", Kirjeldus = "Ulmekirjandus" },
                new Genre { Zanr_ID = 6, Nimetus = "noorteraamat", Kirjeldus = "Noortekirjandus" },
                new Genre { Zanr_ID = 7, Nimetus = "seiklus", Kirjeldus = "Seikluslood" }
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
