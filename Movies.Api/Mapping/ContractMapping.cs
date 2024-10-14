using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
    public static MovieResponse MapToMovieResponse(this Movie movie)
    {
        return new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            YearOfRelease = movie.YearOfRelease,
            Genres = movie.Genres,
            Slug = movie.Slug,
            Rating = movie.Rating,
            UserRating = movie.UserRating
        };
    }

    public static Movie MapToMovie(this CreateMovieRequest movieRequest)
    {
        return new Movie()
        {
            Id = Guid.NewGuid(),
            Title = movieRequest.Title,
            YearOfRelease = movieRequest.YearOfRelease,
            Genres = movieRequest.Genres.ToList(),
        };
    }
    
    public static Movie MapToMovie(this UpdateMovieRequest movieRequest, Guid id)
    {
        return new Movie()
        {
            Id = id,
            Title = movieRequest.Title,
            YearOfRelease = movieRequest.YearOfRelease,
            Genres = movieRequest.Genres.ToList(),
        };
    }

    public static MoviesResponse MapToResponse (this IEnumerable<Movie> movies, int page, int pageSize, int totalCount)
    {
        return new MoviesResponse()
        {
            Items = movies.Select(MapToMovieResponse),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public static MovieRatingResponse MapToResponse(this MovieRating movieRating)
    {
        return new MovieRatingResponse()
        {
            MovieId = movieRating.MovieId,
            Slug = movieRating.Slug,
            Rating = movieRating.Rating
        };
    }

    public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
    {
        return new GetAllMoviesOptions()
        {
            Title = request.Title,
            YearOfRelease = request.Year,
            SortField = request.SortBy?.TrimStart('+', '-'),
            SortOrder = request.SortBy is null ? SortOrder.Unspecified : 
                request.SortBy.StartsWith('-')? SortOrder.Descending: SortOrder.Ascending,
            Page = request.Page.GetValueOrDefault(PagedRequest.DefaultPage),
            PageSize = request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize)
        };
    }

    public static GetAllMoviesOptions WithUser(this GetAllMoviesOptions options, Guid? userId)
    {
        options.UserId = userId;
        return options;
    }
}