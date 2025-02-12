using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace BuditelPhonebook.Infrastructure.Seed
{
    public class ExcelDataSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public ExcelDataSeeder(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task SeedData(string filePath)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "System";

            // Ensure EPPlus license is set
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["КОНТАКТИ ЕКИП"];
                if (worksheet == null)
                {
                    throw new Exception("Sheet 'КОНТАКТИ ЕКИП' not found.");
                }

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {

                    if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
                    {
                        break;
                    }

                    string name = worksheet.Cells[row, 1].Text.Trim();
                    string roleName = worksheet.Cells[row, 2].Text.Trim();
                    string subjectGroup = worksheet.Cells[row, 3].Text.Trim();
                    string subjectOrDepartment = worksheet.Cells[row, 7].Text.Trim();
                    string businessEmail = worksheet.Cells[row, 8].Text.Trim();
                    string personalPhone = worksheet.Cells[row, 9].Text.Replace(" ", "").Trim();
                    string businessPhone = worksheet.Cells[row, 10].Text.Replace(" ", "").Trim();
                    string birthdate = worksheet.Cells[row, 11].Text.Trim();

                    if (personalPhone.StartsWith("359"))
                    {
                        personalPhone = personalPhone.Remove(0, 3);
                        personalPhone = personalPhone.Insert(0, "0");
                    }
                    else if (personalPhone.StartsWith("8"))
                    {
                        personalPhone = personalPhone.Insert(0, "0");
                    }

                    if (businessPhone.StartsWith("359"))
                    {
                        businessPhone = businessPhone.Remove(0, 3);
                        businessPhone = businessPhone.Insert(0, "0");
                    }
                    else if (businessPhone.StartsWith("8"))
                    {
                        businessPhone = businessPhone.Insert(0, "0");
                    }
                    else if (businessPhone.StartsWith("00359"))
                    {
                        businessPhone = businessPhone.Remove(0, 5);
                        businessPhone = businessPhone.Insert(0, "0");
                    }

                    if (birthdate.Length == 5)
                    {
                        birthdate += '.';
                    }

                    // Split full name into parts
                    List<string> nameParts = name.Split(' ').ToList();

                    if (nameParts.Contains("майчинство)"))
                    {
                        nameParts.RemoveRange(nameParts.Count - 2, 2);
                    }

                    string firstName = nameParts.Count > 0 ? nameParts[0] : "";
                    string? middleName = nameParts.Count > 2 ? nameParts[1] : null;
                    string lastName = nameParts.Count > 1 ? nameParts[^1] : "";

                    string departmentName = string.Empty;
                    string subject = null;

                    if (roleName == "Учител")
                    {
                        subject = subjectOrDepartment;
                        departmentName = "Образование";
                    }
                    else
                    {
                        departmentName = subjectOrDepartment;
                    }

                    Department? department = null;

                    if (!_context.Departments.Any(d => d.Name == departmentName))
                    {
                        department = new Department()
                        {
                            Name = departmentName
                        };

                        await _context.Departments.AddAsync(department);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == departmentName);
                    }

                    Role? role = null;

                    if (!_context.Roles.Any(r => r.Name == roleName))
                    {
                        role = new Role()
                        {
                            Name = roleName
                        };

                        await _context.Roles.AddAsync(role);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                    }

                    // Map to Person entity
                    Person person = new Person
                    {
                        FirstName = firstName,
                        MiddleName = middleName,
                        LastName = lastName,
                        Email = businessEmail,
                        PersonalPhoneNumber = personalPhone,
                        BusinessPhoneNumber = string.IsNullOrWhiteSpace(businessPhone) ? null : businessPhone,
                        Birthdate = string.IsNullOrWhiteSpace(birthdate) ? null : birthdate,
                        Role = role,
                        Department = department,
                        SubjectGroup = string.IsNullOrWhiteSpace(subjectGroup) ? null : subjectGroup,
                        Subject = string.IsNullOrWhiteSpace(subject) ? null : subject,
                        IsDeleted = false,
                        HireDate = DateTime.Now
                    };

                    await _context.People.AddAsync(person);
                    await _context.SaveChangesAsync();

                    ChangeLog change = new ChangeLog()
                    {
                        ChangedAt = DateTime.Now,
                        ChangedBy = currentUser,
                        ChangesDescriptions = new List<string> { "Добавен контакт от Org Chart." },
                        PersonId = person.Id,
                    };

                    await _context.ChangeLogs.AddAsync(change);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
