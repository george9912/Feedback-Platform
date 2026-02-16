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
        }
    }
}
