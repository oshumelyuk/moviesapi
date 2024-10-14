using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Movies;

public static class GetAllMoviesEndpoint
{
    private const string Name = "GetAllMovies";
    
    public static IEndpointRouteBuilder MapGetAllMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.GetAll, async (
                [AsParameters] GetAllMoviesRequest request,
                IMovieService movieService,
                HttpContext context,
                CancellationToken token
                ) =>
            {
                var userId = context.GetUserId();
                var options = request.MapToOptions().WithUser(userId);
                var movies = await movieService.GetAllAsync(options, token);
                var moviesCount = await movieService.GetCountAsync(options, token);
                return TypedResults.Ok(movies.MapToResponse(options.Page, options.PageSize, moviesCount));
            })
            .WithName(Name)
            .CacheOutput("movies")
            .Produces<MoviesResponse>(StatusCodes.Status200OK);
        
        return app;
    }
}