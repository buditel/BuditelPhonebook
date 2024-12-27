using BuditelPhonebook.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace BuditelPhonebook.Common.CustomAttributtes
{
    public class RequiredIfTeacherAttribute : ValidationAttribute
    {
        private string _role;

        public RequiredIfTeacherAttribute(string role, string errorMessage)
        {
            _role = role;
            ErrorMessage = errorMessage;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = (CreatePersonViewModel)validationContext.ObjectInstance;

            if (model.Role == _role && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
}
