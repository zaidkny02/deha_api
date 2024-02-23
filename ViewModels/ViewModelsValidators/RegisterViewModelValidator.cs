using FluentValidation;

namespace deha_api_exam.ViewModels.ViewModelsValidators
{
    public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterViewModelValidator()
        {
            RuleFor(model => model.UserName)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(model => model.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches("[!@#$%^&*(),.?\":{}|<>]").WithMessage("Password must contain at least one special character.");

            RuleFor(model => model.ConfirmPassword)
                .NotEmpty().WithMessage("ConfirmPassword is required.")
                .Equal(model => model.Password).WithMessage("Password and confirmation do not match.");

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
