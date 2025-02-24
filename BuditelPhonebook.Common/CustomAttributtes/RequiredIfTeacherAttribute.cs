using System.ComponentModel.DataAnnotations;

namespace BuditelPhonebook.Common.CustomAttributes
{
    public class RequiredIfTeacherAttribute : ValidationAttribute
    {
        private readonly string _rolePropertyName;
        private readonly string _requiredRole;

        public RequiredIfTeacherAttribute(string rolePropertyName, string requiredRole, string errorMessage)
        {
            _rolePropertyName = rolePropertyName;
            _requiredRole = requiredRole;
            ErrorMessage = errorMessage;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var roleProperty = validationContext.ObjectType.GetProperty(_rolePropertyName);
            if (roleProperty == null)
            {
                throw new ArgumentException($"Property '{_rolePropertyName}' not found on the object.");
            }

            var roleValue = roleProperty.GetValue(validationContext.ObjectInstance)?.ToString();

            if (roleValue == _requiredRole && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
            }

            return ValidationResult.Success;
        }
    }
}
