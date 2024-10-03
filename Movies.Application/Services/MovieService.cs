using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IValidator<Movie> _movieValidator;
    private readonly IValidator<GetAllMoviesOptions> _getAllMoviesOptionsValidator;

    public MovieService(IMovieRepository movieRepository, 
        IValidator<Movie> movieValidator, 
        IRatingRepository ratingRepository, 
        IValidator<GetAllMoviesOptions> getAllMoviesOptionsValidator)
    {
        _movieRepository = movieRepository;
        _movieValidator = movieValidator;
        _ratingRepository = ratingRepository;
        _getAllMoviesOptionsValidator = getAllMoviesOptionsValidator;
    }
    
    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie);
        return await _movieRepository.CreateAsync(movie, token);
    }

    public Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
    {
        return _movieRepository.GetByIdAsync(id, userId, token);
    }

    public Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
    {
        return _movieRepository.GetBySlugAsync(slug, userId, token);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token)
    {
        await _getAllMoviesOptionsValidator.ValidateAndThrowAsync(options);
        
        return await _movieRepository.GetAllAsync(options, token);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken token = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie);
        
        if (!await _movieRepository.ExistsByIdAsync(movie.Id))
            return null;   
        
        await _movieRepository.UpdateAsync(movie, token);
        if (userId.HasValue)
        {
            var (rating, userRating) = await _ratingRepository.GetUserRatingAsync(movie.Id, userId.Value);
            movie.UserRating = userRating;
            movie.Rating = rating;
        }
        else
        {
            movie.Rating = await _ratingRepository.GetRatingAsync(movie.Id);
        }

        return movie;
    }

    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        return _movieRepository.DeleteByIdAsync(id, token);
    }
}

