using CQSAirborne.Model.Validator;
using CQSAirborne.Services.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace CQSAirborne.Services.API.Validators
{
    public class AddEditDocumentModelExtendValidator : AddEditDocumentModelValidator
    {
        public AddEditDocumentModelExtendValidator(IDocumentService documentService)
        {
            RuleFor(w => w.DocumentNumber)
                .Must((w, model) => documentService.IsDocumentNumberUnique(w.Id, model))
                .When(w => w.Id > 0)
                .WithMessage("Document Number must be unique");

            RuleFor(w => w.Uploads)
                .Must(w => w != null && w.Count > 0)
                .WithName("Please upload document to save document");

            RuleFor(w => w.Uploads)
               .Must((m, w) =>
               {
                   return w.SelectMany(s => s.Plants).GroupBy(g => g.Id).All(c => c.Count() == 1);
               })
               .WithMessage("Duplicate plant cannot be added for same document");
        }
    }
}
