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

    public static MoviesResponse MapToResponse (this IEnumerable<Movie> movies)
    {
        return new MoviesResponse()
        {
            Items = movies.Select(MapToMovieResponse)
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
}