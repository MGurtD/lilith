using Application.Contracts;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services.System
{
    public class AuthenticationService(IOptions<AppSettings> settings, IUnitOfWork unitOfWork, TokenValidationParameters tokenValidationParameters, ILocalizationService localizationService) : IAuthenticationService
    {
        public async Task<AuthResponse> Register(UserRegisterRequest request)
        {
            // Validate existence of the unique user key
            var exists = unitOfWork.Users.Find(u => u.Username == request.Username).FirstOrDefault();
            if (exists is not null)
            {
                return new AuthResponse() 
                {
                    Result = false,
                    Errors = new List<string>() { localizationService.GetLocalizedString("UserNotAvailable", request.Username) }
                };
            }

            // Retrive the default role
            var defaultRole = unitOfWork.Roles.Find(r => r.Name == "user").FirstOrDefault();
            if (defaultRole is null)
            {
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>() { localizationService.GetLocalizedString("UserRoleNotFound") }
                };
            }

            // Generate instance of the user and add to database
            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Disabled = true,
                RoleId = defaultRole.Id
            };
            await unitOfWork.Users.Add(user);

            var authResponse = await GenerateJwtToken(user);
            return authResponse;
        }

        public async Task<bool> ChangePassword(UserLoginRequest request)
        {
            var user = unitOfWork.Users.Find((u) => u.Username == request.Username).FirstOrDefault();
            if (user is null) 
            {
                return false;
            }

            var encrPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
            user.Password = encrPassword; // assign new password before update
            await unitOfWork.Users.Update(user);
            return true;
        }

        public async Task<AuthResponse> Login(UserLoginRequest request)
        {
            var user = unitOfWork.Users.Find(u => u.Username == request.Username).FirstOrDefault();
            if (user is null)
            {
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>() { localizationService.GetLocalizedString("UserNotExist", request.Username) }
                };
            }

            var isPasswordValid = BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password);
            if (!isPasswordValid)
            {
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        localizationService.GetLocalizedString("UserPasswordInvalid")
                    }
                };
            }

            if (user.Disabled)
            {
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        localizationService.GetLocalizedString("UserDisabled", request.Username)
                    }
                };
            }

            return await GenerateJwtToken(user);
        }
        public async Task<AuthResponse> RefreshToken(TokenRequest request)
        {
            return await VerifyAndGenerateToken(request);
        }
        public async Task<bool> Enable(Guid id)
        {
            var user = await unitOfWork.Users.Get(id);
            if (user is null) return false;

            user.Disabled = false;
            await unitOfWork.Users.Update(user);

            return true;
        }
        public Task<AuthResponse> Logout(Guid id)
        {
            throw new NotImplementedException();
        }


        private async Task<AuthResponse> GenerateJwtToken(User user)
        {
            var signKey = Encoding.ASCII.GetBytes(settings.Value.JwtConfig.Secret);

            // Token specifications
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            // Build claims with optional email claim
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token identifier
                new Claim("id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("locale", string.IsNullOrWhiteSpace(user.PreferredLanguage) ? "ca" : user.PreferredLanguage)
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            }

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(settings.Value.JwtConfig.ExpirationTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signKey), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = Guid.NewGuid();

            var userRefreshToken = new UserRefreshToken()
            {
                JwtId = Guid.Parse(token.Id),
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Used = false,
                Revoked = false,
                UserId = user.Id
            };

            await unitOfWork.UserRefreshTokens.Add(userRefreshToken);

            return new AuthResponse()
            {
                Result = true,
                RefreshToken = refreshToken,
                Token = jwtToken
            };
        }

        private async Task<AuthResponse> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Clone validation parameters and disable lifetime validation for refresh flow
                var validationParams = tokenValidationParameters.Clone();
                validationParams.ValidateLifetime = false;

                var principal = jwtTokenHandler.ValidateToken(tokenRequest.Token, validationParams, out var validatedToken);

                // Ensure token algorithm is HmacSha256 and token type is JwtSecurityToken
                if (validatedToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { localizationService.GetLocalizedString("AuthInvalidAlgorithm") }
                    };
                }

                // Token is still valid?
                var expValue = principal.Claims.FirstOrDefault(t => t.Type == JwtRegisteredClaimNames.Exp)?.Value;
                if (string.IsNullOrWhiteSpace(expValue) || !long.TryParse(expValue, out var utcExpiryDate))
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { localizationService.GetLocalizedString("AuthInvalidParameters") }
                    };
                }

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);
                if (expiryDate > DateTime.UtcNow)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { localizationService.GetLocalizedString("AuthTokenValid", expiryDate) }
                    };
                }

                // Exist on the persistence layer?
                var storedToken = unitOfWork.UserRefreshTokens.Find(urt => urt.Token == tokenRequest.RefreshToken).FirstOrDefault();
                if (storedToken is null)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { localizationService.GetLocalizedString("AuthRefreshTokenNotExist", tokenRequest.RefreshToken) }
                    };
                }

                if (storedToken.Used)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { localizationService.GetLocalizedString("AuthRefreshTokenUsed") }
                    };
                }

                // Is a revoked token?
                if (storedToken.Revoked)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { localizationService.GetLocalizedString("AuthRefreshTokenRevoked") }
                    };
                }

                // Has the same identifier?
                var jti = principal.Claims.FirstOrDefault(t => t.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrWhiteSpace(jti) || storedToken.JwtId != Guid.Parse(jti))
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { localizationService.GetLocalizedString("AuthRefreshTokenMismatch", storedToken.JwtId, jti ?? "null") }
                    };
                }

                // Optional: user consistency check (defense-in-depth)
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userIdFromToken) || userIdFromToken != storedToken.UserId)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { localizationService.GetLocalizedString("UserNotFound") }
                    };
                }

                // Refresh token has been expired?
                if (storedToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { localizationService.GetLocalizedString("AuthTokenExpired") }
                    };
                }

                storedToken.Used = true;
                await unitOfWork.UserRefreshTokens.Update(storedToken);

                var user = await unitOfWork.Users.Get(storedToken.UserId);
                if (user is null)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { localizationService.GetLocalizedString("UserNotFound") }
                    };
                }

                return await GenerateJwtToken(user);
            }
            catch (SecurityTokenException)
            {
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>() { localizationService.GetLocalizedString("AuthTokenInvalid") }
                };
            }
            catch (Exception ex)
            {
                // Unexpected error
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>() { ex.Message }
                };
            }
        }

        // Seconds from 1-1-1970 00:00:00
        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeValue = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dateTimeValue.AddSeconds(unixTimeStamp).ToUniversalTime();
        }
    }
}





