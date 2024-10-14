using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Movies;

public static class CreateMovieEndpoint
{
    public const string Name = "CreateMovie";

    public static IEndpointRouteBuilder MapCreateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Movies.Create, async (
            CreateMovieRequest request,
            IMovieService movieService,
            IOutputCacheStore outputCacheStore,
            CancellationToken token) =>
        {
            var movie = request.MapToMovie();

            var created = await movieService.CreateAsync(movie, token);
            if (created)
            {
                await outputCacheStore.EvictByTagAsync("movies", token);
                var movieResponse = movie.MapToMovieResponse();
                return TypedResults.CreatedAtRoute(movieResponse, GetMovieEndpoint.Name, new { idOrSlug = movie.Id });
            }

            return Results.BadRequest($"movie '{movie.Title}' already exists in the library");
        })
        .WithName(Name)
        .RequireAuthorization(AuthConstants.AdminUserPolicyName)
        .Produces<MovieResponse>(StatusCodes.Status201Created)
        .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest);
        return app;
    }
}