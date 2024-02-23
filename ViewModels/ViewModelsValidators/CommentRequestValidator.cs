using FluentValidation;

namespace deha_api_exam.ViewModels.ViewModelsValidators
{
    public class CommentRequestValidator : AbstractValidator<CommentRequest>
    {
        public CommentRequestValidator()
        {
            RuleFor(model => model.Content)
                .NotEmpty().WithMessage("Content is required.");




            RuleFor(model => model.PostID)
                .NotEmpty().WithMessage("PostID is required.");



        }
    }
}
