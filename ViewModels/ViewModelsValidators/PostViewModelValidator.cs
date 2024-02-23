using FluentValidation;

namespace deha_api_exam.ViewModels.ViewModelsValidators
{
    public class PostViewModelValidator : AbstractValidator<PostViewModel>
    {
        public PostViewModelValidator()
        {
            RuleFor(model => model.Title)
                .NotEmpty().WithMessage("Title is required.")
                .Must(mytitlevalidator).WithMessage("Title's length must > 7");




            RuleFor(model => model.Content)
                .NotEmpty().WithMessage("Content is required.");

            RuleFor(model => model.DateCreated)
                .NotEmpty().WithMessage("DateCreated is required.");

            RuleFor(model => model.UserID)
                .NotEmpty().WithMessage("User is required.");

        }

        private bool mytitlevalidator(string title)
        {
            return title.Count() > 7;
        }
    }
}
