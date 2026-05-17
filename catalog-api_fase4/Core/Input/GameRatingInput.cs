using System.ComponentModel.DataAnnotations;

namespace Core.Input
{
    public class GameRatingInput
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }
    }
}
