using CQSAirborne.Model.Employee;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace CQSAirborne.Model.Validator
{
    public class AddEditEmployeeViewModelValidator : AbstractValidator<AddEditEmployeeViewModel>
    {
        public AddEditEmployeeViewModelValidator()
        {
            RuleFor(w => w.EmpId).NotEmpty();

            RuleFor(w => w.EmployeeName).NotEmpty();
        }
    }
}
