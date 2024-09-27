using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IRatingRepository
{
    Task<bool>  RateMovieAsync(Guid movieId, Guid userId, int userRating, CancellationToken token);
    Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default);
    Task<(float? Rating, int? UserRating)> GetUserRatingAsync(Guid movieId, Guid userId, CancellationToken token = default);
    Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token);
    Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token = default);
}