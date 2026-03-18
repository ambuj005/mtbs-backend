namespace MovieTicketBooking.Api.DTO;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
}
