using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Validator
{
    public abstract class BaseValidator<T> : AbstractValidator<T>
        where T : BaseValidateModel
    {
        public BaseValidator()
        {
            AddValidation();
        }
        public abstract void AddValidation();
    }
}
