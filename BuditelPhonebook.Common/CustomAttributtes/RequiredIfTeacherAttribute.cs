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

            var roleValue = roleProperty.GetValue(validationContext.ObjectInstance) as List<string>;
            roleValue = roleValue?.Where(r => r != null).ToList() ?? new List<string>();

            if (roleValue.Any(r => r.Contains(_requiredRole)))
            {
                // Handle collections (Subjects)
                if (value is IEnumerable<string> subjectValues)
                {
                    if (!subjectValues.Any(s => !string.IsNullOrWhiteSpace(s))) // All empty/null
                    {
                        return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
                    }
                }
                // Handle single value (SubjectGroup)
                else if (value is string subjectGroupValue)
                {
                    if (string.IsNullOrWhiteSpace(subjectGroupValue))
                    {
                        return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
                    }
                }
                // Handle unexpected types (e.g., null value)
                else if (value == null)
                {
                    return new ValidationResult(ErrorMessage, new[] { validationContext.MemberName });
                }
            }

            return ValidationResult.Success;
        }

    }
}
