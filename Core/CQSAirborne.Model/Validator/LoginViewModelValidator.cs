using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Validator
{
    public class LoginViewModelValidator : BaseValidator<LoginViewModel>
    {
        public override void AddValidation()
        {
            RuleFor(w => w.UserName)
                .NotEmpty()
                .WithMessage("Username is required");

            RuleFor(w => w.Password)
                .NotEmpty()
                .WithMessage("Password is required");

        }
    }
}
