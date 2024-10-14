using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Ratings;

public static class GetUserRatingsEndpoint
{
    private const string Name = "GetUserRatings";

    public static IEndpointRouteBuilder MapGetUserRatings(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Ratings.GetUserRatings, async (
                HttpContext context,
                IRatingService ratingService,
                CancellationToken token
            ) =>
            {
                var userId = context.GetUserId();
                var result = await ratingService.GetRatingsForUserAsync(userId.Value, token);
                return TypedResults.Ok(result.Select(x => x.MapToResponse()));
            })
            .WithName(Name)
            .RequireAuthorization()
            .Produces<IEnumerable<MovieRatingResponse>>(StatusCodes.Status200OK);
        return app;
    }
}