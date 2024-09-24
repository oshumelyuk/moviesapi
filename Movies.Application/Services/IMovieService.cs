using Movies.Application.Models;

namespace Movies.Application.Services;

public interface IMovieService
{
    public Task<bool> CreateAsync(Movie movie, CancellationToken token);
    public Task<Movie?> GetByIdAsync(Guid id, CancellationToken token);
    public Task<Movie?> GetBySlugAsync(string slug, CancellationToken token);
    public Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token);
    public Task<Movie?> UpdateAsync(Movie movie, CancellationToken token); 
    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken token);
}
