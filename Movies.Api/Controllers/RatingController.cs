using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers;

[ApiVersion(1.0)]
[ApiController]
public class RatingController: ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [Authorize]
    [HttpPut(ApiEndpoints.Movies.Rate)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RateMovieAsync(
        [FromRoute] Guid id, 
        [FromBody] RateMovieRequest request,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.RateMovieAsync(id, userId.Value, request.Rating, token);
        return result ? Ok() : NotFound(id);
    }
    
    [Authorize]
    [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRatingAsync(
        [FromRoute] Guid id, 
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.DeleteRatingAsync(id, userId.Value, token);
        return result ? Ok() : NotFound(id);
    }
    
    [Authorize]
    [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
    [ProducesResponseType(typeof(IEnumerable<MovieRatingResponse>),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRatingFourUserAsync(
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.GetRatingsForUserAsync(userId.Value, token);
        return Ok(result.Select(x => x.MapToResponse()));
    }
}