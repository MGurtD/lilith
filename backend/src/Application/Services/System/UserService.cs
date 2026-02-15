using Application.Contracts;
using Application.Services;
using Domain.Entities.Auth;

namespace Application.Services.System
{
    public class UserService(IUnitOfWork unitOfWork, ILocalizationService localizationService) : IUserService
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
