using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Interfaces;

public interface IMovieService
{
    Task<IEnumerable<MovieListItemDTO>> GetMoviesByCityAsync(string cityId);

    Task<PaginatedMoviesResponse> GetMoviesByCityPaginatedAsync(
    string cityId,
    int page,
    int pageSize,
    string? search,
    string? genre,
    string? language
);

    Task<IEnumerable<MovieListItemDTO>> SearchMoviesByCityAsync(
        string cityId,
        string query);

    Task<Movie?> GetMovieByIdAsync(string id);
}




//using MovieTicketBooking.Api.DTO;
//using MovieTicketBooking.Api.Models;

//namespace MovieTicketBooking.Api.Interfaces;

//public interface IMovieService
//{ 
//    Task<IEnumerable<MovieListItemDTO>> GetMoviesByCityAsync(string cityId);

//    Task<PaginatedMoviesResponse> GetMoviesByCityPaginatedAsync(string cityId, int page = 1, int pageSize = 4);

//    Task<IEnumerable<MovieListItemDTO>> SearchMoviesByCityAsync(string cityId, string query);
//    Task<Movie?> GetMovieByIdAsync(string id);
//}