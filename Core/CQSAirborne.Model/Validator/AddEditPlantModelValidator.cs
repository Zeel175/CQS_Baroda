using CQSAirborne.Model.Plant;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Validator
{
    public class AddEditPlantModelValidator : BaseValidator<AddEditPlantModel>
    {
        public override void AddValidation()
        {
            RuleFor(w => w.Name)
                .NotEmpty()
                .WithMessage("Plant Name is required");

            RuleFor(w => w.Alias)
                .NotEmpty()
                .WithMessage("Alias is required");

            RuleFor(w => w.Alias)
                .MaximumLength(25)
                .WithMessage("Alias can have maximum size of 25 characters.");

        }
    }
}
