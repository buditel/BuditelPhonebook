﻿namespace BuditelPhonebook.Common
{
    public static class EntityValidationMessages
    {
        public static class Person
        {
            public const string FirstNameRequiredMessage = "Трябва да въведете име.";
            public const string FirstNameLengthMessage = "Дължината на името трябва да е между 2 и 50 символа.";
            public const string MiddleNameLengthMessage = "Дължината на презимето трябва да е между 2 и 50 символа.";
            public const string LastNameRequiredMessage = "Трябва да въведете фамилия.";
            public const string LastNameLengthMessage = "Дължината на фамилията трябва да е между 2 и 50 символа.";
            public const string EmailRequiredMessage = "Трябва да въведете служебен имейл.";
            public const string EmailLengthMessage = "Дължината на служебния имейл трябва да е между 2 и 50 символа.";
            public const string EmailWrongFormatMessage = "Имейлът трябва да завършва на buditel.bg";
            public const string EmailUniqueMessage = "Вече съществува такъв имейл.";
            public const string PersonalPhoneNumberRequiredMessage = "Трябва да въведете личен телефон.";
            public const string PersonalPhoneNumberLengthMessage = "Личният телефон трябва да е между 7 и 20 символа.";
            public const string BusinessPhoneNumberLengthMessage = "Служебният телефон трябва да е между 7 и 20 символа.";
            public const string BirthDateWrongFormatMessage = "Рождената дата трябва да е във формата 01.01.";
            public const string HireDateRequiredMessage = "Трябва да въведете дата на постъпване.";
            public const string HireDateWrongFormatMessage = "Датата на постъпване трябва да е във формата 01.01.0001.";
            public const string RoleRequiredMessage = "Трябва да изберете длъжност.";
            public const string DepartmentRequiredMessage = "Трябва да изберете отдел.";
            public const string SubjectGroupRequiredMessage = "Трябва да изберете група предмети.";
            public const string SubjectRequiredMessage = "Трябва да въведете предмет, по който учителят преподава.";
        }

        public static class Role
        {
            public const string NameUniqueMessage = "Вече съществува такава длъжност.";
            public const string NameRequiredMessage = "Трябва да въведете име.";
            public const string NameLengthMessage = "Дължината на името трябва да е между 2 и 100 символа.";
        }

        public static class Department
        {
            public const string NameUniqueMessage = "Вече съществува такъв отдел.";
            public const string NameRequiredMessage = "Трябва да въведете име.";
            public const string NameLengthMessage = "Дължината на името трябва да е между 2 и 100 символа.";
        }

        public static class UserRole
        {
            public const string UserRoleRequiredMessage = "Трябва да изберете роля.";
            public const string EmailRequiredMessage = "Трябва да въведете служебен имейл.";
            public const string EmailLengthMessage = "Дължината на служебния имейл трябва да е между 2 и 50 символа.";
            public const string UserInSuperAdminRoleMessage = "{0} е в роля Супер Админ, не може да му се постави допълнителна роля.";
            public const string UserIsInSameRoleMessage = "{0} вече е в роля {1}";
            public const string EmailWrongFormatMessage = "Имейлът трябва да завършва на buditel.bg";
            public const string RoleWrongFormatMessage = "Ролята може да бъде Администратор или Модератор";
        }
    }
}
