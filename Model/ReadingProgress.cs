using Postgrest.Attributes;
using Postgrest.Models;

namespace E_Raamatud.Model
{
    [Table("reading_progress")]
    public class ReadingProgress : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("kasutaja_id")]
        public int Kasutaja_ID { get; set; }

        [Column("raamat_id")]
        public int Raamat_ID { get; set; }

        // Text book progress
        [Column("current_page")]
        public int CurrentPage { get; set; }

        [Column("total_pages")]
        public int TotalPages { get; set; }

        // Audio book progress
        [Column("audio_chapter")]
        public int AudioChapter { get; set; }   // 0-based chapter index

        [Column("audio_position")]
        public double AudioPosition { get; set; } // seconds into the chapter
    }
}