using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IMovieRepository
{
    public Task<bool> CreateAsync(Movie movie, CancellationToken token);
    public Task<Movie?> GetByIdAsync(Guid id, CancellationToken token);
    public Task<Movie?> GetBySlugAsync(string slug, CancellationToken token);
    public Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token);
    public Task<bool> UpdateAsync(Movie movie, CancellationToken token); 
    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken token);
    public Task<bool> ExistsByIdAsync(Guid id);
}