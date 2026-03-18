namespace MovieTicketBooking.Api.Models
{
    public class CityMovie : BaseEntity
    {
        public string CityId { get; set; } = default!;
        public string MovieId { get; set; } = default!;
        public int TheatreCount { get; set; }
        public string PartitionKey => CityId;
    }

}
