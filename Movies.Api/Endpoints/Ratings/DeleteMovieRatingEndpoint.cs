using Movies.Api.Auth;
using Movies.Application.Services;

namespace Movies.Api.Endpoints.Ratings;

public static class DeleteMovieRatingEndpoint
{
    private const string Name = "DeleteMovieRating";

    public static IEndpointRouteBuilder MapDeleteMovieRating(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.DeleteRating, async (
            Guid id,
            HttpContext context,
            IRatingService ratingService,
            CancellationToken token) =>
        {
            var userId = context.GetUserId();
            var result = await ratingService.DeleteRatingAsync(id, userId.Value, token);
            return result ? Results.Ok() : Results.NotFound(id);
        })
        .WithName(Name)
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
        return app;
    }
}