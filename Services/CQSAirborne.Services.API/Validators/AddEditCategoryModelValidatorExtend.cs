using CQSAirborne.Model.Validator;
using CQSAirborne.Services.Contract;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CQSAirborne.Services.API.Validators
{
    public class AddEditCategoryModelValidatorExtend : AddEditCategoryModelValidator
    {
        public AddEditCategoryModelValidatorExtend(ICategoryService categoryService)
        {
            RuleFor(w => w.Name)
                .Must((w, m) => categoryService.IsCategryNameUnique(m, w.Id))
                .WithMessage("Category Name should be unique");

            RuleFor(w => w.DisplayOrder)
                .Must((w, m) => categoryService.IsCategryDisplayOrderUnique(m, w.Id, w.PrimaryCategoryId))
                .WithMessage("Category Display Order should be unique");
        }
    }
}
