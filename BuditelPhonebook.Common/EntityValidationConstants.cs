﻿namespace BuditelPhonebook.Common
{
    public static class EntityValidationConstants
    {
        public static class Person
        {
            public const string BirthDateRegexPattern = @"\d{2}.\d{2}.";
            public const string EmailRegexPattern = @"\S+@buditel\.bg";
            public const string HireAndLeaveDateRegexPattern = @"\d{2}.\d{2}.\d{4}.";
            public const string HireAndLeaveDateFormat = "dd.MM.yyyy.";
        }

        public static class UserRole
        {
            public const string EmailRegexPattern = @"\S+@buditel\.bg";
            public const string UserRoleRegexPattern = "Admin|Moderator";
        }

        public static class ChangeLog
        {
            public const string ChangeLogDateTimeFormat = "dd.MM.yyyy. HH:mm";
        }
    }
}
