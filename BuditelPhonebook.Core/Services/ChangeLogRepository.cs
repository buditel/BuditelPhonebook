using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Person;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using static BuditelPhonebook.Common.EntityValidationConstants.Person;

namespace BuditelPhonebook.Core.Services
{
    public class ChangeLogRepository : IChangeLogRepository
    {
        private readonly ApplicationDbContext _context;

        public ChangeLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddChangeAsync(ChangeLog change)
        {
            await _context.AddAsync(change);
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GenerateChangeDescription(Person oldPerson, EditPersonViewModel newPerson)
        {
            var changes = new List<string>();

            if (oldPerson.FirstName != newPerson.FirstName)
            {
                changes.Add($"Редактирано първо име: {oldPerson.FirstName} -> {newPerson.FirstName}");
            }

            if (oldPerson.MiddleName != newPerson.MiddleName)
            {
                if (oldPerson.MiddleName == null)
                {
                    changes.Add($"Добавено второ име: {newPerson.MiddleName}");
                }
                else if (newPerson.MiddleName == null)
                {
                    changes.Add($"Премахнато второ име - {oldPerson.MiddleName}");
                }
                else
                {
                    changes.Add($"Редактирано второ име: {oldPerson.MiddleName} -> {newPerson.MiddleName}");
                }
            }

            if (oldPerson.LastName != newPerson.LastName)
            {
                changes.Add($"Редактирана фамилия: {oldPerson.LastName} -> {newPerson.LastName}");
            }

            if (oldPerson.PersonalPhoneNumber != newPerson.PersonalPhoneNumber)
            {
                changes.Add($"Редактиран личен телефон: {oldPerson.PersonalPhoneNumber} -> {newPerson.PersonalPhoneNumber}");
            }

            if (oldPerson.BusinessPhoneNumber != newPerson.BusinessPhoneNumber)
            {
                if (oldPerson.BusinessPhoneNumber == null)
                {
                    changes.Add($"Добавен служебен телефон: {newPerson.BusinessPhoneNumber}");

                }
                else if (newPerson.BusinessPhoneNumber == null)
                {
                    changes.Add($"Премахнат служебен телефон - {oldPerson.BusinessPhoneNumber}");
                }
                else
                {
                    changes.Add($"Редактиран служебен телефон: {oldPerson.BusinessPhoneNumber} -> {newPerson.BusinessPhoneNumber}");
                }
            }

            if (oldPerson.Email != newPerson.Email)
            {
                changes.Add($"Редактиран служебен имейл: {oldPerson.Email} -> {newPerson.Email}");
            }

            if (oldPerson.Role.Name != newPerson.Role)
            {
                changes.Add($"Редактирана длъжност: {oldPerson.Role.Name} -> {newPerson.Role}");
            }

            if (oldPerson.Department.Name != newPerson.Department)
            {
                changes.Add($"Редактирана отдел: {oldPerson.Department.Name} -> {newPerson.Department}");
            }

            if (oldPerson.SubjectGroup != newPerson.SubjectGroup)
            {
                if (oldPerson.SubjectGroup == null)
                {
                    changes.Add($"Добавена група предмети: {newPerson.SubjectGroup}");
                }
                else if (newPerson.SubjectGroup == null)
                {
                    changes.Add($"Добавена група предмети - {oldPerson.SubjectGroup}");
                }
                else
                {
                    changes.Add($"Редактирана група предмети: {oldPerson.SubjectGroup} -> {newPerson.SubjectGroup}");
                }
            }

            if (oldPerson.Subject != newPerson.Subject)
            {
                if (oldPerson.Subject == null)
                {
                    changes.Add($"Добавен предмет: {newPerson.Subject}");
                }
                else if (newPerson.Subject == null)
                {
                    changes.Add($"Премахнат предмет - {oldPerson.Subject}");
                }
                else
                {
                    changes.Add($"Редактиран предмет: {oldPerson.Subject} -> {newPerson.Subject}");
                }
            }

            if (oldPerson.Birthdate != newPerson.Birthdate)
            {
                if (oldPerson.Birthdate == null)
                {
                    changes.Add($"Добавена рождена дата: {newPerson.Birthdate}");
                }
                else if (newPerson.Birthdate == null)
                {
                    changes.Add($"Премахната рождена дата - {oldPerson.Birthdate}");
                }
                else
                {
                    changes.Add($"Редактирана рождена дата: {oldPerson.Birthdate} -> {newPerson.Birthdate}");
                }
            }

            if (oldPerson.HireDate.ToString(HireAndLeaveDateFormat) != newPerson.HireDate)
            {
                changes.Add($"Редактирана дата на постъпване: {oldPerson.HireDate.ToString(HireAndLeaveDateFormat)} -> {newPerson.HireDate}");
            }

            byte[] personPictureData = null;


            if (newPerson.PersonPicture != null)
            {
                using MemoryStream memoryStream = new MemoryStream();
                await newPerson.PersonPicture.CopyToAsync(memoryStream);
                personPictureData = memoryStream.ToArray();
            }

            if (!ArePicturesEqual(oldPerson.PersonPicture, personPictureData))
            {
                //TODO:
                if (oldPerson.PersonPicture == null && personPictureData != null)
                {
                    var base64Thumbnail = Convert.ToBase64String(CreateThumbnail(personPictureData));
                    changes.Add($"Добавена снимка: <img src='data:image/png;base64,{base64Thumbnail}' alt='Updated Picture' />");
                }
                else if (oldPerson.PersonPicture == null && personPictureData == null)
                {
                    var base64Thumbnail = Convert.ToBase64String(CreateThumbnail(oldPerson.PersonPicture));
                    changes.Add($"Премахната снимка: <img src='data:image/png;base64,{base64Thumbnail}' alt='Old Picture' />");
                }
                else if (oldPerson.PersonPicture != null && personPictureData != null)
                {
                    var base64ThumbnailOld = Convert.ToBase64String(CreateThumbnail(oldPerson.PersonPicture));
                    var base64ThumbnailNew = Convert.ToBase64String(CreateThumbnail(personPictureData));

                    changes.Add($"Редактирана снимка: <img src='data:image/png;base64,{base64ThumbnailOld}' alt='Old Picture' /> -> <img src='data:image/png;base64,{base64ThumbnailNew}' alt='Updated Picture' />");
                }
            }

            return changes;
        }

        public IQueryable<ChangeLog> GetAllAttached()
        {
            return _context.ChangeLogs.AsQueryable();
        }

        private bool ArePicturesEqual(byte[]? oldPicture, byte[]? newPicture)
        {
            if (oldPicture == null && newPicture == null)
            {
                return true;
            }

            if (oldPicture == null || newPicture == null)
            {
                return false;
            }

            return oldPicture.SequenceEqual(newPicture);
        }

        private byte[] CreateThumbnail(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
            {
                throw new ArgumentException("Image data is invalid or empty.");
            }

            using (var inputStream = new MemoryStream(imageData))
            using (var image = SixLabors.ImageSharp.Image.Load(inputStream))
            {
                image.Mutate(x => x.Resize(100, 130)); // Resize the image

                using (var outputStream = new MemoryStream())
                {
                    image.SaveAsPng(outputStream); // Save as PNG
                    return outputStream.ToArray();
                }
            }
        }
    }
}
