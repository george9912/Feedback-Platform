using FluentValidation;
using static FeedbackService.API.Features.Feedback.Create.Command;

namespace FeedbackService.API.Features.Feedback.Create
{
    public class Validator : AbstractValidator<CreateFeedbackRequest>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");

            RuleFor(x => x.Comment)
                .MaximumLength(2000)
                .WithMessage("Comment cannot exceed 2000 characters.");

            RuleFor(x => x.Visibility)
                .Must(v => Enum.TryParse<Features.Feedback.FeedbackVisibility>(v, true, out _))
                .WithMessage("Visibility must be one of: Public, Private, HROnly.");

            RuleFor(x => x.Tags)
                .Must(tags => tags is null || tags.Length <= 10)
                .WithMessage("You can set up to 10 tags.");

            RuleForEach(x => x.Tags)
                .Must(tag => !string.IsNullOrWhiteSpace(tag))
                .WithMessage("Tags cannot be empty.")
                .MaximumLength(30)
                .WithMessage("Each tag can have at most 30 characters.");
        }
    }
}
