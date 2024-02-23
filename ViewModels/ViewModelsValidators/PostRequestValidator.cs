using FluentValidation;

namespace deha_api_exam.ViewModels.ViewModelsValidators
{
    public class PostRequestValidator : AbstractValidator<PostRequest>
    {
        public PostRequestValidator()
        {
            RuleFor(model => model.Title)
                .NotEmpty().WithMessage("Title is required.")
                .Must(mytitlevalidator).WithMessage("Title's length must > 7");




            RuleFor(model => model.Content)
                .NotEmpty().WithMessage("Content is required.");



        }

        private bool mytitlevalidator(string title)
        {
            return title.Count() > 7;
        }
    }
}
