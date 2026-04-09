using Application.Contracts;
using Application.Services;
using Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace Application.Services.System
{
    public class UserService(IUnitOfWork unitOfWork, ILocalizationService localizationService, ILanguageCatalog languageCatalog) : IUserService
    {
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            var users = await unitOfWork.Users.GetAll();
            return users.OrderBy(u => u.Username);
        }

        public async Task<User?> GetUserById(Guid id)
        {
            return await unitOfWork.Users.Get(id);
        }

        public async Task<GenericResponse> CreateUser(User user)
        {
            // Validate username uniqueness
            var exists = unitOfWork.Users.Find(u => u.Username == user.Username).Any();
            if (exists)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityAlreadyExists"));
            }

            // Persist
            await unitOfWork.Users.Add(user);
            return new GenericResponse(true, user);
        }

        public async Task<GenericResponse> CreateManagedUser(CreateManagedUserRequest request)
        {
            var errors = new List<string>();

            if (request.Password != request.RepeatPassword)
            {
                errors.Add(localizationService.GetLocalizedString("UserPasswordsDoNotMatch"));
            }

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(request.Email))
            {
                errors.Add(localizationService.GetLocalizedString("Validation.InvalidEmail"));
            }

            var existingUser = unitOfWork.Users.Find(u => u.Username == request.Username).FirstOrDefault();
            if (existingUser is not null)
            {
                errors.Add(localizationService.GetLocalizedString("UserNotAvailable", request.Username));
            }

            var role = await unitOfWork.Roles.Get(request.RoleId);
            if (role is null || role.Disabled)
            {
                errors.Add(localizationService.GetLocalizedString("RoleNotFound", request.RoleId));
            }

            var language = await languageCatalog.GetByCodeAsync(request.PreferredLanguage);
            if (language is null)
            {
                errors.Add(localizationService.GetLocalizedString("LanguageNotFound", request.PreferredLanguage));
            }

            if (errors.Count > 0)
            {
                return new GenericResponse(false, errors);
            }

            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PreferredLanguage = request.PreferredLanguage,
                RoleId = request.RoleId,
                ProfileId = request.ProfileId,
                Disabled = false
            };

            await unitOfWork.Users.Add(user);
            return new GenericResponse(true, user);
        }

        public async Task<GenericResponse> UpdateUser(User user)
        {
            // Check if user exists
            var existing = await unitOfWork.Users.Get(user.Id);
            if (existing == null)
            {
                return new GenericResponse(false,
                    localizationService.GetLocalizedString("EntityNotFound", user.Id));
            }

            // Update
            await unitOfWork.Users.Update(user);
            return new GenericResponse(true, user);
        }
    }
}
