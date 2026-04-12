using Application.Contracts;

namespace Application.Contracts
{
    public interface IAuthenticationService
    {
        Task<AuthResponse> Register(UserRegisterRequest request);
        Task<AuthResponse> Login(UserLoginRequest request);
        Task<GenericResponse> ChangePassword(Guid userId, ChangePasswordRequest request);
        Task<AuthResponse> RefreshToken(TokenRequest request);
        Task<bool> Enable(Guid id);
        Task<AuthResponse> Logout(Guid userId);
        Task<int> PurgeExpiredRefreshTokens();
    }
}
