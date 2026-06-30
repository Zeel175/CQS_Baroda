using CQSAirborne.Model.Category;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Validator
{
    public class AddEditCategoryModelValidator : BaseValidator<AddEditCategoryModel>
    {
        public override void AddValidation()
        {
            RuleFor(w => w.Name)
                .NotEmpty()
                .WithMessage("Category Name is required");

            RuleFor(w => w.CategoryTypeId)
                .NotEmpty()
                .WithMessage("Category Type is required");
        }
    }
}
