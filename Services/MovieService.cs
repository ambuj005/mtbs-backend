using MovieTicketBooking.Api.DTO;
using MovieTicketBooking.Api.Interfaces;
using MovieTicketBooking.Api.Models;

namespace MovieTicketBooking.Api.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _repo;
    private readonly ICityMovieRepository _cityMovieRepo;

    public MovieService(
        IMovieRepository repo,
        ICityMovieRepository cityMovieRepo)
    {
        _repo = repo;
        _cityMovieRepo = cityMovieRepo;
    }
 public async Task<IEnumerable<MovieListItemDTO>> GetMoviesByCityAsync(string cityId)
    {
         const int page = 1;
        const int pageSize = 100;

        var cityMovies = await _cityMovieRepo
            .GetByCityPagedAsync(cityId, page, pageSize);

        if (!cityMovies.Items.Any())
            return Enumerable.Empty<MovieListItemDTO>();

        var movieIds = cityMovies.Items
            .Select(x => x.MovieId)
            .ToList();

        var movies = await _repo.QueryAsync(q =>
            q.Where(m => movieIds.Contains(m.Id)));

        return movies
            .OrderBy(m => m.Title)
            .Select(m =>
            {
                var cm = cityMovies.Items.First(x => x.MovieId == m.Id);
                return ToListItem(m, cm.TheatreCount);
            })
            .ToList();
    }

    public async Task<PaginatedMoviesResponse> GetMoviesByCityPaginatedAsync(
        string cityId,
        int page,
        int pageSize,
        string? search,
        string? genre,
        string? language)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

        var cityMovies = await _cityMovieRepo.GetByCityPagedAsync(
            cityId,
            page,
            pageSize
        );

        if (!cityMovies.Items.Any())
            return new PaginatedMoviesResponse();

        var movieIds = cityMovies.Items
            .Select(x => x.MovieId)
            .ToList();

        var movies = await _repo.QueryAsync(q =>
            q.Where(m => m.IsActive && movieIds.Contains(m.Id))
        );

        if (!string.IsNullOrWhiteSpace(search))
            movies = movies.Where(m =>
                m.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(language))
            movies = movies.Where(m => m.Language == language);

        if (!string.IsNullOrWhiteSpace(genre))
            movies = movies.Where(m => m.Genre.Contains(genre));

        var totalPages = (int)Math.Ceiling(
            cityMovies.TotalCount / (double)pageSize);

        return new PaginatedMoviesResponse
        {
            Movies = movies
                .OrderByDescending(m => m.ReleaseDate)
                .Select(m =>
                {
                    var cm = cityMovies.Items.First(x => x.MovieId == m.Id);
                    return ToListItem(m, cm.TheatreCount);
                })
                .ToList(),

            Page = page,
            PageSize = pageSize,
            TotalCount = cityMovies.TotalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

   public async Task<IEnumerable<MovieListItemDTO>> SearchMoviesByCityAsync(
        string cityId,
        string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Enumerable.Empty<MovieListItemDTO>();

        var movies = await GetMoviesByCityAsync(cityId);

        return movies
            .Where(m => m.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(m => m.Title);
    }

    public async Task<Movie?> GetMovieByIdAsync(string id)
        => (await _repo.QueryAsync(q => q.Where(m => m.Id == id)))
            .FirstOrDefault();

    private static MovieListItemDTO ToListItem(
        Movie m, int theatreCount)
    {
        return new MovieListItemDTO
        {
            Id = m.Id,
            Title = m.Title,
            PosterUrl = m.PosterUrl,
            BannerUrl = m.BannerUrl,
            TrailerUrl = m.TrailerUrl,
            Language = m.Language,
            Duration = m.Duration,
            Rating = m.Rating,
            ImdbRating = (double)(m.ImdbRating ?? 0),
            ReleaseDate = m.ReleaseDate,
            Genre = m.Genre,
            AvailableTheatresCount = theatreCount
        };
    }
}
