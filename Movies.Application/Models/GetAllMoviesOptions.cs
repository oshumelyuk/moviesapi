namespace Movies.Application.Models;

public class GetAllMoviesOptions
{
    public Guid? UserId { get; set; }
    public string? Title { get; init; }
    public int? YearOfRelease { get; init; }
    public string? SortField { get; init; }
    
    public SortOrder SortOrder { get; init; }
}

public enum SortOrder
{
    Unspecified,
    Ascending,
    Descending
}