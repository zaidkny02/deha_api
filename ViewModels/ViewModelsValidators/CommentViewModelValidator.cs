using FluentValidation;

namespace deha_api_exam.ViewModels.ViewModelsValidators
{
    public class CommentViewModelValidator : AbstractValidator<CommentViewModel>
    {
        public CommentViewModelValidator() {
            RuleFor(model => model.Content)
                .NotEmpty().WithMessage("Content is required.");




            RuleFor(model => model.PostID)
                .NotEmpty().WithMessage("PostID is required.");
        }
    }
}
