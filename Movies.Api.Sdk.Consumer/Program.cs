using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Movies.Contracts.Requests;
using Refit;

// direct rest service
//var moviesApi = RestService.For<IMoviesApi>("http://localhost:5021");

// rest service factory
var services = new ServiceCollection();
services
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(s => new RefitSettings()
    {
        AuthorizationHeaderValueGetter = async (m,t) => await s.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
    })
    .ConfigureHttpClient(x => x.BaseAddress = new Uri("http://localhost:5021"));
var provider = services.BuildServiceProvider();
var moviesApi = provider.GetRequiredService<IMoviesApi>();

var movies = await moviesApi.GetMoviesAsync(new GetAllMoviesRequest
{
    Page = 1,
    PageSize = 10,
    SortBy = "title"
});
Console.WriteLine(JsonSerializer.Serialize(movies));

await moviesApi.CreateMovieAsync(new CreateMovieRequest()
{
    Title = "Star Wars",
    YearOfRelease = 2019,
    Genres = new List<string>
    {
        "Action",
        "Adventure"
    }
});

var movie = await moviesApi.GetMovieAsync("star-wars-2019");
Console.WriteLine(JsonSerializer.Serialize(movie, options: JsonSerializerOptions.Web));

await moviesApi.RateMovieAsync(movie.Id, new RateMovieRequest()
{
    Rating = 5
});

 movie = await moviesApi.GetMovieAsync("star-wars-2019");
Console.WriteLine("Movie rated {0}", JsonSerializer.Serialize(movie, options: JsonSerializerOptions.Web));

var myRatings = await moviesApi.GetUserRatingsAsync();
Console.WriteLine("My ratings {0}", JsonSerializer.Serialize(myRatings, options: JsonSerializerOptions.Web));

await moviesApi.DeleteRatingAsync(movie.Id);

movie = await moviesApi.GetMovieAsync("star-wars-2019");
Console.WriteLine("Movie unrated {0}", JsonSerializer.Serialize(movie, options: JsonSerializerOptions.Web));

await moviesApi.DeleteMovieAsync(movie.Id.ToString());
try
{
    var deletedMovie = await moviesApi.GetMovieAsync("star-wars-2019");
    Console.WriteLine("Movie still exists");
}
catch (ValidationApiException e)
{
    if (e.StatusCode == HttpStatusCode.NotFound)
    {
        Console.WriteLine($"movie deleted");
    }
    else
    {
        throw;
    }
}

    