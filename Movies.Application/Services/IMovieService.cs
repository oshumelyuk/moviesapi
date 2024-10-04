using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
    public Task<bool> CreateAsync(Movie movie, CancellationToken token);
    public Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken token);
    public Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken token);
    public Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token);
    public Task<Movie?> UpdateAsync(Movie movie, Guid? userId, CancellationToken token); 
    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken token);
    public Task<int> GetCountAsync(GetAllMoviesOptions options, CancellationToken token);
}
