namespace E_Raamatud.Model
{
    public class BookWithGenre
    {
        public int Raamat_ID { get; set; }
        public string Pealkiri { get; set; }
        public string Kirjeldus { get; set; }
        public string Zanr_Nimi { get; set; }
        public string Pilt { get; set; }
        public string Tekstifail { get; set; }
        public string Audiofail { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string PiltOrPlaceholder =>
            string.IsNullOrWhiteSpace(Pilt) ? "placeholder_cover.png" : Pilt;

        public bool IsFinished => TotalPages > 0 && CurrentPage >= TotalPages - 1;
        public bool IsNew => CurrentPage == 0;

        public string StatusLabel =>
            IsFinished ? "Loetud" :
            IsNew ? "Uus" : null;

        public Color StatusColor =>
            IsFinished ? Color.FromArgb("#2d6e68") :
            Color.FromArgb("#d97a4a");

        public bool HasAudio => !string.IsNullOrWhiteSpace(Audiofail);
    }
}