using System.Data.Common;
using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    
    public RatingRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int userRating, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        
        var result = await connection.ExecuteAsync(
            new CommandDefinition("""
                                  insert into ratings (userid, movieid, rating) 
                                  values (@userId, @movieId, @userRating)
                                  on conflict (userid, movieid) do update 
                                    set rating = @userRating
                                  """, new {userId, movieId, userRating}, cancellationToken: token));
        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var rating = await connection.ExecuteAsync(
            new CommandDefinition("""
                                  select round(avg(r.rating),1)
                                  from ratings r
                                  where r.movieid = @movieId
                                  """, new {movieId}, cancellationToken: token));

        return rating;
    }

    public async Task<(float? Rating, int? UserRating)> GetUserRatingAsync(Guid movieId, Guid userId, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(
            new CommandDefinition("""
                                  select round(avg(r.rating),1), 
                                         (
                                             select rating
                                             from ratings rx
                                             where rx.movieid = @movieId and rx.userid = @userId
                                             limit 1
                                         )
                                  from ratings r
                                  where r.movieid = @movieId
                                  """, new {movieId, userId}, cancellationToken: token));
        
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var isDeleted = await connection.ExecuteAsync(
            new CommandDefinition("""
                                  DELETE 
                                  from ratings
                                  where ratings.movieid = @movieId and ratings.userid = @userId
                                  """, new { movieId, userId }, cancellationToken: token));
        return isDeleted > 0;
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken token)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var result = await connection.QueryAsync<MovieRating>(
            new CommandDefinition("""
                                  Select r.rating as rating, r.movieid as movieId, m.slug as slug 
                                  from ratings r
                                  inner join movies m on r.movieid = m.id
                                  where r.userid = @userId
                                  """, new {userId}, cancellationToken: token));
        return result;
    }
}