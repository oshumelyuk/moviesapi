using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator: AbstractValidator<GetAllMoviesOptions>
{
    private static readonly string[] AcceptableSortFields =
    {
        "title",
        "yearofrelease"
    };
    
    public GetAllMoviesOptionsValidator()
    {
        RuleFor(x => x.YearOfRelease)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);

        RuleFor(x => x.SortField)
            .Must(x => x is null || AcceptableSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage("you can only sort by 'title' or 'yearofrelease'");
        
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("page number must be greater than or equal to 1");
        
        RuleFor(x => x.PageSize)
            .LessThanOrEqualTo(25)
            .GreaterThan(0)
            .WithMessage("page size should be between 1 and 25 items");
    }
}