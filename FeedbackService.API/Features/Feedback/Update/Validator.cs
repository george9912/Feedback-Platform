using FluentValidation;
using static FeedbackService.API.Features.Feedback.Update.Command;

namespace FeedbackService.API.Features.Feedback.Update
{
    public sealed class Validator : AbstractValidator<UpdateFeedbackRequest>
    {
        public Validator()
        {
            RuleFor(x => x.Rating).InclusiveBetween(1, 5);
            RuleFor(x => x.Comment).MaximumLength(2000);
        }
    }
}
