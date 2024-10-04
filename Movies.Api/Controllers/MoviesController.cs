using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

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
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request,
        CancellationToken token)
    {
        var movie = request.MapToMovie();

        var created =  await _movieService.CreateAsync(movie, token);
        if (created)
        {
            return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie);
        } 
        
        return BadRequest($"movie '{movie.Title}' already exists in the library");
    }
    
    [AllowAnonymous]
    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug,
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
    public async Task<IActionResult> Delete([FromRoute] Guid id,
        CancellationToken token)
    {
        var deleted = await _movieService.DeleteByIdAsync(id, token);
        if (!deleted) return NotFound();
        return Ok();
    }
}