using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Movies;

public static class UpdateMovieEndpoint
{
    private const string Name = "UpdateMovie";

    public static IEndpointRouteBuilder MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Update, async (
            Guid id,
            UpdateMovieRequest request,
            IMovieService movieService,
            IOutputCacheStore outputCacheStore,
            HttpContext context,
            CancellationToken token) =>
        {
            var userId =  context.GetUserId();
            var movie = request.MapToMovie(id);
            var updated = await movieService.UpdateAsync(movie, userId, token);
            if (updated is null) return Results.NotFound();
            await outputCacheStore.EvictByTagAsync("movies", token);
            return TypedResults.Ok(updated.MapToMovieResponse());
        })
        .WithName(Name)
        .RequireAuthorization(AuthConstants.TrustedMemberPolicyName)
        .Produces<MovieResponse>(StatusCodes.Status200OK)
        .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
        return app;
    }
}