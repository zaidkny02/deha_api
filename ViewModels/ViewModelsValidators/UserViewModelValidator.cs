using FluentValidation;

namespace deha_api_exam.ViewModels.ViewModelsValidators
{
    public class UserViewModelValidator : AbstractValidator<UserViewModel>
    {
        public UserViewModelValidator() {
            RuleFor(model => model.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(model => model.FullName)
                .NotEmpty().WithMessage("Fullname is required.");

            RuleFor(model => model.Dob)
                .NotEmpty().WithMessage("Date of birth is required.")
                .WithName("Date of birth");
        }
    }
}
