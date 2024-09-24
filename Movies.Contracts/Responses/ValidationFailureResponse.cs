namespace Movies.Contracts.Responses;

public class ValidationFailureResponse
{
    public IEnumerable<ValidationResponse> Errors { get; init; } = Enumerable.Empty<ValidationResponse>();
}

public class ValidationResponse
{
    public required string PropertyName { get; init; }
    public required string Message { get; init; }
}
