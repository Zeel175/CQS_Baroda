using CQSAirborne.Model.Document;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Validator
{
    public class AddEditDocumentModelValidator : AbstractValidator<AddEditDocumentModel>
    {
        public AddEditDocumentModelValidator()
        {
            RuleFor(w => w.Name)
                .NotEmpty()
                .WithMessage("Document Name is required");

            RuleFor(w => w.CategoryId)
                .NotEmpty()
                .WithMessage("Category is required");

            RuleFor(w => w.DocumentNumber)
                .NotEmpty()
                .WithMessage("Document Number is required");

            RuleFor(w => w.RevisionNumber)
                .NotEmpty()
                .WithMessage("Revision Number is required");

            RuleFor(w => w.Alias)
                .NotEmpty()
                .WithMessage("Alias is required");

            RuleFor(w => w.UploadDate)
                .NotEmpty()
                .WithMessage("Upload date is required");

        }
    }
}
