namespace E_Raamatud.Model
{
    public class BookWithProgress
    {
        public int Raamat_ID { get; set; }
        public string Pealkiri { get; set; }
        public string Pilt { get; set; }
        public string Tekstifail { get; set; }
        public string Audiofail { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public double ProgressValue =>
            TotalPages > 1 ? (double)CurrentPage / (TotalPages - 1) : 0;

        public string ProgressText =>
            TotalPages > 1 ? $"{(int)(ProgressValue * 100)}%" : "Pole alustatud";

        public bool HasProgress => TotalPages > 0 && CurrentPage > 0;

        public bool HasAudio => !string.IsNullOrWhiteSpace(Audiofail);
    }
}