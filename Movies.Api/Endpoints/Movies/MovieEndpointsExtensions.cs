namespace Movies.Api.Endpoints.Movies;

public static class MovieEndpointsExtensions
{
    public static IEndpointRouteBuilder MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
         app.MapCreateMovie();
         app.MapUpdateMovie();
         app.MapDeleteMovie();
         app.MapGetMovie();
         app.MapGetAllMovies();
        return app;
    }
}