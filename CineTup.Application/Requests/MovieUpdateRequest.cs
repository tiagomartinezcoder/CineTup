namespace CineTup.Application.Requests
{
    public class MovieUpdateRequest
    {
        public string? Title { get; set; }
        public string? Director { get; set; }
        public string? Category { get; set; }
        public string? Summary { get; set; }
        public string? ImageUrl { get; set; }
        public string? BannerUrl { get; set; }
        public int? Duration { get; set; }
        public string? Language { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
