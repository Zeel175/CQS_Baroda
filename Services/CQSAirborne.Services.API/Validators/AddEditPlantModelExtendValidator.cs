using CQSAirborne.Model.Validator;
using CQSAirborne.Services.Contract;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Services.API.Validators
{
    public class AddEditPlantModelExtendValidator : AddEditPlantModelValidator
    {
        public AddEditPlantModelExtendValidator(IPlantService plantService)
        {
            RuleFor(w => w.DisplayOrder)
                .Must((model, value) => plantService.IsDisplayOrderUnique(model.Id, value))
                .WithMessage("Display order must be unique");
        }
    }
}
