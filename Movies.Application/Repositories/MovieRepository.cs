using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    
    public async Task<bool> CreateAsync(Movie movie, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(
            new CommandDefinition("""
                                  INSERT INTO movies(id, slug, title, yearofrelease) 
                                  VALUES (@Id, @Slug, @Title, @YearOfRelease)
                                  """, movie, cancellationToken: token));
        
        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition("""
                                          INSERT INTO genres(movieId, name)
                                          VALUES (@MovieId, @Name)
                                          """, new {MovieId = movie.Id, Name = genre}, cancellationToken: token));
            }
        }

        transaction.Commit();
        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                  SELECT * from movies where id = @id
                                  """, new { id }, cancellationToken: token));
        if (movie is null) return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
                                  SELECT name from genres where movieId = @id
                                  """, new { id }, cancellationToken: token));
        movie.Genres.AddRange(genres);
        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                  SELECT * from movies where slug = @slug
                                  """, new { slug }, cancellationToken: token));
        if (movie is null) return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
                                  SELECT name from genres where movieId = @id
                                  """, new { movie.Id }, cancellationToken: token));
        movie.Genres.AddRange(genres);
        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var result = await connection.QueryAsync(
            new CommandDefinition("""
                                  SELECT m.*, string_agg(g.name, ',') as genres
                                  from movies m left join genres g on m.id = g.movieId
                                  group by id
                                  """, cancellationToken: token ));

        return result.Select(x => new Movie()
        {
             Id = x.id,
             Title = x.title,
             YearOfRelease = x.yearofrelease,
             Genres = string.IsNullOrEmpty(x.genres) 
                 ? new List<string>()
                 : Enumerable.ToList(x.genres.Split(',')) 
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        
        var deleted = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition("""
                                  delete from genres where movieid = @id
                                  """, new { id = movie.Id }, cancellationToken: token));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(
                new CommandDefinition("""
                                      insert into genres(movieid, name)
                                      values (@MovieId, @Name)
                                      """, new { MovieId = movie.Id, Name = genre}, cancellationToken: token));
        }

        var result = await connection.ExecuteAsync(
            new CommandDefinition("""
                                  update movies set slug = @Slug, title = @Title, yearofrelease = @YearOfRelease
                                  where id = @Id
                                  """, movie, cancellationToken: token));
        
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(
            new CommandDefinition("""
                                  delete from genres where movieid = @id
                                  """, new { id }, cancellationToken: token));
        
        var deleted = await connection.ExecuteAsync(
            new CommandDefinition("""
                                  delete from movies where id = @id
                                  """, new { id }, cancellationToken: token));
        transaction.Commit();
        return deleted > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition("""
                                  select count(1) from movies where id = @id
                                  """, new {id}));
    }
}