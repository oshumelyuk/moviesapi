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
    
    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(
            new CommandDefinition($"""
                                   INSERT INTO movies(id, slug, title, yearofrelease) 
                                   VALUES (@Id, @Slug, @Title, @YearOfRelease)
                                   """, movie, cancellationToken: token));
        
        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition($"""
                                           INSERT INTO genres(movieId, name)
                                           VALUES (@MovieId, @Name)
                                           """, new {MovieId = movie.Id, Name = genre}, cancellationToken: token));
            }
        }

        transaction.Commit();
        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition($"""
                                   SELECT m.*, round (avg(r.rating), 1) as rating, myr.rating as userrating
                                   from movies m 
                                   left join ratings r on m.id = r.movieid
                                   left join ratings myr on m.id = myr.movieid and myr.userid = @userId
                                       
                                   where m.id = @id
                                   group by id, userrating
                                   """, new { id, userId }, cancellationToken: token));
        if (movie is null) return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition($"""
                                   SELECT name from genres where movieId = @id
                                   """, new { id }, cancellationToken: token));
        movie.Genres.AddRange(genres);
        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition($"""
                                   SELECT m.*, round (avg(r.rating), 1) as rating, myr.rating as userrating
                                   from movies m 
                                   left join ratings r on m.id = r.movieid
                                   left join ratings myr on m.id = myr.movieid and myr.userid = @userId
                                       
                                   where m.slug = @slug
                                   group by id, userrating
                                   """, new { slug, userId }, cancellationToken: token));
        if (movie is null) return null;

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition($"""
                                   SELECT name from genres where movieId = @id
                                   """, new { movie.Id }, cancellationToken: token));
        movie.Genres.AddRange(genres);
        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var offset = (options.Page - 1) * options.PageSize; 
        var orderClause = string.Empty;
        if (options.SortField is not null)
        {
            orderClause = $"""
                           , m.{options.SortField.ToLower()}
                           order by m.{options.SortField.ToLower()} 
                           {(options.SortOrder == SortOrder.Descending ? "DESC" : "ASC")}
                           """;
        }

        var result = await connection.QueryAsync(
            new CommandDefinition($"""
                                   SELECT m.*, 
                                            string_agg(distinct g.name, ',') as genres,
                                            round (avg(r.rating), 1) as rating, 
                                            myr.rating as userrating
                                   from movies m 
                                   left join genres g on m.id = g.movieId
                                   left join ratings r on m.id = r.movieid
                                   left join ratings myr on m.id = myr.movieid and myr.userid = @userId
                                   where (@title is null or LOWER(m.title) like ('%' || @title || '%'))
                                   and (@yearOfRelease is null or m.yearofrelease = @yearOfRelease)
                                   group by id, userrating {orderClause}
                                   limit @pagesize offset @offset 
                                   """, new {userId = options.UserId, 
                                            title = options.Title?.ToLower(), 
                                            yearOfRelease = options.YearOfRelease,
                                            pagesize = options.PageSize,
                                            offset }, 
                                    cancellationToken: token ));

        return result.Select(x => new Movie()
        {
             Id = x.id,
             Title = x.title,
             YearOfRelease = x.yearofrelease,
             Genres = string.IsNullOrEmpty(x.genres) 
                 ? new List<string>()
                 : Enumerable.ToList(x.genres.Split(',')) ,
             Rating = (float?) x.rating,
             UserRating = x.userrating
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();
        
        var deleted = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition($"""
                                   delete from genres where movieid = @id
                                   """, new { id = movie.Id }, cancellationToken: token));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(
                new CommandDefinition($"""
                                       insert into genres(movieid, name)
                                       values (@MovieId, @Name)
                                       """, new { MovieId = movie.Id, Name = genre}, cancellationToken: token));
        }

        var result = await connection.ExecuteAsync(
            new CommandDefinition($"""
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
            new CommandDefinition($"""
                                   delete from genres where movieid = @id
                                   """, new { id }, cancellationToken: token));
        
        var deleted = await connection.ExecuteAsync(
            new CommandDefinition($"""
                                   delete from movies where id = @id
                                   """, new { id }, cancellationToken: token));
        transaction.Commit();
        return deleted > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition($"""
                                   select count(1) from movies where id = @id
                                   """, new {id}));
    }

    public async Task<int> GetCountAsync(GetAllMoviesOptions options, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var result = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition($"""
                                   SELECT count(1) 
                                   from movies m 
                                   where (@title is null or LOWER(m.title) like ('%' || @title || '%'))
                                   and (@yearOfRelease is null or m.yearofrelease = @yearOfRelease)
                                   """, new { 
                                            title = options.Title?.ToLower(), 
                                            yearOfRelease = options.YearOfRelease,
                                            }, 
                                    cancellationToken: token ));
        return result;
    }
}