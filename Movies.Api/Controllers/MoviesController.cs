using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Asp.Versioning;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers;

[ApiVersion(1.0)]
[ApiVersion(2.0)]
[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request,
        CancellationToken token)
    {
        var movie = request.MapToMovie();

        var created =  await _movieService.CreateAsync(movie, token);
        if (created)
        {
            return CreatedAtAction(nameof(GetV1), new { idOrSlug = movie.Id }, movie);
        } 
        
        return BadRequest($"movie '{movie.Title}' already exists in the library");
    }
    
    [MapToApiVersion(1.0)]
    [AllowAnonymous]
    [HttpGet(ApiEndpoints.Movies.Get)]
    [ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetV1([FromRoute] string idOrSlug,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        
        var movie = Guid.TryParse(idOrSlug, out Guid id)
            ? await _movieService.GetByIdAsync(id, userId, token)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, token);

        if (movie is null) return NotFound();
        return Ok(movie.MapToMovieResponse());
    }
    
    [AllowAnonymous]
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    [ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", VaryByQueryKeys = new []{"title", "sortby", "yearofrelease", "pagesize", "page"}, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllMoviesRequest request,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var options = request.MapToOptions().WithUser(userId);
        var movies = await _movieService.GetAllAsync(options, token);
        var moviesCount = await _movieService.GetCountAsync(options, token);
        return Ok(movies.MapToResponse(options.Page, options.PageSize, moviesCount));
    }
    
    [HttpPut(ApiEndpoints.Movies.Update)]
    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType( StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, 
        [FromBody] UpdateMovieRequest request,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var movie = request.MapToMovie(id);
        var updated = await _movieService.UpdateAsync(movie, userId, token);
        if (updated is null) return NotFound();
        return Ok(updated.MapToMovieResponse());
    }
    
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    [Authorize(AuthConstants.AdminUserPolicyName)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType( StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id,
        CancellationToken token)
    {
        var deleted = await _movieService.DeleteByIdAsync(id, token);
        if (!deleted) return NotFound();
        return Ok();
    }
}