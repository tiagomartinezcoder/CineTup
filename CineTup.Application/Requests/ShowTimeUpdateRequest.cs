namespace CineTup.Application.Requests
{
    public class ShowTimeUpdateRequest
    {
        public int? MovieId { get; set; }
        public DateTime? StartTime { get; set; }
        public decimal? TicketPrice { get; set; }
    }
}
